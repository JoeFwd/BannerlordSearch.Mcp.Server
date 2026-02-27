using Xunit;

namespace BannerlordSearch.Domain.Tests;

/// <summary>
/// Tests for the <see cref="InvalidSourcePathError"/> class.
/// </summary>
public class InvalidSourcePathExceptionTests
{
    /// <summary>
    /// Verifies that the <see cref="InvalidSourcePathError"/> can be instantiated successfully.
    /// </summary>
    [Fact]
    public void InvalidSourcePathException_CanBeInstantiated()
    {
        // Act
        var exception = new InvalidSourcePathError("Test message");

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidSourcePathError>(exception);
    }

    /// <summary>
    /// Verifies that the <see cref="InvalidSourcePathError"/> can be instantiated with a message.
    /// </summary>
    [Fact]
    public void InvalidSourcePathException_CanBeInstantiated_WithMessage()
    {
        // Arrange
        const string message = "Test exception message";

        // Act
        var exception = new InvalidSourcePathError(message);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(message, exception.Message);
        Assert.IsType<InvalidSourcePathError>(exception);
    }

    /// <summary>
    /// Verifies that the <see cref="InvalidSourcePathError"/> can be instantiated with a message and inner exception.
    /// </summary>
    [Fact]
    public void InvalidSourcePathException_CanBeInstantiated_WithMessageAndInnerException()
    {
        // Arrange
        const string message = "Test exception message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new InvalidSourcePathError(message, innerException);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
        Assert.IsType<InvalidSourcePathError>(exception);
    }

    /// <summary>
    /// Verifies that the <see cref="InvalidSourcePathError"/> inherits from <see cref="BannerlordSearchError"/>.
    /// </summary>
    [Fact]
    public void InvalidSourcePathException_InheritsFromBannerlordSearchException()
    {
        // Act
        var exception = new InvalidSourcePathError("Test message");

        // Assert
        Assert.IsAssignableFrom<BannerlordSearchError>(exception);
    }
}
