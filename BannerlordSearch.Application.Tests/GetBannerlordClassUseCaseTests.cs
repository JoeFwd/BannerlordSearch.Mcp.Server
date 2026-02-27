using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.Repositories;
using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using BannerlordSearch.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BannerlordSearch.Application.Tests;

public class GetBannerlordClassUseCaseTests
{
    [Fact]
    public void CanBeInstantiated()
    {
        var useCase = new GetBannerlordClassUseCase(new Mock<ICodeIndex>(MockBehavior.Strict).Object, NullLogger<GetBannerlordClassUseCase>.Instance);
        Assert.NotNull(useCase);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCodeIndexIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new GetBannerlordClassUseCase(null!, NullLogger<GetBannerlordClassUseCase>.Instance));
    }

    [Fact]
    public void Execute_ReturnsError_WhenClassNameIsNull()
    {
        var useCase = new GetBannerlordClassUseCase(new Mock<ICodeIndex>(MockBehavior.Strict).Object, NullLogger<GetBannerlordClassUseCase>.Instance);

        var result = useCase.Execute(null!);

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsError_WhenClassNameIsEmpty()
    {
        var useCase = new GetBannerlordClassUseCase(new Mock<ICodeIndex>(MockBehavior.Strict).Object, NullLogger<GetBannerlordClassUseCase>.Instance);

        var result = useCase.Execute(string.Empty);

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsError_WhenClassNotFound()
    {
        var index = new Mock<ICodeIndex>(MockBehavior.Strict);
        index.Setup(ci => ci.FindClass("NonExistentClass")).Returns((IndexedFile?)null);
        var useCase = new GetBannerlordClassUseCase(index.Object, NullLogger<GetBannerlordClassUseCase>.Instance);

        var result = useCase.Execute("NonExistentClass");

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsClassSource_WhenClassFound()
    {
        var index = new Mock<ICodeIndex>(MockBehavior.Strict);
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(TestHelper.MakeFile("TestClass.cs",
                "namespace TestNamespace",
                "{",
                "    class TestClass { }",
                "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, NullLogger<GetBannerlordClassUseCase>.Instance);

        var result = useCase.Execute("TestNamespace.TestClass");

        Assert.Contains("class TestClass", result);
        Assert.DoesNotContain("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsClassSource_WhenClassHasNoNamespace()
    {
        var index = new Mock<ICodeIndex>(MockBehavior.Strict);
        index.Setup(ci => ci.FindClass("TestClass"))
            .Returns(TestHelper.MakeFile("TestClass.cs", "class TestClass { }"));
        var useCase = new GetBannerlordClassUseCase(index.Object, NullLogger<GetBannerlordClassUseCase>.Instance);

        var result = useCase.Execute("TestClass");

        Assert.Contains("class TestClass", result);
        Assert.DoesNotContain("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsError_WhenStartLineGreaterThanEndLine()
    {
        var index = new Mock<ICodeIndex>(MockBehavior.Strict);
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(TestHelper.MakeFile("TestClass.cs",
                "namespace TestNamespace", "{", "    class TestClass { }", "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, NullLogger<GetBannerlordClassUseCase>.Instance);

        var result = useCase.Execute("TestNamespace.TestClass", startLine: 5, endLine: 3);

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsSpecificLineRange_WhenValidLineRangeProvided()
    {
        var index = new Mock<ICodeIndex>(MockBehavior.Strict);
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(TestHelper.MakeFile("TestClass.cs",
                "namespace TestNamespace",
                "{",
                "    public class TestClass",
                "    {",
                "        public void Foo() { }",
                "    }",
                "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, NullLogger<GetBannerlordClassUseCase>.Instance);

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
        var index = new Mock<ICodeIndex>(MockBehavior.Strict);
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(TestHelper.MakeFile("TestClass.cs",
                "namespace TestNamespace",
                "{",
                "    public class TestClass",
                "    {",
                "        public void Foo() { }",
                "    }",
                "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, NullLogger<GetBannerlordClassUseCase>.Instance);

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
        var index = new Mock<ICodeIndex>(MockBehavior.Strict);
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(TestHelper.MakeFile("TestClass.cs",
                "namespace TestNamespace", "{", "    class TestClass { }", "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, NullLogger<GetBannerlordClassUseCase>.Instance);

        var result = useCase.Execute("TestNamespace.TestClass", startLine: 1, endLine: 10);

        Assert.StartsWith("[Error]", result);
    }

    [Fact]
    public void Execute_ReturnsError_WhenLargeEndLineExceedsFileSize()
    {
        var index = new Mock<ICodeIndex>(MockBehavior.Strict);
        index.Setup(ci => ci.FindClass("TestNamespace.TestClass"))
            .Returns(TestHelper.MakeFile("TestClass.cs",
                "namespace TestNamespace", "{", "    class TestClass { }", "}"));
        var useCase = new GetBannerlordClassUseCase(index.Object, NullLogger<GetBannerlordClassUseCase>.Instance);

        var result = useCase.Execute("TestNamespace.TestClass", startLine: 1, endLine: int.MaxValue);

        Assert.StartsWith("[Error]", result);
    }
}
