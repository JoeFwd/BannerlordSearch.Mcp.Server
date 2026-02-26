using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using Moq;
using Xunit;

namespace BannerlordSearch.Application.Tests;

/// <summary>
/// Tests for the <see cref="SearchBannerlordCodeUseCase"/> class.
/// </summary>
public class SearchBannerlordCodeUseCaseTests
{
    /// <summary>
    /// Verifies that the <see cref="SearchBannerlordCodeUseCase"/> can be instantiated successfully.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_CanBeInstantiated()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();

        // Act
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);

        // Assert
        Assert.NotNull(useCase);
    }

    /// <summary>
    /// Verifies that the constructor properly validates its parameters and throws
    /// <see cref="ArgumentNullException"/> when the repository is null.
    /// </summary>
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>();
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SearchBannerlordCodeUseCase(null, mockFileSystem.Object));
    }

    /// <summary>
    /// Verifies that the constructor properly validates its parameters and throws
    /// <see cref="ArgumentNullException"/> when the file system is null.
    /// </summary>
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFileSystemIsNull()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SearchBannerlordCodeUseCase(mockRepository.Object, null));
    }

    /// <summary>
    /// Verifies that the Execute method returns an empty list when the root path does not exist.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_ReturnsEmptyList_WhenRootPathDoesNotExist()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "nonexistent_path";
        var regexp = "test";
        var maxResults = 1000;
        var contextLines = 10;

        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(false);

        // Act
        var results = useCase.Execute(regexp, rootPath, maxResults, contextLines);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    /// Verifies that the Execute method returns results when valid input is provided.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_ReturnsResults_WhenValidInputProvided()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = "TestClass";
        var maxResults = 1000;
        var contextLines = 10;
        
        // Setup mocks
        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockRepository.Setup(r => r.GetCsFiles(rootPath)).Returns(new List<string> { "test.cs" });
        mockRepository.Setup(r => r.ReadAllLines("test.cs")).Returns(new string[] { "class TestClass { }" });

        // Act
        var results = useCase.Execute(regexp, rootPath, maxResults, contextLines);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    /// <summary>
    /// Verifies that the Execute method handles file reading exceptions gracefully.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_HandlesFileReadingExceptions()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = "TestClass";
        var maxResults = 1000;
        var contextLines = 10;
        
        // Setup mocks
        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockRepository.Setup(r => r.GetCsFiles(rootPath)).Returns(new List<string> { "test.cs" });
        mockRepository.Setup(r => r.ReadAllLines("test.cs")).Throws(new IOException("File access error"));

        // Act
        var results = useCase.Execute(regexp, rootPath, maxResults, contextLines);

        // Assert
        Assert.NotNull(results);
        // Should not crash and return empty results or minimal results
        // The method should handle exceptions gracefully and return empty list or a limit result
        // With file reading exceptions, we might get a limit result indicating no matches
        Assert.True(results.Count <= 1);
        // If we get a result, it should be a limit result or total matches result
        if (results.Count > 0)
        {
            Assert.Contains("\nTotal matches for", results[0].CodeLine);
        }
    }

    /// <summary>
    /// Verifies that the Execute method handles null regex pattern correctly.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_HandlesNullRegexPattern()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = (string)null;
        var maxResults = 1000;
        var contextLines = 10;
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => useCase.Execute(regexp, rootPath, maxResults, contextLines));
    }

    /// <summary>
    /// Verifies that the Execute method handles negative max results correctly.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_HandlesNegativeMaxResults()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = "TestClass";
        var maxResults = -1;
        var contextLines = 10;
        
        // Setup mocks
        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockRepository.Setup(r => r.GetCsFiles(rootPath)).Returns(new List<string> { "test.cs" });
        mockRepository.Setup(r => r.ReadAllLines("test.cs")).Returns(new string[] { "class TestClass { }" });

        // Act
        var results = useCase.Execute(regexp, rootPath, maxResults, contextLines);

        // Assert
        Assert.NotNull(results);
        // With negative maxResults, we expect a result indicating the limit was reached
        Assert.Single(results);
        Assert.Contains("\nTotal matches for", results[0].CodeLine);
    }

    /// <summary>
    /// Verifies that the Execute method handles zero max results correctly.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_HandlesZeroMaxResults()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = "TestClass";
        var maxResults = 0;
        var contextLines = 10;
        
        // Setup mocks
        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockRepository.Setup(r => r.GetCsFiles(rootPath)).Returns(new List<string> { "test.cs" });
        mockRepository.Setup(r => r.ReadAllLines("test.cs")).Returns(new string[] { "class TestClass { }" });

        // Act
        var results = useCase.Execute(regexp, rootPath, maxResults, contextLines);

        // Assert
        Assert.NotNull(results);
        // With zero maxResults, we expect a result indicating the limit was reached
        Assert.Single(results);
        Assert.Contains("\nTotal matches for", results[0].CodeLine);
    }

    /// <summary>
    /// Verifies that the Execute method handles context lines correctly.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_HandlesContextLines()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = "TestClass";
        var maxResults = 1000;
        var contextLines = 2;
        
        // Setup mocks
        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockRepository.Setup(r => r.GetCsFiles(rootPath)).Returns(new List<string> { "test.cs" });
        mockRepository.Setup(r => r.ReadAllLines("test.cs")).Returns(new string[]
        {
            "namespace TestNamespace",
            "{",
            "    public class TestClass",
            "    {",
            "        public void Foo() { }",
            "    }",
            "}"
        });

        // Act
        var results = useCase.Execute(regexp, rootPath, maxResults, contextLines);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        // Should include context lines around matches
        Assert.Contains(results, r => r.ContextBefore.Count > 0 || r.ContextAfter.Count > 0);
    }

    /// <summary>
    /// Verifies that the Execute method respects the maximum results limit.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_RespectsMaxResultsLimit()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = "TestClass";
        var maxResults = 1;
        var contextLines = 10;
        
        // Setup mocks
        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockRepository.Setup(r => r.GetCsFiles(rootPath)).Returns(new List<string> { "test1.cs", "test2.cs" });
        mockRepository.Setup(r => r.ReadAllLines("test1.cs")).Returns(new string[] { "class TestClass { }" });
        mockRepository.Setup(r => r.ReadAllLines("test2.cs")).Returns(new string[] { "class TestClass { }" });

        // Act
        var results = useCase.Execute(regexp, rootPath, maxResults, contextLines);

        // Assert
        Assert.NotNull(results);
        // Should respect the limit and not exceed maxResults
        // The implementation adds a total result line at the end, so we expect at most maxResults + 1
        Assert.True(results.Count <= maxResults + 1);
    }

    /// <summary>
    /// Verifies that the Execute method handles empty regex patterns correctly.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_HandlesEmptyRegexPattern()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = "";
        var maxResults = 1000;
        var contextLines = 10;
        
        // Setup mocks
        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockRepository.Setup(r => r.GetCsFiles(rootPath)).Returns(new List<string> { "test.cs" });
        mockRepository.Setup(r => r.ReadAllLines("test.cs")).Returns(new string[] { "class TestClass { }" });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => useCase.Execute(regexp, rootPath, maxResults, contextLines));
    }

    /// <summary>
    /// Verifies that the Execute method handles very large max results correctly.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_HandlesLargeMaxResults()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = "TestClass";
        var maxResults = int.MaxValue;
        var contextLines = 10;
        
        // Setup mocks
        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockRepository.Setup(r => r.GetCsFiles(rootPath)).Returns(new List<string> { "test.cs" });
        mockRepository.Setup(r => r.ReadAllLines("test.cs")).Returns(new string[] { "class TestClass { }" });

        // Act
        var results = useCase.Execute(regexp, rootPath, maxResults, contextLines);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    /// <summary>
    /// Verifies that the Execute method handles edge case with exact match limit.
    /// </summary>
    [Fact]
    public void SearchBannerlordCodeUseCase_Execute_HandlesExactMatchLimit()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        
        var rootPath = "valid_path";
        var regexp = "TestClass";
        var maxResults = 2;
        var contextLines = 10;
        
        // Setup mocks
        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockRepository.Setup(r => r.GetCsFiles(rootPath)).Returns(new List<string> { "test1.cs", "test2.cs", "test3.cs" });
        mockRepository.Setup(r => r.ReadAllLines("test1.cs")).Returns(new string[] { "class TestClass { }" });
        mockRepository.Setup(r => r.ReadAllLines("test2.cs")).Returns(new string[] { "class TestClass { }" });
        mockRepository.Setup(r => r.ReadAllLines("test3.cs")).Returns(new string[] { "class TestClass { }" });

        // Act
        var results = useCase.Execute(regexp, rootPath, maxResults, contextLines);

        // Assert
        Assert.NotNull(results);
        // Should respect the limit and not exceed maxResults
        Assert.True(results.Count <= maxResults + 1); // +1 for total result line
    }
}