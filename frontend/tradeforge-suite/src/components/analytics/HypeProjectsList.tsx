import { Avatar, Card, Empty, List, Tag, Typography } from 'antd';
import { ThunderboltTwoTone } from '@ant-design/icons';
import dayjs from 'dayjs';
import type { ProjectHighlight } from '@/types/project';

interface HypeProjectsListProps {
  data?: ProjectHighlight[];
  loading?: boolean;
}

const renderVelocity = (project: ProjectHighlight) => {
  return `${project.currency} ${project.fundingVelocity.toFixed(2)}/天`;
};

export const HypeProjectsList = ({ data = [], loading }: HypeProjectsListProps) => {
  const hasData = data.length > 0;

  return (
    <Card title="爆款潜力" loading={loading} bodyStyle={{ padding: 16 }}>
      {!loading && !hasData ? (
        <Empty description="暂无爆款候选" />
      ) : (
        <List
          itemLayout="horizontal"
          dataSource={data}
          renderItem={(item, index) => (
            <List.Item>
              <List.Item.Meta
                avatar={<Avatar icon={<ThunderboltTwoTone twoToneColor="#faad14" />} />}
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
                <Tag color="orange">{renderVelocity(item)}</Tag>
              </div>
            </List.Item>
          )}
        />
      )}
    </Card>
  );
};
