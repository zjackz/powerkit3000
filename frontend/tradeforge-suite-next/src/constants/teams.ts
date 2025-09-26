import type { AnalyticsFilterRequest } from '@/types/project';

export interface TeamProfile {
  key: string;
  name: string;
  description: string;
  focusCategories?: string[];
  focusCountries?: string[];
  defaultProjectFilters?: AnalyticsFilterRequest;
}

export const TEAM_PROFILES: TeamProfile[] = [
  {
    key: 'global-hq',
    name: '全球 HQ',
    description: '统筹 Kickstarter + Amazon 双域策略，负责全局指标监控。',
    defaultProjectFilters: { minPercentFunded: 200 },
  },
  {
    key: 'na-crossborder',
    name: '北美跨境组',
    description: '聚焦美国 / 加拿大生活类目，偏好高达成率项目。',
    focusCountries: ['US', 'CA'],
    focusCategories: ['Design', 'Technology'],
    defaultProjectFilters: { countries: ['US', 'CA'], minPercentFunded: 150 },
  },
  {
    key: 'eu-growth',
    name: '欧洲增长组',
    description: '围绕德英站家居 / 设计品类追踪新品势能。',
    focusCountries: ['DE', 'GB'],
    focusCategories: ['Design'],
    defaultProjectFilters: { countries: ['DE', 'GB'], categories: ['Design'], minPercentFunded: 120 },
  },
];

export const DEFAULT_TEAM_KEY = TEAM_PROFILES[0]?.key ?? 'global-hq';
