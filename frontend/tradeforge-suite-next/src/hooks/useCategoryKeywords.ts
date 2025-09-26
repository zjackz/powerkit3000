import { useQuery } from '@tanstack/react-query';
import { fetchCategoryKeywords } from '@/services/projectsService';
import type { AnalyticsFilterRequest, CategoryKeywordInsight } from '@/types/project';

export const CATEGORY_KEYWORDS_QUERY_KEY = 'category-keywords';

export const useCategoryKeywords = (category: string | undefined, filters: AnalyticsFilterRequest) =>
  useQuery<CategoryKeywordInsight[]>({
    queryKey: [CATEGORY_KEYWORDS_QUERY_KEY, category, filters],
    queryFn: () => fetchCategoryKeywords(category!, filters),
    staleTime: 1000 * 60 * 10,
    enabled: Boolean(category),
  });
