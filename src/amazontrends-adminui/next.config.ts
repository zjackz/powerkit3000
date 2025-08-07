import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        // 目标 API 的 URL，假设 .NET 后端运行在 http://localhost:5001
        // 如果后端使用 HTTPS 或不同端口，需要相应修改
        destination: 'http://localhost:5001/api/:path*',
      },
    ];
  },
};

export default nextConfig;
