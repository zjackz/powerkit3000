// lib/swr.ts
import { SWRConfig } from 'swr';

// 定义一个全局的 fetcher 函数，用于 SWR
// 这个函数接收一个 URL，然后使用原生的 fetch API 来获取数据
export const fetcher = (url: string) => fetch(url).then((res) => res.json());

// 你可以在这里创建一个 SWRConfigWrapper 组件，以便在 _app.tsx 中全局应用配置
// 但为了简单起见，我们暂时只导出 fetcher，在需要的地方单独使用它。
