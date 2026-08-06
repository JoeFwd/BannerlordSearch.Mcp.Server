using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.Repositories;
using Microsoft.Extensions.Logging;

namespace BannerlordSearch.Application.UseCases;

/// <summary>
/// Use case for retrieving the full source of a specified class.
/// Uses the in-memory <see cref="ICodeIndex"/> for O(1) lookup by fully-qualified class name.
/// The index must be built before calling this method (see <see cref="IndexBannerlordCodeUseCase"/>).
/// </summary>
public class GetBannerlordClassUseCase
{
    private readonly ICodeIndex _codeIndex;
    private readonly ILogger<GetBannerlordClassUseCase> _logger;

    public GetBannerlordClassUseCase(ICodeIndex codeIndex, ILogger<GetBannerlordClassUseCase> logger)
    {
        _codeIndex = codeIndex ?? throw new ArgumentNullException(nameof(codeIndex));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns the source code of the class identified by its fully qualified name,
    /// or by its simple (unqualified) name, e.g. "Hero" instead of "TaleWorlds.CampaignSystem.Hero".
    /// If the simple name matches more than one class, an error listing the fully-qualified
    /// candidates is returned so the caller can disambiguate.
    /// If <paramref name="startLine"/> and <paramref name="endLine"/> are both &gt; 0,
    /// only that line range (inclusive, 1-based) is returned.
    /// Errors are reported as a plain string prefixed with "[Error]".
    /// </summary>
    public string Execute(string className, int startLine = 0, int endLine = 0)
    {
        if (string.IsNullOrWhiteSpace(className))
            return "[Error] className must be provided";

        _logger.LogDebug("Looking up class '{ClassName}'", className);

        var file = _codeIndex.FindClass(className);
        if (file == null)
        {
            var candidates = _codeIndex.FindFullyQualifiedNames(className);
            if (candidates.Count == 1)
            {
                file = _codeIndex.FindClass(candidates[0]);
            }
            else if (candidates.Count > 1)
            {
                _logger.LogWarning("Class name '{ClassName}' is ambiguous: {Candidates}", className, string.Join(", ", candidates));
                return $"[Error] Multiple classes named '{className}' found: {string.Join(", ", candidates)}. Please specify one of these fully-qualified names.";
            }
        }

        if (file == null)
        {
            _logger.LogWarning("Class '{ClassName}' not found in index", className);
            return $"[Error] Class '{className}' not found";
        }

        _logger.LogInformation("Found class '{ClassName}' in {FilePath}", className, file.FilePath);

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
