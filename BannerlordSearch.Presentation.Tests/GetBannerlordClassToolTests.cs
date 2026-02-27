using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.Repositories;
using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using BannerlordSearch.Domain.Errors;
using BannerlordSearch.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BannerlordSearch.Presentation.Tests;

public class GetBannerlordClassToolTests
{
    private readonly GetBannerlordClassTool _tool;

    public GetBannerlordClassToolTests()
    {
        var useCase = new GetBannerlordClassUseCase(
            new Mock<ICodeIndex>(MockBehavior.Strict).Object,
            NullLogger<GetBannerlordClassUseCase>.Instance);
        _tool = new GetBannerlordClassTool(useCase);
    }

    [Fact]
    public void GetBannerlordClassTool_CanBeInstantiated()
    {
        var mockClassUseCase = new Mock<GetBannerlordClassUseCase>(
            MockBehavior.Strict,
            new Mock<ICodeIndex>(MockBehavior.Strict).Object,
            NullLogger<GetBannerlordClassUseCase>.Instance);
        Assert.NotNull(mockClassUseCase.Object);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ThrowsValidationError_WhenClassNameIsNull()
    {
        var ex = Assert.Throws<ValidationError>(() => _tool.GetBannerlordClassDefinition(null!));
        Assert.Equal("className must be provided", ex.Message);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ThrowsValidationError_WhenClassNameIsEmpty()
    {
        var ex = Assert.Throws<ValidationError>(() => _tool.GetBannerlordClassDefinition(""));
        Assert.Equal("className must be provided", ex.Message);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ThrowsValidationError_WhenClassNameIsWhitespace()
    {
        var ex = Assert.Throws<ValidationError>(() => _tool.GetBannerlordClassDefinition("   "));
        Assert.Equal("className must be provided", ex.Message);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ReturnsClassSource_WhenClassFound()
    {
        var mockCodeIndex = new Mock<ICodeIndex>(MockBehavior.Strict);
        var className = "TestNamespace.TestClass";

        mockCodeIndex.Setup(ci => ci.FindClass(className))
            .Returns(TestHelper.MakeFile("TestClass.cs", "class TestClass { }"));

        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, NullLogger<GetBannerlordClassUseCase>.Instance);
        var tool = new GetBannerlordClassTool(useCase);

        var result = tool.GetBannerlordClassDefinition(className);

        Assert.NotNull(result);
        Assert.Contains("class TestClass", result);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ReturnsEmpty_WhenFileHasOnlyEmptyLines()
    {
        var mockCodeIndex = new Mock<ICodeIndex>(MockBehavior.Strict);
        var className = "TestNamespace.TestClass";

        mockCodeIndex.Setup(ci => ci.FindClass(className))
            .Returns(TestHelper.MakeFile("TestClass.cs", ""));

        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, NullLogger<GetBannerlordClassUseCase>.Instance);
        var tool = new GetBannerlordClassTool(useCase);

        var result = tool.GetBannerlordClassDefinition(className);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetBannerlordClassDefinition_ReturnsError_WhenClassNotFound()
    {
        var mockCodeIndex = new Mock<ICodeIndex>(MockBehavior.Strict);
        var className = "TestNamespace.NonExistentClass";

        mockCodeIndex.Setup(ci => ci.FindClass(className)).Returns((IndexedFile?)null);

        var useCase = new GetBannerlordClassUseCase(mockCodeIndex.Object, NullLogger<GetBannerlordClassUseCase>.Instance);
        var tool = new GetBannerlordClassTool(useCase);

        var result = tool.GetBannerlordClassDefinition(className);

        Assert.NotNull(result);
        Assert.StartsWith("[Error]", result);
    }
}
