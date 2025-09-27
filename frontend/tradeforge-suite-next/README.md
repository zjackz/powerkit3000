# MISSION X (Next.js)

## 概览
全新后台体验基于 Next.js App Router + Ant Design Pro：
- `src/layouts/AntdApp.tsx`：注入 Ant Design 主题与 React Query。
- `src/layouts/ProShell.tsx`：统一导航、页头、KPI 模块与内容栅格。
- `app/dashboard/page.tsx`：MISSION X 仪表盘示例，集成 KPI 卡片、趋势图、爆款榜单、品类占比。
- 导航按业务拆分为三大模块：Kickstarter 分析、亚马逊榜单、亚马逊运营仪表盘。

该工程现已成为唯一的 MISSION X 前端入口，负责全部运营与分析看板体验。

## 本地开发
```bash
npm install
npm run dev
```
> 需要 Node.js 18+，并在 `.env.local` 中设置 `NEXT_PUBLIC_API_BASE` 后续对接后端。

## TODO
- 完成 Workspace 自定义看板与更丰富的 KPI 组件。
- 扩展 Amazon 趋势与调度可视化能力。
- 引入命令面板、指标 Builder 以及实时告警提示。
- 配置 Storybook、Playwright、Lighthouse CI 作为新架构质量保障。
