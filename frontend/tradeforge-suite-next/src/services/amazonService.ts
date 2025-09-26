import { httpClient } from '@/services/httpClient';
import { notifyApiError, notifyApiFallback } from '@/utils/apiNotifications';
import type {
  AmazonCoreMetrics,
  AmazonLatestReportResponse,
  AmazonOperationalIssuesQueryParams,
  AmazonOperationalIssuesResponse,
  AmazonOperationalSummary,
  AmazonProductHistoryPoint,
  AmazonProductListItem,
  AmazonProductsQueryParams,
  AmazonTrendListItem,
  AmazonTrendsQueryParams,
} from '@/types/amazon';
import {
  AMAZON_CORE_METRICS_MOCK,
  AMAZON_REPORT_MOCK,
  filterAmazonProductsMock,
  filterTrendsMock,
  getHistoryMock,
} from '@/mocks/amazon';

interface AmazonFetchOptions {
  useMockFallback?: boolean;
}

const DEFAULT_AD_PLACEHOLDER = { status: 'comingSoon', message: '广告浪费分析开发中，敬请期待。' } as const;
const EMPTY_ISSUE_SUMMARY = { total: 0, high: 0, medium: 0, low: 0 } as const;

/**
 * 获取最新快照的核心指标；若后端返回 204 表示暂未采集数据。
 */
export const fetchAmazonCoreMetrics = async (
  options: AmazonFetchOptions = {},
): Promise<AmazonCoreMetrics | null> => {
  const { useMockFallback = true } = options;
  try {
    const response = await httpClient.get<AmazonCoreMetrics>('/amazon/core-metrics');
    return response.data;
  } catch (error) {
    if ((error as { response?: { status?: number } }).response?.status === 204) {
      return null;
    }
    if (!useMockFallback) {
      notifyApiError('Amazon 核心指标', error);
      throw error;
    }
    notifyApiFallback('Amazon 核心指标');
    console.warn('无法加载 Amazon 核心指标，使用本地示例数据回退。', error);
    return AMAZON_CORE_METRICS_MOCK;
  }
};

/**
 * 查询榜单商品列表，支持传入筛选参数。
 */
export const fetchAmazonProducts = async (
  params: AmazonProductsQueryParams,
  options: AmazonFetchOptions = {},
): Promise<AmazonProductListItem[]> => {
  const { useMockFallback = true } = options;
  try {
    const response = await httpClient.get<AmazonProductListItem[]>('/amazon/products', {
      params,
    });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      notifyApiError('Amazon 榜单产品', error);
      throw error;
    }
    notifyApiFallback('Amazon 榜单产品');
    console.warn('无法加载 Amazon 榜单产品列表，使用本地示例数据回退。', error);
    return filterAmazonProductsMock({ search: params.search });
  }
};

/**
 * 查询最新快照的趋势数据。
 */
export const fetchAmazonTrends = async (
  params: AmazonTrendsQueryParams,
  options: AmazonFetchOptions = {},
): Promise<AmazonTrendListItem[]> => {
  const { useMockFallback = true } = options;
  try {
    const response = await httpClient.get<AmazonTrendListItem[]>('/amazon/trends', {
      params,
    });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      notifyApiError('Amazon 趋势', error);
      throw error;
    }
    notifyApiFallback('Amazon 趋势');
    console.warn('无法加载 Amazon 趋势数据，使用本地示例数据回退。', error);
    return filterTrendsMock(params.trendType);
  }
};

/**
 * 获取指定 ASIN 的历史榜单记录。
 */
export const fetchAmazonProductHistory = async (
  asin: string,
  options: AmazonFetchOptions = {},
): Promise<AmazonProductHistoryPoint[]> => {
  const { useMockFallback = true } = options;
  try {
    const response = await httpClient.get<AmazonProductHistoryPoint[]>(`/amazon/products/${asin}/history`);
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      notifyApiError('Amazon 产品历史', error);
      throw error;
    }
    notifyApiFallback('Amazon 产品历史');
    console.warn(`无法加载 ASIN ${asin} 的历史数据，使用本地示例数据回退。`, error);
    return getHistoryMock(asin);
  }
};

/**
 * 拉取最新报告内容，用于前端展示或导出。
 */
export const fetchLatestAmazonReport = async (
  options: AmazonFetchOptions = {},
): Promise<AmazonLatestReportResponse | null> => {
  const { useMockFallback = true } = options;
  try {
    const response = await httpClient.get<AmazonLatestReportResponse>('/amazon/report/latest');
    return response.data;
  } catch (error) {
    if ((error as { response?: { status?: number } }).response?.status === 204) {
      return null;
    }
    if (!useMockFallback) {
      notifyApiError('Amazon 最新报告', error);
      throw error;
    }
    notifyApiFallback('Amazon 最新报告');
    console.warn('无法加载 Amazon 最新报告，使用本地示例数据回退。', error);
    return AMAZON_REPORT_MOCK;
  }
};

export const fetchAmazonOperationalSummary = async (
  options: AmazonFetchOptions = {},
): Promise<AmazonOperationalSummary> => {
  const { useMockFallback = true } = options;
  try {
    const response = await httpClient.get<AmazonOperationalSummary>('/amazon/operations/summary');
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      notifyApiError('Amazon 运营概览', error);
      throw error;
    }
    notifyApiFallback('Amazon 运营概览');
    console.warn('无法加载 Amazon 运营概览，返回空数据占位。', error);
    return {
      lastUpdatedAt: null,
      isStale: false,
      lowStock: { ...EMPTY_ISSUE_SUMMARY },
      negativeReview: { ...EMPTY_ISSUE_SUMMARY },
      adWastePlaceholder: DEFAULT_AD_PLACEHOLDER,
    };
  }
};

export const fetchAmazonOperationalIssues = async (
  params: AmazonOperationalIssuesQueryParams,
  options: AmazonFetchOptions = {},
): Promise<AmazonOperationalIssuesResponse> => {
  const { useMockFallback = true } = options;
  try {
    const response = await httpClient.get<AmazonOperationalIssuesResponse>('/amazon/operations/issues', {
      params,
    });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      notifyApiError('Amazon 运营问题列表', error);
      throw error;
    }
    notifyApiFallback('Amazon 运营问题列表');
    console.warn('无法加载 Amazon 运营问题列表，返回空列表占位。', error);
    return {
      lastUpdatedAt: null,
      isStale: false,
      items: [],
      total: 0,
      adWastePlaceholder: DEFAULT_AD_PLACEHOLDER,
    };
  }
};
