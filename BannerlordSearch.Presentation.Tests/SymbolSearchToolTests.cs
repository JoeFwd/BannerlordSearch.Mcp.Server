using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
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
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object);

        Assert.NotNull(mockSearchUseCase.Object);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationException_WhenRegexpIsNull()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, mockSearchUseCase.Object);

        var ex = Assert.Throws<ValidationException>(() => tool.SearchBannerlordCode(null!, 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationException_WhenRegexpIsEmpty()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, mockSearchUseCase.Object);

        var ex = Assert.Throws<ValidationException>(() => tool.SearchBannerlordCode("", 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ThrowsValidationException_WhenRegexpContainsOnlyWhitespace()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, mockSearchUseCase.Object);

        var ex = Assert.Throws<ValidationException>(() => tool.SearchBannerlordCode("   ", 1000, 10));
        Assert.Equal("regexp must be provided", ex.Message);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_CallsSourceProvider_ForRootPath()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        mockCodeIndex.Setup(ci => ci.Files).Returns(new List<IndexedFile>());
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("expected_root_path");
        var useCase = new SearchBannerlordCodeUseCase(mockCodeIndex.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, useCase);

        var result = tool.SearchBannerlordCode("TestClass", 500, 5);

        mockSourceProvider.Verify(p => p.GetBannerlordSourceFolderPath(), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ReturnsEmptyList_WhenNoFilesIndexed()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        mockCodeIndex.Setup(ci => ci.Files).Returns(new List<IndexedFile>());
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("expected_root_path");
        var useCase = new SearchBannerlordCodeUseCase(mockCodeIndex.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, useCase);

        var result = tool.SearchBannerlordCode("TestClass", 500, 5);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_ReturnsResults_WhenMatchFound()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        mockCodeIndex.Setup(ci => ci.Files)
            .Returns(new List<IndexedFile> { MakeFile("test.cs", "Found TestClass") });
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("expected_root_path");
        var useCase = new SearchBannerlordCodeUseCase(mockCodeIndex.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, useCase);

        var result = tool.SearchBannerlordCode("TestClass", 500, 5);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, r => r.CodeLine == "Found TestClass");
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_WithZeroMaxResults_ReturnsTotalSummary()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockCodeIndex = new Mock<ICodeIndex>();
        mockCodeIndex.Setup(ci => ci.Files)
            .Returns(new List<IndexedFile> { MakeFile("test.cs", "Found TestClass") });
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("expected_root_path");
        var useCase = new SearchBannerlordCodeUseCase(mockCodeIndex.Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, useCase);

        var result = tool.SearchBannerlordCode("TestClass", 0, 5);

        Assert.NotNull(result);
        // With maxResults=0, search stops after first match and returns the total summary line
        Assert.Single(result);
        Assert.Contains("\nTotal matches for", result[0].CodeLine);
    }

    [Fact]
    public void SymbolSearchTool_SearchBannerlordCode_WithNegativeMaxResults_UsesMockedUseCase()
    {
        var mockSourceProvider = new Mock<IBannerlordSourcePathProvider>();
        var mockSearchUseCase = new Mock<SearchBannerlordCodeUseCase>(new Mock<ICodeIndex>().Object);
        var tool = new SymbolSearchTool(mockSourceProvider.Object, mockSearchUseCase.Object);

        var expectedRootPath = "expected_root_path";
        mockSourceProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(expectedRootPath);
        mockSearchUseCase.Setup(u => u.Execute("TestClass", expectedRootPath, -1, 5))
            .Returns(new List<SearchResult> { new() { CodeLine = "\nTotal matches for \"TestClass\": 0" } });

        var result = tool.SearchBannerlordCode("TestClass", -1, 5);

        Assert.NotNull(result);
        Assert.Single(result);
    }
}
