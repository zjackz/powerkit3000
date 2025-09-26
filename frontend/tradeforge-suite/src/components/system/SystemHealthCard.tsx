import { Alert, Card, List, Skeleton, Statistic, Tag, Typography } from 'antd';
import dayjs from 'dayjs';
import type { SystemHealthSummary } from '@/types/systemHealth';
import styles from './SystemHealthCard.module.css';

interface SystemHealthCardProps {
  loading?: boolean;
  error?: Error | null;
  summary?: SystemHealthSummary;
}

const STATUS_COLOR_MAP = {
  healthy: 'success',
  warning: 'warning',
  critical: 'error',
} as const;

/**
 * 系统健康卡片：聚合导入作业与查询性能的关键指标，为运营同学提供一眼可见的健康度。
 */
export const SystemHealthCard = ({ loading, error, summary }: SystemHealthCardProps) => {
  const status = summary?.overallStatus ?? 'healthy';
  const lastUpdated = summary?.lastUpdatedUtc
    ? dayjs(summary.lastUpdatedUtc).format('YYYY-MM-DD HH:mm:ss')
    : '暂无数据';

  return (
    <Card
      className={styles.wrapper}
      title={
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography.Text strong>系统健康</Typography.Text>
          <Tag color={STATUS_COLOR_MAP[status]}>{renderStatusLabel(status)}</Tag>
        </div>
      }
      loading={loading}
      extra={<Typography.Text type="secondary">最近更新：{lastUpdated}</Typography.Text>}
    >
      {error ? (
        <Alert
          type="error"
          showIcon
          message="无法加载监控指标"
          description="请检查后端 /monitoring/metrics 接口或使用 CLI metrics 命令排查。"
        />
      ) : !summary ? (
        <Skeleton active paragraph={{ rows: 3 }} />
      ) : (
        <>
          <Typography.Paragraph type="secondary" style={{ marginBottom: 12 }}>
            卡片展示导入文件、成功写入项目以及解析/校验异常数量，并自动提示慢查询等潜在风险。
          </Typography.Paragraph>
          <div className={styles.summaryRow}>
            <Statistic className={styles.statistic} title="导入文件总数" value={summary.totalImportFiles} suffix="个" />
            <Statistic
              className={styles.statistic}
              title="导入项目总数"
              value={summary.totalProjectsImported}
              suffix="条"
            />
            <Statistic className={styles.statistic} title="导入失败" value={summary.failedImports} suffix="次" />
            <Statistic className={styles.statistic} title="解析错误" value={summary.parseErrors} suffix="条" />
            <Statistic className={styles.statistic} title="校验失败" value={summary.validationErrors} suffix="条" />
            <Statistic
              className={styles.statistic}
              title="查询平均耗时"
              value={summary.queryDurationAverageMs ? Math.round(summary.queryDurationAverageMs) : 0}
              suffix="ms"
            />
          </div>
          <div className={styles.alerts}>
            {summary.alerts.length === 0 ? (
              <Alert type="success" showIcon message="监控指标正常，未发现异常告警。" />
            ) : (
              <List
                dataSource={summary.alerts}
                renderItem={(item) => (
                  <List.Item>
                    <Alert type={STATUS_COLOR_MAP[item.level]} showIcon message={item.message} />
                  </List.Item>
                )}
              />
            )}
          </div>
        </>
      )}
    </Card>
  );
};

const renderStatusLabel = (status: SystemHealthSummary['overallStatus']) => {
  switch (status) {
    case 'critical':
      return '需要立即关注';
    case 'warning':
      return '请及时处理';
    default:
      return '运行正常';
  }
};
