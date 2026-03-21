using BannerlordSearch.Domain.Errors;
using BannerlordSearch.Infrastructure;
using Xunit;

namespace BannerlordSearch.Infrastructure.Tests;

public class BannerlordSourceFolderPathProviderTests : IDisposable
{
    private readonly string? _originalSourcePath;

    public BannerlordSourceFolderPathProviderTests()
    {
        _originalSourcePath = Environment.GetEnvironmentVariable("BANNERLORD_SOURCE_PATH");
        Environment.SetEnvironmentVariable("BANNERLORD_SOURCE_PATH", null);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("BANNERLORD_SOURCE_PATH", _originalSourcePath);
    }

    [Fact]
    public void GetBannerlordSourceFolderPath_ReturnsEnvVar_WhenSet()
    {
        Environment.SetEnvironmentVariable("BANNERLORD_SOURCE_PATH", @"C:\some\path");
        var sut = new BannerlordSourceFolderPathProvider();

        var result = sut.GetBannerlordSourceFolderPath();

        Assert.Equal(@"C:\some\path", result);
    }

    [Fact]
    public void GetBannerlordSourceFolderPath_Throws_WhenEnvVarNotSet()
    {
        var sut = new BannerlordSourceFolderPathProvider();

        Assert.Throws<InvalidSourcePathError>(() => sut.GetBannerlordSourceFolderPath());
    }
}
