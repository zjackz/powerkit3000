'use client';
import { ArrowDownOutlined, ArrowUpOutlined } from '@ant-design/icons';
import { Card, Col, Empty, Row, Statistic, Tag, Tooltip } from 'antd';
import { useMemo } from 'react';
import type { MonthlyTrendPoint } from '@/types/project';

interface TrendSummaryBoardProps {
  data?: MonthlyTrendPoint[];
  loading?: boolean;
}

type TrendDirection = 'up' | 'down' | 'flat';

interface TrendMetric {
  label: string;
  value: number;
  formatter?: (value: number) => string;
  delta: number | null;
  deltaPercent: number | null;
  direction: TrendDirection;
}

const formatNumber = (value: number) => value.toLocaleString();
const formatCurrency = (value: number) => `$${value.toLocaleString(undefined, { maximumFractionDigits: 0 })}`;
const formatPercent = (value: number) => `${value.toFixed(1)}%`;

const computeTrendMetric = (
  points: MonthlyTrendPoint[],
  extractor: (point: MonthlyTrendPoint) => number,
): { current: number; delta: number | null; deltaPercent: number | null; direction: TrendDirection } => {
  const sorted = [...points].sort((a, b) => {
    if (a.year === b.year) {
      return a.month - b.month;
    }
    return a.year - b.year;
  });

  const latest = sorted.length > 0 ? sorted[sorted.length - 1] : undefined;
  const previous = sorted.length > 1 ? sorted[sorted.length - 2] : undefined;

  const current = latest ? extractor(latest) : 0;
  if (!previous) {
    return { current, delta: null, deltaPercent: null, direction: 'flat' };
  }

  const prevValue = extractor(previous);
  const delta = current - prevValue;
  const deltaPercent = prevValue === 0 ? null : (delta / prevValue) * 100;
  const direction: TrendDirection = delta > 0 ? 'up' : delta < 0 ? 'down' : 'flat';

  return { current, delta, deltaPercent, direction };
};

const renderDeltaTag = (metric: TrendMetric) => {
  if (metric.delta === null) {
    return <Tag>—</Tag>;
  }

  if (metric.direction === 'flat' || metric.delta === 0) {
    return <Tag color="default">持平</Tag>;
  }

  const ArrowIcon = metric.direction === 'up' ? ArrowUpOutlined : ArrowDownOutlined;
  const color = metric.direction === 'up' ? 'success' : 'error';
  const label = metric.deltaPercent === null
    ? metric.delta.toLocaleString(undefined, { maximumFractionDigits: 1 })
    : `${metric.deltaPercent.toFixed(1)}%`;

  return (
    <Tag color={color} icon={<ArrowIcon />}>{label}</Tag>
  );
};

export const TrendSummaryBoard = ({ data = [], loading }: TrendSummaryBoardProps) => {
  const metrics = useMemo<TrendMetric[]>(() => {
    if (!data.length) {
      return [];
    }

    const totalMetric = computeTrendMetric(data, (point) => point.totalProjects);
    const successRateMetric = computeTrendMetric(data, (point) => {
      const total = point.totalProjects || 1;
      return Number(((point.successfulProjects / total) * 100).toFixed(1));
    });
    const pledgedMetric = computeTrendMetric(data, (point) => point.totalPledged);

    return [
      {
        label: '月度导入项目',
        value: totalMetric.current,
        formatter: formatNumber,
        delta: totalMetric.delta,
        deltaPercent: totalMetric.deltaPercent,
        direction: totalMetric.direction,
      },
      {
        label: '月度成功率',
        value: successRateMetric.current,
        formatter: (value) => formatPercent(value),
        delta: successRateMetric.delta,
        deltaPercent: successRateMetric.deltaPercent,
        direction: successRateMetric.direction,
      },
      {
        label: '月度筹资总额',
        value: pledgedMetric.current,
        formatter: formatCurrency,
        delta: pledgedMetric.delta,
        deltaPercent: pledgedMetric.deltaPercent,
        direction: pledgedMetric.direction,
      },
    ];
  }, [data]);

  const hasData = metrics.length > 0;

  return (
    <Card title="指标趋势" loading={loading}>
      {!loading && !hasData ? (
        <Empty description="暂无趋势数据" />
      ) : (
        <Row gutter={[16, 16]}>
          {metrics.map((metric) => (
            <Col xs={24} md={8} key={metric.label}>
              <Statistic
                title={metric.label}
                value={metric.formatter ? metric.formatter(metric.value) : metric.value}
              />
              <Tooltip title="与上一期相比">
                {renderDeltaTag(metric)}
              </Tooltip>
            </Col>
          ))}
        </Row>
      )}
    </Card>
  );
};
