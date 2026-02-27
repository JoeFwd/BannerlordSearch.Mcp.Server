namespace BannerlordSearch.Domain;

/// <summary>
/// Base exception class for all domain-specific exceptions in the Bannerlord Search application.
/// </summary>
public abstract class BannerlordSearchError : Exception
{
    protected BannerlordSearchError(string message) : base(message)
    {
    }

    protected BannerlordSearchError(string message, Exception innerException) : base(message, innerException)
    {
    }
}