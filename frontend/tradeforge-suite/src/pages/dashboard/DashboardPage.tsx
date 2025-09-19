import { Card, Col, Row, Skeleton, Statistic, Typography } from 'antd';
import { useProjectSummary } from '@/hooks/useProjectSummary';
import styles from './DashboardPage.module.css';

export const DashboardPage = () => {
  const { data, isLoading } = useProjectSummary();

  return (
    <div className={styles.wrapper}>
      <Typography.Title level={3}>跨境热点概览</Typography.Title>
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
      <Card className={styles.placeholderCard}>
        <Typography.Text type="secondary">
          即将上线：动态趋势图、成功案例榜单、收藏夹等 TradeForge 模块。
        </Typography.Text>
      </Card>
    </div>
  );
};
