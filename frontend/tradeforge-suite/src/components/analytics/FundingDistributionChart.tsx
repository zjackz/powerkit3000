import { Card, Empty } from 'antd';
import { Pie } from '@ant-design/plots';
import type { FundingDistributionBin } from '@/types/project';

interface FundingDistributionChartProps {
  data?: FundingDistributionBin[];
  loading?: boolean;
}

export const FundingDistributionChart = ({ data = [], loading }: FundingDistributionChartProps) => {
  const pieData = data.map((bin) => ({
    type: bin.label,
    value: bin.totalProjects,
    successRate: bin.totalProjects === 0 ? 0 : Number(((bin.successfulProjects / bin.totalProjects) * 100).toFixed(1)),
  }));

  const hasData = pieData.length > 0 && pieData.some((item) => item.value > 0);

  return (
    <Card title="筹资达成率分布" loading={loading} style={{ height: '100%' }}>
      {!loading && !hasData ? (
        <Empty description="暂无分布数据" />
      ) : (
        <Pie
          data={pieData}
          angleField="value"
          colorField="type"
          radius={1}
          innerRadius={0.6}
          label={{ type: 'inner', offset: '-50%', content: '{value}' }}
          interactions={[{ type: 'element-active' }]}
          statistic={{
            title: { formatter: () => '总项目' },
            content: { formatter: () => pieData.reduce((sum, item) => sum + item.value, 0).toString() },
          }}
          tooltip={{
            formatter: (datum: { type: string; successRate: number }) => ({
              name: `${datum.type} 成功率`,
              value: `${datum.successRate}%`,
            }),
          }}
        />
      )}
    </Card>
  );
};
