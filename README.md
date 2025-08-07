# 亚马逊热销品发现系统

本项目旨在开发一个“亚马逊趋势数据仪表盘”，通过自动化数据采集、分析与可视化，帮助电商选品专家和卖家高效发现亚马逊美国站的热销产品和新兴趋势。

## 核心功能

*   **BSR 榜单追踪器：** 每日自动抓取并记录指定类目的 BSR 榜单（Top 100）数据，包括“Best Sellers”、“New Releases”、“Movers & Shakers”。
*   **趋势发现与分析：** 通过对比历史数据，自动识别和高亮显示有潜力的产品，如排名飙升、新晋上榜、持续霸榜等。
*   **数据可视化仪表盘：** 提供清晰、直观的界面，汇总所有关键信息，支持数据筛选、排序和单个产品历史曲线查看。
*   **每日报告与提醒：** 每日数据更新完成后，自动生成简报并通过邮件或Discord/Slack消息推送。

## 技术方案

本项目采用 **模块化单体 (Modular Monolith)** 架构，核心功能承载于一个统一的 **ASP.NET Core 应用**中，但在代码层面严格保持模块化划分。

### 后端技术

*   **运行时/框架:** .NET 8, ASP.NET Core 8 Web API
*   **API 文档:** Swashbuckle (Swagger)
*   **数据访问:** Entity Framework Core 8 (EF Core)
*   **Web 抓取:** `HttpClient`, `HtmlAgilityPack` (备选 `Puppeteer Sharp`)
*   **后台任务与调度:** `Hangfire` (使用 PostgreSQL 作为持久化存储)
*   **数据库:** PostgreSQL
*   **缓存 (可选):** Redis
*   **容器化:** Docker

### 前端技术 (Admin 脚手架)

*   **框架:** Next.js v15.4 (App Router)
*   **UI 库:** React v19
*   **样式:** Tailwind CSS v4
*   **数据请求:** SWR
*   **图表库:** Recharts

## 模块设计

解决方案划分为以下几个逻辑清晰的项目：

*   **`AmazonTrends.Data`**: 数据访问层，定义数据库模型和 `DbContext`。
*   **`AmazonTrends.Core`**: 核心业务逻辑层，包含数据抓取、趋势分析等服务。
*   **`AmazonTrends.WebApp`**: 应用层，系统入口，负责配置服务、定义 API 控制器和 Hangfire 作业。
*   **`AmazonTrends.AdminUI`**: 前端层，独立的 Next.js 应用，提供可视化仪表盘和后台管理界面。

## 部署方案

*   **容器化:** 为 `AmazonTrends.WebApp` 和 `AmazonTrends.AdminUI` 创建 `Dockerfile`，使用 `docker-compose.yml` 统一编排。
*   **脚本化:** 在 `scripts/` 文件夹下创建构建和启动脚本。
*   **环境配置:** 使用 `appsettings.json` 和环境变量管理配置。

## 风险与挑战

*   **反爬虫机制：** 亚马逊网站结构可能变化，反爬虫技术会升级。
*   **数据准确性：** 亚马逊页面结构可能变更，导致解析失败。
*   **系统性能：** 每日处理大量数据，对数据库和服务器的性能有一定要求。