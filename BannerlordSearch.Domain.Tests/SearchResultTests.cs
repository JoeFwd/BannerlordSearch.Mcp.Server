using Xunit;

namespace BannerlordSearch.Domain.Tests;

public class SearchResultTests
{
    [Fact]
    public void SearchResult_CanBeInstantiated()
    {
        // Arrange
        var searchResult = new SearchResult();

        // Act
        // Nothing to really act here, just checking instantiation

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.ContextBefore);
        Assert.NotNull(searchResult.ContextAfter);
    }

    [Fact]
    public void SearchResult_PropertiesAreInitializedCorrectly()
    {
        // Arrange
        var searchResult = new SearchResult();

        // Act
        // Properties are initialized by default

        // Assert
        Assert.Equal(string.Empty, searchResult.Location);
        Assert.Equal(string.Empty, searchResult.Method);
        Assert.Equal(string.Empty, searchResult.CodeLine);
        Assert.NotNull(searchResult.ContextBefore);
        Assert.NotNull(searchResult.ContextAfter);
    }

    [Fact]
    public void SearchResult_SetPropertiesCorrectly()
    {
        // Arrange
        var searchResult = new SearchResult();
        var contextBefore = new List<string> { "line1", "line2" };
        var contextAfter = new List<string> { "line3", "line4" };

        // Act
        searchResult.Location = "TestLocation";
        searchResult.Method = "TestMethod";
        searchResult.CodeLine = "TestCodeLine";
        searchResult.ContextBefore = contextBefore;
        searchResult.ContextAfter = contextAfter;

        // Assert
        Assert.Equal("TestLocation", searchResult.Location);
        Assert.Equal("TestMethod", searchResult.Method);
        Assert.Equal("TestCodeLine", searchResult.CodeLine);
        Assert.Same(contextBefore, searchResult.ContextBefore);
        Assert.Same(contextAfter, searchResult.ContextAfter);
    }

    [Fact]
    public void SearchResult_ContextListsAreInitializedAsEmptyCollections()
    {
        // Arrange
        var searchResult = new SearchResult();

        // Act
        // No action needed, just checking initialization

        // Assert
        Assert.NotNull(searchResult.ContextBefore);
        Assert.NotNull(searchResult.ContextAfter);
        Assert.Equal(0, searchResult.ContextBefore.Count);
        Assert.Equal(0, searchResult.ContextAfter.Count);
    }

    [Fact]
    public void SearchResult_ContextListsAreIndependent()
    {
        // Arrange
        var searchResult = new SearchResult();

        // Act
        searchResult.ContextBefore.Add("testLine");
        var beforeCount = searchResult.ContextBefore.Count;

        // Assert
        Assert.Equal(1, beforeCount);
        Assert.Equal(0, searchResult.ContextAfter.Count);
    }

    [Fact]
    public void SearchResult_LocationPropertyHandlesNullValue()
    {
        // Arrange
        var searchResult = new SearchResult();

        // Act
        searchResult.Location = null;

        // Assert
        Assert.Null(searchResult.Location);
    }

    [Fact]
    public void SearchResult_MethodPropertyHandlesNullValue()
    {
        // Arrange
        var searchResult = new SearchResult();

        // Act
        searchResult.Method = null;

        // Assert
        Assert.Null(searchResult.Method);
    }

    [Fact]
    public void SearchResult_CodeLinePropertyHandlesNullValue()
    {
        // Arrange
        var searchResult = new SearchResult();

        // Act
        searchResult.CodeLine = null;

        // Assert
        Assert.Null(searchResult.CodeLine);
    }
}