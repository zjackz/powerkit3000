import { useQuery } from '@tanstack/react-query';
import { fetchProjectFilters } from '@/services/projectsService';

export const PROJECT_FILTERS_QUERY_KEY = 'project-filters';

export const useProjectFilters = () => {
  return useQuery({
    queryKey: [PROJECT_FILTERS_QUERY_KEY],
    queryFn: () => fetchProjectFilters(),
    staleTime: 1000 * 60 * 10,
  });
};
