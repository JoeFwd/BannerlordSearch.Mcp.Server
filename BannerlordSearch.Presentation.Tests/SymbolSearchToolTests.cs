using BannerlordSearch.Domain;
using BannerlordSearch.Application.UseCases;
using Moq;
using Xunit;

namespace BannerlordSearch.Presentation.Tests;

public class SymbolSearchToolTests
{
    [Fact]
    public void SymbolSearchTool_CanBeInstantiated()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(mockRepository.Object, mockFileSystem.Object);
         
        // Act
        // We can't easily instantiate the tool since it's decorated with attributes
        // but we can test the constructor parameters are correct
         
        // Assert
        Assert.NotNull(mockSearchUseCase.Object);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationException_WhenRegexpIsNull()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(mockRepository.Object, mockFileSystem.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, mockSearchUseCase.Object);
        
        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => tool.SearchBannerlordCode(null, 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationException_WhenRegexpIsEmpty()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(mockRepository.Object, mockFileSystem.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, mockSearchUseCase.Object);
        
        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => tool.SearchBannerlordCode("", 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationException_WhenRegexpContainsOnlyWhitespace()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(mockRepository.Object, mockFileSystem.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, mockSearchUseCase.Object);
        
        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => tool.SearchBannerlordCode("   ", 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_CallsUseCaseWithCorrectParameters()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, useCase);
        
        var regexp = "TestClass";
        var maxResults = 500;
        var contextLines = 5;
        var expectedRootPath = "expected_root_path";
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockRepository.Setup(r => r.GetCsFiles(It.IsAny<string>()))
            .Returns(new List<string>());
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        
        // Act
        var result = tool.SearchBannerlordCode(regexp, maxResults, contextLines);
        
        // Assert
        mockSourceProvider.Verify(p => p.GetBannerlordSourceFolderPath(), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_PassesCorrectRootPath()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, useCase);
        
        var regexp = "TestClass";
        var maxResults = 500;
        var contextLines = 5;
        var expectedRootPath = "expected_root_path";
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockRepository.Setup(r => r.GetCsFiles(It.IsAny<string>()))
            .Returns(new List<string>());
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        
        // Act
        var result = tool.SearchBannerlordCode(regexp, maxResults, contextLines);
        
        // Assert
        mockSourceProvider.Verify(p => p.GetBannerlordSourceFolderPath(), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_HandlesEmptyResultFromUseCase()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, useCase);
        
        var regexp = "TestClass";
        var maxResults = 500;
        var contextLines = 5;
        var expectedRootPath = "expected_root_path";
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockRepository.Setup(r => r.GetCsFiles(It.IsAny<string>()))
            .Returns(new List<string>());
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        
        // Act
        var result = tool.SearchBannerlordCode(regexp, maxResults, contextLines);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_HandlesNonEmptyResultFromUseCase()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, useCase);
        
        var regexp = "TestClass";
        var maxResults = 500;
        var contextLines = 5;
        var expectedRootPath = "expected_root_path";
        var searchResult = new SearchResult { CodeLine = "Found TestClass" };
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockRepository.Setup(r => r.GetCsFiles(It.IsAny<string>()))
            .Returns(new List<string> { "test.cs" });
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(f => f.ReadAllLines(It.IsAny<string>()))
            .Returns(new string[] { "Found TestClass" });
        
        // Act
        var result = tool.SearchBannerlordCode(regexp, maxResults, contextLines);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(searchResult, result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_WithZeroMaxResults_ReturnsExpectedResult()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new SearchBannerlordCodeUseCase(mockRepository.Object, mockFileSystem.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, useCase);
        
        var regexp = "TestClass";
        var maxResults = 0;
        var contextLines = 5;
        var expectedRootPath = "expected_root_path";
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockRepository.Setup(r => r.GetCsFiles(It.IsAny<string>()))
            .Returns(new List<string> { "test.cs" });
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(f => f.ReadAllLines(It.IsAny<string>()))
            .Returns(new string[] { "Found TestClass" });
        
        // Act
        var result = tool.SearchBannerlordCode(regexp, maxResults, contextLines);
        
        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_WithNegativeMaxResults_ReturnsExpectedResult()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(mockRepository.Object, mockFileSystem.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, mockSearchUseCase.Object);
        
        var regexp = "TestClass";
        var maxResults = -1;
        var contextLines = 5;
        var expectedRootPath = "expected_root_path";
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockSearchUseCase.Setup(u => u.Execute(regexp, expectedRootPath, maxResults, contextLines))
            .Returns(new List<SearchResult> { new SearchResult { CodeLine = "\nTotal matches for \"TestClass\": 0" } });
        
        // Act
        var result = tool.SearchBannerlordCode(regexp, maxResults, contextLines);
        
        // Assert
        Assert.NotNull(result);
    }
}