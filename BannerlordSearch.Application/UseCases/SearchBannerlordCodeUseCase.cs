using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BannerlordSearch.Domain;
 
namespace BannerlordSearch.Application.UseCases;
 
/// <summary>
/// Use case for searching Bannerlord code using regular expressions.
/// </summary>
public class SearchBannerlordCodeUseCase
{
    private readonly ISymbolSearchRepository _fileRepository;
    private readonly IFileSystem _fileSystem;
 
    public SearchBannerlordCodeUseCase(ISymbolSearchRepository fileRepository, IFileSystem fileSystem)
    {
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }
 
    /// <summary>
    ///     Searches all .cs files under <paramref name="rootPath" /> for the given <paramref name="regexp" />.
    ///     This method is used internally by the MCP tool to perform regexp searches in Bannerlord decompiled source code.
    ///     It's optimized for performance with parallel processing and caching of file lists.
    /// </summary>
    /// <param name="regexp">The regexp to look for (required). Can be a class name, method name, property name, or variable name.</param>
    /// <param name="rootPath">Folder containing decompiled Bannerlord sources. This should point to the root directory of the decompiled Bannerlord source tree.</param>
    /// <param name="maxResults">Maximum number of results to return (int.MaxValue = unlimited). Default is 1000.</param>
    /// <param name="contextLines">Number of context lines to include before/after each match. Default is 10.</param>
    /// <returns>A list of search results with detailed information including location, code snippet, and context lines.</returns>
    /// <remarks>
    ///     This method is designed for:
    ///     - Rapid exploration of Bannerlord source code
    ///     - Modding development to understand existing implementations
    ///     - Finding specific code elements in large codebases
    ///
    ///     Performance considerations:
    ///     - Uses parallel processing for faster search across multiple files
    ///     - Caches file lists to avoid repeated directory scans
    ///     - Stops searching early when maxResults is reached
    /// </remarks>
    public virtual List<SearchResult> Execute(string regexp, string rootPath, int maxResults, int contextLines)
    {
        var results = new List<SearchResult>();
        if (string.IsNullOrEmpty(regexp))
            throw new ArgumentNullException(nameof(regexp));
            
        if (!_fileSystem.DirectoryExists(rootPath))
        {
            // In a real implementation, this should probably throw an exception or log the error
            return results;
        }

        var symbolRegex = new Regex(regexp, RegexOptions.Compiled);
        var classRegex = new Regex(@"\bclass\s+(\w+)", RegexOptions.Compiled);
        // Simplified method regex to capture method name before '('.
        var methodRegex = new Regex(@"\b(\w+)\s*\(", RegexOptions.Compiled);
        var namespaceRegex = new Regex(@"namespace\s+([\w\.]+)", RegexOptions.Compiled);

        // Use repository to retrieve (and cache) file list for faster repeated searches
        var csFiles = _fileRepository.GetCsFiles(rootPath);
        var resultsBag = new ConcurrentBag<SearchResult>();
        int matchCountLocal = 0; // thread-safe counter

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        Parallel.ForEach(csFiles, parallelOptions, (file, state) =>
        {
            string[] lines;
            try
            {
                lines = _fileRepository.ReadAllLines(file);
            }
            catch (Exception)
            {
                // In a real implementation, this should probably log the error
                return;
            }

            string currentClass = string.Empty;
            string currentMethod = string.Empty;
            string currentNamespace = string.Empty;
            var beforeBuffer = new Queue<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var classMatch = classRegex.Match(lines[i]);
                if (classMatch.Success)
                {
                    currentClass = classMatch.Groups[1].Value;
                    currentMethod = string.Empty;
                }

                var methodMatch = methodRegex.Match(lines[i]);
                if (methodMatch.Success)
                    currentMethod = methodMatch.Groups[1].Value;
                // Simple namespace detection
                if (lines[i].StartsWith("namespace "))
                {
                    var ns = lines[i].Substring("namespace ".Length).Trim();
                    // Remove any trailing opening brace or whitespace
                    ns = ns.Split('{')[0].Trim();
                    currentNamespace = ns;
                }

                if (symbolRegex.IsMatch(lines[i]))
                {
                    int currentMatch = Interlocked.Increment(ref matchCountLocal);

                    // Early exit if limit reached
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

                    // Location should be only the namespace (or <global> if none)
                    string location = !string.IsNullOrEmpty(currentNamespace) ? currentNamespace : "<global>";

                    var result = new SearchResult
                    {
                        Location = location,
                        Method = !string.IsNullOrEmpty(currentMethod) ? currentMethod : string.Empty,
                        CodeLine = lines[i].Trim(),
                        ContextBefore = contextBefore,
                        ContextAfter = contextAfter
                    };

                    resultsBag.Add(result);

                    beforeBuffer.Clear();
                }
                else
                {
                    if (contextLines > 0)
                    {
                        beforeBuffer.Enqueue(lines[i].Trim());
                        if (beforeBuffer.Count > contextLines)
                            beforeBuffer.Dequeue();
                    }
                }
            }
        });

        results.AddRange(resultsBag);
        // Add total matches result only if we have results
        if (matchCountLocal > 0)
        {
            AddTotalResult(results, regexp, matchCountLocal);
        }
        return results;
    }

    private static void AddLimitResult(ConcurrentBag<SearchResult> bag, int maxResults, int currentMatch,
        ParallelLoopState state)
    {
        var limitResult = new SearchResult
        {
            CodeLine = $"\nReached max result limit of {maxResults}. Stopping search."
        };
        bag.Add(limitResult);
        state.Stop();
    }

    private static void AddTotalResult(List<SearchResult> results, string regexp, int total)
    {
        results.Add(new SearchResult
        {
            CodeLine = $"\nTotal matches for \"{regexp}\": {total}"
        });
    }
}