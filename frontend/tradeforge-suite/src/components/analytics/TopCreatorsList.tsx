import { Avatar, Card, List, Typography } from 'antd';
import { CrownTwoTone } from '@ant-design/icons';
import type { CreatorPerformance } from '@/types/project';

interface TopCreatorsListProps {
  data?: CreatorPerformance[];
  loading?: boolean;
}

export const TopCreatorsList = ({ data = [], loading }: TopCreatorsListProps) => (
  <Card title="创作者表现" loading={loading} bodyStyle={{ padding: 16 }}>
    <List
      itemLayout="horizontal"
      dataSource={data}
      renderItem={(item, index) => (
        <List.Item>
          <List.Item.Meta
            avatar={<Avatar icon={<CrownTwoTone twoToneColor={index < 3 ? '#faad14' : '#1f6feb'} />} />}
            title={
              <Typography.Text strong>
                #{index + 1} {item.creatorName}
              </Typography.Text>
            }
            description={
              <Typography.Text type="secondary">
                项目数 {item.totalProjects} · 成功率 {item.successRate}% · 平均达成 {item.averagePercentFunded}%
              </Typography.Text>
            }
          />
          <div style={{ textAlign: 'right' }}>
            <Typography.Text strong>
              ${item.totalPledged.toLocaleString()}
            </Typography.Text>
          </div>
        </List.Item>
      )}
    />
  </Card>
);
