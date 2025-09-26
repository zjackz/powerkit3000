'use client';
import { Card, Empty, Spin, Typography } from 'antd';
import { WordCloud } from '@ant-design/plots';
import type { CategoryKeywordInsight } from '@/types/project';

interface CategoryKeywordCloudProps {
  data?: CategoryKeywordInsight[];
  loading?: boolean;
  category?: string;
}

export const CategoryKeywordCloud = ({ data = [], loading, category }: CategoryKeywordCloudProps) => {
  if (loading) {
    return (
      <Card title="品类关键词">
        <div style={{ minHeight: 240, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <Spin />
        </div>
      </Card>
    );
  }

  if (!data.length) {
    return (
      <Card title="品类关键词">
        <Empty description={category ? `暂无 ${category} 关键词数据` : '请选择品类'} />
      </Card>
    );
  }

  const config = {
    data,
    wordField: 'keyword',
    weightField: 'projectCount',
    colorField: 'keyword',
    wordStyle: {
      fontSize: [16, 58],
      rotation: 0,
    },
    shape: 'cloud',
    tooltip: {
      showTitle: false,
      customContent: (
        _: string,
        items: Array<{ data: CategoryKeywordInsight }> | undefined,
      ) => {
        if (!items?.length) {
          return '';
        }
        const datum = items[0].data;
        return `
          <div style="padding: 4px 8px;">
            <div><strong>${datum.keyword}</strong></div>
            <div>关联项目：${datum.projectCount}</div>
            <div>出现次数：${datum.occurrenceCount}</div>
            <div>平均达成率：${datum.averagePercentFunded.toFixed(1)}%</div>
          </div>
        `;
      },
    },
    interactions: [{ type: 'element-active' }],
  };

  return (
    <Card
      title={
        <Typography.Text strong>
          {category ? `${category} 高频关键词` : '品类关键词'}
        </Typography.Text>
      }
    >
      <div style={{ height: 320 }}>
        <WordCloud {...config} />
      </div>
    </Card>
  );
};
