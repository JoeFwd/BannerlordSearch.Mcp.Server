using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using Moq;
using Xunit;

namespace BannerlordSearch.Presentation.Tests;

public class GetBannerlordClassToolTests
{
    private static IndexedFile MakeFile(string filePath, params string[] lines) =>
        new() { FilePath = filePath, Lines = lines };

    [Fact]
    public void GetBannerlordClassTool_CanBeInstantiated()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockClassUseCase = new Mock<GetBannerlordClassUseCase>(
            Mock.Of<ICodeIndex>(), mockSourceProvider.Object);

        Assert.NotNull(mockClassUseCase.Object);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ThrowsValidationException_WhenClassNameIsNull()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, mockSourceProvider.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);

        var ex = Assert.Throws<ValidationException>(() => tool.GetBannerlordClassDefinition(null!));
        Assert.Equal("className must be provided", ex.Message);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ThrowsValidationException_WhenClassNameIsEmpty()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, mockSourceProvider.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);

        var ex = Assert.Throws<ValidationException>(() => tool.GetBannerlordClassDefinition(""));
        Assert.Equal("className must be provided", ex.Message);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ThrowsValidationException_WhenClassNameIsWhitespace()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, mockSourceProvider.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);

        var ex = Assert.Throws<ValidationException>(() => tool.GetBannerlordClassDefinition("   "));
        Assert.Equal("className must be provided", ex.Message);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ReturnsClassSource_WhenClassFound()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        var className = "TestNamespace.TestClass";

        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("root");
        mockCodeIndex.Setup(ci => ci.FindClass(className))
            .Returns(MakeFile("TestClass.cs", "class TestClass { }"));

        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, mockSourceProvider.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);

        var result = tool.GetBannerlordClassDefinition(className);

        Assert.NotNull(result);
        Assert.Contains("class TestClass", result);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ReturnsEmpty_WhenFileHasOnlyEmptyLines()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        var className = "TestNamespace.TestClass";

        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("root");
        mockCodeIndex.Setup(ci => ci.FindClass(className))
            .Returns(MakeFile("TestClass.cs", ""));

        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, mockSourceProvider.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);

        var result = tool.GetBannerlordClassDefinition(className);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ReturnsError_WhenClassNotFound()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        var className = "TestNamespace.NonExistentClass";

        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("root");
        mockCodeIndex.Setup(ci => ci.FindClass(className)).Returns((IndexedFile?)null);

        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, mockSourceProvider.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);

        var result = tool.GetBannerlordClassDefinition(className);

        Assert.NotNull(result);
        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void GetBannerlordClassDefinition_CallsSourceProvider_ForRootPath()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        var className = "TestNamespace.TestClass";

        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("root");
        mockCodeIndex.Setup(ci => ci.FindClass(className))
            .Returns(MakeFile("TestClass.cs", "class TestClass { }"));

        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, mockSourceProvider.Object);
        var tool = new GetBannerlordClassTool(mockSourceProvider.Object, useCase);

        tool.GetBannerlordClassDefinition(className);

        // The tool itself calls GetBannerlordSourceFolderPath, and the use case also calls it
        mockSourceProvider.Verify(p => p.GetBannerlordSourceFolderPath(), Times.AtLeastOnce);
    }
}
