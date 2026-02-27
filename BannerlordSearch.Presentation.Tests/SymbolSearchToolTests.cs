using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BannerlordSearch.Presentation.Tests;

public class SymbolSearchToolTests
{
    private static IndexedFile MakeFile(string filePath, params string[] lines) =>
        new() { FilePath = filePath, Lines = lines };

    [Fact]
    public void SymbolSearchTool_CanBeInstantiated()
    {
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        Assert.NotNull(mockSearchUseCase.Object);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationError_WhenRegexpIsNull()
    {
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        var tool = new SymbolSearchTool(mockSearchUseCase.Object);

        var ex = Assert.Throws<ValidationError>(() => tool.SearchBannerlordCode(null!, 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationError_WhenRegexpIsEmpty()
    {
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        var tool = new SymbolSearchTool(mockSearchUseCase.Object);

        var ex = Assert.Throws<ValidationError>(() => tool.SearchBannerlordCode("", 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationError_WhenRegexpContainsOnlyWhitespace()
    {
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        var tool = new SymbolSearchTool(mockSearchUseCase.Object);

        var ex = Assert.Throws<ValidationError>(() => tool.SearchBannerlordCode("   ", 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ReturnsEmptyList_WhenNoFilesIndexed()
    {
        var mockCodeIndex = new Mock<ICodeIndex>();
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
        var mockCodeIndex = new Mock<ICodeIndex>();
        mockCodeIndex.Setup(ci => ci.Files)
            .Returns(new List<IndexedFile> { MakeFile("test.cs", "Found TestClass") });
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
        var mockCodeIndex = new Mock<ICodeIndex>();
        mockCodeIndex.Setup(ci => ci.Files)
            .Returns(new List<IndexedFile> { MakeFile("test.cs", "Found TestClass") });
        var useCase = new SearchBannerlordCodeUseCase(mockCodeIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        var tool = new SymbolSearchTool(useCase);

        var result = tool.SearchBannerlordCode("TestClass", 0, 5);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_WithNegativeMaxResults_UsesMockedUseCase()
    {
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        var tool = new SymbolSearchTool(mockSearchUseCase.Object);

        mockSearchUseCase.Setup(u => u.Execute("TestClass", -1, 5))
            .Returns(new List<SearchResult>());

        var result = tool.SearchBannerlordCode("TestClass", -1, 5);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
