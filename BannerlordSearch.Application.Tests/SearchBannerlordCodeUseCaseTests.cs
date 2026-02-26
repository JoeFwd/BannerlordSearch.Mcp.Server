using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
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
        var useCase = new SearchBannerlordCodeUseCase(new Mock<ICodeIndex>().Object);
        Assert.NotNull(useCase);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCodeIndexIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SearchBannerlordCodeUseCase(null!));
    }

    [Fact]
    public void Execute_ThrowsArgumentNullException_WhenRegexpIsNull()
    {
        var useCase = new SearchBannerlordCodeUseCase(EmptyIndex().Object);
        Assert.Throws<ArgumentNullException>(() => useCase.Execute(null!, "path", 1000, 10));
    }

    [Fact]
    public void Execute_ThrowsArgumentNullException_WhenRegexpIsEmpty()
    {
        var useCase = new SearchBannerlordCodeUseCase(EmptyIndex().Object);
        Assert.Throws<ArgumentNullException>(() => useCase.Execute("", "path", 1000, 10));
    }

    [Fact]
    public void Execute_ReturnsEmptyList_WhenIndexHasNoFiles()
    {
        var useCase = new SearchBannerlordCodeUseCase(EmptyIndex().Object);

        var results = useCase.Execute("test", "any_path", 1000, 10);

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void Execute_ReturnsEmptyList_WhenNoFilesMatchPattern()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class SomeOtherClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object);

        var results = useCase.Execute("TestClass", "valid_path", 1000, 10);

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void Execute_ReturnsResults_WhenPatternMatchesContent()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object);

        var results = useCase.Execute("TestClass", "valid_path", 1000, 10);

        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void Execute_StopsAndReturnsTotalResult_WhenMaxResultsIsNegative()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object);

        var results = useCase.Execute("TestClass", "valid_path", -1, 10);

        Assert.Single(results);
        Assert.Contains("\nTotal matches for", results[0].CodeLine);
    }

    [Fact]
    public void Execute_StopsAndReturnsTotalResult_WhenMaxResultsIsZero()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object);

        var results = useCase.Execute("TestClass", "valid_path", 0, 10);

        Assert.Single(results);
        Assert.Contains("\nTotal matches for", results[0].CodeLine);
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
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object);

        var results = useCase.Execute("TestClass", "valid_path", 1000, 2);

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.ContextBefore.Count > 0 || r.ContextAfter.Count > 0);
    }

    [Fact]
    public void Execute_RespectsMaxResultsLimit()
    {
        var mockIndex = IndexWith(
            MakeFile("test1.cs", "class TestClass { }"),
            MakeFile("test2.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object);

        var results = useCase.Execute("TestClass", "valid_path", 1, 10);

        // At most 1 match result + 1 total summary line
        Assert.True(results.Count <= 2);
    }

    [Fact]
    public void Execute_HandlesLargeMaxResults()
    {
        var mockIndex = IndexWith(MakeFile("test.cs", "class TestClass { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object);

        var results = useCase.Execute("TestClass", "valid_path", int.MaxValue, 10);

        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void Execute_CallsEnsureBuilt_WithRootPath()
    {
        var mockIndex = EmptyIndex();
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object);

        useCase.Execute("test", "my_root_path", 1000, 10);

        mockIndex.Verify(ci => ci.EnsureBuilt("my_root_path"), Times.Once);
    }

    [Fact]
    public void Execute_SetsLocationToNamespace_WhenNamespacePresent()
    {
        var mockIndex = IndexWith(MakeFile("test.cs",
            "namespace TaleWorlds.Core;",
            "public class ItemObject { }"));
        var useCase = new SearchBannerlordCodeUseCase(mockIndex.Object);

        var results = useCase.Execute("ItemObject", "valid_path", 1000, 0);

        var matchResult = results.FirstOrDefault(r => r.CodeLine.Contains("ItemObject") && !r.CodeLine.StartsWith("\n"));
        Assert.NotNull(matchResult);
        Assert.Equal("TaleWorlds.Core", matchResult!.Location);
    }
}
