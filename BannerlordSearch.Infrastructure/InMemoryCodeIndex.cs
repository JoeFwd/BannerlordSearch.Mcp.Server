using System.Text.RegularExpressions;
using BannerlordSearch.Domain;

namespace BannerlordSearch.Infrastructure;

/// <summary>
/// Builds and stores a full in-memory index of all C# source files, enabling fast
/// regex searching and O(1) class lookup by fully-qualified name.
/// The index is built once on the first call to <see cref="EnsureBuilt"/> and reused thereafter.
/// </summary>
public sealed class InMemoryCodeIndex : ICodeIndex
{
    private readonly IFileSystem _fileSystem;
    private readonly object _buildLock = new();

    private List<IndexedFile> _files = new();
    private Dictionary<string, IndexedFile> _classLookup = new(StringComparer.Ordinal);
    private string? _builtRootPath;

    private static readonly Regex ClassDeclRegex = new(@"\bclass\s+(\w+)", RegexOptions.Compiled);

    public InMemoryCodeIndex(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public bool IsReady => _builtRootPath != null;

    public IReadOnlyList<IndexedFile> Files => _files;

    public IndexedFile? FindClass(string fullyQualifiedName)
    {
        _classLookup.TryGetValue(fullyQualifiedName, out var file);
        return file;
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
            _files = new List<IndexedFile>();
            _classLookup = new Dictionary<string, IndexedFile>(StringComparer.Ordinal);
            _builtRootPath = rootPath;
            return;
        }

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

        var files = new List<IndexedFile>(csFilePaths.Count);
        var lookup = new Dictionary<string, IndexedFile>(StringComparer.Ordinal);

        foreach (var filePath in csFilePaths)
        {
            string[] lines;
            try { lines = _fileSystem.ReadAllLines(filePath); }
            catch { continue; }

            var indexedFile = new IndexedFile { FilePath = filePath, Lines = lines };
            files.Add(indexedFile);
            ParseAndIndexClasses(lines, indexedFile, lookup);
        }

        _files = files;
        _classLookup = lookup;
        _builtRootPath = rootPath;
    }

    private static void ParseAndIndexClasses(
        string[] lines,
        IndexedFile indexedFile,
        Dictionary<string, IndexedFile> lookup)
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
                lookup.TryAdd(fqn, indexedFile);
            }
        }
    }
}
