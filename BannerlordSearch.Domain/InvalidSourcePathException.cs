namespace BannerlordSearch.Domain;

/// <summary>
/// Exception thrown when the Bannerlord source path is invalid or not configured.
/// </summary>
public class InvalidSourcePathException : BannerlordSearchException
{
    public InvalidSourcePathException(string message) : base(message)
    {
    }

    public InvalidSourcePathException(string message, Exception innerException) : base(message, innerException)
    {
    }
}