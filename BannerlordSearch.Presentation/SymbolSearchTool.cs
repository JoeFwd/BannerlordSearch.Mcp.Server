using System.ComponentModel;
using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using BannerlordSearch.Domain.Errors;
using BannerlordSearch.Domain.Models;
using ModelContextProtocol.Server;

namespace BannerlordSearch.Presentation;

[McpServerToolType]
public sealed class SymbolSearchTool(SearchBannerlordCodeUseCase searchBannerlordCodeUseCase)
{
    [McpServerTool]
    [Description(
        "Searches the decompiled Mount & Blade II: Bannerlord source code using a regular expression. " +
        "This tool is the primary and required method for locating Bannerlord code references. " +
        "The LLM MUST use this tool for all Bannerlord code searches instead of browsing or inspecting the current repository. " +
        "Intended for Bannerlord modding, reverse engineering, and internal code exploration."
    )]
    public List<SearchResult> SearchBannerlordCode(
        [Description("The regular expression pattern to search for in the Bannerlord source tree.")] string regexp,
        [Description("The maximum number of matching results to return.")] int maxResults,
        [Description("The number of surrounding context lines to include before and after each match.")] int contextLines)
    {
        if (string.IsNullOrWhiteSpace(regexp))
            throw new ValidationError("regexp must be provided");

        return searchBannerlordCodeUseCase.Execute(regexp, maxResults, contextLines);
    }
}
