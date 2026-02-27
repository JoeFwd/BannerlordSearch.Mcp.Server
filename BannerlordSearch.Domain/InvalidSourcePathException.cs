namespace BannerlordSearch.Domain;

/// <summary>
/// Exception thrown when the Bannerlord source path is invalid or not configured.
/// </summary>
public class InvalidSourcePathError : BannerlordSearchError
{
    public InvalidSourcePathError(string message) : base(message)
    {
    }

    public InvalidSourcePathError(string message, Exception innerException) : base(message, innerException)
    {
    }
}