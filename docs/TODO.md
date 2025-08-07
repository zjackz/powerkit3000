# 项目待办事项 (TODO List)

## 1. 后端 API 服务 (AmazonTrends.WebApp)

*   **核心功能实现**
    *   实现 BSR 榜单追踪器的数据抓取逻辑 (ScrapingService)。
    *   实现趋势发现与分析逻辑 (AnalysisService)。
    *   实现每日报告与提醒功能。
*   **API 接口开发**
    *   为 BSR 榜单数据提供查询 API。
    *   为趋势分析结果提供查询 API。
    *   为单个产品历史曲线数据提供查询 API。
    *   实现用户自定义关键词下的产品动态 API。
*   **Hangfire 集成与任务调度**
    *   配置 Hangfire 使用 PostgreSQL 作为持久化存储。
    *   定义并调度数据采集作业 (Scraping Job)。
    *   定义并调度数据分析作业 (Analysis Job)。
    *   定义并调度每日报告生成与推送作业。
*   **数据库集成 (AmazonTrends.Data)**
    *   定义所有数据库实体模型 (Category, Product, ProductDataPoint, DataCollectionRun)。
    *   配置 AppDbContext。
    *   完成 EF Core 迁移和数据库初始化。
*   **Web 抓取模块**
    *   实现基于 HttpClient 和 HtmlAgilityPack 的基础抓取逻辑。
    *   考虑并实现反爬虫机制 (代理、User-Agent 轮换、请求节流)。
    *   评估是否需要集成 Puppeteer Sharp 以应对复杂场景。
*   **缓存机制 (可选)**
    *   评估是否需要引入 Redis 进行 API 数据缓存。
    *   如果引入，实现 Redis 缓存逻辑。
*   **错误处理与日志**
    *   实现全局异常处理。
    *   配置 Logger with File Output 到 logs/ 目录下。

## 2. 前端管理脚手架 (AmazonTrends.AdminUI)

*   **基础框架搭建**
    *   确认 Next.js v15.4, React v19, Tailwind CSS v4 的环境配置。
    *   完成 App Router 基础结构搭建。
*   **数据可视化仪表盘**
    *   设计并实现核心指标概览界面。
    *   实现可筛选/排序的数据表格，展示 BSR 榜单数据。
    *   实现单个产品历史曲线图 (使用 Recharts)。
*   **用户交互与管理**
    *   实现用户选择关心产品大类的功能。
    *   实现用户自定义关键词的功能。
*   **数据请求与状态管理**
    *   集成 SWR 进行数据获取和缓存。
*   **UI/UX 优化**
    *   确保界面清晰、直观、美观。

## 3. 部署与运维

*   **Docker 化**
    *   为 AmazonTrends.WebApp 编写 Dockerfile。
    *   为 AmazonTrends.AdminUI 编写 Dockerfile。
    *   编写 docker-compose.yml 文件，统一编排所有服务 (后端、前端、PostgreSQL, Redis)。
*   **脚本化**
    *   在 scripts/ 目录下创建 build.sh (用于构建 Docker 镜像)。
    *   在 scripts/ 目录下创建 run-dev.sh (用于启动开发环境)。
    *   在 scripts/ 目录下创建 run-prod.sh (用于启动生产环境)。
    *   确保所有 Run & Debug 操作都通过 scripts/ 下的 .sh 脚本进行。
*   **环境配置**
    *   完善 appsettings.json 和环境变量配置。

## 4. 代码质量与规范

*   **遵循编码规范**
    *   C#/.NET 和 TypeScript 编码规范。
    *   C# 文件代码行数不超过 400 行。
    *   TSX/TS 文件代码行数不超过 300 行。
    *   每层文件夹中的文件不超过 8 个。
*   **文档与注释**
    *   所有公开 API 端点和核心方法添加完整 XML 或 JSDoc 注释。
*   **测试**
    *   为核心业务逻辑和关键 React 组件编写单元测试。

## 5. 风险管理

*   持续监控亚马逊反爬虫机制，并及时调整抓取策略。
*   建立数据准确性监控和告警机制。
*   关注系统性能，为未来扩展做准备。