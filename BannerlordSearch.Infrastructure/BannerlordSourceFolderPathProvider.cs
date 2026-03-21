using BannerlordSearch.Application.Ports.Configuration;
using BannerlordSearch.Domain.Errors;

namespace BannerlordSearch.Infrastructure;

/// <summary>
/// Concrete implementation of <see cref="IBannerlordSourcePathProvider"/> that reads
/// the Bannerlord decompiled source root path from the BANNERLORD_SOURCE_PATH environment variable.
/// </summary>
public class BannerlordSourceFolderPathProvider : IBannerlordSourcePathProvider
{
    public string GetBannerlordSourceFolderPath()
    {
        var envRoot = Environment.GetEnvironmentVariable("BANNERLORD_SOURCE_PATH");
        if (!string.IsNullOrWhiteSpace(envRoot)) return envRoot;

        throw new InvalidSourcePathError(
            "BANNERLORD_SOURCE_PATH is not set. " +
            "Set it to the root folder containing the Bannerlord decompiled .cs source files.");
    }
}
