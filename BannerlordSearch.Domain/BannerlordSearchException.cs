namespace BannerlordSearch.Domain;

/// <summary>
/// Base exception class for all domain-specific exceptions in the Bannerlord Search application.
/// </summary>
public abstract class BannerlordSearchException : Exception
{
    protected BannerlordSearchException(string message) : base(message)
    {
    }

    protected BannerlordSearchException(string message, Exception innerException) : base(message, innerException)
    {
    }
}