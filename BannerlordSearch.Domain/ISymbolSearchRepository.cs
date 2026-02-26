namespace BannerlordSearch.Domain;

/// <summary>
/// Repository interface for symbol search functionality.
/// </summary>
public interface ISymbolSearchRepository
{
    /// <summary>
    /// Returns a list of all *.cs files under <paramref name="rootPath"/>.
    /// Results are cached for the lifetime of the repository instance.
    /// </summary>
    List<string> GetCsFiles(string rootPath);

    /// <summary>
    /// Reads all lines from the specified file.
    /// </summary>
    string[] ReadAllLines(string path);
}