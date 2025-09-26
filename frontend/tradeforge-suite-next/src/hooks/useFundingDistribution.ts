import { useQuery } from '@tanstack/react-query';
import { fetchFundingDistribution } from '@/services/projectsService';
import type { AnalyticsFilterRequest, FundingDistributionBin } from '@/types/project';

export const FUNDING_DISTRIBUTION_QUERY_KEY = 'funding-distribution';

export const useFundingDistribution = (filters: AnalyticsFilterRequest) =>
  useQuery<FundingDistributionBin[]>({
    queryKey: [FUNDING_DISTRIBUTION_QUERY_KEY, filters],
    queryFn: () => fetchFundingDistribution(filters),
    staleTime: 1000 * 60 * 5,
  });
