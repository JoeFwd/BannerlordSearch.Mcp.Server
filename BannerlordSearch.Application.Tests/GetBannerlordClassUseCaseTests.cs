using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using Moq;
using Xunit;

namespace BannerlordSearch.Application.Tests;

public class GetBannerlordClassUseCaseTests
{
    private static IndexedFile MakeFile(string filePath, params string[] lines) =>
        new() { FilePath = filePath, Lines = lines };

    private static (Mock<ICodeIndex> index, Mock<IBannerlordSourcePathProvider> provider) MakeMocks(
        string rootPath = "valid_path")
    {
        var index = new Mock<ICodeIndex>();
        var provider = new Mock<IBannerlordSourcePathProvider>();
        provider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns(rootPath);
        return (index, provider);
    }

    [Fact]
    public void CanBeInstantiated()
    {
        var (index, provider) = MakeMocks();
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);
        Assert.NotNull(useCase);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCodeIndexIsNull()
    {
        var provider = new Mock<IBannerlordSourcePathProvider>();
        Assert.Throws<ArgumentNullException>(() => new GetBannerlordClassUseCase(null!, provider.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenSourceProviderIsNull()
    {
        var index = new Mock<ICodeIndex>();
        Assert.Throws<ArgumentNullException>(() => new GetBannerlordClassUseCase(index.Object, null!));
    }

    [Fact]
    public void Execute_ReturnsError_WhenClassNameIsNull()
    {
        var (index, provider) = MakeMocks();
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute(null!);

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsError_WhenClassNameIsEmpty()
    {
        var (index, provider) = MakeMocks();
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute(string.Empty);

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsError_WhenClassNotFound()
    {
        var (index, provider) = MakeMocks();
        index.Setup(ci => ci.FindClass(It.IsAny<string>())).Returns((IndexedFile?)null);
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute("NonExistentClass");

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsClassSource_WhenClassFound()
    {
        var (index, provider) = MakeMocks();
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(MakeFile("TestClass.cs",
                "namespace TestNamespace",
                "{",
                "    class TestClass { }",
                "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute("TestNamespace.TestClass");

        Assert.Contains("class TestClass", result);
        Assert.DoesNotContain("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsClassSource_WhenClassHasNoNamespace()
    {
        var (index, provider) = MakeMocks();
        index.Setup(ci => ci.FindClass("TestClass"))
            .Returns(MakeFile("TestClass.cs", "class TestClass { }"));
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute("TestClass");

        Assert.Contains("class TestClass", result);
        Assert.DoesNotContain("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsError_WhenStartLineGreaterThanEndLine()
    {
        var (index, provider) = MakeMocks();
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(MakeFile("TestClass.cs",
                "namespace TestNamespace", "{", "    class TestClass { }", "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute("TestNamespace.TestClass", startLine: 5, endLine: 3);

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsSpecificLineRange_WhenValidLineRangeProvided()
    {
        var (index, provider) = MakeMocks();
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(MakeFile("TestClass.cs",
                "namespace TestNamespace",
                "{",
                "    public class TestClass",
                "    {",
                "        public void Foo() { }",
                "    }",
                "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute("TestNamespace.TestClass", startLine: 3, endLine: 5);

        Assert.DoesNotContain("[Error]", result);
        Assert.Contains("public class TestClass", result);
        Assert.Contains("public void Foo", result);
        Assert.DoesNotContain("namespace TestNamespace", result);
        Assert.DoesNotContain("{", result);
        Assert.DoesNotContain("}", result);
    }

    [Fact]
    public void Execute_ReturnsSingleLine_WhenSingleLineRangeProvided()
    {
        var (index, provider) = MakeMocks();
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(MakeFile("TestClass.cs",
                "namespace TestNamespace",
                "{",
                "    public class TestClass",
                "    {",
                "        public void Foo() { }",
                "    }",
                "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute("TestNamespace.TestClass", startLine: 3, endLine: 3);

        Assert.DoesNotContain("[Error]", result);
        Assert.Contains("public class TestClass", result);
        Assert.DoesNotContain("namespace TestNamespace", result);
        Assert.DoesNotContain("{", result);
        Assert.DoesNotContain("}", result);
    }

    [Fact]
    public void Execute_ReturnsError_WhenLineRangeExceedsFileSize()
    {
        var (index, provider) = MakeMocks();
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(MakeFile("TestClass.cs",
                "namespace TestNamespace", "{", "    class TestClass { }", "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute("TestNamespace.TestClass", startLine: 1, endLine: 10);

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsError_WhenLargeEndLineExceedsFileSize()
    {
        var (index, provider) = MakeMocks();
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(MakeFile("TestClass.cs",
                "namespace TestNamespace", "{", "    class TestClass { }", "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        var result = useCase.Execute("TestNamespace.TestClass", startLine: 1, endLine: int.MaxValue);

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_CallsEnsureBuilt_WithRootPath()
    {
        var (index, provider) = MakeMocks("my_root_path");
        index.Setup(ci => ci.FindClass(It.IsAny<string>())).Returns((IndexedFile?)null);
        var useCase = new GetBannerlordClassUseCase(index.Object, provider.Object);

        useCase.Execute("SomeClass");

        index.Verify(ci => ci.EnsureBuilt("my_root_path"), Times.Once);
    }
}
