import { useQuery } from '@tanstack/react-query';
import { fetchCreatorPerformance } from '@/services/projectsService';
import type { AnalyticsFilterRequest, CreatorPerformance } from '@/types/project';

export const CREATOR_PERFORMANCE_QUERY_KEY = 'creator-performance';

export const useCreatorPerformance = (filters: AnalyticsFilterRequest) =>
  useQuery<CreatorPerformance[]>({
    queryKey: [CREATOR_PERFORMANCE_QUERY_KEY, filters],
    queryFn: () => fetchCreatorPerformance(filters),
    staleTime: 1000 * 60 * 5,
  });
