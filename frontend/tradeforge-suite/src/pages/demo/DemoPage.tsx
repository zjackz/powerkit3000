import { useMemo, useState } from 'react';
import {
  Card,
  Col,
  Divider,
  List,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Typography,
} from 'antd';
import {
  CalendarOutlined,
  FireFilled,
  FundFilled,
  ShopOutlined,
  ThunderboltOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { MonthlyTrendChart } from '@/components/analytics/MonthlyTrendChart';
import { FundingDistributionChart } from '@/components/analytics/FundingDistributionChart';
import { CategorySuccessChart } from '@/components/analytics/CategorySuccessChart';
import { CountrySuccessChart } from '@/components/analytics/CountrySuccessChart';
import { TopProjectsList } from '@/components/analytics/TopProjectsList';
import { TopCreatorsList } from '@/components/analytics/TopCreatorsList';
import { HypeProjectsList } from '@/components/analytics/HypeProjectsList';
import { CategoryKeywordCloud } from '@/components/analytics/CategoryKeywordCloud';
import type { AmazonProductListItem } from '@/types/amazon';
import {
  DEMO_AMAZON_CORE_METRICS,
  DEMO_AMAZON_HISTORY,
  DEMO_AMAZON_PRODUCTS,
  DEMO_AMAZON_TRENDS,
  DEMO_CATEGORY_INSIGHTS,
  DEMO_CATEGORY_KEYWORDS,
  DEMO_COUNTRY_INSIGHTS,
  DEMO_CREATOR_PERFORMANCE,
  DEMO_FUNDING_DISTRIBUTION,
  DEMO_HYPE_PROJECTS,
  DEMO_MONTHLY_TREND,
  DEMO_PROJECT_SUMMARY,
  DEMO_TOP_PROJECTS,
} from '@/mocks/demoDashboard';
import styles from './DemoPage.module.css';

const formatPrice = (price?: number | null) => (price === undefined || price === null ? '—' : `$${price.toFixed(2)}`);
const formatNumber = (value?: number | null) => (value === undefined || value === null ? '—' : value.toLocaleString());

const amazonColumns = [
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
    width: 180,
  },
  {
    title: '最新排名',
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
    render: (value: number | null) => formatPrice(value),
  },
  {
    title: '评分',
    dataIndex: 'latestRating',
    key: 'latestRating',
    width: 120,
    render: (value: number | null) => (value ? value.toFixed(1) : '—'),
  },
  {
    title: '评论数',
    dataIndex: 'latestReviews',
    key: 'latestReviews',
    width: 140,
    render: (value: number | null) => formatNumber(value),
  },
  {
    title: '更新时间',
    dataIndex: 'lastUpdated',
    key: 'lastUpdated',
    width: 180,
    render: (value: string | null) => (value ? dayjs(value).format('YYYY-MM-DD HH:mm') : '—'),
  },
];

const actionHighlights = [
  '建议优先跟进技术与设计双类目合作商，成功率维持在 70%+。',
  'Amazon 榜单中 30-50 美元价格段热度攀升，可策划“入门价爆品”合集。',
  'PulseFit、VoltShield 等跨境场景项目适合纳入直播推广排期。',
];

const DemoPage = () => {
  const categoryOptions = useMemo(
    () => Object.keys(DEMO_CATEGORY_KEYWORDS).map((value) => ({ value, label: value })),
    [],
  );
  const [selectedCategory, setSelectedCategory] = useState<string>(categoryOptions[0]?.value ?? 'Technology');
  const keywordData = useMemo(
    () => DEMO_CATEGORY_KEYWORDS[selectedCategory] ?? [],
    [selectedCategory],
  );

  const [selectedAsin, setSelectedAsin] = useState<string>(DEMO_AMAZON_PRODUCTS[0]?.asin ?? '');
  const selectedProduct = useMemo(
    () => DEMO_AMAZON_PRODUCTS.find((item) => item.asin === selectedAsin),
    [selectedAsin],
  );
  // 演示用固定历史数据，强调趋势呈现即可。
  const selectedHistory = useMemo(() => DEMO_AMAZON_HISTORY, []);

  return (
    <div className={styles.wrapper}>
      <div>
        <div className={styles.sectionHeader}>
          <Typography.Title level={3}>客户演示仪表盘</Typography.Title>
          <Tag icon={<CalendarOutlined />}>{dayjs().format('YYYY 年 M 月 D 日')}</Tag>
        </div>
        <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
          汇总 Kickstarter 与 Amazon 榜单的热点项目、品类趋势与行动建议，适用于售前演示或客户路演。
        </Typography.Paragraph>
      </div>

      <Card>
        <div className={styles.metricRow}>
          <Statistic
            title="导入项目"
            value={DEMO_PROJECT_SUMMARY.totalProjects}
            suffix="个"
            valueStyle={{ color: '#1f6feb' }}
          />
          <Statistic
            title="成功率"
            value={DEMO_PROJECT_SUMMARY.successRate}
            suffix="%"
            precision={1}
            valueStyle={{ color: '#0fbf61' }}
          />
          <Statistic
            title="总筹资"
            value={DEMO_PROJECT_SUMMARY.totalPledged}
            prefix="$"
            valueStyle={{ color: '#fa541c' }}
          />
          <Statistic
            title="覆盖国家"
            value={DEMO_PROJECT_SUMMARY.distinctCountries}
            suffix="个"
          />
        </div>
      </Card>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={16}>
          <MonthlyTrendChart data={DEMO_MONTHLY_TREND} loading={false} />
        </Col>
        <Col xs={24} xl={8}>
          <FundingDistributionChart data={DEMO_FUNDING_DISTRIBUTION} loading={false} />
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={12}>
          <CategorySuccessChart data={DEMO_CATEGORY_INSIGHTS} loading={false} />
        </Col>
        <Col xs={24} xl={12}>
          <CountrySuccessChart data={DEMO_COUNTRY_INSIGHTS} loading={false} />
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={14}>
          <TopProjectsList data={DEMO_TOP_PROJECTS} loading={false} />
        </Col>
        <Col xs={24} xl={10}>
          <TopCreatorsList data={DEMO_CREATOR_PERFORMANCE} loading={false} />
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={14}>
          <HypeProjectsList data={DEMO_HYPE_PROJECTS} loading={false} />
        </Col>
        <Col xs={24} xl={10}>
          <Space style={{ marginBottom: 8 }} align="center">
            <Typography.Text strong>品类关键词热度</Typography.Text>
            <Select
              size="small"
              options={categoryOptions}
              value={selectedCategory}
              onChange={setSelectedCategory}
              style={{ minWidth: 140 }}
            />
          </Space>
          <CategoryKeywordCloud data={keywordData} loading={false} category={selectedCategory} />
        </Col>
      </Row>

      <Divider orientation="left">Amazon 榜单演示</Divider>

      <Row gutter={[16, 16]}>
        <Col xs={24} md={6}>
          <Card bordered={false} style={{ background: '#f0f5ff' }}>
            <Statistic
              title="采集产品"
              value={DEMO_AMAZON_CORE_METRICS.totalProducts}
              prefix={<ShopOutlined style={{ color: '#1f6feb', marginRight: 8 }} />}
            />
          </Card>
        </Col>
        <Col xs={24} md={6}>
          <Card bordered={false} style={{ background: '#f6ffed' }}>
            <Statistic
              title="新晋上榜"
              value={DEMO_AMAZON_CORE_METRICS.totalNewEntries}
              prefix={<FireFilled style={{ color: '#0fbf61', marginRight: 8 }} />}
            />
          </Card>
        </Col>
        <Col xs={24} md={6}>
          <Card bordered={false} style={{ background: '#fff7e6' }}>
            <Statistic
              title="排名飙升"
              value={DEMO_AMAZON_CORE_METRICS.totalRankSurges}
              prefix={<ThunderboltOutlined style={{ color: '#fa8c16', marginRight: 8 }} />}
            />
          </Card>
        </Col>
        <Col xs={24} md={6}>
          <Card bordered={false} style={{ background: '#e6f7ff' }}>
            <Statistic
              title="持续霸榜"
              value={DEMO_AMAZON_CORE_METRICS.totalConsistentPerformers}
              prefix={<FundFilled style={{ color: '#1677ff', marginRight: 8 }} />}
            />
          </Card>
        </Col>
      </Row>

      <div className={`${styles.amazonSplit} ${styles.amazonSplitFull}`}>
        <Card title="榜单产品一览">
          <Table<AmazonProductListItem>
            rowKey={(record) => record.asin}
            columns={amazonColumns}
            dataSource={DEMO_AMAZON_PRODUCTS}
            pagination={false}
            onRow={(record) => ({ onClick: () => setSelectedAsin(record.asin) })}
            bordered
            size="small"
          />
        </Card>
        <Card
          title={selectedProduct ? `${selectedProduct.title} · 趋势` : '选择左侧产品查看趋势'}
          extra={selectedProduct ? <Tag color="blue">ASIN {selectedProduct.asin}</Tag> : null}
        >
          <List
            size="small"
            dataSource={[...selectedHistory].reverse()}
            renderItem={(item) => (
              <List.Item>
                <List.Item.Meta
                  title={`#${item.rank} · ${dayjs(item.timestamp).format('MM-DD HH:mm')}`}
                  description={`价格 ${formatPrice(item.price)} · 评分 ${item.rating ?? '—'} · 评论 ${
                    formatNumber(item.reviewsCount)
                  }`}
                />
              </List.Item>
            )}
          />
        </Card>
      </div>

      <div className={styles.trendCards}>
        {(['NewEntry', 'RankSurge', 'ConsistentPerformer'] as const).map((trendType) => (
          <Card key={trendType} title={
            trendType === 'NewEntry'
              ? '新晋上榜'
              : trendType === 'RankSurge'
              ? '排名飙升'
              : '持续霸榜'
          }>
            <List
              size="small"
              dataSource={DEMO_AMAZON_TRENDS[trendType]}
              renderItem={(item) => (
                <List.Item>
                  <List.Item.Meta
                    title={
                      <Space size="middle">
                        <Tag color={trendType === 'NewEntry' ? 'green' : trendType === 'RankSurge' ? 'volcano' : 'blue'}>
                          {trendType === 'NewEntry' ? 'New Entry' : trendType === 'RankSurge' ? 'Rank Surge' : 'Consistent'}
                        </Tag>
                        <Typography.Text strong>{item.title}</Typography.Text>
                      </Space>
                    }
                    description={`${item.description} · ${dayjs(item.recordedAt).format('MM-DD HH:mm')}`}
                  />
                </List.Item>
              )}
            />
          </Card>
        ))}
      </div>

      <Card title="行动建议" type="inner">
        <List
          dataSource={actionHighlights}
          renderItem={(item, index) => (
            <List.Item>
              <List.Item.Meta
                avatar={<Tag color="blue">建议 {index + 1}</Tag>}
                description={item}
              />
            </List.Item>
          )}
        />
      </Card>
    </div>
  );
};

export default DemoPage;
