import { httpClient } from './httpClient';
import type {
  AmazonCoreMetrics,
  AmazonLatestReportResponse,
  AmazonProductHistoryPoint,
  AmazonProductListItem,
  AmazonProductsQueryParams,
  AmazonTrendListItem,
  AmazonTrendsQueryParams,
} from '@/types/amazon';

/**
 * 获取最新快照的核心指标；若后端返回 204 表示暂未采集数据。
 */
export const fetchAmazonCoreMetrics = async (): Promise<AmazonCoreMetrics | null> => {
  try {
    const response = await httpClient.get<AmazonCoreMetrics>('/amazon/core-metrics');
    return response.data;
  } catch (error) {
    if ((error as { response?: { status?: number } }).response?.status === 204) {
      return null;
    }
    throw error;
  }
};

/**
 * 查询榜单商品列表，支持传入筛选参数。
 */
export const fetchAmazonProducts = async (
  params: AmazonProductsQueryParams,
): Promise<AmazonProductListItem[]> => {
  const response = await httpClient.get<AmazonProductListItem[]>('/amazon/products', {
    params,
  });
  return response.data;
};

/**
 * 查询最新快照的趋势数据。
 */
export const fetchAmazonTrends = async (
  params: AmazonTrendsQueryParams,
): Promise<AmazonTrendListItem[]> => {
  const response = await httpClient.get<AmazonTrendListItem[]>('/amazon/trends', {
    params,
  });
  return response.data;
};

/**
 * 获取指定 ASIN 的历史榜单记录。
 */
export const fetchAmazonProductHistory = async (
  asin: string,
): Promise<AmazonProductHistoryPoint[]> => {
  const response = await httpClient.get<AmazonProductHistoryPoint[]>(`/amazon/products/${asin}/history`);
  return response.data;
};

/**
 * 拉取最新报告内容，用于前端展示或导出。
 */
export const fetchLatestAmazonReport = async (): Promise<AmazonLatestReportResponse | null> => {
  try {
    const response = await httpClient.get<AmazonLatestReportResponse>('/amazon/report/latest');
    return response.data;
  } catch (error) {
    if ((error as { response?: { status?: number } }).response?.status === 204) {
      return null;
    }
    throw error;
  }
};
