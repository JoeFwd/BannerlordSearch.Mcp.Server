namespace BannerlordSearch.Domain;

/// <summary>
/// Exception thrown when validation fails in the domain layer.
/// </summary>
public class ValidationException : BannerlordSearchException
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}