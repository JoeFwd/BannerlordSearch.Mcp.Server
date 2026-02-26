namespace BannerlordSearch.Application.Ports;

/// <summary>
/// Abstraction over file-system operations to enable unit testing without touching the real disk.
/// </summary>
public interface IFileSystem
{
    /// <summary>Checks whether a directory exists.</summary>
    bool DirectoryExists(string path);

    /// <summary>Checks whether a file exists.</summary>
    bool FileExists(string path);

    /// <summary>Enumerates files matching a pattern.</summary>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

    /// <summary>Reads all lines from a file.</summary>
    string[] ReadAllLines(string path);
}
