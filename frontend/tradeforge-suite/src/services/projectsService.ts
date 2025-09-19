import { PROJECTS_MOCK } from '@/mocks/projects';
import { DEFAULT_PAGE_SIZE } from '@/constants/projectOptions';
import {
  ProjectFilters,
  ProjectQueryParams,
  ProjectQueryResponse,
  ProjectSummary,
} from '@/types/project';
import { httpClient } from './httpClient';

const filterProjects = (params: ProjectQueryParams): ProjectQueryResponse => {
  const {
    search,
    states,
    countries,
    categories,
    minGoal,
    maxGoal,
    minPercentFunded,
    launchedAfter,
    launchedBefore,
    page = 1,
    pageSize = DEFAULT_PAGE_SIZE,
  } = params;

  const filtered = PROJECTS_MOCK.filter((project) => {
    if (states && states.length > 0 && !states.includes(project.state)) {
      return false;
    }

    if (countries && countries.length > 0 && !countries.includes(project.country)) {
      return false;
    }

    if (categories && categories.length > 0 && !categories.includes(project.categoryName)) {
      return false;
    }

    if (minGoal !== undefined && project.goal < minGoal) {
      return false;
    }

    if (maxGoal !== undefined && project.goal > maxGoal) {
      return false;
    }

    if (minPercentFunded !== undefined && project.percentFunded < minPercentFunded) {
      return false;
    }

    if (launchedAfter && new Date(project.launchedAt) < new Date(launchedAfter)) {
      return false;
    }

    if (launchedBefore && new Date(project.launchedAt) > new Date(launchedBefore)) {
      return false;
    }

    if (search) {
      const lowered = search.toLowerCase();
      const hit = [
        project.name,
        project.blurb,
        project.categoryName,
        project.creatorName,
        project.locationName ?? '',
      ]
        .filter(Boolean)
        .some((value) => value.toLowerCase().includes(lowered));

      if (!hit) {
        return false;
      }
    }

    return true;
  });

  const start = (page - 1) * pageSize;
  const end = start + pageSize;

  return {
    total: filtered.length,
    items: filtered.slice(start, end),
  };
};

export const fetchProjects = async (
  params: ProjectQueryParams,
  useMockFallback = true,
): Promise<ProjectQueryResponse> => {
  try {
    const response = await httpClient.get<ProjectQueryResponse>('/projects', {
      params,
    });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    console.warn('API 不可用，使用本地模拟数据。');
    return new Promise((resolve) => {
      setTimeout(() => resolve(filterProjects(params)), 300);
    });
  }
};

export const fetchProjectFilters = async (useMockFallback = true): Promise<ProjectFilters> => {
  try {
    const response = await httpClient.get<ProjectFilters>('/filters');
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    return {
      states: Array.from(new Set(PROJECTS_MOCK.map((project) => project.state))).sort(),
      countries: Array.from(new Set(PROJECTS_MOCK.map((project) => project.country))).sort(),
      categories: Array.from(new Set(PROJECTS_MOCK.map((project) => project.categoryName))).sort(),
    };
  }
};

export const fetchProjectSummary = async (useMockFallback = true): Promise<ProjectSummary> => {
  try {
    const response = await httpClient.get<ProjectSummary>('/projects/summary');
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    const totalProjects = PROJECTS_MOCK.length;
    const successfulProjects = PROJECTS_MOCK.filter((project) => project.state === 'successful').length;
    const totalPledged = PROJECTS_MOCK.reduce((acc, project) => acc + project.pledged, 0);
    const distinctCountries = new Set(PROJECTS_MOCK.map((project) => project.country)).size;
    const successRate = totalProjects === 0 ? 0 : Math.round((successfulProjects / totalProjects) * 1000) / 10;

    return {
      totalProjects,
      successfulProjects,
      totalPledged,
      distinctCountries,
      successRate,
    };
  }
};
