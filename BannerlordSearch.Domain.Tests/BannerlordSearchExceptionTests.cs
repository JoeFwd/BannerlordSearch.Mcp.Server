using BannerlordSearch.Domain;
using System;
using Xunit;

namespace BannerlordSearch.Domain.Tests;

/// <summary>
/// Tests for the <see cref="BannerlordSearchError"/> class.
/// </summary>
public class BannerlordSearchExceptionTests
{
    /// <summary>
    /// Verifies that the <see cref="BannerlordSearchError"/> can be instantiated successfully.
    /// </summary>
    [Fact]
    public void BannerlordSearchException_CanBeInstantiated()
    {
        // Act
        var exception = new InvalidSourcePathError("Test message");

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidSourcePathError>(exception);
    }

    /// <summary>
    /// Verifies that the <see cref="BannerlordSearchError"/> can be instantiated with a message.
    /// </summary>
    [Fact]
    public void BannerlordSearchException_CanBeInstantiated_WithMessage()
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
    /// Verifies that the <see cref="BannerlordSearchError"/> can be instantiated with a message and inner exception.
    /// </summary>
    [Fact]
    public void BannerlordSearchException_CanBeInstantiated_WithMessageAndInnerException()
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
    /// Verifies that the <see cref="BannerlordSearchError"/> inherits from Exception.
    /// </summary>
    [Fact]
    public void BannerlordSearchException_InheritsFromException()
    {
        // Act
        var exception = new InvalidSourcePathError("Test message");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }
}
