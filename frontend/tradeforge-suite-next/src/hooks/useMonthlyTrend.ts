import { useQuery } from '@tanstack/react-query';
import { fetchMonthlyTrend } from '@/services/projectsService';
import type { AnalyticsFilterRequest, MonthlyTrendPoint } from '@/types/project';

export const MONTHLY_TREND_QUERY_KEY = 'monthly-trend';

export const useMonthlyTrend = (filters: AnalyticsFilterRequest) =>
  useQuery<MonthlyTrendPoint[]>({
    queryKey: [MONTHLY_TREND_QUERY_KEY, filters],
    queryFn: () => fetchMonthlyTrend(filters),
    staleTime: 1000 * 60 * 5,
  });
