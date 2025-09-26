import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('@/services/httpClient', () => ({
  httpClient: {
    get: vi.fn(),
  },
}));

vi.mock('@/utils/apiNotifications', () => ({
  notifyApiFallback: vi.fn(),
  notifyApiError: vi.fn(),
}));

import { httpClient } from '@/services/httpClient';
import { notifyApiError, notifyApiFallback } from '@/utils/apiNotifications';
import {
  fetchAmazonCoreMetrics,
  fetchAmazonProducts,
  fetchAmazonProductHistory,
  fetchAmazonTrends,
  fetchLatestAmazonReport,
} from '@/services/amazonService';
import {
  AMAZON_CORE_METRICS_MOCK,
  AMAZON_REPORT_MOCK,
  AMAZON_TRENDS_MOCK,
} from '@/mocks/amazon';

const mockedGet = httpClient.get as unknown as ReturnType<typeof vi.fn>;

beforeEach(() => {
  vi.clearAllMocks();
  mockedGet.mockRejectedValue(new Error('network-error'));
});

describe('amazonService fallbacks', () => {
  it('returns mock metrics when API fails', async () => {
    const result = await fetchAmazonCoreMetrics();
    expect(result).toEqual(AMAZON_CORE_METRICS_MOCK);
    expect(notifyApiFallback).toHaveBeenCalledWith('Amazon 核心指标');
  });

  it('returns filtered mock products when API fails', async () => {
    const result = await fetchAmazonProducts({ search: 'projector' });
    expect(result).toHaveLength(1);
    expect(result[0]?.asin).toBe('B0PKSMART01');
    expect(notifyApiFallback).toHaveBeenCalledWith('Amazon 榜单产品');
  });

  it('returns mock trends when API fails', async () => {
    const result = await fetchAmazonTrends({ trendType: 'RankSurge' });
    expect(result).toEqual(AMAZON_TRENDS_MOCK.filter((item) => item.trendType === 'RankSurge'));
    expect(notifyApiFallback).toHaveBeenCalledWith('Amazon 趋势');
  });

  it('returns mock history when API fails', async () => {
    const result = await fetchAmazonProductHistory('B0PKSMART01');
    expect(result).not.toHaveLength(0);
    expect(notifyApiFallback).toHaveBeenCalledWith('Amazon 产品历史');
  });

  it('returns mock report when API fails', async () => {
    const result = await fetchLatestAmazonReport();
    expect(result).toEqual(AMAZON_REPORT_MOCK);
    expect(notifyApiFallback).toHaveBeenCalledWith('Amazon 最新报告');
  });

  it('propagates error when fallback disabled', async () => {
    await expect(fetchAmazonProducts({}, { useMockFallback: false })).rejects.toThrow('network-error');
    expect(notifyApiError).toHaveBeenCalledWith('Amazon 榜单产品', expect.any(Error));
    expect(notifyApiFallback).not.toHaveBeenCalled();
  });
});
