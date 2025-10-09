# PowerKit3000 项目总览

PowerKit3000 聚焦于打造面向跨境团队的 Kickstarter 数据分析工具链，包括数据导入 CLI、共享领域服务、分析 API 以及 React 前端。该仓库已提供端到端的最小可运行版本（MVP），并在持续迭代中扩展可视化洞察与协作能力。

## 核心能力
- 数据导入：通过 `import`、`split` 等命令把 JSON/JSONL 数据批量写入 Postgres，并处理创作者、分类、地区等实体去重。
- 数据查询：`backend/pk.core` 暴露查询服务，支持复用到 CLI 与 API，涵盖分页过滤、统计汇总等场景。
- 分析 API：`backend/pk.api` 提供项目列表、筛选器、统计摘要及多维分析端点，兼容 Swagger 文档。
- 前端体验：`frontend/naive-ui-admin` 基于 Vue 3 + Vite + Naive UI Admin 模板构建运营驾驶舱，实时联通后端 API 并承载 Kickstarter/Amazon 分析页面。
- 测试保障：`backend/pk.consoleapp.Tests` 使用 NUnit 验证导入、查询关键流程，便于持续交付。
- 多语言扩展：CLI 提供 `translate` 命令批量生成项目名称/简介的中文字段，API 与前端自动读取并展示。

## 快速上手
1. **安装依赖**
   ```bash
   dotnet restore pk3000.sln
   pnpm install --dir frontend/naive-ui-admin
   ```
2. **构建与测试**
   ```bash
   dotnet build pk3000.sln
   dotnet test backend/pk.consoleapp.Tests/pk.consoleapp.Tests.csproj
   ```
3. **运行 CLI**
   ```bash
   scripts/console.sh -- import backend/pk.consoleapp/data/sample2.json
   scripts/console.sh -- counts
   scripts/console.sh -- query --state successful --country US
   scripts/console.sh -- translate --max-projects 50
   ```
4. **启动 API（默认端口 5200）**
   ```bash
   dotnet run --project backend/pk.api/pk.api.csproj
   ```
5. **启动前端（Vite Dev Server，默认 5173）**
   ```bash
   pnpm run dev --dir frontend/naive-ui-admin
   ```

> **数据库连接**：后端默认读取 `ConnectionStrings:AppDb`（参见 `backend/pk.consoleapp/appsettings.json`），可通过环境变量 `ConnectionStrings__AppDb` 覆盖；执行迁移需进入 `backend/pk.data` 目录运行 `dotnet ef database update`。

## 翻译配置
- `backend/pk.consoleapp/appsettings.json` 提供 `Translation` 节点，可配置默认 Provider（`noop`/`openai`/`gemini`/`deepseek` 等）、目标语言、批量大小与重试次数。
- 针对具体 Provider 在 `Translation:Providers:<name>` 下填入 `ApiKey`、`ApiBase`、`Model` 等参数；也可通过环境变量覆盖，例如：`Translation__Providers__openai__ApiKey`。
- CLI `translate` 命令支持 `--batch-size`、`--max-projects`、`--dry-run`，便于分批翻译与成本控制。
- API/前端会优先展示 `NameCn`/`BlurbCn` 字段，缺失时自动回退英文内容。

## 项目结构
- `backend/pk.consoleapp`：Spectre.Console 命令行入口，包含 `ImportCommand`, `QueryCommand`, `CountsCommand`, `SplitCommand`, `ClearDbCommand` 等。
- `backend/pk.core`：共享领域逻辑（例如 `KickstarterDataImportService`, `KickstarterProjectQueryService`）。
- `backend/pk.data`：EF Core 模型与迁移，`AppDbContext` 定义持久化实体。
- `backend/pk.api`：.NET Minimal API，向前端输出查询与分析结果。
- `frontend/naive-ui-admin`：Vue 3 + Naive UI 管理台，聚合 Kickstarter 分析、亚马逊榜单与运营模块，已替换历史 Tradeforge 应用。
- `scripts/`：开发辅助脚本，如 `console.sh` 一键执行 CLI。
- `docs/`：需求、规划、技术方案与进度记录。

## 协作指引
- 产品/需求更新优先同步到 `docs/需求文档.md` 与 `docs/产品规划.md`，并在进度表中标记影响模块。
- 研发在提交功能时更新 `docs/工程进度.md`，补充落地状态、风险与下一步操作。
- 涉及数据库/配置的改动需同步到 `docs/技术方案.md` 及 README 快速上手段落，确保新同学能按文档完成部署。
- 建议采用“需求说明 → 技术方案 → 实现 → 验证/文档更新”闭环，减少跨职能信息缺口。

如需更多上下文，可从 `docs/产品规划.md` 和 `docs/市场运营规划.md` 了解愿景与用户场景，或查看测试用例掌握核心行为。
