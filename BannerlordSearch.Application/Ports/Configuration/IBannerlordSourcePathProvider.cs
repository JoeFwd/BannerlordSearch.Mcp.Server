namespace BannerlordSearch.Application.Ports.Configuration;

/// <summary>
/// Interface for providing the Bannerlord decompiled source root path.
/// </summary>
public interface IBannerlordSourcePathProvider
{
    /// <summary>
    ///     Returns the Bannerlord decompiled source root path.
    ///     Checks environment variable BANNERLORD_SOURCE_PATH, otherwise falls back to default decompiler cache path.
    /// </summary>
    string GetBannerlordSourceFolderPath();
}
