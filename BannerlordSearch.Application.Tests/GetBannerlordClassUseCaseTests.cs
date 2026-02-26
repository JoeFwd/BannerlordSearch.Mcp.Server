using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using Moq;
using Xunit;

namespace BannerlordSearch.Application.Tests;

/// <summary>
/// Tests for the <see cref="GetBannerlordClassUseCase"/> class.
/// </summary>
public class GetBannerlordClassUseCaseTests
{
    /// <summary>
    /// Verifies that the <see cref="GetBannerlordClassUseCase"/> can be instantiated successfully.
    /// </summary>
    [Fact]
    public void GetBannerlordClassUseCase_CanBeInstantiated()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();

        // Act
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);

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
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetBannerlordClassUseCase(null, mockSourceProvider.Object, mockFileSystem.Object));
    }

    /// <summary>
    /// Verifies that the constructor properly validates its parameters and throws
    /// <see cref="ArgumentNullException"/> when the source provider is null.
    /// </summary>
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenSourceProviderIsNull()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetBannerlordClassUseCase(mockRepository.Object, null, mockFileSystem.Object));
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
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, null));
    }

    /// <summary>
    /// Verifies that the Execute method returns an error when the class name is null.
    /// </summary>
    [Fact]
    public void Execute_ReturnsError_WhenClassNameIsNull()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);

        // Act
        var result = useCase.Execute(null);

        // Assert
        Assert.StartsWith("[Error]", result);
    }

    /// <summary>
    /// Verifies that the Execute method returns an error when the class name is empty.
    /// </summary>
    [Fact]
    public void Execute_ReturnsError_WhenClassNameIsEmpty()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);

        // Act
        var result = useCase.Execute(string.Empty);

        // Assert
        Assert.StartsWith("[Error]", result);
    }

    /// <summary>
    /// Verifies that the Execute method returns an error when the class is not found in the file system.
    /// </summary>
    [Fact]
    public void Execute_ReturnsError_WhenClassNotFound()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var className = "NonExistentClass";

        // Setup mocks
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("valid_path");
        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);
        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);
        mockRepository.Setup(r => r.GetCsFiles("valid_path")).Returns(new List<string>());

        // Act
        var result = useCase.Execute(className);

        // Assert
        Assert.StartsWith("[Error]", result);
    }

   /// <summary>
   /// Verifies that the Execute method returns the class source when the class is found in the file system.
   /// </summary>
   [Fact]
   public void Execute_ReturnsClassSource_WhenClassFoundInFileSystem()
   {
       // Arrange
       var mockRepository = new Mock<ISymbolSearchRepository>();
       var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
       var mockFileSystem = new Mock<IFileSystem>();
       var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
       var className = "TestNamespace.TestClass";

       // Setup mocks
       mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("valid_path");
       var expectedPath = Path.Combine("valid_path", "TestNamespace", "TestClass.cs");
       mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
       mockFileSystem.Setup(fs => fs.FileExists(expectedPath)).Returns(true);
       mockFileSystem.Setup(fs => fs.ReadAllLines(expectedPath)).Returns(new[] { "namespace TestNamespace", "{", "    class TestClass { }", "}" });

       // Act
       var result = useCase.Execute(className);

       // Assert
       // The implementation should return the class content, not an error
       // If it returns an error, that's a bug in the implementation
       Assert.Contains("class TestClass", result);
       Assert.DoesNotContain("[Error]", result);
   }

    /// <summary>
    /// Verifies that the Execute method handles invalid line ranges correctly.
    /// </summary>
    [Fact]
    public void Execute_ReturnsError_WhenInvalidLineRangeProvided()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var className = "TestNamespace.TestClass";

        // Setup mocks
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("valid_path");
        mockFileSystem.Setup(fs => fs.DirectoryExists("valid_path/TestNamespace")).Returns(true);
        mockFileSystem.Setup(fs => fs.FileExists("valid_path/TestNamespace/TestClass.cs")).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllLines("valid_path/TestNamespace/TestClass.cs")).Returns(new[] { "namespace TestNamespace", "{", "    class TestClass { }", "}" });

        // Act
        var result = useCase.Execute(className, startLine: 5, endLine: 3);

        // Assert
        Assert.StartsWith("[Error]", result);
    }

    /// <summary>
    /// Verifies that the Execute method returns a specific line range when valid parameters are provided.
    /// </summary>
    [Fact]
    public void Execute_ReturnsSpecificLineRange_WhenValidLineRangeProvided()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var className = "TestNamespace.TestClass";

        // Setup mocks
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("valid_path");
        var expectedPath = Path.Combine("valid_path", "TestNamespace", "TestClass.cs");
        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.FileExists(expectedPath)).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllLines(expectedPath)).Returns(new[]
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
        var result = useCase.Execute(className, startLine: 3, endLine: 5);

        // Assert
        Assert.DoesNotContain("[Error]", result);
        Assert.Contains("public class TestClass", result);
        Assert.Contains("public void Foo", result);
        Assert.DoesNotContain("namespace TestNamespace", result);
        Assert.DoesNotContain("{", result);
        Assert.DoesNotContain("}", result);
    }

    /// <summary>
    /// Verifies that the Execute method handles edge case with single line range.
    /// </summary>
    [Fact]
    public void Execute_ReturnsSingleLine_WhenSingleLineRangeProvided()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var className = "TestNamespace.TestClass";

        // Setup mocks
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("valid_path");
        var expectedPath = Path.Combine("valid_path", "TestNamespace", "TestClass.cs");
        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.FileExists(expectedPath)).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllLines(expectedPath)).Returns(new[]
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
        var result = useCase.Execute(className, startLine: 3, endLine: 3);

        // Assert
        Assert.DoesNotContain("[Error]", result);
        Assert.Contains("public class TestClass", result);
        Assert.DoesNotContain("namespace TestNamespace", result);
        Assert.DoesNotContain("{", result);
        Assert.DoesNotContain("}", result);
    }

    /// <summary>
    /// Verifies that the Execute method handles line range beyond file size.
    /// </summary>
    [Fact]
    public void Execute_ReturnsError_WhenLineRangeExceedsFileSize()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var className = "TestNamespace.TestClass";

        // Setup mocks
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("valid_path");
        mockFileSystem.Setup(fs => fs.DirectoryExists("valid_path/TestNamespace")).Returns(true);
        mockFileSystem.Setup(fs => fs.FileExists("valid_path/TestNamespace/TestClass.cs")).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllLines("valid_path/TestNamespace/TestClass.cs")).Returns(new[]
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
        var result = useCase.Execute(className, startLine: 1, endLine: 10);

        // Assert
        Assert.StartsWith("[Error]", result);
    }

    /// <summary>
    /// Verifies that the Execute method handles edge case with large line range.
    /// </summary>
    [Fact]
    public void Execute_ReturnsWholeFile_WhenLargeLineRangeProvided()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var className = "TestNamespace.TestClass";

        // Setup mocks
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("valid_path");
        mockFileSystem.Setup(fs => fs.DirectoryExists("valid_path/TestNamespace")).Returns(true);
        mockFileSystem.Setup(fs => fs.FileExists("valid_path/TestNamespace/TestClass.cs")).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllLines("valid_path/TestNamespace/TestClass.cs")).Returns(new[]
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
        var result = useCase.Execute(className, startLine: 1, endLine: int.MaxValue);

        // Assert
        Assert.StartsWith("[Error]", result);
    }

    /// <summary>
    /// Verifies that the Execute method handles class name with no namespace.
    /// </summary>
    [Fact]
    public void Execute_ReturnsClassSource_WhenClassHasNoNamespace()
    {
        // Arrange
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var className = "TestClass";

        // Setup mocks
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("valid_path");
        var expectedPath = Path.Combine("valid_path", "TestClass.cs");
        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.FileExists(expectedPath)).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllLines(expectedPath)).Returns(new[] { "class TestClass { }" });

        // Act
        var result = useCase.Execute(className);

        // Assert
        Assert.Contains("class TestClass", result);
        Assert.False(result.StartsWith("[Error]"));
    }
}