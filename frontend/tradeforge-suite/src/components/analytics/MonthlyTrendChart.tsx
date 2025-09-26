import { Card, Empty, Segmented } from 'antd';
import { useMemo, useState } from 'react';
import { Line } from '@ant-design/plots';
import type { MonthlyTrendPoint } from '@/types/project';

interface MonthlyTrendChartProps {
  data?: MonthlyTrendPoint[];
  loading?: boolean;
}

const formatLabel = (point: MonthlyTrendPoint) => `${point.year}-${`${point.month}`.padStart(2, '0')}`;

export const MonthlyTrendChart = ({ data = [], loading }: MonthlyTrendChartProps) => {
  const [mode, setMode] = useState<'volume' | 'successRate' | 'pledged'>('volume');

  const chartData = useMemo(() => {
    if (data.length === 0) {
      return [];
    }

    if (mode === 'successRate') {
      return data.map((point) => {
        const total = point.totalProjects || 1;
        const rate = Number(((point.successfulProjects / total) * 100).toFixed(1));
        return { month: formatLabel(point), value: rate, type: '成功率' };
      });
    }

    if (mode === 'pledged') {
      return data.map((point) => ({
        month: formatLabel(point),
        value: Number(point.totalPledged.toFixed(2)),
        type: '总筹资',
      }));
    }

    return data.flatMap((point) => {
      const month = formatLabel(point);
      return [
        { month, value: point.totalProjects, type: '全部项目' },
        { month, value: point.successfulProjects, type: '成功项目' },
      ];
    });
  }, [data, mode]);

  const hasData = chartData.length > 0;
  const segmentedOptions = [
    { label: '项目数量', value: 'volume' },
    { label: '成功率', value: 'successRate' },
    { label: '筹资总额', value: 'pledged' },
  ];

  const yAxisLabelFormatter = mode === 'successRate' ? (value: string) => `${value}%` : undefined;

  return (
    <Card
      title="近 12 个月项目趋势"
      loading={loading}
      style={{ height: '100%' }}
      extra={<Segmented size="small" value={mode} onChange={(value) => setMode(value as typeof mode)} options={segmentedOptions} />}
    >
      {!loading && !hasData ? (
        <Empty description="暂无趋势数据" />
      ) : (
        <Line
          data={chartData}
          xField="month"
          yField="value"
          seriesField="type"
          smooth
          color={mode === 'volume' ? ['#1f6feb', '#0fbf61'] : ['#faad14']}
          xAxis={{ label: { autoHide: true } }}
          yAxis={{ label: yAxisLabelFormatter ? { formatter: yAxisLabelFormatter } : undefined }}
          tooltip={{
            formatter: (datum: { type: string; value: number }) => ({
              name: datum.type,
              value: mode === 'successRate' ? `${datum.value}%` : datum.value,
            }),
          }}
        />
      )}
    </Card>
  );
};
