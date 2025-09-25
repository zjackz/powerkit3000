'use client';

import type { TinyAreaConfig } from '@ant-design/plots';
import { TinyArea } from '@ant-design/plots';

export interface TrendPoint {
  month: string;
  value: number;
}

interface TrendAreaChartProps {
  data: TrendPoint[];
  loading?: boolean;
}

export const TrendAreaChart = ({ data, loading }: TrendAreaChartProps) => {
  const config: TinyAreaConfig = {
    data: data.map((item) => item.value),
    smooth: true,
    color: '#5b8ff9',
    tooltip: {
      customContent: (index) => {
        const target = typeof index === 'number' ? data[index] : undefined;
        if (!target) return '';
        return `<div style="padding: 12px 8px;">${target.month}: ${target.value.toLocaleString()}</div>`;
      },
    },
    areaStyle: {
      fill: 'l(270) 0:#274472 1:#5b8ff9',
    },
    line: {
      color: '#60a5fa',
    },
  };

  if (loading) {
    return <div style={{ height: 120, background: 'rgba(15,23,42,0.4)', borderRadius: 12 }} />;
  }

  return <TinyArea {...config} height={120} />;
};
