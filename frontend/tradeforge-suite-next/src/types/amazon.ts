export type AmazonBestsellerType = 'BestSellers' | 'NewReleases' | 'MoversAndShakers';
export type AmazonTrendType = 'NewEntry' | 'RankSurge' | 'ConsistentPerformer';

export interface AmazonCoreMetrics {
  snapshotId: number;
  capturedAt: string;
  totalProducts: number;
  totalNewEntries: number;
  totalRankSurges: number;
  totalConsistentPerformers: number;
}

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

export interface AmazonTrendListItem {
  asin: string;
  title: string;
  trendType: AmazonTrendType;
  description: string;
  recordedAt: string;
}

export interface AmazonProductHistoryPoint {
  timestamp: string;
  rank: number;
  price?: number | null;
  rating?: number | null;
  reviewsCount?: number | null;
}

export interface AmazonLatestReportResponse {
  metrics: AmazonCoreMetrics;
  trends: AmazonTrendListItem[];
  reportText: string;
}

export interface AmazonProductsQueryParams {
  categoryId?: number;
  search?: string;
}

export interface AmazonTrendsQueryParams {
  trendType?: AmazonTrendType;
}

export interface AmazonFetchSnapshotPayload {
  categoryId: number;
  bestsellerType?: AmazonBestsellerType;
}

export type AmazonTaskCategorySelectorType = 'url' | 'node';

export interface AmazonTaskCategorySelector {
  type: AmazonTaskCategorySelectorType;
  value: string;
}

export interface AmazonTaskPriceRange {
  min?: number | null;
  max?: number | null;
}

export interface AmazonTaskKeywordRules {
  include: string[];
  exclude: string[];
}

export interface AmazonTaskFilterRules {
  minRating?: number | null;
  minReviews?: number | null;
}

export type AmazonTaskScheduleType = 'once' | 'recurring';

export interface AmazonTaskSchedule {
  type: AmazonTaskScheduleType;
  cron?: string;
  timezone: string;
  runAt?: string;
}

export interface AmazonTaskLimits {
  maxProducts?: number | null;
  maxRequestsPerHour?: number | null;
}

export type AmazonTaskStatus = 'draft' | 'active' | 'paused';

export interface AmazonTask {
  id: string;
  name: string;
  site: string;
  categories: AmazonTaskCategorySelector[];
  leaderboards: AmazonBestsellerType[] | string[];
  priceRange: AmazonTaskPriceRange;
  keywords: AmazonTaskKeywordRules;
  filters: AmazonTaskFilterRules;
  schedule: AmazonTaskSchedule;
  limits: AmazonTaskLimits;
  proxyPolicy: string;
  status: AmazonTaskStatus;
  notes?: string;
  llmSummary?: string;
  createdAt: string;
  updatedAt: string;
}
