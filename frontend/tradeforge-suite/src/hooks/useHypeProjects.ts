import { useQuery } from '@tanstack/react-query';
import { fetchHypeProjects } from '@/services/projectsService';
import type { AnalyticsFilterRequest, ProjectHighlight } from '@/types/project';

export const HYPE_PROJECTS_QUERY_KEY = 'hype-projects';

export const useHypeProjects = (
  params: AnalyticsFilterRequest & { minPercentFunded?: number; limit?: number },
) =>
  useQuery<ProjectHighlight[]>({
    queryKey: [HYPE_PROJECTS_QUERY_KEY, params],
    queryFn: () => fetchHypeProjects(params),
    staleTime: 1000 * 60 * 5,
  });
