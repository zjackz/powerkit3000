'use client';

import { useEffect, useMemo, useState } from 'react';
import { ProCard } from '@ant-design/pro-components';
import { Card, Empty, Input, Skeleton, Space, Statistic, Table, Typography } from 'antd';
import dayjs from 'dayjs';
import type { ColumnsType } from 'antd/es/table';
import { ProShell } from '@/layouts/ProShell';
import { TeamSwitcher } from '@/components/team/TeamSwitcher';
import { useTeamContext } from '@/contexts/TeamContext';
import { AmazonHistoryChart } from '@/components/amazon/AmazonHistoryChart';
import {
  useAmazonCoreMetrics,
  useAmazonProductHistory,
  useAmazonProducts,
  useAmazonTrends,
} from '@/hooks/useAmazonDashboard';
import type { AmazonProductListItem } from '@/types/amazon';

const formatCurrency = (value?: number | null) => {
  if (value === null || value === undefined) {
    return '—';
  }
  return `$${value.toFixed(2)}`;
};

const formatNumber = (value?: number | null) => {
  if (value === null || value === undefined) {
    return '—';
  }
  return value.toLocaleString();
};

const OperationsContent = () => {
  const { team } = useTeamContext();
  const [search, setSearch] = useState('');
  const [query, setQuery] = useState('');
  const [selectedAsin, setSelectedAsin] = useState<string>();

  useEffect(() => {
    if (team.focusCategories?.length) {
      setQuery(team.focusCategories[0] ?? '');
    }
  }, [team]);

  const { data: metrics, isLoading: metricsLoading } = useAmazonCoreMetrics();
  const { data: products, isLoading: productsLoading } = useAmazonProducts({ search: query || undefined });
  const { data: trendsNew, isLoading: newLoading } = useAmazonTrends({ trendType: 'NewEntry' });
  const { data: trendsSurge, isLoading: surgeLoading } = useAmazonTrends({ trendType: 'RankSurge' });
  const { data: trendsConsistent, isLoading: consistentLoading } = useAmazonTrends({ trendType: 'ConsistentPerformer' });
  const { data: history, isLoading: historyLoading } = useAmazonProductHistory(selectedAsin);

  useEffect(() => {
    if (!selectedAsin && products && products.length > 0) {
      setSelectedAsin(products[0].asin);
    }
  }, [products, selectedAsin]);

  const columns = useMemo<ColumnsType<AmazonProductListItem>>(
    () => [
      {
        title: 'ASIN',
        dataIndex: 'asin',
        key: 'asin',
        width: 140,
      },
      {
        title: '产品标题',
        dataIndex: 'title',
        key: 'title',
      },
      {
        title: '类目',
        dataIndex: 'categoryName',
        key: 'categoryName',
        width: 200,
      },
      {
        title: '排名',
        dataIndex: 'latestRank',
        key: 'latestRank',
        width: 120,
        render: (value: number | null) => (value ? `#${value}` : '—'),
      },
      {
        title: '价格',
        dataIndex: 'latestPrice',
        key: 'latestPrice',
        width: 120,
        render: formatCurrency,
      },
      {
        title: '评分',
        dataIndex: 'latestRating',
        key: 'latestRating',
        width: 100,
        render: (value: number | null) => (value ? value.toFixed(1) : '—'),
      },
      {
        title: '评论数',
        dataIndex: 'latestReviews',
        key: 'latestReviews',
        width: 120,
        render: formatNumber,
      },
      {
        title: '更新时间',
        dataIndex: 'lastUpdated',
        key: 'lastUpdated',
        width: 180,
        render: (value: string | null) => (value ? dayjs(value).format('MM-DD HH:mm') : '—'),
      },
    ],
    [],
  );

  return (
    <ProShell
      title="Amazon 运营中控台"
      description="跟踪榜单产品核心指标、趋势信号与历史走势，为跨境运营提供决策依据。"
      overview={
        <Card bordered={false} style={{ background: 'rgba(15,23,42,0.75)' }}>
          <Space direction="vertical" size={12} style={{ width: '100%' }}>
            <TeamSwitcher />
            <Typography.Text type="secondary">榜单核心指标</Typography.Text>
            {metricsLoading ? (
              <Skeleton active paragraph={false} />
            ) : metrics ? (
              <Space size={16} wrap>
                <Statistic title="采集产品" value={metrics.totalProducts} valueStyle={{ color: '#38bdf8' }} />
                <Statistic title="新晋上榜" value={metrics.totalNewEntries} valueStyle={{ color: '#0fbf61' }} />
                <Statistic title="排名飙升" value={metrics.totalRankSurges} valueStyle={{ color: '#f97316' }} />
                <Statistic title="持续霸榜" value={metrics.totalConsistentPerformers} valueStyle={{ color: '#1677ff' }} />
              </Space>
            ) : (
              <Empty description="暂无采集数据" />
            )}
          </Space>
        </Card>
      }
    >
      <ProCard colSpan={{ xs: 24, xl: 14 }} bordered>
        <Space direction="vertical" size={12} style={{ width: '100%' }}>
          <Space align="center" style={{ width: '100%', justifyContent: 'space-between' }}>
            <Typography.Title level={4} style={{ margin: 0 }}>
              榜单产品
            </Typography.Title>
            <Input.Search
              allowClear
              placeholder="搜索 ASIN 或标题"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              onSearch={setQuery}
              style={{ width: 260 }}
            />
          </Space>
          <Table<AmazonProductListItem>
            rowKey={(record) => record.asin}
            dataSource={products ?? []}
            columns={columns}
            loading={productsLoading}
            pagination={{ pageSize: 20 }}
            onRow={(record) => ({
              onClick: () => setSelectedAsin(record.asin),
              style: { cursor: 'pointer' },
            })}
            size="small"
          />
        </Space>
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 10 }} bordered>
        <Typography.Title level={4} style={{ marginTop: 0 }}>
          历史走势
        </Typography.Title>
        <AmazonHistoryChart data={history} loading={historyLoading} asin={selectedAsin} />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 8 }} bordered title="新晋上榜">
        {newLoading ? (
          <Skeleton active />
        ) : !trendsNew?.length ? (
          <Empty description="暂无数据" />
        ) : (
          trendsNew.map((trend) => (
            <Card key={trend.asin} size="small" style={{ marginBottom: 12 }}>
              <Typography.Text strong>{trend.title}</Typography.Text>
              <div style={{ color: '#94a3b8', marginTop: 4 }}>{trend.description}</div>
            </Card>
          ))
        )}
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 8 }} bordered title="排名飙升">
        {surgeLoading ? (
          <Skeleton active />
        ) : !trendsSurge?.length ? (
          <Empty description="暂无数据" />
        ) : (
          trendsSurge.map((trend) => (
            <Card key={trend.asin} size="small" style={{ marginBottom: 12 }}>
              <Typography.Text strong>{trend.title}</Typography.Text>
              <div style={{ color: '#94a3b8', marginTop: 4 }}>{trend.description}</div>
            </Card>
          ))
        )}
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 8 }} bordered title="持续霸榜">
        {consistentLoading ? (
          <Skeleton active />
        ) : !trendsConsistent?.length ? (
          <Empty description="暂无数据" />
        ) : (
          trendsConsistent.map((trend) => (
            <Card key={trend.asin} size="small" style={{ marginBottom: 12 }}>
              <Typography.Text strong>{trend.title}</Typography.Text>
              <div style={{ color: '#94a3b8', marginTop: 4 }}>{trend.description}</div>
            </Card>
          ))
        )}
      </ProCard>
    </ProShell>
  );
};

export default function AmazonOperationsPage() {
  return <OperationsContent />;
}
