using BannerlordSearch.Application.Ports;

namespace BannerlordSearch.Application.UseCases;

public class IndexBannerlordCodeUseCase
{
    private readonly ICodeIndex _codeIndex;
    private readonly IBannerlordSourcePathProvider _pathProvider;

    public IndexBannerlordCodeUseCase(ICodeIndex codeIndex, IBannerlordSourcePathProvider pathProvider)
    {
        _codeIndex = codeIndex ?? throw new ArgumentNullException(nameof(codeIndex));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    }

    public void Execute()
    {
        var rootPath = _pathProvider.GetBannerlordSourceFolderPath();
        _codeIndex.EnsureBuilt(rootPath);
    }
}
