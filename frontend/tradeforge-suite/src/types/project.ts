export interface Project {
  id: number;
  name: string;
  blurb: string;
  categoryId: number;
  categoryName: string;
  country: string;
  state: string;
  goal: number;
  pledged: number;
  percentFunded: number;
  backersCount: number;
  currency: string;
  launchedAt: string;
  deadline: string;
  creatorName: string;
  locationName?: string | null;
}

export interface ProjectQueryParams {
  search?: string;
  states?: string[];
  countries?: string[];
  categories?: string[];
  minGoal?: number;
  maxGoal?: number;
  minPercentFunded?: number;
  launchedAfter?: string;
  launchedBefore?: string;
  page?: number;
  pageSize?: number;
}

export interface ProjectQueryResponse {
  total: number;
  items: Project[];
}

export interface ProjectFilters {
  states: string[];
  countries: string[];
  categories: string[];
}

export interface ProjectSummary {
  totalProjects: number;
  successfulProjects: number;
  totalPledged: number;
  distinctCountries: number;
  successRate: number;
}

export interface CategoryInsight {
  categoryName: string;
  totalProjects: number;
  successfulProjects: number;
  successRate: number;
  averagePercentFunded: number;
  totalPledged: number;
}

export interface CountryInsight {
  country: string;
  totalProjects: number;
  successfulProjects: number;
  successRate: number;
  totalPledged: number;
}

export interface ProjectHighlight {
  id: number;
  name: string;
  categoryName: string;
  country: string;
  percentFunded: number;
  pledged: number;
  backersCount: number;
  currency: string;
  launchedAt: string;
}

export interface MonthlyTrendPoint {
  year: number;
  month: number;
  totalProjects: number;
  successfulProjects: number;
  totalPledged: number;
}

export interface FundingDistributionBin {
  label: string;
  minPercent: number;
  maxPercent: number;
  totalProjects: number;
  successfulProjects: number;
}

export interface CreatorPerformance {
  creatorId: number;
  creatorName: string;
  totalProjects: number;
  successfulProjects: number;
  successRate: number;
  averagePercentFunded: number;
  totalPledged: number;
}

export interface AnalyticsFilterRequest {
  launchedAfter?: string;
  launchedBefore?: string;
  countries?: string[];
  categories?: string[];
}
