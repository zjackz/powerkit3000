'use client';

import { Card, Statistic, Typography, Space, Tag } from 'antd';

interface InsightMetricCardProps {
  title: string;
  value: string | number;
  unit?: string;
  trendLabel?: string;
  trendValue?: string;
  caption?: string;
  accent?: 'blue' | 'green' | 'orange' | 'pink';
}

const accentMap: Record<NonNullable<InsightMetricCardProps['accent']>, { color: string; shadow: string }> = {
  blue: { color: '#60a5fa', shadow: '0 10px 25px -15px #1d4ed8' },
  green: { color: '#34d399', shadow: '0 10px 25px -15px #047857' },
  orange: { color: '#fb923c', shadow: '0 10px 25px -15px #c2410c' },
  pink: { color: '#f472b6', shadow: '0 10px 25px -15px #be185d' },
};

export const InsightMetricCard = ({
  title,
  value,
  unit,
  trendLabel,
  trendValue,
  caption,
  accent = 'blue',
}: InsightMetricCardProps) => {
  const accentTheme = accentMap[accent];

  return (
    <Card
      bordered={false}
      style={{
        background: 'linear-gradient(135deg, rgba(15,23,42,0.9), rgba(30,41,59,0.8))',
        boxShadow: accentTheme.shadow,
      }}
    >
      <Space direction="vertical" size="small">
        <Typography.Text style={{ color: 'rgba(226,232,240,0.75)' }}>{title}</Typography.Text>
        <Space align="baseline" size={4}>
          <Statistic
            value={value}
            suffix={unit}
            precision={typeof value === 'number' ? 1 : undefined}
            valueStyle={{ color: accentTheme.color, fontWeight: 600 }}
          />
          {trendLabel && trendValue ? (
            <Tag color="blue-inverse" bordered={false} style={{ fontSize: 12 }}>
              {trendLabel} {trendValue}
            </Tag>
          ) : null}
        </Space>
        {caption ? (
          <Typography.Paragraph style={{ color: 'rgba(148,163,184,0.85)', margin: 0 }}>
            {caption}
          </Typography.Paragraph>
        ) : null}
      </Space>
    </Card>
  );
};
