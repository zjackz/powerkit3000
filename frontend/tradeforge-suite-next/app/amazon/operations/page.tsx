'use client';

import { useMemo, useState } from 'react';
import { ProCard, StatisticCard } from '@ant-design/pro-components';
import {
  Alert,
  Badge,
  Button,
  Card,
  Divider,
  Drawer,
  Empty,
  Input,
  Select,
  Skeleton,
  Space,
  Segmented,
  Table,
  Tag,
  Typography,
  Descriptions,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { ProShell } from '@/layouts/ProShell';
import { TeamSwitcher } from '@/components/team/TeamSwitcher';
import {
  useAmazonOperationalIssues,
  useAmazonOperationalSummary,
} from '@/hooks/useAmazonDashboard';
import type {
  AmazonOperationalIssue,
  AmazonOperationalIssueType,
  AmazonOperationalSeverity,
} from '@/types/amazon';
import {
  AlertOutlined,
  DatabaseOutlined,
  FireTwoTone,
  MessageOutlined,
  StopOutlined,
  ThunderboltOutlined,
} from '@ant-design/icons';
import { theme as antdTheme } from 'antd';
import { useThemeMode } from '@/contexts/ThemeContext';

const ISSUE_TYPE_LABEL: Record<Exclude<AmazonOperationalIssueType, 'AdWaste'>, string> = {
  LowStock: '库存告警',
  NegativeReview: '差评风险',
};

const SEVERITY_LABEL: Record<AmazonOperationalSeverity, string> = {
  High: '高',
  Medium: '中',
  Low: '低',
};

const SEVERITY_COLOR: Record<AmazonOperationalSeverity, string> = {
  High: 'red',
  Medium: 'orange',
  Low: 'gold',
};

const pageSize = 20;

const severityLabels: Record<AmazonOperationalSeverity, string> = {
  High: '高风险',
  Medium: '中风险',
  Low: '低风险',
};

const issueTypeSegments: Array<{ label: string; value: AmazonOperationalIssueType | 'all'; icon: JSX.Element }> = [
  { label: '全部', value: 'all', icon: <DatabaseOutlined /> },
  { label: ISSUE_TYPE_LABEL.LowStock, value: 'LowStock', icon: <AlertOutlined /> },
  { label: ISSUE_TYPE_LABEL.NegativeReview, value: 'NegativeReview', icon: <MessageOutlined /> },
];

const formatInventoryDays = (value?: number | null) => {
  if (value === null || value === undefined) {
    return '—';
  }
  if (value < 1) {
    return '不足 1 天';
  }
  return `${value.toFixed(1)} 天`;
};

const formatDateTime = (value?: string | null) =>
  value ? dayjs(value).format('YYYY-MM-DD HH:mm') : '—';

const OperationsContent = () => {
  const [issueType, setIssueType] = useState<AmazonOperationalIssueType | 'all'>('all');
  const [severity, setSeverity] = useState<AmazonOperationalSeverity | 'all'>('all');
  const [search, setSearch] = useState('');
  const [pendingSearch, setPendingSearch] = useState('');
  const [page, setPage] = useState(1);
  const [selectedIssue, setSelectedIssue] = useState<AmazonOperationalIssue | null>(null);
  const { mode } = useThemeMode();
  const isDark = mode === 'dark';
  const { token } = antdTheme.useToken();

  const {
    data: summary,
    isLoading: summaryLoading,
  } = useAmazonOperationalSummary();
  const {
    data: issuesData,
    isLoading: issuesLoading,
  } = useAmazonOperationalIssues({
    issueType: issueType === 'all' ? undefined : issueType,
    severity: severity === 'all' ? undefined : severity,
    search: search || undefined,
    page,
    pageSize,
  });

  const issues = issuesData?.items ?? [];
  const total = issuesData?.total ?? 0;

  const columns = useMemo<ColumnsType<AmazonOperationalIssue>>((): ColumnsType<AmazonOperationalIssue> => [
    {
      title: 'ASIN',
      dataIndex: 'asin',
      key: 'asin',
      width: 140,
      render: (value: string) => (
        <Typography.Text strong copyable>{value}</Typography.Text>
      ),
    },
    {
      title: '产品',
      dataIndex: 'title',
      key: 'title',
      ellipsis: true,
      render: (value: string) => <Typography.Text>{value}</Typography.Text>,
    },
    {
      title: '问题类型',
      dataIndex: 'issueType',
      key: 'issueType',
      width: 120,
      render: (value: AmazonOperationalIssueType) => (
        <Space size={4}>
          {value === 'LowStock' ? <AlertOutlined style={{ color: '#f59e0b' }} /> : <MessageOutlined style={{ color: '#ef4444' }} />}
          <Typography.Text>{ISSUE_TYPE_LABEL[value as Exclude<AmazonOperationalIssueType, 'AdWaste'>] ?? value}</Typography.Text>
        </Space>
      ),
    },
    {
      title: '严重度',
      dataIndex: 'severity',
      key: 'severity',
      width: 120,
      render: (value: AmazonOperationalSeverity) => (
        <Tag color={SEVERITY_COLOR[value]} style={{ fontWeight: 600 }}>
          {SEVERITY_LABEL[value]}
        </Tag>
      ),
    },
    {
      title: '关键指标',
      key: 'kpi',
      render: (_, record) => {
        if (record.issueType === 'LowStock') {
          return (
            <Space direction="vertical" size={4}>
              <Typography.Text type="secondary">库存天数：{formatInventoryDays(record.kpi.inventoryDays)}</Typography.Text>
              <Typography.Text type="secondary">库存数量：{record.kpi.inventoryQuantity ?? '—'}</Typography.Text>
              <Typography.Text type="secondary">近 7 日销量：{record.kpi.unitsSold7d ?? '—'}</Typography.Text>
            </Space>
          );
        }

        return (
          <Space direction="vertical" size={4}>
            <Typography.Text type="secondary">差评数量：{record.kpi.negativeReviewCount}</Typography.Text>
            <Typography.Text type="secondary">最新差评时间：{formatDateTime(record.kpi.latestNegativeReviewAt)}</Typography.Text>
            {record.kpi.latestNegativeReviewExcerpt && (
              <Typography.Paragraph ellipsis={{ rows: 2 }} style={{ marginBottom: 0 }}>
                {record.kpi.latestNegativeReviewExcerpt}
              </Typography.Paragraph>
            )}
          </Space>
        );
      },
    },
    {
      title: '建议动作',
      dataIndex: 'recommendation',
      key: 'recommendation',
      ellipsis: true,
      render: (value: string) => (
        <Typography.Text type="secondary">{value}</Typography.Text>
      ),
    },
    {
      title: '采集时间',
      dataIndex: 'capturedAt',
      key: 'capturedAt',
      width: 160,
      render: (value: string) => dayjs(value).format('MM-DD HH:mm'),
    },
    {
      title: '操作',
      key: 'action',
      width: 120,
      render: (_, record) =>
        record.kpi.latestNegativeReviewUrl ? (
          <Button
            type="link"
            size="small"
            href={record.kpi.latestNegativeReviewUrl}
            target="_blank"
            rel="noopener noreferrer"
          >
            查看差评
          </Button>
        ) : (
          <Typography.Text type="secondary">—</Typography.Text>
        ),
    },
  ], []);

  const renderSummaryCards = () => {
    if (summaryLoading) {
      return <Skeleton active />;
    }

    if (!summary) {
      return <Empty description="暂无运营数据" />;
    }

    const cards = [
      {
        title: '库存风险',
        value: summary.lowStock.total,
        footer: `高 ${summary.lowStock.high} / 中 ${summary.lowStock.medium} / 低 ${summary.lowStock.low}`,
        icon: <AlertOutlined style={{ color: isDark ? '#fbbf24' : '#f97316' }} />,
        color: isDark
          ? 'linear-gradient(135deg, rgba(251, 191, 36, 0.32), rgba(250, 204, 21, 0.12))'
          : 'linear-gradient(135deg, rgba(255, 214, 102, 0.45), rgba(253, 230, 138, 0.2))',
      },
      {
        title: '差评警报',
        value: summary.negativeReview.total,
        footer: `高 ${summary.negativeReview.high} / 中 ${summary.negativeReview.medium} / 低 ${summary.negativeReview.low}`,
        icon: <MessageOutlined style={{ color: isDark ? '#fb7185' : '#e11d48' }} />,
        color: isDark
          ? 'linear-gradient(135deg, rgba(248, 113, 113, 0.38), rgba(251, 146, 146, 0.15))'
          : 'linear-gradient(135deg, rgba(252, 165, 165, 0.5), rgba(254, 226, 226, 0.25))',
      },
      {
        title: '广告模块',
        value: summary.adWastePlaceholder.status === 'comingSoon' ? '开发中' : summary.adWastePlaceholder.status,
        footer: summary.adWastePlaceholder.message,
        icon: <ThunderboltOutlined style={{ color: token.colorPrimary }} />,
        color: isDark
          ? 'linear-gradient(135deg, rgba(56,189,248,0.32), rgba(59,130,246,0.16))'
          : 'linear-gradient(135deg, rgba(219,234,254,0.65), rgba(191,219,254,0.35))',
      },
    ];

    return (
      <>
        <Space align="center" size={12} wrap>
          <TeamSwitcher />
          <Divider type="vertical" style={{ margin: 0, height: 20 }} />
          <Typography.Text type="secondary">最后更新时间：</Typography.Text>
          <Typography.Text strong style={{ fontSize: 16 }}>
            {summary.lastUpdatedAt ? dayjs(summary.lastUpdatedAt).format('YYYY-MM-DD HH:mm:ss') : '暂无数据'}
          </Typography.Text>
          {summary.isStale && <Tag color="orange">数据超过阈值</Tag>}
        </Space>
        <Space size={16} wrap style={{ width: '100%', marginTop: 16 }}>
          {cards.map((item) => (
            <StatisticCard
              key={item.title}
              statistic={{
                title: item.title,
                value: item.value,
                suffix: item.title === '广告模块' ? undefined : '项',
              }}
              chart={<div style={{ fontSize: 32 }}>{item.icon}</div>}
              style={{
                width: 260,
                background: item.color,
                border: 'none',
                backdropFilter: 'blur(6px)',
              }}
              footer={<Typography.Text type="secondary">{item.footer}</Typography.Text>}
            />
          ))}
        </Space>
      </>
    );
  };

  const resetFilters = () => {
    setIssueType('all');
    setSeverity('all');
    setSearch('');
    setPendingSearch('');
    setPage(1);
  };

  const severityLegend = useMemo(
    () => (
      <Space size={12} wrap>
        {Object.entries(severityLabels).map(([key, label]) => (
          <Badge
            key={key}
            color={SEVERITY_COLOR[key as AmazonOperationalSeverity]}
            text={label}
          />
        ))}
      </Space>
    ),
    [],
  );

  return (
    <ProShell
      title="Amazon 运营仪表盘"
      description="聚焦库存与差评风险，帮助运营快速锁定优先处理事项。"
      overview={
        <Card
          bordered={false}
          style={{
            background: isDark
              ? 'linear-gradient(135deg, rgba(15,23,42,0.88), rgba(37,99,235,0.55))'
              : 'linear-gradient(135deg, rgba(219,234,254,0.9), rgba(191,219,254,0.6))',
            color: isDark ? '#fff' : token.colorTextBase,
            boxShadow: isDark
              ? '0 18px 42px rgba(30,64,175,0.35)'
              : '0 16px 36px rgba(148, 163, 184, 0.25)',
          }}
        >
          {renderSummaryCards()}
        </Card>
      }
    >
      <ProCard colSpan={{ xs: 24, xl: 18 }} bordered headerBordered title="风险清单">
        <Space direction="vertical" size={16} style={{ width: '100%' }}>
          <Space direction="vertical" size={10} style={{ width: '100%' }}>
            <Space wrap align="center" style={{ width: '100%', justifyContent: 'space-between' }}>
              <Space wrap size={12}>
                <Segmented
                  size="large"
                  value={issueType}
                  onChange={(value) => {
                    setIssueType(value as AmazonOperationalIssueType | 'all');
                    setPage(1);
                  }}
                  options={issueTypeSegments.map((item) => ({
                    label: (
                      <Space size={6}>
                        {item.icon}
                        <span>{item.label}</span>
                      </Space>
                    ),
                    value: item.value,
                  }))}
                />
                <Select
                  value={severity}
                  style={{ width: 160 }}
                  onChange={(value) => {
                    setSeverity(value);
                    setPage(1);
                  }}
                  options={[
                    { label: '全部严重度', value: 'all' },
                    { label: '高风险', value: 'High' },
                    { label: '中风险', value: 'Medium' },
                    { label: '低风险', value: 'Low' },
                  ]}
                />
              </Space>
              <Space size={12}>
                <Input.Search
                  allowClear
                  placeholder="搜索 ASIN 或标题"
                  value={pendingSearch}
                  onChange={(event) => setPendingSearch(event.target.value)}
                  onSearch={(value) => {
                    setSearch(value.trim());
                    setPage(1);
                  }}
                  style={{ width: 260 }}
                />
                <Button onClick={resetFilters} icon={<StopOutlined />}>重置</Button>
              </Space>
            </Space>
            <Space style={{ width: '100%', justifyContent: 'space-between' }} wrap>
              {severityLegend}
              <Typography.Text type="secondary">点击行可查看详细建议</Typography.Text>
            </Space>
          </Space>
          <Table<AmazonOperationalIssue>
            rowKey={(record) => `${record.asin}-${record.issueType}`}
            dataSource={issues}
            columns={columns}
            loading={issuesLoading}
            locale={{ emptyText: issuesLoading ? <Skeleton active /> : <Empty description="暂无风险" /> }}
            pagination={{
              current: page,
              pageSize,
              total,
              onChange: (nextPage) => setPage(nextPage),
              showTotal: (count) => `共 ${count} 条记录`,
            }}
            onRow={(record) => ({
              onClick: () => setSelectedIssue(record),
              style: { cursor: 'pointer' },
            })}
            size="middle"
          />
        </Space>
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 6 }} bordered title="运营提示">
        <Space direction="vertical" size={16} style={{ width: '100%' }}>
          <Card
            bordered={false}
            style={{
              background: isDark
                ? 'linear-gradient(120deg, rgba(59,130,246,0.25), rgba(14,165,233,0.25))'
                : 'linear-gradient(120deg, rgba(191,219,254,0.6), rgba(165,243,252,0.4))',
              borderRadius: 16,
            }}
          >
            <Space align="start">
              <FireTwoTone twoToneColor={token.colorPrimary} style={{ fontSize: 28 }} />
              <Space direction="vertical" size={12} style={{ flex: 1 }}>
                <Typography.Title level={5} style={{ margin: 0 }}>
                  广告模块占位
                </Typography.Title>
                <Typography.Paragraph style={{ marginBottom: 0 }}>
                  {issuesData?.adWastePlaceholder?.message ?? '广告浪费分析正在接入中，敬请期待下一迭代。'}
                </Typography.Paragraph>
              </Space>
            </Space>
          </Card>
          <Alert
            type={isDark ? 'success' : 'info'}
            showIcon
            message="操作建议"
            description="优先处理高风险库存，其次关注差评集中 ASIN，可结合榜单趋势判定是否调价或暂停广告。"
          />
        </Space>
      </ProCard>
      <Drawer
        title={selectedIssue?.title}
        open={Boolean(selectedIssue)}
        onClose={() => setSelectedIssue(null)}
        width={420}
      >
        {!selectedIssue ? null : (
          <Space direction="vertical" size={16} style={{ width: '100%' }}>
            <Space size={8}>
              <Tag color={SEVERITY_COLOR[selectedIssue.severity]}>{SEVERITY_LABEL[selectedIssue.severity]}</Tag>
              <Typography.Text type="secondary">{ISSUE_TYPE_LABEL[selectedIssue.issueType as Exclude<AmazonOperationalIssueType, 'AdWaste'>]}</Typography.Text>
            </Space>
            <Typography.Paragraph>{selectedIssue.recommendation}</Typography.Paragraph>
            <Descriptions bordered size="small" column={1}>
              <Descriptions.Item label="ASIN">{selectedIssue.asin}</Descriptions.Item>
              <Descriptions.Item label="问题类型">
                {ISSUE_TYPE_LABEL[selectedIssue.issueType as Exclude<AmazonOperationalIssueType, 'AdWaste'>] ?? selectedIssue.issueType}
              </Descriptions.Item>
              <Descriptions.Item label="采集时间">
                {dayjs(selectedIssue.capturedAt).format('YYYY-MM-DD HH:mm:ss')}
              </Descriptions.Item>
              {selectedIssue.issueType === 'LowStock' ? (
                <>
                  <Descriptions.Item label="库存天数">
                    {formatInventoryDays(selectedIssue.kpi.inventoryDays)}
                  </Descriptions.Item>
                  <Descriptions.Item label="库存数量">
                    {selectedIssue.kpi.inventoryQuantity ?? '—'}
                  </Descriptions.Item>
                  <Descriptions.Item label="近 7 日销量">
                    {selectedIssue.kpi.unitsSold7d ?? '—'}
                  </Descriptions.Item>
                </>
              ) : (
                <>
                  <Descriptions.Item label="差评数量">
                    {selectedIssue.kpi.negativeReviewCount}
                  </Descriptions.Item>
                  <Descriptions.Item label="最新差评时间">
                    {formatDateTime(selectedIssue.kpi.latestNegativeReviewAt)}
                  </Descriptions.Item>
                  {selectedIssue.kpi.latestNegativeReviewExcerpt && (
                    <Descriptions.Item label="差评摘要">
                      {selectedIssue.kpi.latestNegativeReviewExcerpt}
                    </Descriptions.Item>
                  )}
                  {selectedIssue.kpi.latestNegativeReviewUrl && (
                    <Descriptions.Item label="差评链接">
                      <Button
                        type="link"
                        href={selectedIssue.kpi.latestNegativeReviewUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        size="small"
                      >
                        查看原文
                      </Button>
                    </Descriptions.Item>
                  )}
                </>
              )}
            </Descriptions>
          </Space>
        )}
      </Drawer>
    </ProShell>
  );
};

export default function AmazonOperationsPage() {
  return <OperationsContent />;
}
