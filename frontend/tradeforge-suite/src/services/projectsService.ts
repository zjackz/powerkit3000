import dayjs from 'dayjs';
import { PROJECTS_MOCK } from '@/mocks/projects';
import { DEFAULT_PAGE_SIZE, PROJECT_CATEGORIES, PROJECT_COUNTRIES, PROJECT_STATES } from '@/constants/projectOptions';
import {
  AnalyticsFilterRequest,
  CategoryInsight,
  CategoryKeywordInsight,
  CountryInsight,
  CreatorPerformance,
  FundingDistributionBin,
  MonthlyTrendPoint,
  Project,
  ProjectFilters,
  ProjectHighlight,
  ProjectQueryParams,
  ProjectQueryResponse,
  ProjectQueryStats,
  ProjectSummary,
  ProjectFavoriteRecord,
} from '@/types/project';
import { httpClient } from './httpClient';

const buildQueryStats = (projects: Project[]): ProjectQueryStats => {
  const total = projects.length;
  if (total === 0) {
    return {
      successfulCount: 0,
      totalPledged: 0,
      averagePercentFunded: 0,
      totalBackers: 0,
      averageGoal: 0,
      topProject: null,
    };
  }

  const successfulCount = projects.filter((project) => project.state === 'successful').length;
  const totalPledged = projects.reduce((sum, project) => sum + project.pledged, 0);
  const totalBackers = projects.reduce((sum, project) => sum + project.backersCount, 0);
  const averagePercentFunded = Math.round(
    (projects.reduce((sum, project) => sum + project.percentFunded, 0) / total) * 10,
  ) / 10;
  const averageGoal = Math.round(
    (projects.reduce((sum, project) => sum + project.goal, 0) / total) * 100,
  ) / 100;
  const topProject = [...projects].sort((a, b) => {
    if (b.percentFunded === a.percentFunded) {
      return b.pledged - a.pledged;
    }
    return b.percentFunded - a.percentFunded;
  })[0];

  return {
    successfulCount,
    totalPledged,
    averagePercentFunded,
    totalBackers,
    averageGoal,
    topProject,
  };
};

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
      .some((value) => value?.toLowerCase().includes(lowered) ?? false);

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
    stats: buildQueryStats(filtered),
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
    if (response.data.stats) {
      return response.data;
    }

    // 兼容旧版本后端：如果缺少 stats 字段，则在前端补齐
    const derivedStats = buildQueryStats(response.data.items);
    return {
      ...response.data,
      stats: derivedStats,
    };
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

    const stateCounts = PROJECTS_MOCK.reduce<Record<string, number>>((acc, project) => {
      acc[project.state] = (acc[project.state] ?? 0) + 1;
      return acc;
    }, {});

    const countryCounts = PROJECTS_MOCK.reduce<Record<string, number>>((acc, project) => {
      acc[project.country] = (acc[project.country] ?? 0) + 1;
      return acc;
    }, {});

    const categoryCounts = PROJECTS_MOCK.reduce<Record<string, number>>((acc, project) => {
      acc[project.categoryName] = (acc[project.categoryName] ?? 0) + 1;
      return acc;
    }, {});

    const states = Object.entries(stateCounts)
      .map(([value, count]) => ({
        value,
        label: PROJECT_STATES.find((item) => item.value === value)?.label ?? value,
        count,
      }))
      .sort((a, b) => (b.count - a.count) || a.label.localeCompare(b.label));

    const countries = Object.entries(countryCounts)
      .map(([value, count]) => ({
        value,
        label: PROJECT_COUNTRIES.find((item) => item.value === value)?.label ?? value,
        count,
      }))
      .sort((a, b) => (b.count - a.count) || a.label.localeCompare(b.label));

    const categories = Object.entries(categoryCounts)
      .map(([value, count]) => ({
        value,
        label: PROJECT_CATEGORIES.find((item) => item.value === value)?.label ?? value,
        count,
      }))
      .sort((a, b) => (b.count - a.count) || a.label.localeCompare(b.label));

    return {
      states,
      countries,
      categories,
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
        nameCn: project.nameCn ?? project.name,
        categoryName: project.categoryName,
        country: project.country,
        percentFunded: project.percentFunded,
        pledged: project.pledged,
        fundingVelocity: project.fundingVelocity,
        backersCount: project.backersCount,
        currency: project.currency,
        launchedAt: project.launchedAt,
      }));
  }
};

export const fetchHypeProjects = async (
  params: AnalyticsFilterRequest & { limit?: number; minPercentFunded?: number },
  useMockFallback = true,
): Promise<ProjectHighlight[]> => {
  const { limit, ...filters } = params;
  try {
    const response = await httpClient.get<ProjectHighlight[]>('/analytics/hype', {
      params: { ...filters, limit, minPercentFunded: filters.minPercentFunded ?? params.minPercentFunded },
    });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    const threshold = filters.minPercentFunded ?? params.minPercentFunded ?? 200;
    return PROJECTS_MOCK.filter((project) => {
      if (filters.launchedAfter && new Date(project.launchedAt) < new Date(filters.launchedAfter)) return false;
      if (filters.launchedBefore && new Date(project.launchedAt) > new Date(filters.launchedBefore)) return false;
      if (filters.countries?.length && !filters.countries.includes(project.country)) return false;
      if (filters.categories?.length && !filters.categories.includes(project.categoryName)) return false;
      if (threshold && project.percentFunded < threshold) return false;
      return true;
    })
      .sort((a, b) => {
        if (b.fundingVelocity === a.fundingVelocity) {
          if (b.percentFunded === a.percentFunded) {
            return b.pledged - a.pledged;
          }
          return b.percentFunded - a.percentFunded;
        }
        return b.fundingVelocity - a.fundingVelocity;
      })
      .slice(0, limit ?? 8)
      .map((project) => ({
        id: project.id,
        name: project.name,
        nameCn: project.nameCn ?? project.name,
        categoryName: project.categoryName,
        country: project.country,
        percentFunded: project.percentFunded,
        pledged: project.pledged,
        fundingVelocity: project.fundingVelocity,
        backersCount: project.backersCount,
        currency: project.currency,
        launchedAt: project.launchedAt,
      }));
  }
};

export const fetchCategoryKeywords = async (
  category: string,
  params: AnalyticsFilterRequest,
  useMockFallback = true,
): Promise<CategoryKeywordInsight[]> => {
  try {
    const response = await httpClient.get<CategoryKeywordInsight[]>('/analytics/category-keywords', {
      params: { ...params, category },
    });
    return response.data;
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    const projects = PROJECTS_MOCK.filter((project) => {
      if (project.categoryName !== category) return false;
      if (project.state !== 'successful') return false;
      if (params.launchedAfter && new Date(project.launchedAt) < new Date(params.launchedAfter)) return false;
      if (params.launchedBefore && new Date(project.launchedAt) > new Date(params.launchedBefore)) return false;
      if (params.countries?.length && !params.countries.includes(project.country)) return false;
      if (params.minPercentFunded && project.percentFunded < params.minPercentFunded) return false;
      return true;
    });

    if (projects.length === 0) {
      return [];
    }

    const stopWords = new Set(['the', 'and', 'for', 'with', 'from', 'this', 'that', 'project', 'your']);
    const separators = /[\s,.;:!?"'()\[\]{}\\/\-]+/;

    const aggregates = new Map<string, { projectCount: number; occurrenceCount: number; totalPercent: number }>();

    projects.forEach((project) => {
      const tokens = [project.name, project.nameCn, project.blurb, project.blurbCn]
        .filter(Boolean)
        .flatMap((field) => (field as string).split(separators))
        .map((token) => token.trim().toLowerCase())
        .filter((token) => token.length > 1 && !stopWords.has(token));

      const uniqueTokens = Array.from(new Set(tokens));

      tokens.forEach((token) => {
        const aggregate = aggregates.get(token) ?? { projectCount: 0, occurrenceCount: 0, totalPercent: 0 };
        aggregate.occurrenceCount += 1;
        aggregates.set(token, aggregate);
      });

      uniqueTokens.forEach((token) => {
        const aggregate = aggregates.get(token)!;
        aggregate.projectCount += 1;
        aggregate.totalPercent += project.percentFunded;
      });
    });

    return Array.from(aggregates.entries())
      .filter(([, value]) => value.projectCount > 1)
      .map(([keyword, value]) => ({
        keyword,
        projectCount: value.projectCount,
        occurrenceCount: value.occurrenceCount,
        averagePercentFunded: Number((value.totalPercent / value.projectCount).toFixed(1)),
      }))
      .sort((a, b) => {
        if (b.projectCount === a.projectCount) {
          return b.occurrenceCount - a.occurrenceCount;
        }
        return b.projectCount - a.projectCount;
      })
      .slice(0, 30);
  }
};

const mapFavoriteResponse = (favorite: ProjectFavoriteRecord): ProjectFavoriteRecord => ({
  ...favorite,
  note: favorite.note ?? undefined,
});

export const fetchFavorites = async (clientId: string): Promise<ProjectFavoriteRecord[]> => {
  const response = await httpClient.get<ProjectFavoriteRecord[]>('/favorites', {
    params: { clientId },
  });

  return response.data.map(mapFavoriteResponse);
};

export const saveFavorite = async ({
  clientId,
  projectId,
  note,
}: {
  clientId: string;
  projectId: number;
  note?: string;
}): Promise<ProjectFavoriteRecord> => {
  const response = await httpClient.post<ProjectFavoriteRecord>('/favorites', {
    clientId,
    projectId,
    note,
  });

  return mapFavoriteResponse(response.data);
};

export const deleteFavorite = async (clientId: string, projectId: number): Promise<void> => {
  await httpClient.delete(`/favorites/${projectId}`, {
    params: { clientId },
  });
};

export const clearFavorites = async (clientId: string): Promise<void> => {
  await httpClient.delete('/favorites', {
    params: { clientId },
  });
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
