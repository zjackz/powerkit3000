import { Avatar, Card, Empty, List, Tag, Typography } from 'antd';
import { FireTwoTone } from '@ant-design/icons';
import dayjs from 'dayjs';
import type { ProjectHighlight } from '@/types/project';

interface TopProjectsListProps {
  data?: ProjectHighlight[];
  loading?: boolean;
}

export const TopProjectsList = ({ data = [], loading }: TopProjectsListProps) => {
  const hasData = data.length > 0;

  return (
    <Card title="Top 项目" loading={loading} bodyStyle={{ padding: 16 }}>
      {!loading && !hasData ? (
        <Empty description="暂无项目" />
      ) : (
        <List
          itemLayout="horizontal"
          dataSource={data}
          renderItem={(item, index) => (
            <List.Item>
              <List.Item.Meta
                avatar={<Avatar icon={<FireTwoTone twoToneColor="#fa541c" />} />}
                title={
                  <Typography.Text strong>
                    #{index + 1} {item.nameCn ?? item.name}
                  </Typography.Text>
                }
                description={
                  <Typography.Text type="secondary">
                    {item.categoryName} · {item.country} · {dayjs(item.launchedAt).format('YYYY-MM-DD')}
                  </Typography.Text>
                }
              />
              <div style={{ textAlign: 'right' }}>
                <Typography.Text strong style={{ display: 'block' }}>
                  {item.currency} {item.pledged.toLocaleString()}
                </Typography.Text>
                <Typography.Text type="success">达成率 {item.percentFunded.toFixed(1)}%</Typography.Text>
                <div>
                  <Tag color="blue">{`${item.currency} ${item.fundingVelocity.toFixed(2)}/天`}</Tag>
                </div>
              </div>
            </List.Item>
          )}
        />
      )}
    </Card>
  );
};
