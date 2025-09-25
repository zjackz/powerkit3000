import { useQuery } from '@tanstack/react-query';
import { fetchSystemHealth } from '@/services/systemHealthService';
import type { SystemHealthSummary } from '@/types/systemHealth';

export const SYSTEM_HEALTH_QUERY_KEY = 'system-health';

/**
 * 封装系统健康数据的轮询逻辑，默认每 30 秒刷新一次，便于仪表盘实时掌握导入状态。
 */
export const useSystemHealth = () => {
  return useQuery<SystemHealthSummary>({
    queryKey: [SYSTEM_HEALTH_QUERY_KEY],
    queryFn: () => fetchSystemHealth(),
    refetchInterval: 30_000,
    staleTime: 30_000,
  });
};
