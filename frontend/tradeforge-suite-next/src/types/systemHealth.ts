/**
 * 系统健康监控数据结构，复用 CLI/APIs 的指标字段，便于 MISSION X 直接消费。
 */
export interface HistogramSummary {
  count: number;
  sum: number;
  min: number;
  max: number;
  average: number;
}

export interface MetricsSnapshot {
  counters: Record<string, number>;
  histograms: Record<string, HistogramSummary>;
  lastUpdatedUtc: string;
}

export type SystemHealthStatus = 'healthy' | 'warning' | 'critical';

export interface SystemHealthAlert {
  level: SystemHealthStatus;
  message: string;
}

export interface SystemHealthSummary {
  totalImportFiles: number;
  totalProjectsImported: number;
  failedImports: number;
  parseErrors: number;
  validationErrors: number;
  queryDurationAverageMs?: number;
  amazonSnapshots: number;
  amazonTrendJobs: number;
  amazonFailures: number;
  lastUpdatedUtc?: string;
  alerts: SystemHealthAlert[];
  overallStatus: SystemHealthStatus;
}
