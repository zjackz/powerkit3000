import { Card } from 'antd';
import { Line } from '@ant-design/plots';
import type { MonthlyTrendPoint } from '@/types/project';

interface MonthlyTrendChartProps {
  data?: MonthlyTrendPoint[];
  loading?: boolean;
}

const formatLabel = (point: MonthlyTrendPoint) => `${point.year}-${`${point.month}`.padStart(2, '0')}`;

export const MonthlyTrendChart = ({ data = [], loading }: MonthlyTrendChartProps) => {
  const chartData = data.flatMap((point) => {
    const month = formatLabel(point);
    return [
      { month, value: point.totalProjects, type: '全部项目' },
      { month, value: point.successfulProjects, type: '成功项目' },
    ];
  });

  return (
    <Card title="近 12 个月项目趋势" loading={loading} style={{ height: '100%' }}>
      <Line
        data={chartData}
        xField="month"
        yField="value"
        seriesField="type"
        smooth
        color={['#1f6feb', '#0fbf61']}
        xAxis={{ label: { autoHide: true } }}
        tooltip={{ formatter: (datum: { type: string; value: number }) => ({ name: datum.type, value: datum.value }) }}
      />
    </Card>
  );
};
