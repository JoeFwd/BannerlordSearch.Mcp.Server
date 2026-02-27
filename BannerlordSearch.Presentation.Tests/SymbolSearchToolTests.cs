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

public class SymbolSearchToolTests
{
    private readonly SymbolSearchTool _tool;

    public SymbolSearchToolTests()
    {
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(
            MockBehavior.Strict,
            new Mock<ICodeIndex>(MockBehavior.Strict).Object,
            NullLogger<SearchBannerlordCodeUseCase>.Instance);
        _tool = new SymbolSearchTool(mockSearchUseCase.Object);
    }

    [Fact]
    public void SymbolSearchTool_CanBeInstantiated()
    {
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(
            MockBehavior.Strict,
            new Mock<ICodeIndex>(MockBehavior.Strict).Object,
            NullLogger<SearchBannerlordCodeUseCase>.Instance);
        Assert.NotNull(mockSearchUseCase.Object);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationError_WhenRegexpIsNull()
    {
        var ex = Assert.Throws<ValidationError>(() => _tool.SearchBannerlordCode(null!, 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationError_WhenRegexpIsEmpty()
    {
        var ex = Assert.Throws<ValidationError>(() => _tool.SearchBannerlordCode("", 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationError_WhenRegexpContainsOnlyWhitespace()
    {
        var ex = Assert.Throws<ValidationError>(() => _tool.SearchBannerlordCode("   ", 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ReturnsEmptyList_WhenNoFilesIndexed()
    {
        var mockCodeIndex = new Mock<ICodeIndex>(MockBehavior.Strict);
        mockCodeIndex.Setup(ci => ci.Files).Returns(new List<IndexedFile>());
        var useCase = new SearchBannerlordCodeUseCase(mockCodeIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        var tool = new SymbolSearchTool(useCase);

        var result = tool.SearchBannerlordCode("TestClass", 500, 5);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ReturnsResults_WhenMatchFound()
    {
        var mockCodeIndex = new Mock<ICodeIndex>(MockBehavior.Strict);
        mockCodeIndex.Setup(ci => ci.Files)
            .Returns(new List<IndexedFile> { TestHelper.MakeFile("test.cs", "Found TestClass") });
        var useCase = new SearchBannerlordCodeUseCase(mockCodeIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        var tool = new SymbolSearchTool(useCase);

        var result = tool.SearchBannerlordCode("TestClass", 500, 5);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, r => r.CodeLine == "Found TestClass");
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_WithZeroMaxResults_ReturnsEmpty()
    {
        var mockCodeIndex = new Mock<ICodeIndex>(MockBehavior.Strict);
        mockCodeIndex.Setup(ci => ci.Files)
            .Returns(new List<IndexedFile> { TestHelper.MakeFile("test.cs", "Found TestClass") });
        var useCase = new SearchBannerlordCodeUseCase(mockCodeIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        var tool = new SymbolSearchTool(useCase);

        var result = tool.SearchBannerlordCode("TestClass", 0, 5);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_WithNegativeMaxResults_UsesMockedUseCase()
    {
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(
            MockBehavior.Strict,
            new Mock<ICodeIndex>(MockBehavior.Strict).Object,
            NullLogger<SearchBannerlordCodeUseCase>.Instance);
        var tool = new SymbolSearchTool(mockSearchUseCase.Object);

        mockSearchUseCase.Setup(u => u.Execute("TestClass", -1, 5))
            .Returns(new List<SearchResult>());

        var result = tool.SearchBannerlordCode("TestClass", -1, 5);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
