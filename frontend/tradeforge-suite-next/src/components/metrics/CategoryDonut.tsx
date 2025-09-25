'use client';

import type { PieConfig } from '@ant-design/plots';
import { Pie } from '@ant-design/plots';

export interface CategoryDatum {
  name: string;
  value: number;
}

interface CategoryDonutProps {
  data: CategoryDatum[];
  loading?: boolean;
}

export const CategoryDonut = ({ data, loading }: CategoryDonutProps) => {
  if (loading) {
    return <div style={{ height: 260, background: 'rgba(15,23,42,0.4)', borderRadius: 12 }} />;
  }

  const config: PieConfig = {
    data,
    angleField: 'value',
    colorField: 'name',
    radius: 1,
    innerRadius: 0.64,
    label: {
      type: 'spider',
      labelHeight: 40,
      content: '{name} {percentage}',
    },
    interactions: [
      { type: 'element-selected' },
      { type: 'element-active' },
      { type: 'legend-active' },
    ],
    legend: {
      position: 'bottom',
      itemName: {
        style: {
          fill: 'rgba(226,232,240,0.75)',
        },
      },
    },
    tooltip: {
      showMarkers: false,
    },
    statistic: {
      title: {
        content: '类别份额',
        style: { color: '#e2e8f0', fontSize: 16 },
      },
      content: {
        content: '实时占比',
        style: { color: '#cbd5f5', fontSize: 12 },
      },
    },
  };

  return <Pie {...config} />;
};
