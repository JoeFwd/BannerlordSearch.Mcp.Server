# BannerlordSearchMcpServer

A Model Context Protocol (MCP) server that provides code search functionality for Bannerlord modding projects. This tool enables developers to search through Bannerlord source code using regular expressions and retrieve class definitions and code snippets.

## Project Structure

This project follows a clean architecture pattern with the following layers:

```
BannerlordSearchMcpServer.sln
├── BannerlordSearch.Domain/          # Domain Layer (core business logic)
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Interfaces/
│   ├── Exceptions/
│   └── DomainServices/
├── BannerlordSearch.Application/     # Application Layer (use cases)
│   ├── UseCases/
│   ├── DTOs/
│   ├── Services/
│   └── Ports/ (interfaces to infrastructure)
├── BannerlordSearch.Infrastructure/ # Infrastructure Layer (external concerns)
│   ├── FileSystem/
│   ├── Configuration/
│   ├── Caching/
│   └── Repositories/
├── BannerlordSearch.Presentation/    # Presentation Layer (delivery mechanisms)
│   ├── MCP/
│   ├── WebAPI/
│   └── CLI/
├── BannerlordSearch.Tests/           # Test projects per layer
│   ├── Domain.Tests/
│   ├── Application.Tests/
│   ├── Infrastructure.Tests/
│   └── Presentation.Tests/
└── BannerlordSearch.sln
```

## Key Components

### Domain Layer
Contains core business logic and entities that are independent of any framework or technology:
- `SearchResult` entity
- Domain interfaces for external dependencies
- Domain exceptions and validation logic

### Application Layer
Contains application-specific business rules and use cases:
- `GetBannerlordClassUseCase` - Retrieves Bannerlord class definitions
- `SearchBannerlordCodeUseCase` - Searches Bannerlord code using regular expressions
- Application services that coordinate domain objects

### Infrastructure Layer
Contains implementations of interfaces defined in Domain and Application layers:
- `SymbolFileRepository` - File system access and caching
- `RealFileSystem` - File system abstraction
- `BannerlordSourceFolderPathProvider` - Configuration provider

### Presentation Layer
Contains delivery mechanisms and user interfaces:
- `SymbolSearchTool` - MCP tool for searching code
- `GetBannerlordClassTool` - MCP tool for retrieving class definitions

## Testing Strategy

Each layer has its own test project to ensure comprehensive coverage:

1. **Domain Tests** (`BannerlordSearch.Domain.Tests`) - Tests domain entities and logic
2. **Application Tests** (`BannerlordSearch.Application.Tests`) - Tests business logic and use cases
3. **Infrastructure Tests** (`BannerlordSearch.Infrastructure.Tests`) - Tests file system operations and repositories
4. **Presentation Tests** (`BannerlordSearch.Presentation.Tests`) - Tests MCP tools and presentation logic

## Getting Started

### Prerequisites
- .NET 6.0 SDK or later
- Visual Studio 2022 or JetBrains Rider

### Building the Project
```bash
dotnet build
```

### Running Tests
To run all tests:
```bash
dotnet test
```

To run tests for a specific project:
```bash
dotnet test BannerlordSearch.Application.Tests
dotnet test BannerlordSearch.Infrastructure.Tests
dotnet test BannerlordSearch.Presentation.Tests
dotnet test BannerlordSearchMcpServer.Tests
```

To run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Usage

This project is designed to run as an MCP server. You can configure your IDE or tooling to connect to this server to leverage the Bannerlord code search functionality.

## Versioning & Changelog

This project follows [Semantic Versioning](https://semver.org/). The version number is derived automatically from commit history using [GitVersion](https://gitversion.net/), based on [Conventional Commits](https://www.conventionalcommits.org/) prefixes:

- `fix:` bumps the **patch** version
- `feat:` bumps the **minor** version
- `feat!:`, `fix!:`, or a `BREAKING CHANGE` footer bumps the **major** version

On every push to `main`, the CI pipeline determines the next version, publishes the NuGet package, updates [CHANGELOG.md](CHANGELOG.md) (generated with [git-cliff](https://git-cliff.org/) from the commit history, grouped by commit type), tags the release, and publishes a GitHub Release with the generated notes.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Thanks to the Bannerlord modding community for inspiration
- Uses .NET and Model Context Protocol for robust functionality