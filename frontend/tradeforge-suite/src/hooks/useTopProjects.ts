import { useQuery } from '@tanstack/react-query';
import { fetchTopProjects } from '@/services/projectsService';
import type { AnalyticsFilterRequest, ProjectHighlight } from '@/types/project';

export const TOP_PROJECTS_QUERY_KEY = 'top-projects';

export const useTopProjects = (filters: AnalyticsFilterRequest) =>
  useQuery<ProjectHighlight[]>({
    queryKey: [TOP_PROJECTS_QUERY_KEY, filters],
    queryFn: () => fetchTopProjects(filters),
    staleTime: 1000 * 60 * 5,
  });
