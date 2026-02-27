using BannerlordSearch.Domain;
using BannerlordSearch.Domain.Models;
using Xunit;

namespace BannerlordSearch.Domain.Tests;

public class IndexedFileTests
{
    [Fact]
    public void IndexedFile_HasDefaultValues()
    {
        var file = new IndexedFile();

        Assert.Equal(string.Empty, file.FilePath);
        Assert.Empty(file.Lines);
    }

    [Fact]
    public void IndexedFile_CanBeInitializedWithProperties()
    {
        var lines = new[] { "namespace Foo;", "public class Bar { }" };
        var file = new IndexedFile { FilePath = "root/Bar.cs", Lines = lines };

        Assert.Equal("root/Bar.cs", file.FilePath);
        Assert.Equal(2, file.Lines.Length);
        Assert.Equal("namespace Foo;", file.Lines[0]);
        Assert.Equal("public class Bar { }", file.Lines[1]);
    }
}
