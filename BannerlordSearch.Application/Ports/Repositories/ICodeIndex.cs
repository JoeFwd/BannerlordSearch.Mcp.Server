using BannerlordSearch.Domain.Models;

namespace BannerlordSearch.Application.Ports.Repositories;

/// <summary>
/// Provides an in-memory index of all C# source files for fast regex searching
/// and O(1) class lookup by fully-qualified name.
/// </summary>
public interface ICodeIndex
{
    /// <summary>True once <see cref="EnsureBuilt"/> has completed for the current root path.</summary>
    bool IsReady { get; }

    /// <summary>All indexed files. Empty until <see cref="EnsureBuilt"/> has been called.</summary>
    IReadOnlyList<IndexedFile> Files { get; }

    /// <summary>
    /// Returns the <see cref="IndexedFile"/> containing the class identified by its
    /// fully-qualified name (e.g. "TaleWorlds.CampaignSystem.Hero"), or <c>null</c> if not found.
    /// </summary>
    IndexedFile? FindClass(string fullyQualifiedName);

    /// <summary>
    /// Returns the fully-qualified names of all indexed classes whose simple (unqualified)
    /// name matches <paramref name="simpleClassName"/> (e.g. "Hero" matches "TaleWorlds.CampaignSystem.Hero").
    /// Empty if no class has that simple name.
    /// </summary>
    IReadOnlyList<string> FindFullyQualifiedNames(string simpleClassName);

    /// <summary>
    /// Builds the index for <paramref name="rootPath"/> if not already built.
    /// Subsequent calls with the same path are no-ops.
    /// </summary>
    void EnsureBuilt(string rootPath);
}
