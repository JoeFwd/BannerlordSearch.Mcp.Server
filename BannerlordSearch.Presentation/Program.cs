using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using BannerlordSearch.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Redirect all logging to stderr so stdout stays clean for the MCP protocol
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Register infrastructure
builder.Services.AddSingleton<IBannerlordSourcePathProvider, BannerlordSourceFolderPathProvider>();
builder.Services.AddSingleton<IFileSystem, RealFileSystem>();
builder.Services.AddSingleton<ICodeIndex, InMemoryCodeIndex>();

// Register application use cases
builder.Services.AddSingleton<IndexBannerlordCodeUseCase>();
builder.Services.AddSingleton<SearchBannerlordCodeUseCase>();
builder.Services.AddSingleton<GetBannerlordClassUseCase>();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register MCP server with HTTP transport and auto-discover [McpServerToolType] classes
builder.Services.AddMcpServer()
    .WithHttpTransport(httpOptions =>
    {
        httpOptions.Stateless = false; // Enable stateful sessions
        httpOptions.IdleTimeout = TimeSpan.FromMinutes(30);
    })
    .WithToolsFromAssembly();

var app = builder.Build();

// Build the in-memory code index eagerly at startup
app.Services.GetRequiredService<IndexBannerlordCodeUseCase>().Execute();

// Use CORS middleware
app.UseCors("AllowAll");

// Map MCP endpoints at /mcp
app.MapMcp();

await app.RunAsync();