import { useQuery } from '@tanstack/react-query';
import { fetchCountryInsights } from '@/services/projectsService';
import type { AnalyticsFilterRequest, CountryInsight } from '@/types/project';

export const COUNTRY_INSIGHTS_QUERY_KEY = 'country-insights';

export const useCountryInsights = (filters: AnalyticsFilterRequest) =>
  useQuery<CountryInsight[]>({
    queryKey: [COUNTRY_INSIGHTS_QUERY_KEY, filters],
    queryFn: () => fetchCountryInsights(filters),
    staleTime: 1000 * 60 * 5,
  });
