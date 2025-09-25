# TradeForge Control Tower (Next.js)

## 概览
全新后台体验基于 Next.js App Router + Ant Design Pro：
- `src/layouts/AntdApp.tsx`：注入 Ant Design 主题与 React Query。
- `src/layouts/ProShell.tsx`：统一导航、页头、KPI 模块与内容栅格。
- `app/dashboard/page.tsx`：TradeForge Control Tower 仪表盘示例，集成 KPI 卡片、趋势图、爆款榜单、品类占比。

该工程在 Vite SPA(`../tradeforge-suite`) 并行迭代，切换工作将在完成数据接入、交互与测试后执行。

## 本地开发
```bash
npm install
npm run dev
```
> 需要 Node.js 18+，并在 `.env.local` 中设置 `NEXT_PUBLIC_API_BASE` 后续对接后端。

## TODO
- 接入真实 API 数据与 React Query 缓存策略。
- 迁移 Amazon、Projects、Favorites 等页面。
- 引入命令面板、指标 Builder 以及实时告警提示。
- 配置 Storybook、Playwright、Lighthouse CI 作为新架构质量保障。
