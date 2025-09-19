import { PROJECTS_MOCK } from '@/mocks/projects';
import { DEFAULT_PAGE_SIZE } from '@/constants/projectOptions';
import {
  AnalyticsFilterRequest,
  CategoryInsight,
  CountryInsight,
  CreatorPerformance,
  FundingDistributionBin,
  MonthlyTrendPoint,
  ProjectFilters,
  ProjectHighlight,
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

export const fetchProjectSummary = async (
  params: AnalyticsFilterRequest,
  useMockFallback = true,
): Promise<ProjectSummary> => {
  try {
    const response = await httpClient.get<ProjectSummary>('/projects/summary', { params });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    const filtered = PROJECTS_MOCK.filter((project) => {
      if (params.launchedAfter && new Date(project.launchedAt) < new Date(params.launchedAfter)) return false;
      if (params.launchedBefore && new Date(project.launchedAt) > new Date(params.launchedBefore)) return false;
      if (params.countries?.length && !params.countries.includes(project.country)) return false;
      if (params.categories?.length && !params.categories.includes(project.categoryName)) return false;
      return true;
    });

    const totalProjects = filtered.length;
    const successfulProjects = filtered.filter((project) => project.state === 'successful').length;
    const totalPledged = filtered.reduce((acc, project) => acc + project.pledged, 0);
    const distinctCountries = new Set(filtered.map((project) => project.country)).size;
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

export const fetchCategoryInsights = async (
  params: AnalyticsFilterRequest,
  useMockFallback = true,
): Promise<CategoryInsight[]> => {
  try {
    const response = await httpClient.get<CategoryInsight[]>('/analytics/categories', { params });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    const grouped = PROJECTS_MOCK.filter((project) => {
      if (params.launchedAfter && new Date(project.launchedAt) < new Date(params.launchedAfter)) return false;
      if (params.launchedBefore && new Date(project.launchedAt) > new Date(params.launchedBefore)) return false;
      if (params.countries?.length && !params.countries.includes(project.country)) return false;
      if (params.categories?.length && !params.categories.includes(project.categoryName)) return false;
      return true;
    }).reduce<Record<string, CategoryInsight>>((acc, project) => {
      const key = project.categoryName;
      const entry = acc[key] ?? {
        categoryName: key,
        totalProjects: 0,
        successfulProjects: 0,
        successRate: 0,
        averagePercentFunded: 0,
        totalPledged: 0,
      };

      const updated = {
        ...entry,
        totalProjects: entry.totalProjects + 1,
        successfulProjects: entry.successfulProjects + (project.state === 'successful' ? 1 : 0),
        averagePercentFunded: entry.averagePercentFunded + project.percentFunded,
        totalPledged: Number((entry.totalPledged + project.pledged).toFixed(2)),
      };

      acc[key] = updated;
      return acc;
    }, {});

    return Object.values(grouped)
      .map((item) => ({
        ...item,
        averagePercentFunded: Number((item.averagePercentFunded / (item.totalProjects || 1)).toFixed(1)),
        successRate: Number(((item.successfulProjects / (item.totalProjects || 1)) * 100).toFixed(1)),
      }))
      .sort((a, b) => b.successRate - a.successRate);
  }
};

export const fetchCountryInsights = async (
  params: AnalyticsFilterRequest,
  useMockFallback = true,
): Promise<CountryInsight[]> => {
  try {
    const response = await httpClient.get<CountryInsight[]>('/analytics/countries', { params });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    const grouped = PROJECTS_MOCK.filter((project) => {
      if (params.launchedAfter && new Date(project.launchedAt) < new Date(params.launchedAfter)) return false;
      if (params.launchedBefore && new Date(project.launchedAt) > new Date(params.launchedBefore)) return false;
      if (params.countries?.length && !params.countries.includes(project.country)) return false;
      if (params.categories?.length && !params.categories.includes(project.categoryName)) return false;
      return true;
    }).reduce<Record<string, CountryInsight>>((acc, project) => {
      const key = project.country;
      const entry = acc[key] ?? {
        country: key,
        totalProjects: 0,
        successfulProjects: 0,
        successRate: 0,
        totalPledged: 0,
      };

      const updated = {
        ...entry,
        totalProjects: entry.totalProjects + 1,
        successfulProjects: entry.successfulProjects + (project.state === 'successful' ? 1 : 0),
        totalPledged: Number((entry.totalPledged + project.pledged).toFixed(2)),
      };

      acc[key] = updated;
      return acc;
    }, {});

    return Object.values(grouped)
      .map((item) => ({
        ...item,
        successRate: Number(((item.successfulProjects / (item.totalProjects || 1)) * 100).toFixed(1)),
      }))
      .sort((a, b) => b.successRate - a.successRate);
  }
};

export const fetchTopProjects = async (
  params: AnalyticsFilterRequest,
  useMockFallback = true,
): Promise<ProjectHighlight[]> => {
  try {
    const response = await httpClient.get<ProjectHighlight[]>('/analytics/top-projects', { params });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    return PROJECTS_MOCK.filter((project) => {
      if (params.launchedAfter && new Date(project.launchedAt) < new Date(params.launchedAfter)) return false;
      if (params.launchedBefore && new Date(project.launchedAt) > new Date(params.launchedBefore)) return false;
      if (params.countries?.length && !params.countries.includes(project.country)) return false;
      if (params.categories?.length && !params.categories.includes(project.categoryName)) return false;
      return true;
    })
      .sort((a, b) => b.percentFunded - a.percentFunded)
      .slice(0, 10)
      .map((project) => ({
        id: project.id,
        name: project.name,
        categoryName: project.categoryName,
        country: project.country,
        percentFunded: project.percentFunded,
        pledged: project.pledged,
        backersCount: project.backersCount,
        currency: project.currency,
      launchedAt: project.launchedAt,
    }));
  }
};

export const fetchMonthlyTrend = async (
  params: AnalyticsFilterRequest,
  useMockFallback = true,
): Promise<MonthlyTrendPoint[]> => {
  try {
    const response = await httpClient.get<MonthlyTrendPoint[]>('/analytics/monthly-trend', { params });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    const projects = PROJECTS_MOCK.filter((project) => {
      if (params.launchedAfter && new Date(project.launchedAt) < new Date(params.launchedAfter)) return false;
      if (params.launchedBefore && new Date(project.launchedAt) > new Date(params.launchedBefore)) return false;
      if (params.countries?.length && !params.countries.includes(project.country)) return false;
      if (params.categories?.length && !params.categories.includes(project.categoryName)) return false;
      return true;
    }).map((project) => ({
      launchedAt: project.launchedAt,
      pledged: project.pledged,
      state: project.state,
    }));

    const grouped = projects.reduce<Record<string, MonthlyTrendPoint>>((acc, project) => {
      const date = new Date(project.launchedAt);
      const key = `${date.getUTCFullYear()}-${date.getUTCMonth() + 1}`;
      const entry = acc[key] ?? {
        year: date.getUTCFullYear(),
        month: date.getUTCMonth() + 1,
        totalProjects: 0,
        successfulProjects: 0,
        totalPledged: 0,
      };

      acc[key] = {
        ...entry,
        totalProjects: entry.totalProjects + 1,
        successfulProjects: entry.successfulProjects + (project.state === 'successful' ? 1 : 0),
        totalPledged: Number((entry.totalPledged + project.pledged).toFixed(2)),
      };

      return acc;
    }, {});

    return Object.values(grouped).sort((a, b) => (a.year === b.year ? a.month - b.month : a.year - b.year));
  }
};

export const fetchFundingDistribution = async (
  params: AnalyticsFilterRequest,
  useMockFallback = true,
): Promise<FundingDistributionBin[]> => {
  try {
    const response = await httpClient.get<FundingDistributionBin[]>('/analytics/funding-distribution', { params });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    const bins = [
      { label: '<50%', minPercent: 0, maxPercent: 50 },
      { label: '50%-100%', minPercent: 50, maxPercent: 100 },
      { label: '100%-200%', minPercent: 100, maxPercent: 200 },
      { label: '>=200%', minPercent: 200, maxPercent: Number.POSITIVE_INFINITY },
    ];

    return bins.map((bin) => {
      const matching = PROJECTS_MOCK.filter((project) => {
        if (params.launchedAfter && new Date(project.launchedAt) < new Date(params.launchedAfter)) return false;
        if (params.launchedBefore && new Date(project.launchedAt) > new Date(params.launchedBefore)) return false;
        if (params.countries?.length && !params.countries.includes(project.country)) return false;
        if (params.categories?.length && !params.categories.includes(project.categoryName)) return false;
        return project.percentFunded >= bin.minPercent && project.percentFunded < bin.maxPercent;
      });
      const successful = matching.filter((project) => project.state === 'successful').length;

      return {
        label: bin.label,
        minPercent: bin.minPercent,
        maxPercent: bin.maxPercent,
        totalProjects: matching.length,
        successfulProjects: successful,
      };
    });
  }
};

export const fetchCreatorPerformance = async (
  params: AnalyticsFilterRequest,
  useMockFallback = true,
): Promise<CreatorPerformance[]> => {
  try {
    const response = await httpClient.get<CreatorPerformance[]>('/analytics/creators', { params });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    const grouped = PROJECTS_MOCK.filter((project) => {
      if (params.launchedAfter && new Date(project.launchedAt) < new Date(params.launchedAfter)) return false;
      if (params.launchedBefore && new Date(project.launchedAt) > new Date(params.launchedBefore)) return false;
      if (params.countries?.length && !params.countries.includes(project.country)) return false;
      if (params.categories?.length && !params.categories.includes(project.categoryName)) return false;
      return true;
    }).reduce<Record<number, CreatorPerformance>>((acc, project, index) => {
      const key = project.creatorName ? index : index; // fallback when no creator id in mock
      const entry = acc[key] ?? {
        creatorId: key,
        creatorName: project.creatorName ?? 'Unknown',
        totalProjects: 0,
        successfulProjects: 0,
        successRate: 0,
        averagePercentFunded: 0,
        totalPledged: 0,
      };

      const updated = {
        ...entry,
        totalProjects: entry.totalProjects + 1,
        successfulProjects: entry.successfulProjects + (project.state === 'successful' ? 1 : 0),
        averagePercentFunded: entry.averagePercentFunded + project.percentFunded,
        totalPledged: Number((entry.totalPledged + project.pledged).toFixed(2)),
      };

      acc[key] = updated;
      return acc;
    }, {});

    return Object.values(grouped)
      .map((item) => ({
        ...item,
        averagePercentFunded: Number((item.averagePercentFunded / (item.totalProjects || 1)).toFixed(1)),
        successRate: Number(((item.successfulProjects / (item.totalProjects || 1)) * 100).toFixed(1)),
      }))
      .sort((a, b) => b.successRate - a.successRate)
      .slice(0, 10);
  }
};
