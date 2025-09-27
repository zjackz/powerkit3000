'use client';

import { useMemo, useState } from 'react';
import { ProCard } from '@ant-design/pro-components';
import { Card, Col, Row, Skeleton, Statistic, Typography } from 'antd';
import { ProShell } from '@/layouts/ProShell';
import { SystemHealthPanel } from '@/components/monitoring/SystemHealthPanel';
import { AnalyticsFilters } from '@/components/analytics/AnalyticsFilters';
import { TrendSummaryBoard } from '@/components/analytics/TrendSummaryBoard';
import { MonthlyTrendChart } from '@/components/analytics/MonthlyTrendChart';
import { FundingDistributionChart } from '@/components/analytics/FundingDistributionChart';
import { CategorySuccessChart } from '@/components/analytics/CategorySuccessChart';
import { CountrySuccessChart } from '@/components/analytics/CountrySuccessChart';
import { TopProjectsList } from '@/components/analytics/TopProjectsList';
import { TopCreatorsList } from '@/components/analytics/TopCreatorsList';
import { HypeProjectsList } from '@/components/analytics/HypeProjectsList';
import { CategoryKeywordCloud } from '@/components/analytics/CategoryKeywordCloud';
import { useProjectFilters } from '@/hooks/useProjectFilters';
import { useProjectSummary } from '@/hooks/useProjectSummary';
import { useMonthlyTrend } from '@/hooks/useMonthlyTrend';
import { useFundingDistribution } from '@/hooks/useFundingDistribution';
import { useCategoryInsights } from '@/hooks/useCategoryInsights';
import { useCountryInsights } from '@/hooks/useCountryInsights';
import { useTopProjects } from '@/hooks/useTopProjects';
import { useCreatorPerformance } from '@/hooks/useCreatorPerformance';
import { useHypeProjects } from '@/hooks/useHypeProjects';
import { useCategoryKeywords } from '@/hooks/useCategoryKeywords';
import { useSystemHealth } from '@/hooks/useSystemHealth';
import type { AnalyticsFilterRequest } from '@/types/project';

const DEFAULT_ANALYTICS_FILTERS: AnalyticsFilterRequest = { minPercentFunded: 200 };

const DashboardContent = () => {
  const [filters, setFilters] = useState<AnalyticsFilterRequest>(DEFAULT_ANALYTICS_FILTERS);

  const { data: filterOptions, isLoading: filterLoading } = useProjectFilters();

  const normalizedFilters = useMemo(() => filters, [filters]);
  const hypeParams = useMemo(
    () => ({
      ...normalizedFilters,
      minPercentFunded: normalizedFilters.minPercentFunded ?? 200,
      limit: 6,
    }),
    [normalizedFilters],
  );

  const keywordCategory = useMemo(() => {
    if (normalizedFilters.categories && normalizedFilters.categories.length > 0) {
      return normalizedFilters.categories[0];
    }
    return filterOptions?.categories?.[0]?.value;
  }, [normalizedFilters, filterOptions]);

  const { data: summary, isLoading: summaryLoading } = useProjectSummary(normalizedFilters);
  const { data: monthlyTrend, isLoading: trendLoading } = useMonthlyTrend(normalizedFilters);
  const { data: fundingDistribution, isLoading: distributionLoading } = useFundingDistribution(normalizedFilters);
  const { data: categoryInsights, isLoading: categoriesLoading } = useCategoryInsights(normalizedFilters);
  const { data: countryInsights, isLoading: countriesLoading } = useCountryInsights(normalizedFilters);
  const { data: topProjects, isLoading: topProjectsLoading } = useTopProjects(normalizedFilters);
  const { data: creatorPerformance, isLoading: creatorsLoading } = useCreatorPerformance(normalizedFilters);
  const { data: hypeProjects, isLoading: hypeLoading } = useHypeProjects(hypeParams);
  const { data: categoryKeywords, isLoading: keywordsLoading } = useCategoryKeywords(keywordCategory, normalizedFilters);
  const { data: systemHealth } = useSystemHealth();

  const kpiOverview = (
    <Row gutter={[16, 16]}>
      <Col xs={24} md={12} xl={6}>
        <Card bordered={false} style={{ background: 'rgba(30,41,59,0.65)' }}>
          {summaryLoading ? (
            <Skeleton active paragraph={false} title={{ width: '60%' }} />
          ) : (
            <Statistic title="导入项目" value={summary?.totalProjects ?? 0} suffix="个" valueStyle={{ color: '#38bdf8' }} />
          )}
        </Card>
      </Col>
      <Col xs={24} md={12} xl={6}>
        <Card bordered={false} style={{ background: 'rgba(30,41,59,0.65)' }}>
          {summaryLoading ? (
            <Skeleton active paragraph={false} title={{ width: '60%' }} />
          ) : (
            <Statistic title="成功率" value={summary?.successRate ?? 0} suffix="%" precision={1} valueStyle={{ color: '#0fbf61' }} />
          )}
        </Card>
      </Col>
      <Col xs={24} md={12} xl={6}>
        <Card bordered={false} style={{ background: 'rgba(30,41,59,0.65)' }}>
          {summaryLoading ? (
            <Skeleton active paragraph={false} title={{ width: '60%' }} />
          ) : (
            <Statistic title="总筹资" value={summary?.totalPledged ?? 0} prefix="$" precision={0} />
          )}
        </Card>
      </Col>
      <Col xs={24} md={12} xl={6}>
        <Card bordered={false} style={{ background: 'rgba(30,41,59,0.65)' }}>
          <Statistic title="导入文件" value={systemHealth?.totalImportFiles ?? 0} suffix="个" valueStyle={{ color: '#fbbf24' }} />
        </Card>
      </Col>
    </Row>
  );

  return (
    <ProShell
      title="全局驾驶舱"
      description="查看 Kickstarter + Amazon 双域指标，快速洞察高热项目走势。"
      overview={
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <Typography.Text strong style={{ color: '#cbd5f5' }}>MISSION X · 默认视角</Typography.Text>
          {kpiOverview}
          <SystemHealthPanel />
        </div>
      }
    >
      <ProCard colSpan={24} bordered hoverable>
        <Typography.Title level={4} style={{ marginTop: 0 }}>MISSION X · 筛选器</Typography.Title>
        <Typography.Paragraph type="secondary" style={{ marginBottom: 12 }}>
          默认聚焦高达成率项目，可根据需要调整筛选条件。
        </Typography.Paragraph>
        <AnalyticsFilters
          options={filterOptions}
          value={filters}
          onChange={setFilters}
          loading={filterLoading}
        />
      </ProCard>
      <ProCard colSpan={24} bordered>
        <TrendSummaryBoard data={monthlyTrend} loading={trendLoading} />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 16 }} bordered>
        <MonthlyTrendChart data={monthlyTrend} loading={trendLoading} />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 8 }} bordered>
        <FundingDistributionChart data={fundingDistribution} loading={distributionLoading} />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 12 }} bordered>
        <CategorySuccessChart data={categoryInsights?.slice(0, 10)} loading={categoriesLoading} />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 12 }} bordered>
        <CountrySuccessChart data={countryInsights?.slice(0, 10)} loading={countriesLoading} />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 14 }} bordered>
        <TopProjectsList data={topProjects?.slice(0, 8)} loading={topProjectsLoading} />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 10 }} bordered>
        <TopCreatorsList data={creatorPerformance?.slice(0, 10)} loading={creatorsLoading} />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 14 }} bordered>
        <HypeProjectsList data={hypeProjects?.slice(0, 6)} loading={hypeLoading} />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 10 }} bordered>
        <CategoryKeywordCloud data={categoryKeywords} loading={keywordsLoading} category={keywordCategory} />
      </ProCard>
    </ProShell>
  );
};

export default function DashboardPage() {
  return <DashboardContent />;
}
