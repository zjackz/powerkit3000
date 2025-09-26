# Repository Guidelines

## Project Structure & Module Organization
- `backend/pk.consoleapp` is the Spectre.Console CLI; commands live in `Commands`.
- `backend/pk.core` hosts domain services (e.g., `KickstarterDataImportService`) shared by commands and tests.
- `backend/pk.data` defines EF Core models/migrations; always run schema tooling here.
- `backend/pk.consoleapp.tests` contains NUnit suites; treat as reference for DI-friendly patterns.
- `scripts/` bundles launch helpers, and `docs/` records product requirements that must be updated alongside code.

## Build, Test, and Development Commands
- `dotnet restore pk3000.sln && dotnet build pk3000.sln` prepares the backend with nullable warnings on.
- `scripts/console.sh -- counts` cleans, builds, and executes a command; tack on arguments after `--`.
- `dotnet run --project backend/pk.consoleapp/pk.consoleapp.csproj query --state successful` is ideal for quick smoke checks.
- `dotnet test backend/pk.consoleapp.tests/pk.consoleapp.tests.csproj` runs NUnit suites; append `--collect:"XPlat Code Coverage"` when measuring coverage.
- `dotnet ef migrations add <Name>` (from `.config/dotnet-tools.json`) must be run inside `backend/pk.data`.

## Coding Style & Naming Conventions
- Use four-space indentation, file-scoped namespaces when practical, and `async` members returning `Task`/`ValueTask`.
- Keep namespaces, classes, and public members PascalCase; locals, parameters, and services camelCase (prefix injected fields with `_` only when needed).
- Prefer dependency injection over singletons, isolate domain logic in Core, and keep CLI layers focused on IO and Spectre markup.
- Extend localized strings and command output consistently; avoid hard-coded file system paths.

## Testing Guidelines
- Structure test files as `<Feature>Tests.cs` with `[Test]` methods reading `Should_*` or `When_*`.
- Follow the in-memory database setup in `backend/pk.consoleapp.tests/consoleapptests.cs`, loading fixtures from `backend/pk.consoleapp/data`.
- Reset `StringWriter` captures between assertions and assert on both UX text and data effects.
- Target meaningful coverage for new branches and failure handling before requesting review.

## Commit & Pull Request Guidelines
- Use imperative, typed commit subjects such as `feat: add split command summary`; expand details in the body when schema or config changes occur.
- Reference related issues, call out required follow-up commands (e.g., `dotnet ef database update`), and include screenshots for CLI UX tweaks.
- PRs should summarize impacted modules, list manual verification steps (`dotnet test`, `scripts/console.sh counts`), and note any updates under `docs/`.

## Configuration Tips
- CLI 默认使用 `appsettings.{ENVIRONMENT}.json` 中的连接串，本地 `Home` 配置指向 `localhost`，公司环境请设置 `DOTNET_ENVIRONMENT=Company`（或 `ASPNETCORE_ENVIRONMENT=Company`），使用 `192.168.0.124`。
- Keep credentials in user secrets or environment variables and document new setup steps for teammates.

## Collaboration Workflow
- **需求阶段**：产品经理一次性交付背景、目标、验收标准与相关文档位置（例如 `docs/` 条目或页面），同时说明约束、优先级与期望时间窗。
- **方案阶段**：开发代理在阅读现有代码与文档后给出实现思路，必要时更新 `docs/技术方案.md` 或新建设计草案；未获确认前不动代码。
- **执行阶段**：获得确认后按方案实现代码与测试，保持 CLI/API/前端/文档同步更新，并在 README 或需求文档中补充配置与使用说明。
- **验证阶段**：执行约定的命令（如 `dotnet test`、`scripts/console.sh -- translate`）并回传结果；若因环境受限未能运行，需在交付说明里标注。
- **文档回填**：每次迭代结束更新 `docs/需求文档.md`、`docs/工程进度.md`、`AGENTS.md` 等，以记录决策、风险和后续动作，方便后续协作者快速对齐。
