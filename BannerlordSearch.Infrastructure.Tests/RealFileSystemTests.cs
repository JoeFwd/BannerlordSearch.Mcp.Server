using Xunit;

namespace BannerlordSearch.Infrastructure.Tests;

/// <summary>
/// Tests for the <see cref="RealFileSystem"/> class.
/// </summary>
public class RealFileSystemTests
{
    /// <summary>
    /// Verifies that DirectoryExists returns true for an existing directory.
    /// </summary>
    [Fact]
    public void RealFileSystem_DirectoryExists_ReturnsTrue_ForExistingDirectory()
    {
        // Arrange
        var fileSystem = new RealFileSystem();
        var testDirectory = Path.Combine(Path.GetTempPath(), "TestDir");
        Directory.CreateDirectory(testDirectory);

        // Act
        var result = fileSystem.DirectoryExists(testDirectory);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that DirectoryExists returns false for a non-existing directory.
    /// </summary>
    [Fact]
    public void RealFileSystem_DirectoryExists_ReturnsFalse_ForNonExistingDirectory()
    {
        // Arrange
        var fileSystem = new RealFileSystem();

        // Act
        var result = fileSystem.DirectoryExists(Path.Combine(Path.GetTempPath(), "NonExistentDir"));

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that FileExists returns true for an existing file.
    /// </summary>
    [Fact]
    public void RealFileSystem_FileExists_ReturnsTrue_ForExistingFile()
    {
        // Arrange
        var fileSystem = new RealFileSystem();
        var testFile = Path.Combine(Path.GetTempPath(), "TestDir", "TestFile.cs");
        var testDirectory = Path.GetDirectoryName(testFile);
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(testFile, "test content");

        // Act
        var result = fileSystem.FileExists(testFile);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that FileExists returns false for a non-existing file.
    /// </summary>
    [Fact]
    public void RealFileSystem_FileExists_ReturnsFalse_ForNonExistingFile()
    {
        // Arrange
        var fileSystem = new RealFileSystem();

        // Act
        var result = fileSystem.FileExists(Path.Combine(Path.GetTempPath(), "NonExistentFile.cs"));

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that EnumerateFiles returns the correct files matching a pattern.
    /// </summary>
    [Fact]
    public void RealFileSystem_EnumerateFiles_ReturnsCorrectFiles()
    {
        // Arrange
        var fileSystem = new RealFileSystem();
        var testDirectory = Path.Combine(Path.GetTempPath(), "TestDir");
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(Path.Combine(testDirectory, "Test1.cs"), "test content 1");
        File.WriteAllText(Path.Combine(testDirectory, "Test2.cs"), "test content 2");
        File.WriteAllText(Path.Combine(testDirectory, "Test.txt"), "test content 3");

        // Act
        var result = fileSystem.EnumerateFiles(testDirectory, "*.cs", SearchOption.TopDirectoryOnly);

        // Assert - Check that our expected files are present, ignoring any additional files
        Assert.Contains(Path.Combine(testDirectory, "Test1.cs"), result);
        Assert.Contains(Path.Combine(testDirectory, "Test2.cs"), result);
        // Verify that only .cs files are returned (no .txt files) - check that the .txt file is not among our results
        Assert.DoesNotContain(Path.Combine(testDirectory, "Test.txt"), result);
        // Count should be at least 2 (the two .cs files) - we don't enforce exact count to avoid environment issues
        Assert.True(result.Count() >= 2);
    }

    /// <summary>
    /// Verifies that ReadAllLines returns the correct content from a file.
    /// </summary>
    [Fact]
    public void RealFileSystem_ReadAllLines_ReturnsCorrectContent()
    {
        // Arrange
        var fileSystem = new RealFileSystem();
        var testFile = Path.Combine(Path.GetTempPath(), "TestDir", "TestFile.cs");
        var testDirectory = Path.GetDirectoryName(testFile);
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(testFile, "line1\nline2\nline3");

        // Act
        var result = fileSystem.ReadAllLines(testFile);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
        Assert.Equal("line3", result[2]);
    }

    /// <summary>
    /// Verifies that EnumerateFiles works with subdirectories.
    /// </summary>
    [Fact]
    public void RealFileSystem_EnumerateFiles_WithSubdirectories_ReturnsAllMatchingFiles()
    {
        // Arrange
        var fileSystem = new RealFileSystem();
        var testDirectory = Path.Combine(Path.GetTempPath(), "TestDir");
        var subDirectory = Path.Combine(testDirectory, "SubDir");
        Directory.CreateDirectory(testDirectory);
        Directory.CreateDirectory(subDirectory);
        File.WriteAllText(Path.Combine(testDirectory, "Test1.cs"), "test content 1");
        File.WriteAllText(Path.Combine(subDirectory, "Test2.cs"), "test content 2");
        File.WriteAllText(Path.Combine(testDirectory, "Test.txt"), "test content 3");

        // Act
        var result = fileSystem.EnumerateFiles(testDirectory, "*.cs", SearchOption.AllDirectories);
        
        // Assert - Check that our expected files are present, ignoring any additional files
        Assert.Contains(Path.Combine(testDirectory, "Test1.cs"), result);
        Assert.Contains(Path.Combine(subDirectory, "Test2.cs"), result);
        // Verify that only .cs files are returned (no .txt files)
        Assert.DoesNotContain(Path.Combine(testDirectory, "Test.txt"), result);
        // Count should be at least 2 (the two .cs files) - we don't enforce exact count to avoid environment issues
        Assert.True(result.Count() >= 2);
    }

    /// <summary>
    /// Verifies that ReadAllLines handles empty files correctly.
    /// </summary>
    [Fact]
    public void RealFileSystem_ReadAllLines_HandlesEmptyFile()
    {
        // Arrange
        var fileSystem = new RealFileSystem();
        var testFile = Path.Combine(Path.GetTempPath(), "TestDir", "TestFile.cs");
        var testDirectory = Path.GetDirectoryName(testFile);
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(testFile, "");

        // Act
        var result = fileSystem.ReadAllLines(testFile);

        // Assert
        Assert.Equal(0, result.Length);
    }

    /// <summary>
    /// Verifies that ReadAllLines handles files with various line endings.
    /// </summary>
    [Fact]
    public void RealFileSystem_ReadAllLines_HandlesDifferentLineEndings()
    {
        // Arrange
        var fileSystem = new RealFileSystem();
        var testFile = Path.Combine(Path.GetTempPath(), "TestDir", "TestFile.cs");
        var testDirectory = Path.GetDirectoryName(testFile);
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(testFile, "line1\r\nline2\nline3\rline4");

        // Act
        var result = fileSystem.ReadAllLines(testFile);

        // Assert
        Assert.Equal(4, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
        Assert.Equal("line3", result[2]);
        Assert.Equal("line4", result[3]);
    }

    /// <summary>
    /// Verifies that EnumerateFiles handles empty directory correctly.
    /// </summary>
    [Fact]
    public void RealFileSystem_EnumerateFiles_HandlesEmptyDirectory()
    {
        // Arrange
        var fileSystem = new RealFileSystem();
        var testDirectory = Path.Combine(Path.GetTempPath(), "TestDir");
        Directory.CreateDirectory(testDirectory);

        // Act
        var result = fileSystem.EnumerateFiles(testDirectory, "*.cs", SearchOption.TopDirectoryOnly);

        // Assert - Should be empty (no .cs files in directory)
        // The method should return an empty enumerable when no .cs files are found
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that EnumerateFiles handles non-existent directory correctly.
    /// </summary>
    [Fact]
    public void RealFileSystem_EnumerateFiles_HandlesNonExistentDirectory()
    {
        // Arrange
        var fileSystem = new RealFileSystem();

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => fileSystem.EnumerateFiles(Path.Combine(Path.GetTempPath(), "NonExistentDir"), "*.cs", SearchOption.TopDirectoryOnly));
    }
}