export interface Project {
  id: number;
  name: string;
  nameCn?: string | null;
  blurb: string | null;
  blurbCn?: string | null;
  categoryId?: number | null;
  categoryName: string;
  country: string;
  state: string;
  goal: number;
  pledged: number;
  percentFunded: number;
  fundingVelocity: number;
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
  stats: ProjectQueryStats;
}

export interface ProjectQueryStats {
  successfulCount: number;
  totalPledged: number;
  averagePercentFunded: number;
  totalBackers: number;
  averageGoal: number;
  topProject?: Project | null;
}

export interface FilterOption {
  value: string;
  label: string;
  count: number;
}

export interface ProjectFilters {
  states: FilterOption[];
  countries: FilterOption[];
  categories: FilterOption[];
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
  nameCn?: string | null;
  categoryName: string;
  country: string;
  percentFunded: number;
  pledged: number;
  fundingVelocity: number;
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
  minPercentFunded?: number;
}

export interface CategoryKeywordInsight {
  keyword: string;
  projectCount: number;
  occurrenceCount: number;
  averagePercentFunded: number;
}
