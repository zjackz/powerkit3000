'use client';
import { Card, Empty, Segmented, Skeleton } from 'antd';
import { Line } from '@ant-design/plots';
import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import type { AmazonProductHistoryPoint } from '@/types/amazon';

interface AmazonHistoryChartProps {
  data?: AmazonProductHistoryPoint[];
  loading?: boolean;
  asin?: string;
}

type Metric = 'rank' | 'price' | 'rating' | 'reviews';

const METRIC_OPTIONS = [
  { label: '榜单排名', value: 'rank' },
  { label: '价格', value: 'price' },
  { label: '评分', value: 'rating' },
  { label: '评论数', value: 'reviews' },
];

const metricLabelMap: Record<Metric, string> = {
  rank: '榜单排名',
  price: '价格 (USD)',
  rating: '评分',
  reviews: '评论数',
};

export const AmazonHistoryChart = ({ data = [], loading, asin }: AmazonHistoryChartProps) => {
  const [metric, setMetric] = useState<Metric>('rank');

  const chartData = useMemo(() => {
    if (!data.length) {
      return [];
    }

    return data
      .map((point) => ({
        timestamp: dayjs(point.timestamp).format('YYYY-MM-DD HH:mm'),
        value:
          metric === 'rank'
            ? point.rank
            : metric === 'price'
              ? point.price ?? null
              : metric === 'rating'
                ? point.rating ?? null
                : point.reviewsCount ?? null,
      }))
      .filter((point) => point.value !== null) as Array<{ timestamp: string; value: number }>;
  }, [data, metric]);

  const hasSelection = Boolean(asin);
  const hasData = chartData.length > 0;

  return (
    <Card
      title={metricLabelMap[metric]}
      extra={
        <Segmented
          size="small"
          options={METRIC_OPTIONS}
          value={metric}
          onChange={(value) => setMetric(value as Metric)}
        />
      }
    >
      {loading ? (
        <Skeleton active />
      ) : !hasSelection ? (
        <Empty description="选择左侧产品查看历史趋势" />
      ) : !hasData ? (
        <Empty description="暂无可视化数据" />
      ) : (
        <Line
          data={chartData}
          xField="timestamp"
          yField="value"
          smooth
          xAxis={{
            type: 'timeCat',
            label: { autoHide: true },
          }}
          yAxis={{
            label: {
              formatter: (value: string) => {
                if (metric === 'rank') {
                  return `#${value}`;
                }
                if (metric === 'price') {
                  return `$${Number(value).toFixed(2)}`;
                }
                if (metric === 'rating') {
                  return Number(value).toFixed(1);
                }
                return value;
              },
            },
          }}
          tooltip={{
            formatter: (datum: { value: number }) => {
              const formatted =
                metric === 'rank'
                  ? `#${datum.value}`
                  : metric === 'price'
                    ? `$${datum.value.toFixed(2)}`
                    : metric === 'rating'
                      ? datum.value.toFixed(1)
                      : Number(datum.value).toLocaleString();
              return { name: metricLabelMap[metric], value: formatted };
            },
          }}
        />
      )}
    </Card>
  );
};
