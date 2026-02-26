using System.Collections.Concurrent;
using BannerlordSearch.Application.Ports;

namespace BannerlordSearch.Infrastructure;

/// <summary>
/// Repository responsible for retrieving and caching C# source files.
/// It abstracts file-system access via <see cref="IFileSystem"/> to enable unit testing.
/// </summary>
public sealed class SymbolFileRepository : ISymbolSearchRepository
{
    private readonly IFileSystem _fileSystem;
    // Cache per root path. The list is immutable after creation.
    private readonly ConcurrentDictionary<string, List<string>> _cache = new();

    public SymbolFileRepository(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Returns a list of all *.cs files under <paramref name="rootPath"/>.
    /// Results are cached for the lifetime of the repository instance.
    /// </summary>
    public List<string> GetCsFiles(string rootPath)
    {
        if (rootPath == null)
            throw new ArgumentNullException(nameof(rootPath));
            
        if (!_fileSystem.DirectoryExists(rootPath))
        {
            return new List<string>();
        }

        return _cache.GetOrAdd(rootPath, path =>
        {
            try
            {
                var files = _fileSystem
                    .EnumerateFiles(path, "*.cs", SearchOption.AllDirectories)
                    .ToList();
                return files;
            }
            catch (IOException)
            {
                // Return empty list on enumeration exceptions
                return new List<string>();
            }
        });
    }

    /// <summary>
    /// Reads all lines from the specified file using the injected file system.
    /// </summary>
    public string[] ReadAllLines(string path) => _fileSystem.ReadAllLines(path);
}