import { Card, Col, Row, Skeleton, Statistic, Typography } from 'antd';
import { useMemo, useState } from 'react';
import { useProjectSummary } from '@/hooks/useProjectSummary';
import { useCategoryInsights } from '@/hooks/useCategoryInsights';
import { useCountryInsights } from '@/hooks/useCountryInsights';
import { useTopProjects } from '@/hooks/useTopProjects';
import { useMonthlyTrend } from '@/hooks/useMonthlyTrend';
import { useFundingDistribution } from '@/hooks/useFundingDistribution';
import { useCreatorPerformance } from '@/hooks/useCreatorPerformance';
import { CategorySuccessChart } from '@/components/analytics/CategorySuccessChart';
import { CountrySuccessChart } from '@/components/analytics/CountrySuccessChart';
import { TopProjectsList } from '@/components/analytics/TopProjectsList';
import { MonthlyTrendChart } from '@/components/analytics/MonthlyTrendChart';
import { FundingDistributionChart } from '@/components/analytics/FundingDistributionChart';
import { TopCreatorsList } from '@/components/analytics/TopCreatorsList';
import { AnalyticsFilters } from '@/components/analytics/AnalyticsFilters';
import { useProjectFilters } from '@/hooks/useProjectFilters';
import type { AnalyticsFilterRequest } from '@/types/project';
import styles from './DashboardPage.module.css';

export const DashboardPage = () => {
  const [filters, setFilters] = useState<AnalyticsFilterRequest>({});
  const { data: filterOptions, isLoading: filterLoading } = useProjectFilters();

  const normalizedFilters = useMemo(() => filters, [filters]);

  const { data, isLoading } = useProjectSummary(normalizedFilters);
  const { data: categoryInsights, isLoading: categoriesLoading } = useCategoryInsights(normalizedFilters);
  const { data: countryInsights, isLoading: countriesLoading } = useCountryInsights(normalizedFilters);
  const { data: topProjects, isLoading: topProjectsLoading } = useTopProjects(normalizedFilters);
  const { data: monthlyTrend, isLoading: trendLoading } = useMonthlyTrend(normalizedFilters);
  const { data: fundingDistribution, isLoading: distributionLoading } = useFundingDistribution(normalizedFilters);
  const { data: creatorPerformance, isLoading: creatorsLoading } = useCreatorPerformance(normalizedFilters);

  return (
    <div className={styles.wrapper}>
      <Typography.Title level={3}>跨境热点概览</Typography.Title>
      <Card>
        <AnalyticsFilters
          options={filterOptions}
          value={filters}
          onChange={setFilters}
          loading={filterLoading}
        />
      </Card>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className={styles.metricCard}>
            {isLoading ? (
              <Skeleton active paragraph={false} title={{ width: '60%' }} />
            ) : (
              <Statistic
                title="导入项目"
                value={data?.totalProjects ?? 0}
                suffix="个"
                valueStyle={{ color: '#1f6feb' }}
              />
            )}
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className={styles.metricCard}>
            {isLoading ? (
              <Skeleton active paragraph={false} title={{ width: '60%' }} />
            ) : (
              <Statistic
                title="成功率"
                value={data?.successRate ?? 0}
                suffix="%"
                valueStyle={{ color: '#0fbf61' }}
                precision={1}
              />
            )}
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className={styles.metricCard}>
            {isLoading ? (
              <Skeleton active paragraph={false} title={{ width: '60%' }} />
            ) : (
              <Statistic
                title="总筹资"
                value={data?.totalPledged ?? 0}
                prefix="$"
                precision={0}
              />
            )}
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className={styles.metricCard}>
            {isLoading ? (
              <Skeleton active paragraph={false} title={{ width: '60%' }} />
            ) : (
              <Statistic title="覆盖国家" value={data?.distinctCountries ?? 0} suffix="个" />
            )}
          </Card>
        </Col>
      </Row>
      <Row gutter={[16, 16]}>
        <Col xs={24} xl={16}>
          <MonthlyTrendChart data={monthlyTrend} loading={trendLoading} />
        </Col>
        <Col xs={24} xl={8}>
          <FundingDistributionChart data={fundingDistribution} loading={distributionLoading} />
        </Col>
      </Row>
      <Row gutter={[16, 16]}>
        <Col xs={24} xl={12}>
          <CategorySuccessChart data={categoryInsights?.slice(0, 10)} loading={categoriesLoading} />
        </Col>
        <Col xs={24} xl={12}>
          <CountrySuccessChart data={countryInsights?.slice(0, 10)} loading={countriesLoading} />
        </Col>
      </Row>
      <Row gutter={[16, 16]}>
        <Col xs={24} xl={14}>
          <TopProjectsList data={topProjects?.slice(0, 8)} loading={topProjectsLoading} />
        </Col>
        <Col xs={24} xl={10}>
          <TopCreatorsList data={creatorPerformance?.slice(0, 10)} loading={creatorsLoading} />
        </Col>
      </Row>
    </div>
  );
};
