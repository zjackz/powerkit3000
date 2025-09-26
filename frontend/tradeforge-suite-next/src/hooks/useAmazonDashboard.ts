import { useQuery } from '@tanstack/react-query';
import {
  fetchAmazonCoreMetrics,
  fetchAmazonOperationalIssues,
  fetchAmazonOperationalSummary,
  fetchAmazonProducts,
  fetchAmazonProductHistory,
  fetchAmazonTrends,
} from '@/services/amazonService';
import type {
  AmazonCoreMetrics,
  AmazonOperationalIssuesQueryParams,
  AmazonOperationalIssuesResponse,
  AmazonOperationalSummary,
  AmazonProductHistoryPoint,
  AmazonProductListItem,
  AmazonProductsQueryParams,
  AmazonTrendListItem,
  AmazonTrendsQueryParams,
} from '@/types/amazon';

export const AMAZON_CORE_METRICS_KEY = ['amazon', 'core-metrics'];
export const AMAZON_PRODUCTS_KEY = ['amazon', 'products'];
export const AMAZON_TRENDS_KEY = ['amazon', 'trends'];
export const AMAZON_HISTORY_KEY = ['amazon', 'history'];
export const AMAZON_OPERATIONS_SUMMARY_KEY = ['amazon', 'operations', 'summary'];
export const AMAZON_OPERATIONS_ISSUES_KEY = ['amazon', 'operations', 'issues'];

/**
 * React Query Hook：获取最新核心指标。
 */
export const useAmazonCoreMetrics = () =>
  useQuery<AmazonCoreMetrics | null>({
    queryKey: AMAZON_CORE_METRICS_KEY,
    queryFn: () => fetchAmazonCoreMetrics(),
    staleTime: 1000 * 60 * 5,
  });

/**
 * React Query Hook：获取榜单商品列表。
 */
export const useAmazonProducts = (params: AmazonProductsQueryParams) =>
  useQuery<AmazonProductListItem[]>({
    queryKey: [...AMAZON_PRODUCTS_KEY, params],
    queryFn: () => fetchAmazonProducts(params),
    staleTime: 1000 * 60 * 5,
  });

/**
 * React Query Hook：获取最新趋势信息。
 */
export const useAmazonTrends = (params: AmazonTrendsQueryParams) =>
  useQuery<AmazonTrendListItem[]>({
    queryKey: [...AMAZON_TRENDS_KEY, params],
    queryFn: () => fetchAmazonTrends(params),
    staleTime: 1000 * 60 * 5,
  });

/**
 * React Query Hook：获取指定 ASIN 的历史时间序列。
 */
export const useAmazonProductHistory = (asin?: string) =>
  useQuery<AmazonProductHistoryPoint[]>({
    queryKey: [...AMAZON_HISTORY_KEY, asin],
    queryFn: () => fetchAmazonProductHistory(asin!),
    enabled: Boolean(asin),
    staleTime: 1000 * 60 * 5,
  });

export const useAmazonOperationalSummary = () =>
  useQuery<AmazonOperationalSummary>({
    queryKey: AMAZON_OPERATIONS_SUMMARY_KEY,
    queryFn: () => fetchAmazonOperationalSummary(),
    staleTime: 1000 * 60,
  });

export const useAmazonOperationalIssues = (params: AmazonOperationalIssuesQueryParams) =>
  useQuery<AmazonOperationalIssuesResponse>({
    queryKey: [...AMAZON_OPERATIONS_ISSUES_KEY, params],
    queryFn: () => fetchAmazonOperationalIssues(params),
    keepPreviousData: true,
    staleTime: 1000 * 30,
  });
