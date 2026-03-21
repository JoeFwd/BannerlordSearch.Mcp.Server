using BannerlordSearch.Application.Ports.Repositories;
using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using BannerlordSearch.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BannerlordSearch.Presentation.Tests;

public class SymbolSearchToolIntegrationTests
{
    private static SymbolSearchTool CreateTool(params IndexedFile[] files)
    {
        var mockCodeIndex = new Mock<ICodeIndex>(MockBehavior.Strict);
        mockCodeIndex.Setup(ci => ci.Files).Returns(files.ToList());
        var useCase = new SearchBannerlordCodeUseCase(mockCodeIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        return new SymbolSearchTool(useCase);
    }

    [Fact]
    public void SearchBannerlordCode_WithOnlyRegexp_ReturnsResults()
    {
        var tool = CreateTool(TestHelper.MakeFile("test.cs", "public class ItemObject { }"));

        var result = tool.SearchBannerlordCode("ItemObject");

        Assert.NotEmpty(result);
        Assert.Contains(result, r => r.CodeLine.Contains("ItemObject"));
    }

    [Fact]
    public void SearchBannerlordCode_WithDefaultMaxResults_CapsAtSearchDefaultsMaxResults()
    {
        var lines = Enumerable.Range(1, SearchDefaults.MaxResults + 50).Select(i => $"Match_{i}").ToArray();
        var tool = CreateTool(TestHelper.MakeFile("test.cs", lines));

        var result = tool.SearchBannerlordCode("Match_");

        Assert.Equal(SearchDefaults.MaxResults, result.Count);
    }

    [Fact]
    public void SearchBannerlordCode_WithDefaultContextLines_Includes3LinesEachSide()
    {
        var tool = CreateTool(TestHelper.MakeFile("test.cs",
            "before1", "before2", "before3", "TARGET", "after1", "after2", "after3"));

        var result = tool.SearchBannerlordCode("TARGET");

        Assert.Single(result);
        Assert.Equal(3, result[0].ContextBefore.Count);
        Assert.Equal(3, result[0].ContextAfter.Count);
    }
}
