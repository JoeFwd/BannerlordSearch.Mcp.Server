using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.Configuration;
using BannerlordSearch.Application.Ports.Repositories;
using Microsoft.Extensions.Logging;

namespace BannerlordSearch.Application.UseCases;

public class IndexBannerlordCodeUseCase
{
    private readonly ICodeIndex _codeIndex;
    private readonly IBannerlordSourcePathProvider _pathProvider;
    private readonly ILogger<IndexBannerlordCodeUseCase> _logger;

    public IndexBannerlordCodeUseCase(
        ICodeIndex codeIndex,
        IBannerlordSourcePathProvider pathProvider,
        ILogger<IndexBannerlordCodeUseCase> logger)
    {
        _codeIndex = codeIndex ?? throw new ArgumentNullException(nameof(codeIndex));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Execute()
    {
        var rootPath = _pathProvider.GetBannerlordSourceFolderPath();
        _logger.LogInformation("Building index from {RootPath}", rootPath);
        _codeIndex.EnsureBuilt(rootPath);
        _logger.LogInformation("Index built: {FileCount} files", _codeIndex.Files.Count);
    }
}
