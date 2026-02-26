using BannerlordSearch.Domain;

namespace BannerlordSearch.Application.UseCases;

/// <summary>
/// Use case for retrieving the full source of a specified class.
/// Uses the in-memory <see cref="ICodeIndex"/> for O(1) lookup by fully-qualified class name.
/// </summary>
public class GetBannerlordClassUseCase
{
    private readonly ICodeIndex _codeIndex;
    private readonly IBannerlordSourcePathProvider _sourceFolderProvider;

    public GetBannerlordClassUseCase(ICodeIndex codeIndex, IBannerlordSourcePathProvider sourceFolderProvider)
    {
        _codeIndex = codeIndex ?? throw new ArgumentNullException(nameof(codeIndex));
        _sourceFolderProvider = sourceFolderProvider ?? throw new ArgumentNullException(nameof(sourceFolderProvider));
    }

    /// <summary>
    /// Returns the source code of the class identified by its fully qualified name.
    /// If <paramref name="startLine"/> and <paramref name="endLine"/> are both &gt; 0,
    /// only that line range (inclusive, 1-based) is returned.
    /// Errors are reported as a plain string prefixed with "[Error]".
    /// </summary>
    public string Execute(string className, int startLine = 0, int endLine = 0)
    {
        if (string.IsNullOrWhiteSpace(className))
            return "[Error] className must be provided";

        var rootPath = _sourceFolderProvider.GetBannerlordSourceFolderPath();
        _codeIndex.EnsureBuilt(rootPath);

        var file = _codeIndex.FindClass(className);
        if (file == null)
            return $"[Error] Class '{className}' not found";

        var lines = file.Lines;

        if (startLine > 0 && endLine > 0)
        {
            if (startLine > lines.Length || endLine > lines.Length || startLine > endLine)
                return $"[Error] Invalid line range: {startLine}-{endLine} for file {Path.GetFileName(file.FilePath)}";

            var selected = new List<string>();
            for (int l = startLine; l <= endLine; l++)
            {
                var line = lines[l - 1];
                var trimmed = line.Trim();
                if (trimmed.StartsWith("namespace ")) continue;
                var cleaned = line.Replace("{", "").Replace("}", "").Trim();
                if (!string.IsNullOrWhiteSpace(cleaned))
                    selected.Add(cleaned);
            }
            return string.Join("\n", selected);
        }

        return string.Join("\n", lines);
    }
}
