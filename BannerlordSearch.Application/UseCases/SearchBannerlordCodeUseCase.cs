using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BannerlordSearch.Application.Ports;
using BannerlordSearch.Domain;

namespace BannerlordSearch.Application.UseCases;

/// <summary>
/// Use case for searching Bannerlord code using regular expressions.
/// Delegates to <see cref="ICodeIndex"/> so that all file content is searched in-memory
/// rather than read from disk on every call.
/// </summary>
public class SearchBannerlordCodeUseCase
{
    private readonly ICodeIndex _codeIndex;

    public SearchBannerlordCodeUseCase(ICodeIndex codeIndex)
    {
        _codeIndex = codeIndex ?? throw new ArgumentNullException(nameof(codeIndex));
    }

    /// <summary>
    /// Searches all indexed .cs files for the given <paramref name="regexp"/>.
    /// The index must be built before calling this method (see <see cref="IndexBannerlordCodeUseCase"/>).
    /// </summary>
    public virtual List<SearchResult> Execute(string regexp, int maxResults, int contextLines)
    {
        if (string.IsNullOrEmpty(regexp))
            throw new ArgumentNullException(nameof(regexp));

        var files = _codeIndex.Files;
        if (files.Count == 0)
            return new List<SearchResult>();

        var symbolRegex = new Regex(regexp, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var resultsBag = new ConcurrentBag<SearchResult>();
        int matchCountLocal = 0;

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        Parallel.ForEach(files, parallelOptions, (indexedFile, state) =>
        {
            var lines = indexedFile.Lines;
            string currentMethod = string.Empty;
            string currentNamespace = string.Empty;
            var beforeBuffer = new Queue<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();

                if (trimmed.StartsWith("namespace ", StringComparison.Ordinal))
                {
                    var ns = trimmed.Substring("namespace ".Length).Trim();
                    ns = ns.Split('{')[0].Trim().TrimEnd(';').Trim();
                    if (!string.IsNullOrEmpty(ns)) currentNamespace = ns;
                }

                if (Regex.IsMatch(trimmed, @"\bclass\s+\w+"))
                    currentMethod = string.Empty;

                var methodMatch = Regex.Match(trimmed, @"\b(\w+)\s*\(");
                if (methodMatch.Success)
                    currentMethod = methodMatch.Groups[1].Value;

                if (symbolRegex.IsMatch(lines[i]))
                {
                    int currentMatch = Interlocked.Increment(ref matchCountLocal);
                    if (currentMatch > maxResults)
                    {
                        state.Stop();
                        return;
                    }

                    var contextBefore = new List<string>(beforeBuffer);
                    var contextAfter = new List<string>();
                    if (contextLines > 0)
                    {
                        for (int j = 1; j <= contextLines && i + j < lines.Length; j++)
                            contextAfter.Add(lines[i + j].Trim());
                    }

                    string location = !string.IsNullOrEmpty(currentNamespace) ? currentNamespace : "<global>";
                    resultsBag.Add(new SearchResult
                    {
                        Location = location,
                        Method = !string.IsNullOrEmpty(currentMethod) ? currentMethod : string.Empty,
                        CodeLine = lines[i].Trim(),
                        ContextBefore = contextBefore,
                        ContextAfter = contextAfter
                    });

                    beforeBuffer.Clear();
                }
                else
                {
                    if (contextLines > 0)
                    {
                        beforeBuffer.Enqueue(trimmed);
                        if (beforeBuffer.Count > contextLines)
                            beforeBuffer.Dequeue();
                    }
                }
            }
        });

        var results = new List<SearchResult>(resultsBag);
        if (matchCountLocal > 0)
            results.Add(new SearchResult { CodeLine = $"\nTotal matches for \"{regexp}\": {matchCountLocal}" });

        return results;
    }
}
