namespace BannerlordSearch.Domain;

/// <summary>
/// Exception thrown when validation fails in the domain layer.
/// </summary>
public class ValidationError : BannerlordSearchError
{
    public ValidationError(string message) : base(message)
    {
    }

    public ValidationError(string message, Exception innerException) : base(message, innerException)
    {
    }
}