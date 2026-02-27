using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BannerlordSearch.Application.Tests;

public class SearchBannerlordCodeUseCaseTests
{
    private static IndexedFile MakeFile(string filePath, params string[] lines) =>
        new() { FilePath = filePath, Lines = lines };

    private static Mock<ICodeIndex> EmptyIndex()
    {
        var mock = new Mock<ICodeIndex>();
        mock.Setup(ci => ci.Files).Returns(new List<IndexedFile>());
        return mock;
    }

    private static Mock<ICodeIndex> IndexWith(params IndexedFile[] files)
    {
        var mock = new Mock<ICodeIndex>();
        mock.Setup(ci => ci.Files).Returns(files.ToList());
        return mock;
    }

    [Fact]
    public void CanBeInstantiated()
    {
        var useCase = new SearchBannerlordCodeUseCase(new Mock<ICodeIndex>().Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        Assert.NotNull(useCase);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCodeIndexIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SearchBannerlordCodeUseCase(null!, NullLogger<SearchBannerlordCodeUseCase>.Instance));
    }

    [Fact]
    public void Execute_ThrowsArgumentNullException_WhenRegexpIsNull()
    {
        var useCase = new SearchBannerlordCodeUseCase(EmptyIndex().Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        Assert.Throws<ArgumentNullException>(() => useCase.Execute(null!, 1000, 10));
    }

    [Fact]
    public void Execute_ThrowsArgumentNullException_WhenRegexpIsEmpty()
    {
        var useCase = new SearchBannerlordCodeUseCase(EmptyIndex().Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);
        Assert.Throws<ArgumentNullException>(() => useCase.Execute("", 1000, 10));
    }

    [Fact]
    public void Execute_ReturnsEmptyList_WhenIndexHasNoFiles()
    {
        var useCase = new SearchBannerlordCodeUseCase(EmptyIndex().Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("test", 1000, 10);

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void Execute_ReturnsEmptyList_WhenNoFilesMatchPattern()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class SomeOtherClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("TestClass", 1000, 10);

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void Execute_ReturnsResults_WhenPatternMatchesContent()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("TestClass", 1000, 10);

        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void Execute_ReturnsEmpty_WhenMaxResultsIsNegative()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("TestClass", -1, 10);

        Assert.Empty(results);
    }

    [Fact]
    public void Execute_ReturnsEmpty_WhenMaxResultsIsZero()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("TestClass", 0, 10);

        Assert.Empty(results);
    }

    [Fact]
    public void Execute_IncludesContextLines_AroundMatches()
    {
        var mockIndex = IndexWith(MakeFile("test.cs",
            "namespace TestNamespace",
            "{",
            "    public class TestClass",
            "    {",
            "        public void Foo() { }",
            "    }",
            "}"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("TestClass", 1000, 2);

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.ContextBefore.Count > 0 || r.ContextAfter.Count > 0);
    }

    [Fact]
    public void Execute_RespectsMaxResultsLimit()
    {
        var mockIndex = IndexWith(
            MakeFile("test1.cs", "class TestClass { }"),
            MakeFile("test2.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("TestClass", 1, 10);

        // At most 1 match result + 1 total summary line
        Assert.True(results.Count <= 2);
    }

    [Fact]
    public void Execute_HandlesLargeMaxResults()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("TestClass", int.MaxValue, 10);

        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void Execute_MatchesPatternCaseInsensitively()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "public class MobileParty { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("mobileparty", 1000, 0);

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.CodeLine.Contains("MobileParty"));
    }

    [Fact]
    public void Execute_SetsLocationToNamespace_WhenNamespacePresent()
    {
        var mockIndex = IndexWith(MakeFile("test.cs",
            "namespace TaleWorlds.Core;",
            "public class ItemObject { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object, NullLogger<SearchBannerlordCodeUseCase>.Instance);

        var results = useCase.Execute("ItemObject", 1000, 0);

        var matchResult = results.FirstOrDefault(r => r.CodeLine.Contains("ItemObject") && !r.CodeLine.StartsWith("\n"));
        Assert.NotNull(matchResult);
        Assert.Equal("TaleWorlds.Core", matchResult!.Location);
    }
}
