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

// 定义后端返回的核心指标数据类型
interface CoreMetrics {
  dataCollectionRunId: number;
  analysisTime: string;
  totalNewEntries: number;
  totalRankSurges: number;
  totalConsistentPerformers: number;
  totalProductsAnalyzed: number;
}

const Dashboard = () => {
  // 获取产品数据
  const { data: products, error: productsError, isLoading: productsLoading } = useSWR<Product[]>('/api/products', fetcher);

  // 获取核心指标数据
  const { data: metrics, error: metricsError, isLoading: metricsLoading } = useSWR<CoreMetrics>('/api/products/metrics', fetcher);

  if (productsLoading || metricsLoading) {
    return <div className="text-center p-4">Loading data...</div>;
  }

  if (productsError || metricsError) {
    return <div className="text-center p-4 text-red-500">Failed to load data. Please check the API connection.</div>;
  }

  if (!products || products.length === 0) {
    return <div className="text-center p-4">No product data found.</div>;
  }

  return (
    <div className="p-4">
      <h2 className="text-xl font-semibold mb-4">Core Metrics</h2>
      {metrics && (
        <div className="bg-white shadow overflow-hidden sm:rounded-lg mb-6">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Latest Analysis Overview</h3>
            <p className="mt-1 max-w-2xl text-sm text-gray-500">
              Analysis Time: {new Date(metrics.analysisTime).toLocaleString()}
            </p>
          </div>
          <div className="border-t border-gray-200">
            <dl>
              <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Total Products Analyzed</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalProductsAnalyzed}</dd>
              </div>
              <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">New Entries</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalNewEntries}</dd>
              </div>
              <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Rank Surges</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalRankSurges}</dd>
              </div>
              <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Consistent Performers</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalConsistentPerformers}</dd>
              </div>
            </dl>
          </div>
        </div>
      )}

      <h2 className="text-xl font-semibold mb-4">Product List</h2>
      <div className="overflow-x-auto shadow ring-1 ring-black ring-opacity-5 sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-300">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">ASIN</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Title</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Category</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Listing Date</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Rank</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Price</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Rating</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Reviews</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Last Updated</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {products.map((product) => (
              <tr key={product.id}>
                <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{product.id}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.title}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.categoryName}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{new Date(product.listingDate).toLocaleDateString()}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestRank ?? 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestPrice ? `${product.latestPrice.toFixed(2)}` : 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestRating ?? 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestReviewsCount ?? 'N/A'}</td>
                latestReviewsCount: number | null;
  lastUpdated: string | null;
}

// 定义后端返回的核心指标数据类型
interface CoreMetrics {
  dataCollectionRunId: number;
  analysisTime: string;
  totalNewEntries: number;
  totalRankSurges: number;
  totalConsistentPerformers: number;
  totalProductsAnalyzed: number;
}

// 定义后端返回的产品趋势数据类型
interface ProductTrend {
  productId: string;
  title: string;
  trendType: string;
  description: string;
  analysisTime: string;
}

const Dashboard = () => {
  // 获取产品数据
  const { data: products, error: productsError, isLoading: productsLoading } = useSWR<Product[]>('/api/products', fetcher);

  // 获取核心指标数据
  const { data: metrics, error: metricsError, isLoading: metricsLoading } = useSWR<CoreMetrics>('/api/products/metrics', fetcher);

  // 获取新上榜产品趋势
  const { data: newEntries, error: newEntriesError, isLoading: newEntriesLoading } = useSWR<ProductTrend[]>('/api/products/trends?trendType=NewEntry', fetcher);

  // 获取排名飙升产品趋势
  const { data: rankSurges, error: rankSurgesError, isLoading: rankSurgesLoading } = useSWR<ProductTrend[]>('/api/products/trends?trendType=RankSurge', fetcher);

  if (productsLoading || metricsLoading || newEntriesLoading || rankSurgesLoading) {
    return <div className="text-center p-4">Loading data...</div>;
  }

  if (productsError || metricsError || newEntriesError || rankSurgesError) {
    return <div className="text-center p-4 text-red-500">Failed to load data. Please check the API connection.</div>;
  }

  if (!products || products.length === 0) {
    return <div className="text-center p-4">No product data found.</div>;
  }

  return (
    <div className="p-4">
      <h2 className="text-xl font-semibold mb-4">Core Metrics</h2>
      {metrics && (
        <div className="bg-white shadow overflow-hidden sm:rounded-lg mb-6">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Latest Analysis Overview</h3>
            <p className="mt-1 max-w-2xl text-sm text-gray-500">
              Analysis Time: {new Date(metrics.analysisTime).toLocaleString()}
            </p>
          </div>
          <div className="border-t border-gray-200">
            <dl>
              <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Total Products Analyzed</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalProductsAnalyzed}</dd>
              </div>
              <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">New Entries</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalNewEntries}</dd>
              </div>
              <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Rank Surges</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalRankSurges}</dd>
              </div>
              <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Consistent Performers</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalConsistentPerformers}</dd>
              </div>
            </dl>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
        {/* New Entries Card */}
        <div className="bg-white shadow overflow-hidden sm:rounded-lg">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">New Entries</h3>
          </div>
          <div className="border-t border-gray-200 px-4 py-5 sm:p-6">
            {newEntries && newEntries.length > 0 ? (
              <ul className="divide-y divide-gray-200">
                {newEntries.map((trend) => (
                  <li key={trend.productId} className="py-3">
                    <p className="text-sm font-medium text-gray-900">{trend.title}</p>
                    <p className="text-sm text-gray-500">{trend.description}</p>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-gray-500">No new entries found.</p>
            )}
          </div>
        </div>

        {/* Rank Surges Card */}
        <div className="bg-white shadow overflow-hidden sm:rounded-lg">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Rank Surges</h3>
          </div>
          <div className="border-t border-gray-200 px-4 py-5 sm:p-6">
            {rankSurges && rankSurges.length > 0 ? (
              <ul className="divide-y divide-gray-200">
                {rankSurges.map((trend) => (
                  <li key={trend.productId} className="py-3">
                    <p className="text-sm font-medium text-gray-900">{trend.title}</p>
                    <p className="text-sm text-gray-500">{trend.description}</p>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-gray-500">No rank surges found.</p>
            )}
          </div>
        </div>
      </div>

      <h2 className="text-xl font-semibold mb-4">Product List</h2>
      <div className="overflow-x-auto shadow ring-1 ring-black ring-opacity-5 sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-300">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">ASIN</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Title</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Category</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Listing Date</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Rank</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Price</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Rating</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Reviews</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Last Updated</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {products.map((product) => (
              <tr key={product.id}>
                <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{product.id}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.title}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.categoryName}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{new Date(product.listingDate).toLocaleDateString()}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestRank ?? 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestPrice ? `${product.latestPrice.toFixed(2)}` : 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestRating ?? 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestReviewsCount ?? 'N/A'}</td>
                <import useSWR from 'swr';
import { fetcher } from '../../lib/swr';
import { useState } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';

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

// 定义后端返回的核心指标数据类型
interface CoreMetrics {
  dataCollectionRunId: number;
  analysisTime: string;
  totalNewEntries: number;
  totalRankSurges: number;
  totalConsistentPerformers: number;
  totalProductsAnalyzed: number;
}

// 定义后端返回的产品趋势数据类型
interface ProductTrend {
  productId: string;
  title: string;
  trendType: string;
  description: string;
  analysisTime: string;
}

// 定义后端返回的产品历史数据点类型
interface ProductHistoryDataPoint {
  timestamp: string;
  rank: number;
  price: number;
  rating: number;
  reviewsCount: number;
}

const Dashboard = () => {
  const [selectedProductAsin, setSelectedProductAsin] = useState<string | null>(null);

  // 获取产品数据
  const { data: products, error: productsError, isLoading: productsLoading } = useSWR<Product[]>('/api/products', fetcher);

  // 获取核心指标数据
  const { data: metrics, error: metricsError, isLoading: metricsLoading } = useSWR<CoreMetrics>('/api/products/metrics', fetcher);

  // 获取新上榜产品趋势
  const { data: newEntries, error: newEntriesError, isLoading: newEntriesLoading } = useSWR<ProductTrend[]>('/api/products/trends?trendType=NewEntry', fetcher);

  // 获取排名飙升产品趋势
  const { data: rankSurges, error: rankSurgesError, isLoading: rankSurgesLoading } = useSWR<ProductTrend[]>('/api/products/trends?trendType=RankSurge', fetcher);

  // 获取单个产品历史数据
  const { data: productHistory, error: productHistoryError, isLoading: productHistoryLoading } = useSWR<ProductHistoryDataPoint[]>(selectedProductAsin ? `/api/products/${selectedProductAsin}/history` : null, fetcher);

  if (productsLoading || metricsLoading || newEntriesLoading || rankSurgesLoading) {
    return <div className="text-center p-4">Loading data...</div>;
  }

  if (productsError || metricsError || newEntriesError || rankSurgesError) {
    return <div className="text-center p-4 text-red-500">Failed to load data. Please check the API connection.</div>;
  }

  if (!products || products.length === 0) {
    return <div className="text-center p-4">No product data found.</div>;
  }

  return (
    <div className="p-4">
      <h2 className="text-xl font-semibold mb-4">Core Metrics</h2>
      {metrics && (
        <div className="bg-white shadow overflow-hidden sm:rounded-lg mb-6">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Latest Analysis Overview</h3>
            <p className="mt-1 max-w-2xl text-sm text-gray-500">
              Analysis Time: {new Date(metrics.analysisTime).toLocaleString()}
            </p>
          </div>
          <div className="border-t border-gray-200">
            <dl>
              <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Total Products Analyzed</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalProductsAnalyzed}</dd>
              </div>
              <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">New Entries</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalNewEntries}</dd>
              </div>
              <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Rank Surges</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalRankSurges}</dd>
              </div>
              <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Consistent Performers</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalConsistentPerformers}</dd>
              </div>
            </dl>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
        {/* New Entries Card */}
        <div className="bg-white shadow overflow-hidden sm:rounded-lg">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">New Entries</h3>
          </div>
          <div className="border-t border-gray-200 px-4 py-5 sm:p-6">
            {newEntries && newEntries.length > 0 ? (
              <ul className="divide-y divide-gray-200">
                {newEntries.map((trend) => (
                  <li key={trend.productId} className="py-3">
                    <p className="text-sm font-medium text-gray-900">{trend.title}</p>
                    <p className="text-sm text-gray-500">{trend.description}</p>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-gray-500">No new entries found.</p>
            )}
          </div>
        </div>

        {/* Rank Surges Card */}
        <div className="bg-white shadow overflow-hidden sm:rounded-lg">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Rank Surges</h3>
          </div>
          <div className="border-t border-gray-200 px-4 py-5 sm:p-6">
            {rankSurges && rankSurges.length > 0 ? (
              <ul className="divide-y divide-gray-200">
                {rankSurges.map((trend) => (
                  <li key={trend.productId} className="py-3">
                    <p className="text-sm font-medium text-gray-900">{trend.title}</p>
                    <p className="text-sm text-gray-500">{trend.description}</p>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-gray-500">No rank surges found.</p>
            )}
          </div>
        </div>
      </div>

      <h2 className="text-xl font-semibold mb-4">Product List</h2>
      <div className="overflow-x-auto shadow ring-1 ring-black ring-opacity-5 sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-300">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">ASIN</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Title</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Category</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Listing Date</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Rank</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Price</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Rating</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Reviews</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Last Updated</th>
              <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                <span className="sr-only">View History</span>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {products.map((product) => (
              <tr key={product.id}>
                <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{product.id}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.title}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.categoryName}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{new Date(product.listingDate).toLocaleDateString()}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestRank ?? 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestPrice ? `${product.latestPrice.toFixed(2)}` : 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestRating ?? 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestReviewsCount ?? 'N/A'}</td>
                <import useSWR from 'swr';
import { fetcher } from '../../lib/swr';
import { useState } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';

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

// 定义后端返回的核心指标数据类型
interface CoreMetrics {
  dataCollectionRunId: number;
  analysisTime: string;
  totalNewEntries: number;
  totalRankSurges: number;
  totalConsistentPerformers: number;
  totalProductsAnalyzed: number;
}

// 定义后端返回的产品趋势数据类型
interface ProductTrend {
  productId: string;
  title: string;
  trendType: string;
  description: string;
  analysisTime: string;
}

// 定义后端返回的产品历史数据点类型
interface ProductHistoryDataPoint {
  timestamp: string;
  rank: number;
  price: number;
  rating: number;
  reviewsCount: number;
}

// 定义后端返回的分类数据类型
interface Category {
  id: number;
  name: string;
  amazonCategoryId: string;
}

const Dashboard = () => {
  const [selectedProductAsin, setSelectedProductAsin] = useState<string | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');

  // 获取产品数据
  const { data: products, error: productsError, isLoading: productsLoading } = useSWR<Product[]>(`/api/products?categoryId=${selectedCategory}&searchTerm=${searchTerm}`, fetcher);

  // 获取核心指标数据
  const { data: metrics, error: metricsError, isLoading: metricsLoading } = useSWR<CoreMetrics>('/api/products/metrics', fetcher);

  // 获取新上榜产品趋势
  const { data: newEntries, error: newEntriesError, isLoading: newEntriesLoading } = useSWR<ProductTrend[]>('/api/products/trends?trendType=NewEntry', fetcher);

  // 获取排名飙升产品趋势
  const { data: rankSurges, error: rankSurgesError, isLoading: rankSurgesLoading } = useSWR<ProductTrend[]>('/api/products/trends?trendType=RankSurge', fetcher);

  // 获取所有分类
  const { data: categories, error: categoriesError, isLoading: categoriesLoading } = useSWR<Category[]>('/api/categories', fetcher);

  // 获取单个产品历史数据
  const { data: productHistory, error: productHistoryError, isLoading: productHistoryLoading } = useSWR<ProductHistoryDataPoint[]>(selectedProductAsin ? `/api/products/${selectedProductAsin}/history` : null, fetcher);

  if (productsLoading || metricsLoading || newEntriesLoading || rankSurgesLoading || categoriesLoading) {
    return <div className="text-center p-4">Loading data...</div>;
  }

  if (productsError || metricsError || newEntriesError || rankSurgesError || categoriesError) {
    return <div className="text-center p-4 text-red-500">Failed to load data. Please check the API connection.</div>;
  }

  if (!products || products.length === 0) {
    return <div className="text-center p-4">No product data found.</div>;
  }

  return (
    <div className="p-4">
      <h2 className="text-xl font-semibold mb-4">Core Metrics</h2>
      {metrics && (
        <div className="bg-white shadow overflow-hidden sm:rounded-lg mb-6">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Latest Analysis Overview</h3>
            <p className="mt-1 max-w-2xl text-sm text-gray-500">
              Analysis Time: {new Date(metrics.analysisTime).toLocaleString()}
            </p>
          </div>
          <div className="border-t border-gray-200">
            <dl>
              <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Total Products Analyzed</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalProductsAnalyzed}</dd>
              </div>
              <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">New Entries</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalNewEntries}</dd>
              </div>
              <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Rank Surges</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalRankSurges}</dd>
              </div>
              <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-gray-500">Consistent Performers</dt>
                <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">{metrics.totalConsistentPerformers}</dd>
              </div>
            </dl>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
        {/* New Entries Card */}
        <div className="bg-white shadow overflow-hidden sm:rounded-lg">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">New Entries</h3>
          </div>
          <div className="border-t border-gray-200 px-4 py-5 sm:p-6">
            {newEntries && newEntries.length > 0 ? (
              <ul className="divide-y divide-gray-200">
                {newEntries.map((trend) => (
                  <li key={trend.productId} className="py-3">
                    <p className="text-sm font-medium text-gray-900">{trend.title}</p>
                    <p className="text-sm text-gray-500">{trend.description}</p>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-gray-500">No new entries found.</p>
            )}
          </div>
        </div>

        {/* Rank Surges Card */}
        <div className="bg-white shadow overflow-hidden sm:rounded-lg">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Rank Surges</h3>
          </div>
          <div className="border-t border-gray-200 px-4 py-5 sm:p-6">
            {rankSurges && rankSurges.length > 0 ? (
              <ul className="divide-y divide-gray-200">
                {rankSurges.map((trend) => (
                  <li key={trend.productId} className="py-3">
                    <p className="text-sm font-medium text-gray-900">{trend.title}</p>
                    <p className="text-sm text-gray-500">{trend.description}</p>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-gray-500">No rank surges found.</p>
            )}
          </div>
        </div>
      </div>

      <h2 className="text-xl font-semibold mb-4">Product List</h2>
      <div className="mb-4 flex space-x-4">
        <select
          id="category"
          name="category"
          className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm rounded-md"
          value={selectedCategory}
          onChange={(e) => setSelectedCategory(e.target.value)}
        >
          <option value="">All Categories</option>
          {categories && categories.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
        <input
          type="text"
          placeholder="Search by title or ASIN..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="mt-1 block w-full pl-3 pr-3 py-2 text-base border-gray-300 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm rounded-md"
        />
      </div>
      <div className="overflow-x-auto shadow ring-1 ring-black ring-opacity-5 sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-300">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">ASIN</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Title</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Category</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Listing Date</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Rank</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Price</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Rating</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Reviews</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Last Updated</th>
              <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                <span className="sr-only">View History</span>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {products.map((product) => (
              <tr key={product.id}>
                <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{product.id}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.title}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.categoryName}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{new Date(product.listingDate).toLocaleDateString()}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestRank ?? 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestPrice ? `${product.latestPrice.toFixed(2)}` : 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestRating ?? 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.latestReviewsCount ?? 'N/A'}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{product.lastUpdated ? new Date(product.lastUpdated).toLocaleDateString() : 'N/A'}</td>
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  <button onClick={() => setSelectedProductAsin(product.id)} className="text-indigo-600 hover:text-indigo-900">
                    View History<span className="sr-only">, {product.title}</span>
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Product History Modal */}
      {selectedProductAsin && (
        <div className="fixed inset-0 z-10 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-4xl sm:p-6">
              <div className="absolute right-0 top-0 hidden pr-4 pt-4 sm:block">
                <button type="button" className="rounded-md bg-white text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2" onClick={() => setSelectedProductAsin(null)}>
                  <span className="sr-only">Close</span>
                  <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
              <div className="sm:flex sm:items-start">
                <div className="mt-3 text-center sm:ml-4 sm:mt-0 sm:text-left">
                  <h3 className="text-base font-semibold leading-6 text-gray-900" id="modal-title">Product History: {selectedProductAsin}</h3>
                  <div className="mt-2">
                    {productHistoryLoading ? (
                      <p className="text-sm text-gray-500">Loading history...</p>
                    ) : productHistoryError ? (
                      <p className="text-sm text-red-500">Failed to load history.</p>
                    ) : productHistory && productHistory.length > 0 ? (
                      <div className="w-full h-80">
                        <ResponsiveContainer width="100%" height="100%">
                          <LineChart
                            data={productHistory.map(dp => ({ ...dp, timestamp: new Date(dp.timestamp).toLocaleDateString() }))}
                            margin={{
                              top: 5,
                              right: 30,
                              left: 20,
                              bottom: 5,
                            }}
                          >
                            <CartesianGrid strokeDasharray="3 3" />
                            <XAxis dataKey="timestamp" />
                            <YAxis reversed={true} domain={['auto', 'auto']} /> {/* Rank is lower for better products */}
                            <Tooltip />
                            <Legend />
                            <Line type="monotone" dataKey="rank" stroke="#8884d8" activeDot={{ r: 8 }} />
                            <Line type="monotone" dataKey="price" stroke="#82ca9d" />
                          </LineChart>
                        </ResponsiveContainer>
                      </div>
                    ) : (
                      <p className="text-sm text-gray-500">No history data available for this product.</p>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  <button onClick={() => setSelectedProductAsin(product.id)} className="text-indigo-600 hover:text-indigo-900">
                    View History<span className="sr-only">, {product.title}</span>
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Product History Modal */}
      {selectedProductAsin && (
        <div className="fixed inset-0 z-10 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-4xl sm:p-6">
              <div className="absolute right-0 top-0 hidden pr-4 pt-4 sm:block">
                <button type="button" className="rounded-md bg-white text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2" onClick={() => setSelectedProductAsin(null)}>
                  <span className="sr-only">Close</span>
                  <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
              <div className="sm:flex sm:items-start">
                <div className="mt-3 text-center sm:ml-4 sm:mt-0 sm:text-left">
                  <h3 className="text-base font-semibold leading-6 text-gray-900" id="modal-title">Product History: {selectedProductAsin}</h3>
                  <div className="mt-2">
                    {productHistoryLoading ? (
                      <p className="text-sm text-gray-500">Loading history...</p>
                    ) : productHistoryError ? (
                      <p className="text-sm text-red-500">Failed to load history.</p>
                    ) : productHistory && productHistory.length > 0 ? (
                      <div className="w-full h-80">
                        <ResponsiveContainer width="100%" height="100%">
                          <LineChart
                            data={productHistory.map(dp => ({ ...dp, timestamp: new Date(dp.timestamp).toLocaleDateString() }))}
                            margin={{
                              top: 5,
                              right: 30,
                              left: 20,
                              bottom: 5,
                            }}
                          >
                            <CartesianGrid strokeDasharray="3 3" />
                            <XAxis dataKey="timestamp" />
                            <YAxis reversed={true} domain={['auto', 'auto']} /> {/* Rank is lower for better products */}
                            <Tooltip />
                            <Legend />
                            <Line type="monotone" dataKey="rank" stroke="#8884d8" activeDot={{ r: 8 }} />
                            <Line type="monotone" dataKey="price" stroke="#82ca9d" />
                          </LineChart>
                        </ResponsiveContainer>
                      </div>
                    ) : (
                      <p className="text-sm text-gray-500">No history data available for this product.</p>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default Dashboard;
