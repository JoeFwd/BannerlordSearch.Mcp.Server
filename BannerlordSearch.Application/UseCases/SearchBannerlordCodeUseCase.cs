using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.Repositories;
using BannerlordSearch.Domain;
using BannerlordSearch.Domain.Models;
using Microsoft.Extensions.Logging;

namespace BannerlordSearch.Application.UseCases;

/// <summary>
/// Use case for searching Bannerlord code using regular expressions.
/// Delegates to <see cref="ICodeIndex"/> so that all file content is searched in-memory
/// rather than read from disk on every call.
/// </summary>
public class SearchBannerlordCodeUseCase
{
    private readonly ICodeIndex _codeIndex;
    private readonly ILogger<SearchBannerlordCodeUseCase> _logger;

    public SearchBannerlordCodeUseCase(ICodeIndex codeIndex, ILogger<SearchBannerlordCodeUseCase> logger)
    {
        _codeIndex = codeIndex ?? throw new ArgumentNullException(nameof(codeIndex));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches all indexed .cs files for the given <paramref name="regexp"/>.
    /// The index must be built before calling this method (see <see cref="IndexBannerlordCodeUseCase"/>).
    /// </summary>
    public virtual List<SearchResult> Execute(string regexp, int maxResults, int contextLines)
    {
        if (string.IsNullOrEmpty(regexp))
            throw new ArgumentNullException(nameof(regexp));

        _logger.LogDebug("Searching for pattern '{Pattern}' (maxResults={MaxResults}, contextLines={ContextLines})",
            regexp, maxResults, contextLines);

        var files = _codeIndex.Files;
        if (files.Count == 0)
            return new List<SearchResult>();

        var symbolRegex = new Regex(regexp, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var resultsBag = new ConcurrentBag<SearchResult>();
        var matchCount = new int[1];

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        Parallel.ForEach(files, parallelOptions,
            (indexedFile, state) => SearchFile(indexedFile, state, symbolRegex, maxResults, contextLines, resultsBag, matchCount));

        var results = new List<SearchResult>(resultsBag);
        _logger.LogInformation("Search for '{Pattern}' returned {ResultCount} result(s)", regexp, results.Count);
        return results;
    }

    private static void SearchFile(
        IndexedFile file, ParallelLoopState state, Regex symbolRegex,
        int maxResults, int contextLines, ConcurrentBag<SearchResult> results, int[] matchCount)
    {
        string currentNamespace = string.Empty;
        string currentMethod = string.Empty;
        var beforeBuffer = new Queue<string>();
        var lines = file.Lines;

        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();
            UpdateNamespace(trimmed, ref currentNamespace);
            UpdateCurrentMethod(trimmed, ref currentMethod);

            if (!symbolRegex.IsMatch(lines[i]))
            {
                AddToBeforeBuffer(beforeBuffer, trimmed, contextLines);
                continue;
            }

            if (Interlocked.Increment(ref matchCount[0]) > maxResults)
            {
                state.Stop();
                return;
            }

            results.Add(BuildResult(lines, i, contextLines, currentNamespace, currentMethod, beforeBuffer));
            beforeBuffer.Clear();
        }
    }

    private static SearchResult BuildResult(
        string[] lines, int i, int contextLines,
        string currentNamespace, string currentMethod, Queue<string> beforeBuffer) =>
        new SearchResult
        {
            Location = string.IsNullOrEmpty(currentNamespace) ? "<global>" : currentNamespace,
            Method = currentMethod,
            CodeLine = lines[i].Trim(),
            ContextBefore = new List<string>(beforeBuffer),
            ContextAfter = GetContextAfter(lines, i, contextLines)
        };

    private static void UpdateNamespace(string trimmed, ref string currentNamespace)
    {
        if (!trimmed.StartsWith("namespace ", StringComparison.Ordinal)) return;
        var ns = trimmed.Substring("namespace ".Length).Trim().Split('{')[0].Trim().TrimEnd(';').Trim();
        if (!string.IsNullOrEmpty(ns)) currentNamespace = ns;
    }

    private static void UpdateCurrentMethod(string trimmed, ref string currentMethod)
    {
        if (Regex.IsMatch(trimmed, @"\bclass\s+\w+"))
            currentMethod = string.Empty;
        var match = Regex.Match(trimmed, @"\b(\w+)\s*\(");
        if (match.Success)
            currentMethod = match.Groups[1].Value;
    }

    private static void AddToBeforeBuffer(Queue<string> buffer, string trimmed, int contextLines)
    {
        if (contextLines <= 0) return;
        buffer.Enqueue(trimmed);
        if (buffer.Count > contextLines)
            buffer.Dequeue();
    }

    private static List<string> GetContextAfter(string[] lines, int i, int contextLines)
    {
        var after = new List<string>();
        for (int j = 1; j <= contextLines && i + j < lines.Length; j++)
            after.Add(lines[i + j].Trim());
        return after;
    }
}
