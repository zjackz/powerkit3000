'use client';

import type { AreaConfig } from '@ant-design/plots';
import { Area } from '@ant-design/plots';

export interface TrendPoint {
  month: string;
  value: number;
}

interface TrendAreaChartProps {
  data: TrendPoint[];
  loading?: boolean;
}

export const TrendAreaChart = ({ data, loading }: TrendAreaChartProps) => {
  const config: AreaConfig = {
    data,
    xField: 'month',
    yField: 'value',
    color: '#60a5fa',
    areaStyle: {
      fill: 'l(270) 0:#1e293b 1:#60a5fa',
    },
    tooltip: {
      formatter: (datum) => ({
        name: datum.month,
        value: datum.value.toLocaleString(),
      }),
    },
    xAxis: {
      label: { autoHide: true },
    },
    yAxis: { label: null, grid: null },
    animation: {
      appear: {
        animation: 'wave-in',
        duration: 800,
      },
    },
    padding: [16, 0, 8, 0],
  };

  if (loading) {
    return <div style={{ height: 120, background: 'rgba(15,23,42,0.4)', borderRadius: 12 }} />;
  }

  return <Area {...config} height={200} />;
};
