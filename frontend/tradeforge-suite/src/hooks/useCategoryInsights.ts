import { useQuery } from '@tanstack/react-query';
import { fetchCategoryInsights } from '@/services/projectsService';
import type { AnalyticsFilterRequest, CategoryInsight } from '@/types/project';

export const CATEGORY_INSIGHTS_QUERY_KEY = 'category-insights';

export const useCategoryInsights = (filters: AnalyticsFilterRequest) =>
  useQuery<CategoryInsight[]>({
    queryKey: [CATEGORY_INSIGHTS_QUERY_KEY, filters],
    queryFn: () => fetchCategoryInsights(filters),
    staleTime: 1000 * 60 * 5,
  });
