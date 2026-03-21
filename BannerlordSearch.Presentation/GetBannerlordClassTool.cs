using System.ComponentModel;
using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain.Errors;
using ModelContextProtocol.Server;

namespace BannerlordSearch.Presentation;

[McpServerToolType]
public sealed class GetBannerlordClassTool(GetBannerlordClassUseCase getBannerlordClassUseCase)
{
    [McpServerTool]
    [Description(
        "Retrieves the full definition of a Bannerlord class by its name. " +
        "This tool is used to examine the complete implementation of a Bannerlord class, including all its methods, properties, and fields. " +
        "The LLM MUST use this tool for examining Bannerlord class definitions instead of guessing or assuming implementation details. " +
        "Intended for Bannerlord modding, reverse engineering, and internal code exploration."
    )]
    public string GetBannerlordClassDefinition(
        [Description("The full name of the Bannerlord class to retrieve.")] string className)
    {
        if (string.IsNullOrWhiteSpace(className))
            throw new ValidationError("className must be provided");

        return getBannerlordClassUseCase.Execute(className, 0, 0);
    }
}
