'use client';

import { List, Tag, Typography } from 'antd';

interface TopListItem {
  key: string;
  title: string;
  subtitle?: string;
  metric: string;
  delta?: string;
  trend?: 'up' | 'down' | 'flat';
}

interface TopListCardProps {
  title: string;
  items: TopListItem[];
}

const trendColor: Record<NonNullable<TopListItem['trend']>, string> = {
  up: '#34d399',
  down: '#fb7185',
  flat: '#e2e8f0',
};

export const TopListCard = ({ title, items }: TopListCardProps) => {
  return (
    <List
      header={
        <Typography.Title
          level={5}
          style={{ color: '#f8fafc', margin: 0, textTransform: 'uppercase', letterSpacing: 1 }}
        >
          {title}
        </Typography.Title>
      }
      dataSource={items}
      style={{ background: 'rgba(15,23,42,0.7)', borderRadius: 16, padding: '8px 16px' }}
      renderItem={(item, index) => (
        <List.Item
          style={{
            borderBlockEnd: '1px solid rgba(148,163,184,0.15)',
          }}
        >
          <List.Item.Meta
            title={
              <Typography.Text strong style={{ color: '#e2e8f0' }}>
                {index + 1}. {item.title}
              </Typography.Text>
            }
            description={
              <Typography.Text style={{ color: 'rgba(148,163,184,0.85)' }}>
                {item.subtitle}
              </Typography.Text>
            }
          />
          <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 4 }}>
            <Typography.Text strong style={{ color: '#60a5fa' }}>
              {item.metric}
            </Typography.Text>
            {item.delta ? (
              <Tag color={trendColor[item.trend ?? 'flat']} style={{ color: '#0f172a' }}>
                {item.delta}
              </Tag>
            ) : null}
          </div>
        </List.Item>
      )}
    />
  );
};
