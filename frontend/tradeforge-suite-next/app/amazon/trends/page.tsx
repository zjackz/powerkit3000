'use client';

import { Card, Empty, Skeleton, Space, Typography } from 'antd';
import { ProCard } from '@ant-design/pro-components';
import { ProShell } from '@/layouts/ProShell';
import { TeamSwitcher } from '@/components/team/TeamSwitcher';
import { useTeamContext } from '@/contexts/TeamContext';
import { useAmazonTrends } from '@/hooks/useAmazonDashboard';
import type { AmazonTrendListItem } from '@/types/amazon';

const TrendList = ({ title, loading, trends }: { title: string; loading: boolean; trends?: AmazonTrendListItem[] }) => (
  <ProCard colSpan={{ xs: 24, xl: 8 }} bordered title={title}>
    {loading ? (
      <Skeleton active />
    ) : !trends?.length ? (
      <Empty description="暂无趋势数据" />
    ) : (
      <Space direction="vertical" size={12} style={{ width: '100%' }}>
        {trends.map((trend) => (
          <Card key={`${trend.trendType}-${trend.asin}`} size="small" bordered={false} style={{ background: 'rgba(30,41,59,0.65)' }}>
            <Typography.Text strong>{trend.title}</Typography.Text>
            <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
              {trend.description}
            </Typography.Paragraph>
            <Typography.Text type="secondary" style={{ fontSize: 12 }}>
              记录时间：{new Date(trend.recordedAt).toLocaleString()}
            </Typography.Text>
          </Card>
        ))}
      </Space>
    )}
  </ProCard>
);

const TrendsContent = () => {
  const { team } = useTeamContext();
  const { data: newEntries, isLoading: newLoading } = useAmazonTrends({ trendType: 'NewEntry' });
  const { data: surges, isLoading: surgeLoading } = useAmazonTrends({ trendType: 'RankSurge' });
  const { data: consistent, isLoading: consistentLoading } = useAmazonTrends({ trendType: 'ConsistentPerformer' });

  const overview = (
    <Card bordered={false} style={{ background: 'rgba(15,23,42,0.85)' }}>
      <Space direction="vertical" size={12} style={{ width: '100%' }}>
        <Typography.Text type="secondary">团队视角</Typography.Text>
        <TeamSwitcher />
        <Typography.Text type="secondary" style={{ fontSize: 12 }}>
          {team.description}
        </Typography.Text>
      </Space>
    </Card>
  );

  return (
    <ProShell
      title="趋势雷达"
      description="洞察 Amazon 榜单的上新、飙升与稳定霸榜趋势，辅助跨境运营制定任务。"
      overview={overview}
    >
      <TrendList title="新晋上榜" loading={newLoading} trends={newEntries} />
      <TrendList title="排名飙升" loading={surgeLoading} trends={surges} />
      <TrendList title="持续霸榜" loading={consistentLoading} trends={consistent} />
    </ProShell>
  );
};

export default function AmazonTrendsPage() {
  return <TrendsContent />;
}
