import type {
  AmazonCoreMetrics,
  AmazonLatestReportResponse,
  AmazonProductHistoryPoint,
  AmazonProductListItem,
  AmazonTrendListItem,
  AmazonTrendType,
} from '@/types/amazon';

export const AMAZON_CORE_METRICS_MOCK: AmazonCoreMetrics = {
  snapshotId: 1001,
  capturedAt: '2024-03-15T02:30:00Z',
  totalProducts: 120,
  totalNewEntries: 18,
  totalRankSurges: 12,
  totalConsistentPerformers: 24,
};

export const AMAZON_PRODUCTS_MOCK: AmazonProductListItem[] = [
  {
    asin: 'B0PKSMART01',
    title: 'Nebula Smart Projector',
    categoryName: 'Home & Kitchen',
    listingDate: '2023-11-02T00:00:00Z',
    latestRank: 3,
    latestPrice: 39.99,
    latestRating: 4.6,
    latestReviews: 820,
    lastUpdated: '2024-03-15T01:05:00Z',
  },
  {
    asin: 'B0STORAGE02',
    title: 'FoldMate Modular Storage Box',
    categoryName: 'Home & Kitchen',
    listingDate: '2024-01-12T00:00:00Z',
    latestRank: 9,
    latestPrice: 34.5,
    latestRating: 4.4,
    latestReviews: 412,
    lastUpdated: '2024-03-15T01:05:00Z',
  },
  {
    asin: 'B0TOOLKIT03',
    title: 'VoltEdge Precision Driver Kit',
    categoryName: 'Tools & Home Improvement',
    listingDate: '2023-09-25T00:00:00Z',
    latestRank: 6,
    latestPrice: 49.99,
    latestRating: 4.7,
    latestReviews: 1_240,
    lastUpdated: '2024-03-15T01:05:00Z',
  },
  {
    asin: 'B0CLEANBOT04',
    title: 'AeroSweep Cordless Vacuum Lite',
    categoryName: 'Home & Kitchen',
    listingDate: '2023-12-18T00:00:00Z',
    latestRank: 14,
    latestPrice: 44.95,
    latestRating: 4.2,
    latestReviews: 268,
    lastUpdated: '2024-03-15T01:05:00Z',
  },
];

const buildHistory = (values: Array<Pick<AmazonProductHistoryPoint, 'timestamp' | 'rank'> & Partial<AmazonProductHistoryPoint>>) =>
  values.map((item) => ({
    price: null,
    rating: null,
    reviewsCount: null,
    ...item,
  }));

export const AMAZON_HISTORY_MOCK: Record<string, AmazonProductHistoryPoint[]> = {
  B0PKSMART01: buildHistory([
    { timestamp: '2024-03-10T02:30:00Z', rank: 12, price: 42.99, rating: 4.5, reviewsCount: 760 },
    { timestamp: '2024-03-11T02:30:00Z', rank: 9, price: 41.99, rating: 4.6, reviewsCount: 780 },
    { timestamp: '2024-03-12T02:30:00Z', rank: 6, price: 39.99, rating: 4.6, reviewsCount: 800 },
    { timestamp: '2024-03-13T02:30:00Z', rank: 4, price: 39.99, rating: 4.6, reviewsCount: 810 },
    { timestamp: '2024-03-14T02:30:00Z', rank: 3, price: 39.99, rating: 4.6, reviewsCount: 820 },
  ]),
  B0STORAGE02: buildHistory([
    { timestamp: '2024-03-10T02:30:00Z', rank: 24, price: 36.5, rating: 4.3, reviewsCount: 360 },
    { timestamp: '2024-03-11T02:30:00Z', rank: 20, price: 35.99, rating: 4.4, reviewsCount: 372 },
    { timestamp: '2024-03-12T02:30:00Z', rank: 16, price: 34.99, rating: 4.4, reviewsCount: 390 },
    { timestamp: '2024-03-13T02:30:00Z', rank: 12, price: 34.5, rating: 4.4, reviewsCount: 405 },
    { timestamp: '2024-03-14T02:30:00Z', rank: 9, price: 34.5, rating: 4.4, reviewsCount: 412 },
  ]),
  B0TOOLKIT03: buildHistory([
    { timestamp: '2024-03-10T02:30:00Z', rank: 18, price: 54.5, rating: 4.6, reviewsCount: 1_180 },
    { timestamp: '2024-03-11T02:30:00Z', rank: 14, price: 52.99, rating: 4.7, reviewsCount: 1_200 },
    { timestamp: '2024-03-12T02:30:00Z', rank: 11, price: 51.5, rating: 4.7, reviewsCount: 1_220 },
    { timestamp: '2024-03-13T02:30:00Z', rank: 8, price: 50.5, rating: 4.7, reviewsCount: 1_233 },
    { timestamp: '2024-03-14T02:30:00Z', rank: 6, price: 49.99, rating: 4.7, reviewsCount: 1_240 },
  ]),
  B0CLEANBOT04: buildHistory([
    { timestamp: '2024-03-10T02:30:00Z', rank: 22, price: 49.99, rating: 4.3, reviewsCount: 240 },
    { timestamp: '2024-03-11T02:30:00Z', rank: 20, price: 48.99, rating: 4.3, reviewsCount: 245 },
    { timestamp: '2024-03-12T02:30:00Z', rank: 18, price: 46.95, rating: 4.2, reviewsCount: 255 },
    { timestamp: '2024-03-13T02:30:00Z', rank: 16, price: 45.5, rating: 4.2, reviewsCount: 262 },
    { timestamp: '2024-03-14T02:30:00Z', rank: 14, price: 44.95, rating: 4.2, reviewsCount: 268 },
  ]),
};

export const AMAZON_TRENDS_MOCK: AmazonTrendListItem[] = [
  {
    asin: 'B0PKSMART01',
    title: 'Nebula Smart Projector',
    trendType: 'RankSurge',
    description: '榜单排名三天跃升 9 位，夜间影院关键词转化率翻倍。',
    recordedAt: '2024-03-14T03:00:00Z',
  },
  {
    asin: 'B0STORAGE02',
    title: 'FoldMate Modular Storage Box',
    trendType: 'NewEntry',
    description: '首次进入 Top 10，评论集中强调收纳效率与空间利用率。',
    recordedAt: '2024-03-14T03:00:00Z',
  },
  {
    asin: 'B0TOOLKIT03',
    title: 'VoltEdge Precision Driver Kit',
    trendType: 'ConsistentPerformer',
    description: '连续 6 周保持 Top 10，评论数日均增长 40+。',
    recordedAt: '2024-03-14T03:00:00Z',
  },
];

export const AMAZON_REPORT_MOCK: AmazonLatestReportResponse = {
  metrics: AMAZON_CORE_METRICS_MOCK,
  trends: AMAZON_TRENDS_MOCK,
  reportText:
    '今日 Home & Kitchen 榜单新增 18 个候选，其中 Nebula Smart Projector 凭借夜间影院场景迅速上涨；VoltEdge 工具包维持稳定热度，建议关注库存与捆绑销售机会。',
};

export const filterAmazonProductsMock = ({ search }: { search?: string }) => {
  if (!search) {
    return AMAZON_PRODUCTS_MOCK;
  }
  const lowered = search.trim().toLowerCase();
  return AMAZON_PRODUCTS_MOCK.filter((product) =>
    [product.asin, product.title, product.categoryName].some((value) => value.toLowerCase().includes(lowered)),
  );
};

export const filterTrendsMock = (trendType?: AmazonTrendType) => {
  if (!trendType) {
    return AMAZON_TRENDS_MOCK;
  }
  return AMAZON_TRENDS_MOCK.filter((trend) => trend.trendType === trendType);
};

export const getHistoryMock = (asin: string) => AMAZON_HISTORY_MOCK[asin] ?? [];
