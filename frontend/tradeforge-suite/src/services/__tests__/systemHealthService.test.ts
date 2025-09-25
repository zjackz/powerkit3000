import { describe, expect, it } from 'vitest';
import { buildSystemHealthSummary } from '@/services/systemHealthService';
import type { MetricsSnapshot } from '@/types/systemHealth';

const baseSnapshot: MetricsSnapshot = {
  counters: {
    pk_kickstarter_import_files_total: 5,
    pk_kickstarter_import_projects_total: 1500,
  },
  histograms: {},
  lastUpdatedUtc: '2024-01-01T00:00:00Z',
};

describe('buildSystemHealthSummary', () => {
  /**
   * 基线用例：没有异常计数时应保持健康状态。
   */
  it('returns healthy status when no alerts present', () => {
    const result = buildSystemHealthSummary(baseSnapshot);
    expect(result.overallStatus).toBe('healthy');
    expect(result.alerts).toHaveLength(0);
  });

  /**
   * 验证带标签的解析错误计数可以正确累加并触发 warning 告警。
   */
  it('aggregates tagged counters and emits warning for parse errors', () => {
    const snapshot: MetricsSnapshot = {
      ...baseSnapshot,
      counters: {
        ...baseSnapshot.counters,
        'pk_kickstarter_import_parse_errors_total{source=batch,reason=invalid_date}': 3,
        pk_kickstarter_import_validation_errors_total: 2,
      },
    };

    const result = buildSystemHealthSummary(snapshot);
    expect(result.parseErrors).toBe(3);
    expect(result.validationErrors).toBe(2);
    expect(result.overallStatus).toBe('warning');
    expect(result.alerts).toHaveLength(2);
  });

  /**
   * 当导入失败或慢查询超阈值时需要升级为 critical 告警。
   */
  it('marks summary as critical when failures occur', () => {
    const snapshot: MetricsSnapshot = {
      ...baseSnapshot,
      counters: {
        ...baseSnapshot.counters,
        pk_kickstarter_import_failures_total: 1,
      },
      histograms: {
        pk_kickstarter_query_duration_ms: {
          count: 5,
          sum: 15_000,
          min: 2_000,
          max: 5_000,
          average: 3_000,
        },
      },
    };

    const result = buildSystemHealthSummary(snapshot);
    expect(result.failedImports).toBe(1);
    expect(result.queryDurationAverageMs).toBe(3_000);
    expect(result.overallStatus).toBe('critical');
    expect(result.alerts[0]?.level).toBe('critical');
  });
});
