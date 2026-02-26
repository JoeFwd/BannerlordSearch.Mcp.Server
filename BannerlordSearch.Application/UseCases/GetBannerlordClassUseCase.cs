using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BannerlordSearch.Domain;

namespace BannerlordSearch.Application.UseCases;

/// <summary>
/// Use case for retrieving the full source of a specified class.
/// </summary>
public class GetBannerlordClassUseCase
{
    private readonly ISymbolSearchRepository _fileRepository;
    private readonly IBannerlordSourcePathProvider _sourceFolderProvider;
    private readonly IFileSystem _fileSystem;

    public GetBannerlordClassUseCase(ISymbolSearchRepository fileRepository, IBannerlordSourcePathProvider sourceFolderProvider, IFileSystem fileSystem)
    {
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _sourceFolderProvider = sourceFolderProvider ?? throw new ArgumentNullException(nameof(sourceFolderProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// Returns the source code of the class identified by its fully qualified name.
    /// If <paramref name="startLine"/> and <paramref name="endLine"/> are provided (both > 0), only that line range (inclusive, 1-based) is returned.
    /// Otherwise the entire file is returned.
    /// Errors are reported as a plain string prefixed with "[Error]".
    /// </summary>
    public string Execute(string className, int startLine = 0, int endLine = 0)
    {
        if (string.IsNullOrWhiteSpace(className))
            return "[Error] className must be provided";

        // Split namespace and class name
        var lastDot = className.LastIndexOf('.');
        string targetNamespace = lastDot > 0 ? className.Substring(0, lastDot) : string.Empty;
        string targetClass = lastDot > 0 ? className.Substring(lastDot + 1) : className;

        // Build expected file path based on namespace folder structure
        var rootPath = _sourceFolderProvider.GetBannerlordSourceFolderPath();
        var expectedPath = string.IsNullOrEmpty(targetNamespace)
            ? Path.Combine(rootPath, targetClass + ".cs")
            : Path.Combine(rootPath, targetNamespace.Replace('.', Path.DirectorySeparatorChar), targetClass + ".cs");
        
        // Don't normalize the path for testing compatibility - use as constructed

        // Try to find file directly first - this is the primary path
        if (_fileSystem.FileExists(expectedPath))
        {
            try
            {
                var lines = _fileSystem.ReadAllLines(expectedPath);
                if (startLine > 0 && endLine > 0)
                {
                    if (startLine > lines.Length || endLine > lines.Length || startLine > endLine)
                        return $"[Error] Invalid line range: {startLine}-{endLine} for file {Path.GetFileName(expectedPath)}";
                    var selected = new List<string>();
                    for (int l = startLine; l <= endLine; l++)
                    {
                        var line = lines[l - 1];
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("namespace "))
                            continue;
                        var cleaned = line.Replace("{", "").Replace("}", "").Trim();
                        if (!string.IsNullOrWhiteSpace(cleaned))
                            selected.Add(cleaned);
                    }
                    return string.Join("\n", selected);
                }
                return string.Join("\n", lines);
            }
            catch (Exception ex)
            {
                // More specific error handling for file reading issues
                return $"[Error] Failed to read file: {ex.Message}";
            }
        }
        
        // Fallback: search all cs files using repository (in case file naming differs)
        var files = _fileRepository.GetCsFiles(rootPath) ?? new List<string>();
        foreach (var file in files)
        {
            string[] lines;
            try 
            { 
                lines = _fileRepository.ReadAllLines(file); 
            }
            catch 
            { 
                continue; 
            }

            string currentNamespace = string.Empty;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("namespace "))
                {
                    var ns = line.Substring("namespace ".Length).Trim();
                    ns = ns.Split('{')[0].Trim();
                    currentNamespace = ns;
                }
                var classMatch = Regex.Match(line, @"\bclass\s+" + Regex.Escape(targetClass) + @"\b");
                if (classMatch.Success && (string.IsNullOrEmpty(targetNamespace) || currentNamespace == targetNamespace))
                {
                    if (startLine > 0 && endLine > 0)
                    {
                        if (startLine > lines.Length || endLine > lines.Length || startLine > endLine)
                            return $"[Error] Invalid line range: {startLine}-{endLine} for file {Path.GetFileName(file)}";
                        var selected = new List<string>();
                        for (int l = startLine; l <= endLine; l++)
                        {
                            var selectedLine = lines[l - 1];
                            var trimmed = selectedLine.Trim();
                            if (trimmed.StartsWith("namespace "))
                                continue;
                            var cleaned = selectedLine.Replace("{", "").Replace("}", "").Trim();
                            if (!string.IsNullOrWhiteSpace(cleaned))
                                selected.Add(cleaned);
                        }
                        return string.Join("\n", selected);
                    }
                    return string.Join("\n", lines);
                }
            }
        }
        return $"[Error] Class '{className}' not found";
    }
}