import { Card, Empty, Input, List, Skeleton, Statistic, Table, Tag, Typography } from 'antd';
import { useMemo, useState } from 'react';
import dayjs from 'dayjs';
import { useAmazonCoreMetrics, useAmazonProducts, useAmazonProductHistory, useAmazonTrends } from '@/hooks/useAmazonDashboard';
import type { AmazonProductListItem, AmazonTrendListItem, AmazonTrendType } from '@/types/amazon';
import styles from './AmazonPage.module.css';

// 趋势类型映射中文标签，方便在前端直接展示。
const trendTypeLabels: Record<AmazonTrendType, string> = {
  NewEntry: '新晋上榜',
  RankSurge: '排名飙升',
  ConsistentPerformer: '持续霸榜',
};

const trendColors: Record<AmazonTrendType, string> = {
  NewEntry: 'green',
  RankSurge: 'volcano',
  ConsistentPerformer: 'blue',
};

// 工具函数：格式化价格。
const formatCurrency = (value?: number | null) => {
  if (value === null || value === undefined) {
    return '—';
  }
  return `$${value.toFixed(2)}`;
};

// 工具函数：格式化整数，兼容空值。
const formatNumber = (value?: number | null) => {
  if (value === null || value === undefined) {
    return '—';
  }
  return value.toLocaleString();
};

export const AmazonPage = () => {
    const [search, setSearch] = useState('');
    const [query, setQuery] = useState('');
    const [selectedAsin, setSelectedAsin] = useState<string>();

    // React Query 调用服务端 API，实时拉取核心指标、榜单数据和趋势列表。
    const { data: metrics, isLoading: metricsLoading } = useAmazonCoreMetrics();
    const { data: newEntries, isLoading: newEntryLoading } = useAmazonTrends({ trendType: 'NewEntry' });
    const { data: rankSurges, isLoading: rankSurgeLoading } = useAmazonTrends({ trendType: 'RankSurge' });
    const { data: consistent, isLoading: consistentLoading } = useAmazonTrends({ trendType: 'ConsistentPerformer' });
    const { data: products, isLoading: productsLoading } = useAmazonProducts({ search: query || undefined });
    const { data: history, isLoading: historyLoading } = useAmazonProductHistory(selectedAsin);

  const tableData = useMemo(() => products ?? [], [products]);

  const columns = [
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
      width: 160,
    },
    {
      title: '排名',
      dataIndex: 'latestRank',
      key: 'latestRank',
      width: 100,
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
      width: 160,
      render: (value: string | null) => (value ? dayjs(value).format('MM-DD HH:mm') : '—'),
    },
  ];

  const handleSearch = () => {
    setQuery(search.trim());
  };

  const handleRowClick = (record: AmazonProductListItem) => {
    setSelectedAsin(record.asin);
  };

  return (
    <div className={styles.wrapper}>
      <Typography.Title level={3}>Amazon 榜单洞察</Typography.Title>
      <div className={styles.metricRow}>
        <Card bordered={false}>
          {metricsLoading ? (
            <Skeleton active paragraph={false} />
          ) : metrics ? (
            <Statistic title="采集产品" value={metrics.totalProducts} />
          ) : (
            <Empty description="暂无采集数据" />
          )}
        </Card>
        <Card bordered={false}>
          {metricsLoading ? (
            <Skeleton active paragraph={false} />
          ) : metrics ? (
            <Statistic title="新晋上榜" value={metrics.totalNewEntries} valueStyle={{ color: '#0fbf61' }} />
          ) : (
            <Empty description="暂无数据" />
          )}
        </Card>
        <Card bordered={false}>
          {metricsLoading ? (
            <Skeleton active paragraph={false} />
          ) : metrics ? (
            <Statistic title="排名飙升" value={metrics.totalRankSurges} valueStyle={{ color: '#f97316' }} />
          ) : (
            <Empty description="暂无数据" />
          )}
        </Card>
        <Card bordered={false}>
          {metricsLoading ? (
            <Skeleton active paragraph={false} />
          ) : metrics ? (
            <Statistic title="持续霸榜" value={metrics.totalConsistentPerformers} valueStyle={{ color: '#1677ff' }} />
          ) : (
            <Empty description="暂无数据" />
          )}
        </Card>
      </div>

      <div className={styles.splitLayout}>
        <Card title="榜单产品" extra={<Input.Search allowClear placeholder="搜索 ASIN 或标题" value={search} onChange={(e) => setSearch(e.target.value)} onSearch={handleSearch} style={{ width: 260 }} />}>
          <Table<AmazonProductListItem>
            rowKey={(record) => record.asin}
            dataSource={tableData}
            columns={columns}
            loading={productsLoading}
            pagination={{ pageSize: 20 }}
            onRow={(record) => ({
              onClick: () => handleRowClick(record),
            })}
            size="small"
          />
        </Card>
        <Card title="历史走势">
          {historyLoading ? (
            <Skeleton active />
          ) : history && history.length > 0 ? (
            <List
              size="small"
              dataSource={[...history].reverse()}
              renderItem={(item) => (
                <List.Item>
                  <List.Item.Meta
                    title={`#${item.rank} · ${dayjs(item.timestamp).format('MM-DD HH:mm')}`}
                    description={`价格 ${formatCurrency(item.price)} · 评分 ${item.rating ?? '—'} · 评论 ${formatNumber(item.reviewsCount)}`}
                  />
                </List.Item>
              )}
            />
          ) : (
            <Empty description={selectedAsin ? '未找到历史数据' : '点击左侧产品查看历史'} />
          )}
        </Card>
      </div>

      <div className={styles.trendCards}>
        <TrendCard
          title="新晋上榜"
          loading={newEntryLoading}
          items={newEntries ?? []}
          emptyText="暂无新晋上榜记录"
        />
        <TrendCard
          title="排名飙升"
          loading={rankSurgeLoading}
          items={rankSurges ?? []}
          emptyText="暂无排名飙升记录"
        />
        <TrendCard
          title="持续霸榜"
          loading={consistentLoading}
          items={consistent ?? []}
          emptyText="暂无持续霸榜记录"
        />
      </div>
    </div>
  );
};

interface TrendCardProps {
  title: string;
  items: AmazonTrendListItem[];
  loading: boolean;
  emptyText: string;
}

// 趋势列表卡片，复用在三个趋势分组中。
const TrendCard = ({ title, items, loading, emptyText }: TrendCardProps) => (
  <Card title={title} className={styles.trendCardList}>
    {loading ? (
      <Skeleton active />
    ) : items.length === 0 ? (
      <Empty description={emptyText} image={Empty.PRESENTED_IMAGE_SIMPLE} />
    ) : (
      <List
        size="small"
        dataSource={items}
        renderItem={(item) => (
          <List.Item>
            <List.Item.Meta
              title={
                <div>
                  <Tag color={trendColors[item.trendType]}>{trendTypeLabels[item.trendType]}</Tag>
                  <Typography.Text strong style={{ marginLeft: 8 }}>
                    {item.title}
                  </Typography.Text>
                </div>
              }
              description={item.description}
            />
          </List.Item>
        )}
      />
    )}
  </Card>
);
