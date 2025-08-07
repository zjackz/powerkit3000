// src/app/components/Dashboard.tsx
'use client';

import useSWR from 'swr';
import { fetcher } from '../../lib/swr';

// 定义后端返回的产品数据类型
interface Product {
  id: string;
  title: string;
  categoryName: string;
  listingDate: string;
  latestRank: number | null;
  latestPrice: number | null;
  latestRating: number | null;
  latestReviewsCount: number | null;
  lastUpdated: string | null;
}

const Dashboard = () => {
  // 注意：这里的 URL 是相对路径，Next.js 开发服务器会代理到后端 API
  // 为了实现这一点，我们需要在 next.config.ts 中配置 rewrites
  const { data, error, isLoading } = useSWR<Product[]>('/api/products', fetcher);

  if (isLoading) {
    return <div className="text-center p-4">Loading data...</div>;
  }

  if (error) {
    return <div className="text-center p-4 text-red-500">Failed to load data. Please check the API connection.</div>;
  }

  if (!data || data.length === 0) {
    return <div className="text-center p-4">No product data found.</div>;
  }

  return (
    <div className="p-4">
      <h2 className="text-xl font-semibold mb-4">Product List</h2>
      {/* 临时用 JSON 格式展示数据，验证通路 */}
      <pre className="bg-gray-100 p-4 rounded-lg text-sm overflow-x-auto">
        {JSON.stringify(data, null, 2)}
      </pre>
    </div>
  );
};

export default Dashboard;
