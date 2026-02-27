using BannerlordSearch.Application.Ports;
using BannerlordSearch.Domain;
 
namespace BannerlordSearch.Infrastructure;
 
/// <summary>
/// Concrete implementation of <see cref="IBannerlordSourcePathProvider"/> that provides the Bannerlord decompiled source root path.
/// </summary>
public class BannerlordSourceFolderPathProvider : IBannerlordSourcePathProvider
{
    /// <summary>
    ///     Returns the Bannerlord decompiled source root path.
    ///     Checks environment variable BANNERLORD_SOURCE_PATH, otherwise falls back to default decompiler cache path.
    /// </summary>
    public string GetBannerlordSourceFolderPath()
    {
        var envRoot = Environment.GetEnvironmentVariable("BANNERLORD_SOURCE_PATH");
        if (!string.IsNullOrWhiteSpace(envRoot)) return envRoot;
        throw new InvalidSourcePathError("Environment variable BANNERLORD_SOURCE_PATH is not set.");
    }
}