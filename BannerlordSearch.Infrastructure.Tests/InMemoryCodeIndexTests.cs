using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.IO;
using BannerlordSearch.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BannerlordSearch.Infrastructure.Tests;

public class InMemoryCodeIndexTests
{
    private static Mock<IFileSystem> MakeFs(string root, IEnumerable<(string path, string[] lines)> files)
    {
        var fs = new Mock<IFileSystem>();
        fs.Setup(f => f.DirectoryExists(root)).Returns(true);
        fs.Setup(f => f.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            .Returns(files.Select(f => f.path).ToList());
        foreach (var (path, lines) in files)
            fs.Setup(f => f.ReadAllLines(path)).Returns(lines);
        return fs;
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFileSystemIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InMemoryCodeIndex(null!, NullLogger<InMemoryCodeIndex>.Instance));
    }

    [Fact]
    public void IsReady_ReturnsFalse_BeforeEnsureBuilt()
    {
        var index = new InMemoryCodeIndex(new Mock<IFileSystem>().Object, NullLogger<InMemoryCodeIndex>.Instance);
        Assert.False(index.IsReady);
    }

    [Fact]
    public void IsReady_ReturnsTrue_AfterEnsureBuilt()
    {
        var fs = new Mock<IFileSystem>();
        fs.Setup(f => f.DirectoryExists("root")).Returns(false);
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);

        index.EnsureBuilt("root");

        Assert.True(index.IsReady);
    }

    [Fact]
    public void Files_IsEmpty_WhenDirectoryDoesNotExist()
    {
        var fs = new Mock<IFileSystem>();
        fs.Setup(f => f.DirectoryExists("root")).Returns(false);
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);

        index.EnsureBuilt("root");

        Assert.Empty(index.Files);
    }

    [Fact]
    public void Files_ContainsAllCsFiles_AfterEnsureBuilt()
    {
        var fs = MakeFs("root", new[]
        {
            ("root/A.cs", new[] { "class A { }" }),
            ("root/B.cs", new[] { "class B { }" })
        });
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);

        index.EnsureBuilt("root");

        Assert.Equal(2, index.Files.Count);
    }

    [Fact]
    public void FindClass_ReturnsNull_WhenIndexIsEmpty()
    {
        var fs = new Mock<IFileSystem>();
        fs.Setup(f => f.DirectoryExists("root")).Returns(false);
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);
        index.EnsureBuilt("root");

        Assert.Null(index.FindClass("Foo.Bar"));
    }

    [Fact]
    public void FindClass_ReturnsFile_ForBlockNamespace()
    {
        var fs = MakeFs("root", new[]
        {
            ("root/Hero.cs", new[]
            {
                "namespace TaleWorlds.CampaignSystem",
                "{",
                "    public class Hero { }",
                "}"
            })
        });
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);
        index.EnsureBuilt("root");

        var result = index.FindClass("TaleWorlds.CampaignSystem.Hero");

        Assert.NotNull(result);
        Assert.Equal("root/Hero.cs", result!.FilePath);
    }

    [Fact]
    public void FindClass_ReturnsFile_ForFileScopedNamespace()
    {
        var fs = MakeFs("root", new[]
        {
            ("root/Hero.cs", new[]
            {
                "namespace TaleWorlds.CampaignSystem;",
                "public class Hero { }"
            })
        });
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);
        index.EnsureBuilt("root");

        var result = index.FindClass("TaleWorlds.CampaignSystem.Hero");

        Assert.NotNull(result);
    }

    [Fact]
    public void FindClass_ReturnsFile_WhenClassHasNoNamespace()
    {
        var fs = MakeFs("root", new[]
        {
            ("root/Foo.cs", new[] { "public class Foo { }" })
        });
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);
        index.EnsureBuilt("root");

        var result = index.FindClass("Foo");

        Assert.NotNull(result);
    }

    [Fact]
    public void FindClass_ReturnsNull_WhenClassDoesNotExist()
    {
        var fs = MakeFs("root", new[]
        {
            ("root/A.cs", new[] { "namespace Foo;", "public class A { }" })
        });
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);
        index.EnsureBuilt("root");

        Assert.Null(index.FindClass("Foo.NonExistent"));
    }

    [Fact]
    public void EnsureBuilt_IsIdempotent_ForSamePath()
    {
        var fs = MakeFs("root", new[]
        {
            ("root/A.cs", new[] { "class A { }" })
        });
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);

        index.EnsureBuilt("root");
        index.EnsureBuilt("root");

        // EnumerateFiles called only once even though EnsureBuilt called twice
        fs.Verify(f => f.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()), Times.Once);
    }

    [Fact]
    public void EnsureBuilt_HandlesFileReadException_Gracefully()
    {
        var fs = new Mock<IFileSystem>();
        fs.Setup(f => f.DirectoryExists("root")).Returns(true);
        fs.Setup(f => f.EnumerateFiles("root", "*.cs", SearchOption.AllDirectories))
            .Returns(new[] { "root/A.cs" });
        fs.Setup(f => f.ReadAllLines("root/A.cs")).Throws(new IOException("Access denied"));
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);

        index.EnsureBuilt("root"); // must not throw

        Assert.Empty(index.Files);
    }

    [Fact]
    public void EnsureBuilt_HandlesEnumerationException_Gracefully()
    {
        var fs = new Mock<IFileSystem>();
        fs.Setup(f => f.DirectoryExists("root")).Returns(true);
        fs.Setup(f => f.EnumerateFiles("root", "*.cs", SearchOption.AllDirectories))
            .Throws(new IOException("Access denied"));
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);

        index.EnsureBuilt("root"); // must not throw

        Assert.Empty(index.Files);
    }

    [Fact]
    public void FindClass_HandlesClassWithModifiers()
    {
        var fs = MakeFs("root", new[]
        {
            ("root/Item.cs", new[]
            {
                "namespace TaleWorlds.Core;",
                "public sealed class ItemObject { }"
            })
        });
        var index = new InMemoryCodeIndex(fs.Object, NullLogger<InMemoryCodeIndex>.Instance);
        index.EnsureBuilt("root");

        Assert.NotNull(index.FindClass("TaleWorlds.Core.ItemObject"));
    }
}
