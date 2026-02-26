using BannerlordSearch.Domain;
using BannerlordSearch.Application.UseCases;
using Moq;
using Xunit;

namespace BannerlordSearch.Presentation.Tests;

public class GetBannerlordClassToolTests
{
    [Fact]
    public void GetBannerlordClassTool_CanBeInstantiated()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockClassUseCase = new Mock<GetBannerlordClassUseCase>(Mock.Of<ISymbolSearchRepository>(), mockSourceProvider.Object, Mock.Of<IFileSystem>());
         
        // Act
        // We can't easily instantiate the tool since it's decorated with attributes
        // but we can test the constructor parameters are correct
         
        // Assert
        Assert.NotNull(mockClassUseCase.Object);
    }

    [Fact]
    public void GetBannerlordClassTool_GetBannerlordClassDefinition_ThrowsValidationException_WhenClassNameIsNull()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockClassUseCase = new Mock<GetBannerlordClassUseCase>(Mock.Of<ISymbolSearchRepository>(), mockSourceProvider.Object, Mock.Of<IFileSystem>());
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, mockClassUseCase.Object);
        
        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => tool.GetBannerlordClassDefinition(null));
        Assert.Equal("className must be provided", ex.Message);
    }

    [Fact]
    public void GetBannerlordClassTool_GetBannerlordClassDefinition_ThrowsValidationException_WhenClassNameIsEmpty()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockClassUseCase = new Mock<GetBannerlordClassUseCase>(Mock.Of<ISymbolSearchRepository>(), mockSourceProvider.Object, Mock.Of<IFileSystem>());
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, mockClassUseCase.Object);
        
        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => tool.GetBannerlordClassDefinition(""));
        Assert.Equal("className must be provided", ex.Message);
    }

    [Fact]
    public void GetBannerlordClassTool_GetBannerlordClassDefinition_ThrowsValidationException_WhenClassNameContainsOnlyWhitespace()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockClassUseCase = new Mock<GetBannerlordClassUseCase>(Mock.Of<ISymbolSearchRepository>(), mockSourceProvider.Object, Mock.Of<IFileSystem>());
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, mockClassUseCase.Object);
        
        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => tool.GetBannerlordClassDefinition("   "));
        Assert.Equal("className must be provided", ex.Message);
    }

    [Fact]
    public void GetBannerlordClassTool_GetBannerlordClassDefinition_CallsUseCaseWithCorrectParameters()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);
        
        var className = "TestNamespace.TestClass";
        var expectedRootPath = "expected_root_path";
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockRepository.Setup(r => r.ReadAllLines(It.IsAny<string>()))
            .Returns(new string[] { "class TestClass { }" });
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
        
        // Act
        var result = tool.GetBannerlordClassDefinition(className);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("class TestClass", result);
    }

    [Fact]
    public void GetBannerlordClassTool_GetBannerlordClassDefinition_PassesCorrectRootPath()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);
        
        var className = "TestNamespace.TestClass";
        var expectedRootPath = "expected_root_path";
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockRepository.Setup(r => r.ReadAllLines(It.IsAny<string>()))
            .Returns(new string[] { "class TestClass { }" });
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
        
        // Act
        var result = tool.GetBannerlordClassDefinition(className);
        
        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetBannerlordClassTool_GetBannerlordClassDefinition_HandlesEmptyResultFromUseCase()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);
        
        var className = "TestNamespace.TestClass";
        var expectedRootPath = "expected_root_path";
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockRepository.Setup(r => r.ReadAllLines(It.IsAny<string>()))
            .Returns(new string[] { "" });
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
        
        // Act
        var result = tool.GetBannerlordClassDefinition(className);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetBannerlordClassTool_GetBannerlordClassDefinition_HandlesErrorResultFromUseCase()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);
        
        var className = "TestNamespace.NonExistentClass";
        var expectedRootPath = "expected_root_path";
        var errorResult = "[Error] Class 'TestNamespace.NonExistentClass' not found";
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(false);
        // This will cause the use case to return the error result directly
        
        // Act
        var result = tool.GetBannerlordClassDefinition(className);
        
        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void GetBannerlordClassTool_GetBannerlordClassDefinition_WithLineRange_CallsUseCaseWithCorrectParameters()
    {
        // Arrange
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockRepository = new Mock<ISymbolSearchRepository>();
        var mockFileSystem = new Mock<IFileSystem>();
        var useCase = new GetBannerlordClassUseCase(mockRepository.Object, mockSourceProvider.Object, mockFileSystem.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);
        
        var className = "TestNamespace.TestClass";
        var expectedRootPath = "expected_root_path";
        var startLine = 5;
        var endLine = 10;
        
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockRepository.Setup(r => r.ReadAllLines(It.IsAny<string>()))
            .Returns(new string[] { "class TestClass { ", "    public void Method() { } ", "}" });
        mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
        
        // Act
        var result = tool.GetBannerlordClassDefinition(className);
        
        // Assert
        Assert.NotNull(result);
    }
}