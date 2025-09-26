'use client';
import { Card, Empty, Segmented } from 'antd';
import { useMemo, useState } from 'react';
import { Column } from '@ant-design/plots';
import type { CountryInsight } from '@/types/project';

interface CountrySuccessChartProps {
  data?: CountryInsight[];
  loading?: boolean;
}

export const CountrySuccessChart = ({ data = [], loading }: CountrySuccessChartProps) => {
  const [metric, setMetric] = useState<'successRate' | 'totalProjects' | 'totalPledged'>('successRate');

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
            : Number(item.totalPledged.toFixed(2)),
    }));
  }, [data, metric]);

  const hasData = chartData.length > 0;

  const yAxisFormatter = metric === 'successRate' ? (value: string) => `${value}%` : undefined;
  const tooltipFormatter = (datum: CountryInsight & { value: number }) => ({
    name: datum.country,
    value: metric === 'successRate' ? `${datum.value}%` : datum.value.toLocaleString(),
  });

  return (
    <Card
      title="高潜国家"
      loading={loading}
      style={{ height: '100%' }}
      extra={
        <Segmented
          size="small"
          value={metric}
          onChange={(value) => setMetric(value as typeof metric)}
          options={[
            { label: '成功率', value: 'successRate' },
            { label: '项目数', value: 'totalProjects' },
            { label: '筹资总额', value: 'totalPledged' },
          ]}
        />
      }
    >
      {!loading && !hasData ? (
        <Empty description="暂无国家数据" />
      ) : (
        <Column
          data={chartData}
          xField="country"
          yField="value"
          color="#0fbf61"
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
