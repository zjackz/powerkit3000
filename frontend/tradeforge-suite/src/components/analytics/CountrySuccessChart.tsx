import { Card } from 'antd';
import { Column } from '@ant-design/plots';
import type { CountryInsight } from '@/types/project';

interface CountrySuccessChartProps {
  data?: CountryInsight[];
  loading?: boolean;
}

export const CountrySuccessChart = ({ data = [], loading }: CountrySuccessChartProps) => {
  return (
    <Card title="高潜国家" loading={loading} style={{ height: '100%' }}>
      <Column
        data={data}
        xField="country"
        yField="successRate"
        color="#0fbf61"
        columnStyle={{ radius: [4, 4, 0, 0] }}
        interactions={[{ type: 'element-active' }]}
        tooltip={{ formatter: (datum: CountryInsight) => ({ name: datum.country, value: `${datum.successRate}%` }) }}
        xAxis={{ label: { autoHide: true } }}
        yAxis={{ label: { formatter: (value: string) => `${value}%` } }}
        legend={false}
        autoFit
      />
    </Card>
  );
};
