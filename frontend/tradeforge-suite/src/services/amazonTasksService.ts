import dayjs from 'dayjs';
import type { AmazonTask, AmazonTaskCategorySelector, AmazonTaskFilterRules, AmazonTaskKeywordRules, AmazonTaskLimits, AmazonTaskPriceRange, AmazonTaskSchedule } from '@/types/amazon';

const generateId = () => {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return `task_${Math.random().toString(36).slice(2, 10)}`;
};

const createTemplateTask = (overrides: Partial<AmazonTask>): AmazonTask => {
  const now = new Date().toISOString();
  const base: AmazonTask = {
    id: generateId(),
    name: 'new_task',
    site: 'amazon.com',
    categories: [{ type: 'url', value: '' }],
    leaderboards: ['BestSellers'],
    priceRange: { min: 30, max: 50 },
    keywords: { include: [], exclude: [] },
    filters: { minRating: 4, minReviews: 25 },
    schedule: { type: 'recurring', cron: '0 30 2 * * *', timezone: 'UTC' },
    limits: { maxProducts: 200, maxRequestsPerHour: 400 },
    proxyPolicy: 'default',
    status: 'draft',
    notes: '',
    llmSummary: '',
    createdAt: now,
    updatedAt: now,
  };
  return {
    ...base,
    ...overrides,
    categories: overrides.categories ?? base.categories,
    leaderboards: overrides.leaderboards ?? base.leaderboards,
    priceRange: overrides.priceRange ?? base.priceRange,
    keywords: overrides.keywords ?? base.keywords,
    filters: overrides.filters ?? base.filters,
    schedule: overrides.schedule ?? base.schedule,
    limits: overrides.limits ?? base.limits,
    createdAt: overrides.createdAt ?? base.createdAt,
    updatedAt: overrides.updatedAt ?? base.updatedAt,
  };
};

let mockTasks: AmazonTask[] = [
  createTemplateTask({
    id: 'home_new_releases_30_50',
    name: 'home_new_releases_30_50',
    categories: [
      { type: 'url', value: 'https://www.amazon.com/gp/new-releases/home-garden/ref=zg_bsnr_nav_home-garden_0' },
    ],
    leaderboards: ['NewReleases'],
    status: 'active',
    notes: 'MVP: 家居新上架（30-50 USD）',
    llmSummary:
      '每日 02:30 UTC 抓取家居新品榜单，聚焦 30-50 美金段的储物收纳与小家电，建议重点关注评论增速 > 30 的商品。',
    createdAt: dayjs().subtract(2, 'day').toISOString(),
    updatedAt: dayjs().subtract(6, 'hour').toISOString(),
  }),
  createTemplateTask({
    id: 'garden_best_sellers_30_50',
    name: 'garden_best_sellers_30_50',
    categories: [
      { type: 'url', value: 'https://www.amazon.com/Patio-Lawn-Garden/b/ref=dp_bc_1?ie=UTF8&node=2972638011' },
    ],
    leaderboards: ['BestSellers'],
    status: 'active',
    notes: 'MVP: 花园热销（30-50 USD）',
    llmSummary:
      '重点关注园艺工具与户外照明；若排名飙升且平均评分高于 4.3，可优先推送运营跟进供应链。',
    createdAt: dayjs().subtract(3, 'day').toISOString(),
    updatedAt: dayjs().subtract(3, 'hour').toISOString(),
  }),
];

const cloneTask = (task: AmazonTask): AmazonTask => ({
  ...task,
  categories: task.categories.map((item) => ({ ...item })),
  leaderboards: [...task.leaderboards],
  priceRange: { ...task.priceRange },
  keywords: { include: [...task.keywords.include], exclude: [...task.keywords.exclude] },
  filters: { ...task.filters },
  schedule: { ...task.schedule },
  limits: { ...task.limits },
});

export const fetchAmazonTasks = async (): Promise<AmazonTask[]> => {
  return Promise.resolve(mockTasks.map(cloneTask));
};

export const upsertAmazonTask = async (task: AmazonTask): Promise<void> => {
  const index = mockTasks.findIndex((item) => item.id === task.id);
  if (index >= 0) {
    mockTasks[index] = cloneTask(task);
  } else {
    mockTasks = [cloneTask(task), ...mockTasks];
  }
  return Promise.resolve();
};

export const createAmazonTaskDraft = (overrides?: Partial<AmazonTask>): AmazonTask => {
  return createTemplateTask({
    id: generateId(),
    name: `task_${dayjs().format('MMDD_HHmm')}`,
    ...overrides,
  });
};

export const summarizeTask = (task: AmazonTask): string => {
  const categorySources = task.categories.map((c) => (c.type === 'url' ? 'URL' : '节点')).join(' / ');
  const boardText = task.leaderboards.join('、');
  const priceText = [task.priceRange.min ?? '0', task.priceRange.max ?? '∞'].join(' - ');
  return `任务聚焦 ${task.site} ${boardText}，类目来源 (${categorySources})，目标价位 ${priceText} 美元。建议在抓取后复核排名与评论异常。`;
};
