using BannerlordSearch.Domain.Models;

namespace BannerlordSearch.Presentation.Tests;

internal static class TestHelper
{
    internal static IndexedFile MakeFile(string filePath, params string[] lines) =>
        new() { FilePath = filePath, Lines = lines };
}
