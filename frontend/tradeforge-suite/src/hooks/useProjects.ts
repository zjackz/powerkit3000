import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ProjectQueryParams } from '@/types/project';
import { DEFAULT_PAGE_SIZE } from '@/constants/projectOptions';
import { fetchProjects } from '@/services/projectsService';

export const PROJECTS_QUERY_KEY = 'projects';

export const useProjects = (params: ProjectQueryParams = {}) => {
  const normalizedParams = useMemo(() => {
    return {
      page: 1,
      pageSize: DEFAULT_PAGE_SIZE,
      ...params,
    };
  }, [params]);

  const queryResult = useQuery({
    queryKey: [PROJECTS_QUERY_KEY, normalizedParams],
    queryFn: () => fetchProjects(normalizedParams),
    keepPreviousData: true,
  });

  return queryResult;
};
