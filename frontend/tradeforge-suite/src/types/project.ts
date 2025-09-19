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
