using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.IO;
using Moq;
using Xunit;

namespace BannerlordSearch.Infrastructure.Tests;

/// <summary>
/// Tests for the <see cref="SymbolFileRepository"/> class.
/// </summary>
public class SymbolFileRepositoryTests
{
    /// <summary>
    /// Verifies that the <see cref="SymbolFileRepository"/> can be instantiated successfully.
    /// </summary>
    [Fact]
    public void SymbolFileRepository_CanBeInstantiated()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);

        // Act
        var repository = new SymbolFileRepository(mockFileSystem.Object);

        // Assert
        Assert.NotNull(repository);
    }

    /// <summary>
    /// Verifies that GetCsFiles returns an empty list when the directory does not exist.
    /// </summary>
    [Fact]
    public void SymbolFileRepository_GetCsFiles_ReturnsEmptyList_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
        var repository = new SymbolFileRepository(mockFileSystem.Object);

        var rootPath = "nonexistent_path";

        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(false);

        // Act
        var result = repository.GetCsFiles(rootPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Count);
    }

    /// <summary>
    /// Verifies that GetCsFiles returns files when the directory exists.
    /// </summary>
    [Fact]
    public void SymbolFileRepository_GetCsFiles_ReturnsFiles_WhenDirectoryExists()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
        var repository = new SymbolFileRepository(mockFileSystem.Object);

        var rootPath = "valid_path";
        var expectedFiles = new List<string> { "file1.cs", "file2.cs" };

        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockFileSystem.Setup(fs => fs.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories))
            .Returns(expectedFiles);

        // Act
        var result = repository.GetCsFiles(rootPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("file1.cs", result[0]);
        Assert.Equal("file2.cs", result[1]);
    }

    /// <summary>
    /// Verifies that GetCsFiles caches results for the lifetime of the repository instance.
    /// </summary>
    [Fact]
    public void SymbolFileRepository_GetCsFiles_CachesResults()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
        var repository = new SymbolFileRepository(mockFileSystem.Object);

        var rootPath = "valid_path";
        var expectedFiles = new List<string> { "file1.cs", "file2.cs" };

        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockFileSystem.Setup(fs => fs.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories))
            .Returns(expectedFiles);

        // Act
        var result1 = repository.GetCsFiles(rootPath);
        var result2 = repository.GetCsFiles(rootPath);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(2, result1.Count);
        Assert.Equal(2, result2.Count);
        // Verify that the method was called only once due to caching
        mockFileSystem.Verify(fs => fs.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories), Times.Once);
    }

    /// <summary>
    /// Verifies that ReadAllLines calls the underlying file system.
    /// </summary>
    [Fact]
    public void SymbolFileRepository_ReadAllLines_CallsFileSystem()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
        var repository = new SymbolFileRepository(mockFileSystem.Object);

        var filePath = "test.cs";
        var expectedLines = new string[] { "line1", "line2" };

        mockFileSystem.Setup(fs => fs.ReadAllLines(filePath)).Returns(expectedLines);

        // Act
        var result = repository.ReadAllLines(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
        mockFileSystem.Verify(fs => fs.ReadAllLines(filePath), Times.Once);
    }

    /// <summary>
    /// Verifies that GetCsFiles handles directory enumeration exceptions gracefully.
    /// </summary>
    [Fact]
    public void SymbolFileRepository_GetCsFiles_HandlesDirectoryEnumerationExceptions()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
        var repository = new SymbolFileRepository(mockFileSystem.Object);

        var rootPath = "valid_path";

        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockFileSystem.Setup(fs => fs.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories))
            .Throws(new IOException("Directory access error"));

        // Act & Assert
        // Should not throw an exception, but return empty list
        var result = repository.GetCsFiles(rootPath);
        Assert.NotNull(result);
        Assert.Equal(0, result.Count);
    }

    /// <summary>
    /// Verifies that GetCsFiles handles empty directory correctly.
    /// </summary>
    [Fact]
    public void SymbolFileRepository_GetCsFiles_HandlesEmptyDirectory()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
        var repository = new SymbolFileRepository(mockFileSystem.Object);

        var rootPath = "empty_path";

        mockFileSystem.Setup(fs => fs.DirectoryExists(rootPath)).Returns(true);
        mockFileSystem.Setup(fs => fs.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories))
            .Returns(new List<string>());

        // Act
        var result = repository.GetCsFiles(rootPath);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetCsFiles handles null directory path correctly.
    /// </summary>
    [Fact]
    public void SymbolFileRepository_GetCsFiles_HandlesNullDirectoryPath()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
        var repository = new SymbolFileRepository(mockFileSystem.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => repository.GetCsFiles(null));
    }
}
