import { Card } from 'antd';
import { Column } from '@ant-design/plots';
import type { CategoryInsight } from '@/types/project';

interface CategorySuccessChartProps {
  data?: CategoryInsight[];
  loading?: boolean;
}

export const CategorySuccessChart = ({ data = [], loading }: CategorySuccessChartProps) => {
  return (
    <Card title="类目成功率" loading={loading} style={{ height: '100%' }}>
      <Column
        data={data}
        xField="categoryName"
        yField="successRate"
        color="#1f6feb"
        columnStyle={{ radius: [4, 4, 0, 0] }}
        interactions={[{ type: 'element-active' }]}
        tooltip={{ formatter: (datum: CategoryInsight) => ({ name: datum.categoryName, value: `${datum.successRate}%` }) }}
        xAxis={{ label: { autoHide: true } }}
        yAxis={{ label: { formatter: (value: string) => `${value}%` } }}
        legend={false}
        autoFit
      />
    </Card>
  );
};
