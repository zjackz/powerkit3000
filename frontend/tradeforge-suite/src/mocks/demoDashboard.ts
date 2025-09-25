import dayjs from 'dayjs';
import type {
  AnalyticsFilterRequest,
  CategoryInsight,
  CategoryKeywordInsight,
  CountryInsight,
  CreatorPerformance,
  FundingDistributionBin,
  MonthlyTrendPoint,
  ProjectHighlight,
  ProjectSummary,
} from '@/types/project';
import type {
  AmazonCoreMetrics,
  AmazonProductHistoryPoint,
  AmazonProductListItem,
  AmazonTrendListItem,
  AmazonTrendType,
} from '@/types/amazon';

export const DEMO_FILTER_PRESET: AnalyticsFilterRequest = {
  launchedAfter: dayjs().subtract(12, 'month').startOf('month').toISOString(),
  minPercentFunded: 120,
  categories: ['Technology', 'Design', 'Fashion'],
};

export const DEMO_PROJECT_SUMMARY: ProjectSummary = {
  totalProjects: 420,
  successfulProjects: 296,
  totalPledged: 12600000,
  distinctCountries: 18,
  successRate: 70.5,
};

export const DEMO_CATEGORY_INSIGHTS: CategoryInsight[] = [
  {
    categoryName: 'Technology',
    totalProjects: 110,
    successfulProjects: 82,
    successRate: 74.5,
    averagePercentFunded: 268.4,
    totalPledged: 4820000,
  },
  {
    categoryName: 'Design',
    totalProjects: 95,
    successfulProjects: 68,
    successRate: 71.6,
    averagePercentFunded: 214.2,
    totalPledged: 3560000,
  },
  {
    categoryName: 'Fashion',
    totalProjects: 60,
    successfulProjects: 39,
    successRate: 65,
    averagePercentFunded: 176.3,
    totalPledged: 1680000,
  },
  {
    categoryName: 'Games',
    totalProjects: 72,
    successfulProjects: 48,
    successRate: 66.7,
    averagePercentFunded: 190.5,
    totalPledged: 2100000,
  },
  {
    categoryName: 'Music',
    totalProjects: 40,
    successfulProjects: 28,
    successRate: 70,
    averagePercentFunded: 155.8,
    totalPledged: 880000,
  },
];

export const DEMO_COUNTRY_INSIGHTS: CountryInsight[] = [
  {
    country: 'US',
    totalProjects: 180,
    successfulProjects: 138,
    successRate: 76.7,
    totalPledged: 6800000,
  },
  {
    country: 'GB',
    totalProjects: 65,
    successfulProjects: 47,
    successRate: 72.3,
    totalPledged: 1800000,
  },
  {
    country: 'CA',
    totalProjects: 36,
    successfulProjects: 24,
    successRate: 66.7,
    totalPledged: 920000,
  },
  {
    country: 'DE',
    totalProjects: 30,
    successfulProjects: 19,
    successRate: 63.3,
    totalPledged: 740000,
  },
  {
    country: 'AU',
    totalProjects: 24,
    successfulProjects: 16,
    successRate: 66.7,
    totalPledged: 540000,
  },
];

const MONTHLY_TREND_BASE = [
  { totalProjects: 24, successfulProjects: 16, totalPledged: 812000 },
  { totalProjects: 26, successfulProjects: 18, totalPledged: 834000 },
  { totalProjects: 28, successfulProjects: 19, totalPledged: 851000 },
  { totalProjects: 29, successfulProjects: 21, totalPledged: 864000 },
  { totalProjects: 32, successfulProjects: 23, totalPledged: 882000 },
  { totalProjects: 34, successfulProjects: 24, totalPledged: 906000 },
  { totalProjects: 36, successfulProjects: 26, totalPledged: 924000 },
  { totalProjects: 38, successfulProjects: 27, totalPledged: 948000 },
  { totalProjects: 37, successfulProjects: 26, totalPledged: 972000 },
  { totalProjects: 39, successfulProjects: 28, totalPledged: 995000 },
  { totalProjects: 42, successfulProjects: 30, totalPledged: 1018000 },
  { totalProjects: 44, successfulProjects: 32, totalPledged: 1046000 },
];

export const DEMO_MONTHLY_TREND: MonthlyTrendPoint[] = MONTHLY_TREND_BASE.map((entry, index) => {
  const date = dayjs().subtract(11 - index, 'month').startOf('month');
  return {
    year: date.year(),
    month: date.month() + 1,
    totalProjects: entry.totalProjects,
    successfulProjects: entry.successfulProjects,
    totalPledged: entry.totalPledged,
  };
});

export const DEMO_FUNDING_DISTRIBUTION: FundingDistributionBin[] = [
  {
    label: '<50%',
    minPercent: 0,
    maxPercent: 50,
    totalProjects: 62,
    successfulProjects: 0,
  },
  {
    label: '50%-100%',
    minPercent: 50,
    maxPercent: 100,
    totalProjects: 82,
    successfulProjects: 36,
  },
  {
    label: '100%-200%',
    minPercent: 100,
    maxPercent: 200,
    totalProjects: 142,
    successfulProjects: 118,
  },
  {
    label: '>=200%',
    minPercent: 200,
    maxPercent: Number.POSITIVE_INFINITY,
    totalProjects: 134,
    successfulProjects: 134,
  },
];

export const DEMO_TOP_PROJECTS: ProjectHighlight[] = [
  {
    id: 1,
    name: 'HyperLoop Smart Scooter',
    nameCn: '超循环智能滑板车',
    categoryName: 'Technology',
    country: 'US',
    percentFunded: 412.6,
    pledged: 1280000,
    fundingVelocity: 48230.5,
    backersCount: 8350,
    currency: 'USD',
    launchedAt: dayjs().subtract(45, 'day').toISOString(),
  },
  {
    id: 2,
    name: 'AuroraSense Smart Aroma',
    nameCn: '极光感知香薰系统',
    categoryName: 'Design',
    country: 'CA',
    percentFunded: 286.4,
    pledged: 620000,
    fundingVelocity: 21145.3,
    backersCount: 4680,
    currency: 'CAD',
    launchedAt: dayjs().subtract(32, 'day').toISOString(),
  },
  {
    id: 3,
    name: 'VoyageFit Portable Gym',
    nameCn: 'VoyageFit 旅行健身房',
    categoryName: 'Fashion',
    country: 'US',
    percentFunded: 265.1,
    pledged: 415000,
    fundingVelocity: 15370.1,
    backersCount: 2950,
    currency: 'USD',
    launchedAt: dayjs().subtract(58, 'day').toISOString(),
  },
  {
    id: 4,
    name: 'Nomad Chef Smart Grill',
    nameCn: 'Nomad Chef 智能烧烤炉',
    categoryName: 'Design',
    country: 'DE',
    percentFunded: 249.8,
    pledged: 398000,
    fundingVelocity: 12680.4,
    backersCount: 2080,
    currency: 'EUR',
    launchedAt: dayjs().subtract(74, 'day').toISOString(),
  },
  {
    id: 5,
    name: 'Starlit Interactive Lamp',
    nameCn: '星辉互动灯光',
    categoryName: 'Technology',
    country: 'US',
    percentFunded: 236.1,
    pledged: 325000,
    fundingVelocity: 11420.2,
    backersCount: 1820,
    currency: 'USD',
    launchedAt: dayjs().subtract(28, 'day').toISOString(),
  },
];

export const DEMO_HYPE_PROJECTS: ProjectHighlight[] = [
  {
    id: 99,
    name: 'MetroFlow Smart Bottle',
    nameCn: 'MetroFlow 智能水杯',
    categoryName: 'Technology',
    country: 'GB',
    percentFunded: 312.5,
    pledged: 225000,
    fundingVelocity: 18640.3,
    backersCount: 1750,
    currency: 'GBP',
    launchedAt: dayjs().subtract(21, 'day').toISOString(),
  },
  {
    id: 100,
    name: 'EcoWave Solar Charger',
    nameCn: 'EcoWave 太阳能充电器',
    categoryName: 'Design',
    country: 'CA',
    percentFunded: 198.4,
    pledged: 168000,
    fundingVelocity: 9420.5,
    backersCount: 1430,
    currency: 'CAD',
    launchedAt: dayjs().subtract(18, 'day').toISOString(),
  },
  {
    id: 101,
    name: 'Aurora Yoga Mat',
    nameCn: 'Aurora 智能瑜伽垫',
    categoryName: 'Fashion',
    country: 'AU',
    percentFunded: 184.7,
    pledged: 128000,
    fundingVelocity: 7820.4,
    backersCount: 1260,
    currency: 'AUD',
    launchedAt: dayjs().subtract(15, 'day').toISOString(),
  },
  {
    id: 102,
    name: 'PolarWind Adventure Cooler',
    nameCn: 'PolarWind 户外制冷箱',
    categoryName: 'Design',
    country: 'US',
    percentFunded: 228.9,
    pledged: 296000,
    fundingVelocity: 13640.7,
    backersCount: 2024,
    currency: 'USD',
    launchedAt: dayjs().subtract(26, 'day').toISOString(),
  },
];

export const DEMO_CREATOR_PERFORMANCE: CreatorPerformance[] = [
  {
    creatorId: 1,
    creatorName: 'Wander Labs',
    totalProjects: 4,
    successfulProjects: 4,
    successRate: 100,
    averagePercentFunded: 268.4,
    totalPledged: 1880000,
  },
  {
    creatorId: 2,
    creatorName: 'Aurora Active',
    totalProjects: 3,
    successfulProjects: 3,
    successRate: 100,
    averagePercentFunded: 218.6,
    totalPledged: 980000,
  },
  {
    creatorId: 3,
    creatorName: 'Nomad Chef GmbH',
    totalProjects: 2,
    successfulProjects: 2,
    successRate: 100,
    averagePercentFunded: 245.2,
    totalPledged: 620000,
  },
  {
    creatorId: 4,
    creatorName: 'EcoWave Studio',
    totalProjects: 3,
    successfulProjects: 2,
    successRate: 66.7,
    averagePercentFunded: 192.4,
    totalPledged: 540000,
  },
];

export const DEMO_CATEGORY_KEYWORDS: Record<string, CategoryKeywordInsight[]> = {
  Technology: [
    { keyword: '智能', projectCount: 18, occurrenceCount: 64, averagePercentFunded: 284.6 },
    { keyword: 'AI', projectCount: 12, occurrenceCount: 42, averagePercentFunded: 305.2 },
    { keyword: '便携', projectCount: 9, occurrenceCount: 28, averagePercentFunded: 248.4 },
    { keyword: '跨境', projectCount: 7, occurrenceCount: 20, averagePercentFunded: 236.1 },
    { keyword: '实时监测', projectCount: 6, occurrenceCount: 18, averagePercentFunded: 268.7 },
  ],
  Design: [
    { keyword: '模块化', projectCount: 10, occurrenceCount: 30, averagePercentFunded: 230.2 },
    { keyword: '可持续', projectCount: 9, occurrenceCount: 26, averagePercentFunded: 218.4 },
    { keyword: '旅行', projectCount: 8, occurrenceCount: 22, averagePercentFunded: 205.1 },
    { keyword: '零碳', projectCount: 6, occurrenceCount: 18, averagePercentFunded: 198.6 },
    { keyword: '多场景', projectCount: 5, occurrenceCount: 14, averagePercentFunded: 214.3 },
  ],
  Fashion: [
    { keyword: '便携', projectCount: 8, occurrenceCount: 24, averagePercentFunded: 188.4 },
    { keyword: '运动', projectCount: 7, occurrenceCount: 22, averagePercentFunded: 178.2 },
    { keyword: '智能织物', projectCount: 6, occurrenceCount: 18, averagePercentFunded: 192.6 },
    { keyword: '跨境', projectCount: 5, occurrenceCount: 15, averagePercentFunded: 172.4 },
    { keyword: '旅行', projectCount: 4, occurrenceCount: 12, averagePercentFunded: 184.9 },
  ],
};

const trendDescriptions: Record<AmazonTrendType, string> = {
  NewEntry: '近 24 小时首次进入榜单，热度指数翻倍',
  RankSurge: '排名较昨日提升 15 位以上，建议重点关注',
  ConsistentPerformer: '连续 7 天保持 Top 10，需求稳定',
};

export const DEMO_AMAZON_CORE_METRICS: AmazonCoreMetrics = {
  snapshotId: 1205,
  capturedAt: dayjs().subtract(2, 'hour').toISOString(),
  totalProducts: 240,
  totalNewEntries: 18,
  totalRankSurges: 26,
  totalConsistentPerformers: 34,
};

export const DEMO_AMAZON_PRODUCTS: AmazonProductListItem[] = [
  {
    asin: 'B0CXW1A001',
    title: 'LumiGlow 无线灯带 Pro',
    categoryName: 'Home & Kitchen',
    latestRank: 3,
    latestPrice: 39.99,
    latestRating: 4.7,
    latestReviews: 2250,
    lastUpdated: dayjs().subtract(30, 'minute').toISOString(),
  },
  {
    asin: 'B0CXW1A002',
    title: 'FlexiSteam 折叠挂烫机',
    categoryName: 'Home & Kitchen',
    latestRank: 6,
    latestPrice: 42.5,
    latestRating: 4.5,
    latestReviews: 1280,
    lastUpdated: dayjs().subtract(26, 'minute').toISOString(),
  },
  {
    asin: 'B0CXW1A003',
    title: 'GlideChef 智能空气炸锅 Mini',
    categoryName: 'Home & Kitchen',
    latestRank: 9,
    latestPrice: 49.99,
    latestRating: 4.6,
    latestReviews: 3410,
    lastUpdated: dayjs().subtract(18, 'minute').toISOString(),
  },
  {
    asin: 'B0CXW1A004',
    title: 'GripMax 多功能电动螺丝刀',
    categoryName: 'Tools & Home Improvement',
    latestRank: 5,
    latestPrice: 44.99,
    latestRating: 4.4,
    latestReviews: 980,
    lastUpdated: dayjs().subtract(42, 'minute').toISOString(),
  },
  {
    asin: 'B0CXW1A005',
    title: 'AeroBrew 智能冷萃咖啡杯',
    categoryName: 'Home & Kitchen',
    latestRank: 12,
    latestPrice: 34.99,
    latestRating: 4.2,
    latestReviews: 560,
    lastUpdated: dayjs().subtract(35, 'minute').toISOString(),
  },
  {
    asin: 'B0CXW1A006',
    title: 'PulseFit 体感训练带',
    categoryName: 'Sports & Outdoors',
    latestRank: 8,
    latestPrice: 45.99,
    latestRating: 4.8,
    latestReviews: 1680,
    lastUpdated: dayjs().subtract(22, 'minute').toISOString(),
  },
  {
    asin: 'B0CXW1A007',
    title: 'ZenSteam 速热手持熨斗',
    categoryName: 'Home & Kitchen',
    latestRank: 15,
    latestPrice: 36.99,
    latestRating: 4.3,
    latestReviews: 740,
    lastUpdated: dayjs().subtract(55, 'minute').toISOString(),
  },
  {
    asin: 'B0CXW1A008',
    title: 'VoltShield 迷你电源站 300W',
    categoryName: 'Tools & Home Improvement',
    latestRank: 10,
    latestPrice: 49.5,
    latestRating: 4.6,
    latestReviews: 1250,
    lastUpdated: dayjs().subtract(19, 'minute').toISOString(),
  },
];

const buildTrend = (asin: string, title: string, trendType: AmazonTrendType, hoursAgo: number): AmazonTrendListItem => ({
  asin,
  title,
  trendType,
  description: trendDescriptions[trendType],
  recordedAt: dayjs().subtract(hoursAgo, 'hour').toISOString(),
});

export const DEMO_AMAZON_TRENDS: Record<AmazonTrendType, AmazonTrendListItem[]> = {
  NewEntry: [
    buildTrend('B0CXW1A009', 'FreshNest 折叠收纳车', 'NewEntry', 5),
    buildTrend('B0CXW1A010', 'StreamChef 多功能煮锅', 'NewEntry', 7),
    buildTrend('B0CXW1A011', 'OrbitClean 智能洗碗球', 'NewEntry', 6),
  ],
  RankSurge: [
    buildTrend('B0CXW1A004', 'GripMax 多功能电动螺丝刀', 'RankSurge', 4),
    buildTrend('B0CXW1A006', 'PulseFit 体感训练带', 'RankSurge', 3),
    buildTrend('B0CXW1A012', 'AeroDry 无线吹风机', 'RankSurge', 2),
  ],
  ConsistentPerformer: [
    buildTrend('B0CXW1A003', 'GlideChef 智能空气炸锅 Mini', 'ConsistentPerformer', 8),
    buildTrend('B0CXW1A001', 'LumiGlow 无线灯带 Pro', 'ConsistentPerformer', 10),
    buildTrend('B0CXW1A013', 'PureFlow 桌面净化器', 'ConsistentPerformer', 6),
  ],
};

export const DEMO_AMAZON_HISTORY: AmazonProductHistoryPoint[] = [
  {
    timestamp: dayjs().subtract(6, 'day').toISOString(),
    rank: 24,
    price: 44.99,
    rating: 4.5,
    reviewsCount: 840,
  },
  {
    timestamp: dayjs().subtract(5, 'day').toISOString(),
    rank: 18,
    price: 44.49,
    rating: 4.5,
    reviewsCount: 910,
  },
  {
    timestamp: dayjs().subtract(4, 'day').toISOString(),
    rank: 14,
    price: 43.99,
    rating: 4.6,
    reviewsCount: 980,
  },
  {
    timestamp: dayjs().subtract(3, 'day').toISOString(),
    rank: 11,
    price: 43.49,
    rating: 4.6,
    reviewsCount: 1050,
  },
  {
    timestamp: dayjs().subtract(2, 'day').toISOString(),
    rank: 7,
    price: 42.99,
    rating: 4.6,
    reviewsCount: 1120,
  },
  {
    timestamp: dayjs().subtract(1, 'day').toISOString(),
    rank: 5,
    price: 42.49,
    rating: 4.7,
    reviewsCount: 1180,
  },
  {
    timestamp: dayjs().toISOString(),
    rank: 3,
    price: 41.99,
    rating: 4.7,
    reviewsCount: 1250,
  },
];
