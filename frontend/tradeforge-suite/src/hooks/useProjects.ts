import { useMemo } from 'react';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { ProjectQueryParams, ProjectQueryResponse } from '@/types/project';
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

  const queryResult = useQuery<ProjectQueryResponse>({
    queryKey: [PROJECTS_QUERY_KEY, normalizedParams],
    queryFn: () => fetchProjects(normalizedParams),
    placeholderData: keepPreviousData,
  });

  return queryResult;
};
