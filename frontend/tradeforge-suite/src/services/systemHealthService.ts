import { httpClient } from '@/services/httpClient';
import type { MetricsSnapshot, SystemHealthAlert, SystemHealthStatus, SystemHealthSummary } from '@/types/systemHealth';
import { notifyApiFallback } from '@/utils/apiNotifications';

const METRICS_ENDPOINT = '/monitoring/metrics';
const SLOW_QUERY_THRESHOLD_MS = 2000;

/**
 * 构建系统健康摘要，所有告警等级在此集中计算，便于后续卡片直接消费。
 */
export const buildSystemHealthSummary = (snapshot: MetricsSnapshot): SystemHealthSummary => {
  const counters = snapshot.counters ?? {};
  const histograms = snapshot.histograms ?? {};

  const totalImportFiles = counters['pk_kickstarter_import_files_total'] ?? 0;
  const totalProjectsImported = counters['pk_kickstarter_import_projects_total'] ?? 0;
  const failedImports = counters['pk_kickstarter_import_failures_total'] ?? 0;
  const parseErrors = aggregateTaggedCounter(counters, 'pk_kickstarter_import_parse_errors_total');
  const validationErrors = aggregateTaggedCounter(counters, 'pk_kickstarter_import_validation_errors_total');
  const queryDurationAverageMs = histograms['pk_kickstarter_query_duration_ms']?.average;

  const alerts: SystemHealthAlert[] = [];

  if (failedImports > 0) {
    alerts.push({ level: 'critical', message: `存在 ${failedImports.toLocaleString()} 次导入失败，请立即排查导入命令日志。` });
  }

  if (parseErrors > 0) {
    alerts.push({ level: 'warning', message: `检测到 ${parseErrors.toLocaleString()} 条解析错误，建议检查原始数据格式。` });
  }

  if (validationErrors > 0) {
    alerts.push({ level: 'warning', message: `存在 ${validationErrors.toLocaleString()} 条校验失败记录，需确认缺失字段或非法值。` });
  }

  if (queryDurationAverageMs && queryDurationAverageMs > SLOW_QUERY_THRESHOLD_MS) {
    alerts.push({ level: 'warning', message: `查询平均耗时 ${Math.round(queryDurationAverageMs)} ms，已超过 ${SLOW_QUERY_THRESHOLD_MS} ms 阈值。` });
  }

  const overallStatus = pickOverallStatus(alerts);

  return {
    totalImportFiles,
    totalProjectsImported,
    failedImports,
    parseErrors,
    validationErrors,
    queryDurationAverageMs,
    lastUpdatedUtc: snapshot.lastUpdatedUtc,
    alerts,
    overallStatus,
  };
};

/**
 * 聚合所有匹配前缀的计数器，兼容带标签的 OpenTelemetry 指标命名。
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
 * 从后端读取最新指标，支持降级为内置模拟数据，避免仪表盘完全挂空。
 */
export const fetchSystemHealth = async (useMockFallback = true): Promise<SystemHealthSummary> => {
  try {
    const response = await httpClient.get<MetricsSnapshot>(METRICS_ENDPOINT);
    return buildSystemHealthSummary(response.data);
  } catch (error) {
    if (!useMockFallback) {
      throw error;
    }

    notifyApiFallback('系统健康指标');
    console.warn('无法获取实时监控指标，将使用模拟数据渲染系统健康卡片。');
    return buildSystemHealthSummary(buildMockSnapshot());
  }
};

const buildMockSnapshot = (): MetricsSnapshot => ({
  counters: {
    pk_kickstarter_import_files_total: 12,
    pk_kickstarter_import_projects_total: 3850,
    pk_kickstarter_import_failures_total: 0,
    'pk_kickstarter_import_parse_errors_total{source=sample.jsonl,reason=date_format_invalid}': 3,
    pk_kickstarter_import_validation_errors_total: 1,
  },
  histograms: {
    pk_kickstarter_query_duration_ms: {
      count: 120,
      sum: 120000,
      min: 120,
      max: 2800,
      average: 1000,
    },
  },
  lastUpdatedUtc: new Date().toISOString(),
});
