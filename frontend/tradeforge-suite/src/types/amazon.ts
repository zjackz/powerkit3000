// 与后端枚举保持一致的字符串字面量类型。
export type AmazonBestsellerType = 'BestSellers' | 'NewReleases' | 'MoversAndShakers';
export type AmazonTrendType = 'NewEntry' | 'RankSurge' | 'ConsistentPerformer';

/**
 * 核心指标结构体，对应后端最新快照摘要。
 */
export interface AmazonCoreMetrics {
  snapshotId: number;
  capturedAt: string;
  totalProducts: number;
  totalNewEntries: number;
  totalRankSurges: number;
  totalConsistentPerformers: number;
}

/**
 * 榜单商品列表项。
 */
export interface AmazonProductListItem {
  asin: string;
  title: string;
  categoryName: string;
  listingDate?: string | null;
  latestRank?: number | null;
  latestPrice?: number | null;
  latestRating?: number | null;
  latestReviews?: number | null;
  lastUpdated?: string | null;
}

/**
 * 趋势列表项。
 */
export interface AmazonTrendListItem {
  asin: string;
  title: string;
  trendType: AmazonTrendType;
  description: string;
  recordedAt: string;
}

/**
 * 单个商品的历史数据点。
 */
export interface AmazonProductHistoryPoint {
  timestamp: string;
  rank: number;
  price?: number | null;
  rating?: number | null;
  reviewsCount?: number | null;
}

/**
 * 最新报告响应结构。
 */
export interface AmazonLatestReportResponse {
  metrics: AmazonCoreMetrics;
  trends: AmazonTrendListItem[];
  reportText: string;
}

/**
 * 榜单商品查询参数。
 */
export interface AmazonProductsQueryParams {
  categoryId?: number;
  search?: string;
}

/**
 * 趋势查询参数。
 */
export interface AmazonTrendsQueryParams {
  trendType?: AmazonTrendType;
}

/**
 * 采集快照请求体。
 */
export interface AmazonFetchSnapshotPayload {
  categoryId: number;
  bestsellerType?: AmazonBestsellerType;
}
