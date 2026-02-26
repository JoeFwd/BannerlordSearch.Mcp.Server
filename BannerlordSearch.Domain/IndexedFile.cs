namespace BannerlordSearch.Domain;

/// <summary>
/// Represents a C# source file loaded into the in-memory code index.
/// </summary>
public sealed class IndexedFile
{
    public string FilePath { get; init; } = string.Empty;
    public string[] Lines { get; init; } = Array.Empty<string>();
}
