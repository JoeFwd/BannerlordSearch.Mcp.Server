using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using BannerlordSearch.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Register domain interfaces
builder.Services.AddSingleton<IBannerlordSourcePathProvider, BannerlordSourceFolderPathProvider>();
builder.Services.AddSingleton<IFileSystem, RealFileSystem>();
builder.Services.AddSingleton<ISymbolSearchRepository, SymbolFileRepository>();

// Register application use cases
builder.Services.AddSingleton<SearchBannerlordCodeUseCase>();
builder.Services.AddSingleton<GetBannerlordClassUseCase>();

// Register presentation layer tools
builder.Services.AddMcpServer();

var host = builder.Build();
await host.RunAsync();