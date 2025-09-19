import { useQuery } from '@tanstack/react-query';
import { fetchProjectSummary } from '@/services/projectsService';
import type { AnalyticsFilterRequest, ProjectSummary } from '@/types/project';

export const PROJECT_SUMMARY_QUERY_KEY = 'project-summary';

export const useProjectSummary = (filters: AnalyticsFilterRequest) => {
  return useQuery<ProjectSummary>({
    queryKey: [PROJECT_SUMMARY_QUERY_KEY, filters],
    queryFn: () => fetchProjectSummary(filters),
    staleTime: 1000 * 60 * 5,
  });
};
