# TradeForge Suite (Frontend)

TradeForge Suite 是 PowerKit3000 团队的跨境电商分析工具集合的前端入口。本项目基于 React + Vite + TypeScript 构建，配套 Ant Design 生态与 React Query，以便快速迭代业务模块。

## 功能概览（MVP）
- 仪表盘概览：展示导入的 Kickstarter 项目核心指标，可按时间/国家/品类实时筛选。
- 类目/国家洞察：自动计算成功率、平均筹资、热力榜单（内置可视化图表），支持同样的筛选条件联动。
- Top 项目榜单：列出达成率最高的项目，支持点击导出数据。
- 项目浏览页：支持状态/国家/品类/达成率筛选，分页展示结果，可导出 CSV（自动切换 API / Mock 数据）。
- 侧边导航：预留项目浏览、实验室等模块入口。
- 全局主题：深色导航、品牌主色 `#1f6feb`。

## 技术栈
- React 18 + TypeScript
- Vite 5 构建工具
- Ant Design 5 + Ant Design Icons
- React Router 6（模块化路由）
- @tanstack/react-query 负责数据请求、缓存与错误回退
- @ant-design/plots 可视化类目/国家成功率

## 本地开发
> 当前仓库启用了离线脚手架，如需安装依赖请确保可以访问 npm registry。

```bash
cd frontend/tradeforge-suite
npm install
npm run dev
```

构建生产包：
```bash
npm run build
```

若需指向真实 API，可在根目录创建 `.env.local` 并设置：

```bash
cp .env.local.example .env.local
# 然后根据实际地址修改：
VITE_API_BASE_URL=http://localhost:5200
```

当 API 不可用或网络受限时，应用会优雅退回至 `src/mocks/projects.ts` 中的样例数据，并在控制台给出提示；筛选依旧可以演练逻辑。

## 目录结构
```
frontend/tradeforge-suite
├── src
│   ├── components/layout      # Header / Sidebar 等全局布局
│   ├── pages                  # 页面模块（dashboard、projects 等）
│   ├── providers              # React Query / Router Provider 封装
│   ├── styles                 # 全局样式
│   └── main.tsx               # 应用入口
├── index.html
├── package.json
└── vite.config.ts
```

## 下一步规划
1. 连接生产数据库/凭据，完善 `/projects`、`/projects/summary` 等 API 的鉴权策略。
2. 扩展仪表盘图表（趋势、榜单），并实现收藏夹、智能推荐等模块。
3. 接入权限控制（如公司 SSO），完善部署脚本与监控、日志追踪。
4. 编写 Storybook / 自动化测试，保障组件质量。

---
如需更多背景，可阅读 `docs/前端界面规划.md`。
