using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.Configuration;
using BannerlordSearch.Application.Ports.IO;
using BannerlordSearch.Application.Ports.Repositories;
using BannerlordSearch.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Redirect all logging to stderr so stdout stays clean for the MCP protocol
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton<IBannerlordSourcePathProvider, BannerlordSourceFolderPathProvider>();
builder.Services.AddSingleton<IFileSystem, RealFileSystem>();
builder.Services.AddSingleton<ICodeIndex, InMemoryCodeIndex>();

builder.Services.AddSingleton<IndexBannerlordCodeUseCase>();
builder.Services.AddSingleton<SearchBannerlordCodeUseCase>();
builder.Services.AddSingleton<GetBannerlordClassUseCase>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddMcpServer()
    .WithHttpTransport(httpOptions =>
    {
        httpOptions.Stateless = true;
        httpOptions.IdleTimeout = TimeSpan.FromMinutes(30);
    })
    .WithToolsFromAssembly();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Starting BannerlordSearch MCP server");

app.Services.GetRequiredService<IndexBannerlordCodeUseCase>().Execute();

logger.LogInformation("MCP server ready");

app.UseCors("AllowAll");
app.MapMcp();

await app.RunAsync();