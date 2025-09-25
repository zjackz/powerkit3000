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

export interface SystemHealthSummary {
  totalImportFiles: number;
  totalProjectsImported: number;
  failedImports: number;
  parseErrors: number;
  validationErrors: number;
  queryDurationAverageMs?: number;
  lastUpdatedUtc?: string;
  alerts: SystemHealthAlert[];
  overallStatus: SystemHealthStatus;
}

export type SystemHealthStatus = 'healthy' | 'warning' | 'critical';

export interface SystemHealthAlert {
  level: SystemHealthStatus;
  message: string;
}
