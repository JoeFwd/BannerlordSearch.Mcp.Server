using System.Text.RegularExpressions;
using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.IO;
using BannerlordSearch.Application.Ports.Repositories;
using BannerlordSearch.Domain;
using BannerlordSearch.Domain.Models;
using Microsoft.Extensions.Logging;

namespace BannerlordSearch.Infrastructure;

/// <summary>
/// Builds and stores a full in-memory index of all C# source files, enabling fast
/// regex searching and O(1) class lookup by fully-qualified name.
/// The index is built once on the first call to <see cref="EnsureBuilt"/> and reused thereafter.
/// </summary>
public sealed class InMemoryCodeIndex : ICodeIndex
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<InMemoryCodeIndex> _logger;
    private readonly object _buildLock = new();

    private List<IndexedFile> _files = new();
    private Dictionary<string, IndexedFile> _classLookup = new(StringComparer.Ordinal);
    private Dictionary<string, List<string>> _simpleNameLookup = new(StringComparer.Ordinal);
    private string? _builtRootPath;

    private static readonly Regex ClassDeclRegex = new(@"\bclass\s+(\w+)", RegexOptions.Compiled);

    public InMemoryCodeIndex(IFileSystem fileSystem, ILogger<InMemoryCodeIndex> logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsReady => _builtRootPath != null;

    public IReadOnlyList<IndexedFile> Files => _files;

    public IndexedFile? FindClass(string fullyQualifiedName)
    {
        _classLookup.TryGetValue(fullyQualifiedName, out var file);
        return file;
    }

    public IReadOnlyList<string> FindFullyQualifiedNames(string simpleClassName)
    {
        return _simpleNameLookup.TryGetValue(simpleClassName, out var fqns)
            ? fqns
            : Array.Empty<string>();
    }

    /// <summary>
    /// Builds the index for <paramref name="rootPath"/> on the first call.
    /// Subsequent calls with the same path are no-ops (idempotent).
    /// Thread-safe via double-checked locking.
    /// </summary>
    public void EnsureBuilt(string rootPath)
    {
        ArgumentNullException.ThrowIfNull(rootPath);
        if (_builtRootPath == rootPath) return;

        lock (_buildLock)
        {
            if (_builtRootPath == rootPath) return;
            Build(rootPath);
        }
    }

    private void Build(string rootPath)
    {
        if (!_fileSystem.DirectoryExists(rootPath))
        {
            _logger.LogWarning("Root path not found: {RootPath}", rootPath);
            _files = new List<IndexedFile>();
            _classLookup = new Dictionary<string, IndexedFile>(StringComparer.Ordinal);
            _simpleNameLookup = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            _builtRootPath = rootPath;
            return;
        }

        _logger.LogDebug("Enumerating .cs files under {RootPath}", rootPath);

        List<string> csFilePaths;
        try
        {
            csFilePaths = _fileSystem
                .EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories)
                .ToList();
        }
        catch (IOException)
        {
            csFilePaths = new List<string>();
        }

        _logger.LogInformation("Found {FileCount} .cs files", csFilePaths.Count);

        var files = new List<IndexedFile>(csFilePaths.Count);
        var lookup = new Dictionary<string, IndexedFile>(StringComparer.Ordinal);
        var simpleNameLookup = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var filePath in csFilePaths)
        {
            string[] lines;
            try { lines = _fileSystem.ReadAllLines(filePath); }
            catch (Exception ex)
            {
                _logger.LogWarning("Skipping file {FilePath}: {Message}", filePath, ex.Message);
                continue;
            }

            var indexedFile = new IndexedFile { FilePath = filePath, Lines = lines };
            files.Add(indexedFile);
            ParseAndIndexClasses(lines, indexedFile, lookup, simpleNameLookup);
        }

        _files = files;
        _classLookup = lookup;
        _simpleNameLookup = simpleNameLookup;
        _builtRootPath = rootPath;

        _logger.LogInformation("Index ready: {FileCount} files, {ClassCount} classes indexed", files.Count, lookup.Count);
    }

    private static void ParseAndIndexClasses(
        string[] lines,
        IndexedFile indexedFile,
        Dictionary<string, IndexedFile> lookup,
        Dictionary<string, List<string>> simpleNameLookup)
    {
        string currentNamespace = string.Empty;

        foreach (var rawLine in lines)
        {
            var trimmed = rawLine.Trim();

            if (trimmed.StartsWith("namespace ", StringComparison.Ordinal))
            {
                // Handles block namespaces ("namespace Foo {") and file-scoped ("namespace Foo;")
                var ns = trimmed.Substring("namespace ".Length).Trim();
                ns = ns.Split('{')[0].Trim().TrimEnd(';').Trim();
                if (!string.IsNullOrEmpty(ns))
                    currentNamespace = ns;
                continue;
            }

            var classMatch = ClassDeclRegex.Match(trimmed);
            if (classMatch.Success)
            {
                var simpleClassName = classMatch.Groups[1].Value;
                var fqn = string.IsNullOrEmpty(currentNamespace)
                    ? simpleClassName
                    : $"{currentNamespace}.{simpleClassName}";
                if (lookup.TryAdd(fqn, indexedFile))
                {
                    if (!simpleNameLookup.TryGetValue(simpleClassName, out var fqns))
                    {
                        fqns = new List<string>();
                        simpleNameLookup[simpleClassName] = fqns;
                    }
                    fqns.Add(fqn);
                }
            }
        }
    }
}
