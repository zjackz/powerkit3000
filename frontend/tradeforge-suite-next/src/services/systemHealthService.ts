import type {
  MetricsSnapshot,
  SystemHealthAlert,
  SystemHealthStatus,
  SystemHealthSummary,
} from '@/types/systemHealth';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://172.31.69.200:5200';
const METRICS_ENDPOINT = '/monitoring/metrics';
const SLOW_QUERY_THRESHOLD_MS = 2000;

/**
 * 将后端指标快照转换为 MISSION X 所需的告警与摘要信息。
 */
export const buildSystemHealthSummary = (snapshot: MetricsSnapshot): SystemHealthSummary => {
  const counters = snapshot.counters ?? {};
  const histograms = snapshot.histograms ?? {};

  const totalImportFiles = counters['pk_kickstarter_import_files_total'] ?? 0;
  const totalProjectsImported = counters['pk_kickstarter_import_projects_total'] ?? 0;
  const failedImports = counters['pk_kickstarter_import_failures_total'] ?? 0;
  const parseErrors = aggregateTaggedCounter(counters, 'pk_kickstarter_import_parse_errors_total');
  const validationErrors = aggregateTaggedCounter(counters, 'pk_kickstarter_import_validation_errors_total');

  const amazonSnapshots = aggregateTaggedCounter(counters, 'pk_amazon_snapshots_total');
  const amazonTrendJobs = aggregateTaggedCounter(counters, 'pk_amazon_trend_jobs_total');
  const amazonFailures = aggregateTaggedCounter(counters, 'pk_amazon_failures_total');

  const queryDurationAverageMs = histograms['pk_kickstarter_query_duration_ms']?.average;

  const alerts: SystemHealthAlert[] = [];

  if (failedImports > 0) {
    alerts.push({ level: 'critical', message: `存在 ${failedImports.toLocaleString()} 次 Kickstarter 导入失败，请立即处理。` });
  }

  if (parseErrors > 0) {
    alerts.push({ level: 'warning', message: `检测到 ${parseErrors.toLocaleString()} 条解析错误，建议检查数据源格式。` });
  }

  if (validationErrors > 0) {
    alerts.push({ level: 'warning', message: `存在 ${validationErrors.toLocaleString()} 条校验失败记录，需排查缺失字段或非法值。` });
  }

  if (queryDurationAverageMs && queryDurationAverageMs > SLOW_QUERY_THRESHOLD_MS) {
    alerts.push({ level: 'warning', message: `查询平均耗时 ${Math.round(queryDurationAverageMs)} ms，已经超过 ${SLOW_QUERY_THRESHOLD_MS} ms 阈值。` });
  }

  if (amazonFailures > 0) {
    alerts.push({ level: 'warning', message: `Amazon 采集出现 ${amazonFailures.toLocaleString()} 次失败，请检查代理与抓取配置。` });
  }

  const overallStatus = pickOverallStatus(alerts);

  return {
    totalImportFiles,
    totalProjectsImported,
    failedImports,
    parseErrors,
    validationErrors,
    queryDurationAverageMs,
    amazonSnapshots,
    amazonTrendJobs,
    amazonFailures,
    lastUpdatedUtc: snapshot.lastUpdatedUtc,
    alerts,
    overallStatus,
  };
};

/**
 * 聚合带标签的计数器值，兼容 OTLP 风格的 `{key=value}` 命名格式。
 */
const aggregateTaggedCounter = (counters: Record<string, number>, prefix: string): number => {
  return Object.entries(counters).reduce((sum, [key, value]) => {
    if (key === prefix || key.startsWith(`${prefix}{`)) {
      return sum + value;
    }
    return sum;
  }, 0);
};

const pickOverallStatus = (alerts: SystemHealthAlert[]): SystemHealthStatus => {
  if (alerts.some((alert) => alert.level === 'critical')) {
    return 'critical';
  }
  if (alerts.some((alert) => alert.level === 'warning')) {
    return 'warning';
  }
  return 'healthy';
};

/**
 * 请求监控数据，失败时回退到示例快照，避免驾驶舱完全空白。
 */
export const fetchSystemHealth = async (useMockFallback = true): Promise<SystemHealthSummary> => {
  try {
    const response = await fetch(`${API_BASE_URL}${METRICS_ENDPOINT}`, {
      cache: 'no-store',
    });

    if (!response.ok) {
      throw new Error(`Failed to load metrics: ${response.status}`);
    }

    const snapshot = (await response.json()) as MetricsSnapshot;
    return buildSystemHealthSummary(snapshot);
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    console.warn('无法获取实时指标，使用模拟数据填充系统健康卡片。', error);
    return buildSystemHealthSummary(buildMockSnapshot());
  }
};

const buildMockSnapshot = (): MetricsSnapshot => ({
  counters: {
    pk_kickstarter_import_files_total: 18,
    pk_kickstarter_import_projects_total: 5120,
    pk_kickstarter_import_failures_total: 0,
    'pk_kickstarter_import_parse_errors_total{source=sample.jsonl,reason=date_format_invalid}': 4,
    pk_kickstarter_import_validation_errors_total: 2,
    pk_amazon_snapshots_total: 14,
    pk_amazon_trend_jobs_total: 14,
    pk_amazon_failures_total: 1,
  },
  histograms: {
    pk_kickstarter_query_duration_ms: {
      count: 180,
      sum: 180000,
      min: 150,
      max: 3200,
      average: 1000,
    },
  },
  lastUpdatedUtc: new Date().toISOString(),
});
