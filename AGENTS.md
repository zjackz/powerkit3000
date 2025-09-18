# Repository Guidelines

## Project Structure & Module Organization
- `backend/PowerKit3000.ConsoleApp` is the Spectre.Console CLI; commands live in `ConsoleApp/Commands`.
- `backend/PowerKit3000.Core` hosts domain services (e.g., `KickstarterDataImportService`) shared by commands and tests.
- `backend/PowerKit3000.Data` defines EF Core models/migrations; always run schema tooling here.
- `backend/PowerKit3000.ConsoleApp.Tests` contains NUnit suites; treat as reference for DI-friendly patterns.
- `scripts/` bundles launch helpers, and `docs/` records product requirements that must be updated alongside code.

## Build, Test, and Development Commands
- `dotnet restore powerkit3000.sln && dotnet build powerkit3000.sln` prepares the backend with nullable warnings on.
- `scripts/run-console.sh -- counts` cleans, builds, and executes a command; tack on arguments after `--`.
- `dotnet run --project backend/PowerKit3000.ConsoleApp/PowerKit3000.ConsoleApp.csproj query --state successful` is ideal for quick smoke checks.
- `dotnet test backend/PowerKit3000.ConsoleApp.Tests/PowerKit3000.ConsoleApp.Tests.csproj` runs NUnit suites; append `--collect:"XPlat Code Coverage"` when measuring coverage.
- `dotnet ef migrations add <Name>` (from `.config/dotnet-tools.json`) must be run inside `backend/PowerKit3000.Data`.

## Coding Style & Naming Conventions
- Use four-space indentation, file-scoped namespaces when practical, and `async` members returning `Task`/`ValueTask`.
- Keep namespaces, classes, and public members PascalCase; locals, parameters, and services camelCase (prefix injected fields with `_` only when needed).
- Prefer dependency injection over singletons, isolate domain logic in Core, and keep CLI layers focused on IO and Spectre markup.
- Extend localized strings and command output consistently; avoid hard-coded file system paths.

## Testing Guidelines
- Structure test files as `<Feature>Tests.cs` with `[Test]` methods reading `Should_*` or `When_*`.
- Follow the in-memory database setup in `backend/PowerKit3000.ConsoleApp.Tests/ConsoleAppTests.cs`, loading fixtures from `backend/PowerKit3000.ConsoleApp/data`.
- Reset `StringWriter` captures between assertions and assert on both UX text and data effects.
- Target meaningful coverage for new branches and failure handling before requesting review.

## Commit & Pull Request Guidelines
- Use imperative, typed commit subjects such as `feat: add split command summary`; expand details in the body when schema or config changes occur.
- Reference related issues, call out required follow-up commands (e.g., `dotnet ef database update`), and include screenshots for CLI UX tweaks.
- PRs should summarize impacted modules, list manual verification steps (`dotnet test`, `scripts/run-console.sh counts`), and note any updates under `docs/`.

## Configuration Tips
- The CLI defaults to Postgres at `192.168.0.124`; update `Program.cs` or inject `UseNpgsql` options via environment configuration before committing.
- Keep credentials in user secrets or environment variables and document new setup steps for teammates.
