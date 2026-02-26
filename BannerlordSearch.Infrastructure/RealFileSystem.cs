using BannerlordSearch.Domain;

namespace BannerlordSearch.Infrastructure;

/// <summary>
/// Concrete implementation of <see cref="IFileSystem"/> that delegates to System.IO.
/// </summary>
public sealed class RealFileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) =>
        Directory.EnumerateFiles(path, searchPattern, searchOption);

    public string[] ReadAllLines(string path) => File.ReadAllLines(path);
}