import { useQuery } from '@tanstack/react-query';
import { fetchProjectSummary } from '@/services/projectsService';

export const PROJECT_SUMMARY_QUERY_KEY = 'project-summary';

export const useProjectSummary = () => {
  return useQuery({
    queryKey: [PROJECT_SUMMARY_QUERY_KEY],
    queryFn: () => fetchProjectSummary(),
    staleTime: 1000 * 60 * 5,
  });
};
