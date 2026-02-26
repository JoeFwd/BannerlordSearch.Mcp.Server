namespace BannerlordSearch.Domain;

/// <summary>
/// Represents a single result from a symbol search.
/// </summary>
public class SearchResult
{
    /// <summary>
    /// The location where the symbol was found (class.method or class).
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// The method name where the symbol was found, if applicable.
    /// </summary>
    public string Method { get; set; } = string.Empty;
    
    /// <summary>
    /// The line of code where the symbol was found.
    /// </summary>
    public string CodeLine { get; set; } = string.Empty;
    
    /// <summary>
    /// Context lines before the found symbol.
    /// </summary>
    public List<string> ContextBefore { get; set; } = new List<string>();
    
    /// <summary>
    /// Context lines after the found symbol.
    /// </summary>
    public List<string> ContextAfter { get; set; } = new List<string>();
}