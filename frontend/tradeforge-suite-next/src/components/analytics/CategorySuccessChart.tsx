'use client';
import { Card, Empty, Segmented } from 'antd';
import { useMemo, useState } from 'react';
import { Column } from '@ant-design/plots';
import type { CategoryInsight } from '@/types/project';

interface CategorySuccessChartProps {
  data?: CategoryInsight[];
  loading?: boolean;
}

export const CategorySuccessChart = ({ data = [], loading }: CategorySuccessChartProps) => {
  const [metric, setMetric] = useState<'successRate' | 'totalProjects' | 'totalPledged' | 'averagePercentFunded'>('successRate');

  const chartData = useMemo(() => {
    if (!data.length) {
      return [];
    }

    return data.map((item) => ({
      ...item,
      value:
        metric === 'successRate'
          ? item.successRate
          : metric === 'totalProjects'
            ? item.totalProjects
            : metric === 'totalPledged'
              ? Number(item.totalPledged.toFixed(2))
              : item.averagePercentFunded,
    }));
  }, [data, metric]);

  const hasData = chartData.length > 0;

  const metricOptions = [
    { label: '成功率', value: 'successRate' },
    { label: '项目数', value: 'totalProjects' },
    { label: '平均达成率', value: 'averagePercentFunded' },
    { label: '筹资总额', value: 'totalPledged' },
  ];

  const yAxisFormatter = metric === 'successRate' || metric === 'averagePercentFunded'
    ? (value: string) => `${value}%`
    : undefined;

  const tooltipFormatter = (datum: CategoryInsight & { value: number }) => ({
    name: datum.categoryName,
    value:
      metric === 'successRate' || metric === 'averagePercentFunded'
        ? `${datum.value}%`
        : datum.value.toLocaleString(),
  });

  return (
    <Card
      title="类目洞察"
      loading={loading}
      style={{ height: '100%' }}
      extra={
        <Segmented
          size="small"
          value={metric}
          onChange={(value) => setMetric(value as typeof metric)}
          options={metricOptions}
        />
      }
    >
      {!loading && !hasData ? (
        <Empty description="暂无类目数据" />
      ) : (
        <Column
          data={chartData}
          xField="categoryName"
          yField="value"
          color="#1f6feb"
          columnStyle={{ radius: [4, 4, 0, 0] }}
          interactions={[{ type: 'element-active' }]}
          tooltip={{ formatter: tooltipFormatter }}
          xAxis={{ label: { autoHide: true } }}
          yAxis={{ label: yAxisFormatter ? { formatter: yAxisFormatter } : undefined }}
          legend={false}
          autoFit
        />
      )}
    </Card>
  );
};
