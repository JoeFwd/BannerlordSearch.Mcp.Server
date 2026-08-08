# Changelog

All notable changes to this project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and version numbers follow [Semantic Versioning](https://semver.org/).
## [0.2.0] - 2026-08-08

### CI/CD

- Automate semantic versioning and changelog generation (bdc27a7)


### Features

- *(get-class)* Resolve Bannerlord classes by simple name (4425378)

## [0.1.1] - 2026-08-01

### Bug Fixes

- *(search)* Remove total match summary line and return empty results for zero/negative max results (0ab6fc4)

- *(infrastructure)* Improve BANNERLORD_SOURCE_PATH error message and add tests (53af430)

- *(presentation)* Switch MCP transport to stateless mode and remove stale comments (cabee85)


### CI/CD

- Add GitVersion config and GitHub Actions for CI and NuGet publishing (980c246)

- Enable .NET tool packaging, set tool command name and version (face9ea)

- Add PackageId to BannerlordSearch.Mcp.Server project (7126135)

- Publish whne pushing to main (8a3a4b8)

- Upgrade GitVersion.yml to v6 (1b40ed7)

- Debug gitversion (695bd35)

- Add regex for hotfix branch naming in GitVersion.yml (52acc05)

- Drop Set version variables step and use GitVersion output directly for PackageVersion (2bd73a3)

- Add debug step to print GitVersion outputs in publish workflow (57141f1)

- Use GitVersion_SemVer for PackageVersion in publish workflow (6ad9049)

- Allow manual publish workflow runs (3d07b3e)

- Tag published package releases (6b96bbd)

- Capture GitVersion package output (de7fd64)

- Use supported GitVersion SemVer variable (7c293b1)


### Features

- Add initial bannerlord code search MCP server (35476ce)

- Feat: add in‑memory code index
=> ICodeIndex, IndexedFile
=> integrate it into GetBannerlordClassUseCase; include domain tests, infrastructure implementation, and update presentation project settings. (1bdea2a)

- *(search)* Make pattern matching case-insensitive with new test coverage (87e9598)

- Feat: add default values for MaxResults and ContextLines search arguments
-  add integration tests for default search parameter behavior
-  default search tool parameters from SearchDefaults (32a8366)


### Miscellaneous

- *(presentation)* Remove unused BannerlordSearch.Domain imports (4100339)


### Refactor

- Eliminate root path injection in use cases and simplify tool initialization (e10775a)

- Replace domain references with application ports across modules (ecd1a54)

- Rename exception classes to improve naming consistency (9f6795a)

- *(logging)* Inject ILogger into InMemoryCodeIndex, use cases and program, add NullLogger usage in tests and csproj updates. (3f6c778)

- *(namespaces)* Relocate ports to IO, Configuration, Repositories and move domain models/errors to dedicated sub‑namespaces; update all imports and references accordingly (7b7ac1a)


### Testing

- *(refactor)* Add shared TestHelper, switch to MockBehavior.Strict, simplify tool initialization and assertions across unit tests. Use TestHelper.MakeFile in place of local helpers and verify exact EnumerateFiles parameters. (284ee67)

- Clean up test directory before creation to avoid leftover files from previous runs (1ac3475)


