'use client';

import { Alert, Col, Row, Skeleton, Space, Statistic, Tag, Typography } from 'antd';
import { ProCard } from '@ant-design/pro-components';
import dayjs from 'dayjs';
import { useSystemHealth } from '@/hooks/useSystemHealth';
import type { SystemHealthSummary } from '@/types/systemHealth';

const STATUS_COLOR_MAP: Record<SystemHealthSummary['overallStatus'], string> = {
  healthy: 'success',
  warning: 'warning',
  critical: 'error',
};

/**
 * 系统健康面板：汇总 Kickstarter 导入与 Amazon 采集指标，帮助运营快速掌握风险。
 */
export const SystemHealthPanel = () => {
  const { data, isLoading, error } = useSystemHealth();

  const status = data?.overallStatus ?? 'healthy';
  const statusTag = <Tag color={STATUS_COLOR_MAP[status]}>{renderStatusLabel(status)}</Tag>;
  const lastUpdated = data?.lastUpdatedUtc
    ? dayjs(data.lastUpdatedUtc).local().format('YYYY-MM-DD HH:mm:ss')
    : '暂无数据';

  return (
    <ProCard
      ghost
      title={
        <Space size="small">
          <Typography.Text strong>系统健康监控</Typography.Text>
          {statusTag}
        </Space>
      }
      extra={<Typography.Text type="secondary">最近同步：{lastUpdated}</Typography.Text>}
    >
      {error ? (
        <Alert
          type="error"
          showIcon
          message="无法加载监控指标"
          description="请检查后端 /monitoring/metrics 接口或使用 CLI metrics 命令排查。"
        />
      ) : isLoading || !data ? (
        <Skeleton active paragraph={{ rows: 3 }} />
      ) : (
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
            卡片实时展示导入批次、解析/校验异常数量以及 Amazon 采集表现，并针对慢查询或抓取失败自动发出预警。
          </Typography.Paragraph>
          <Row gutter={[16, 16]}>
            <Col xs={24} xl={12}>
              <ProCard title="Kickstarter 导入" bordered>
                <Row gutter={[12, 12]}>
                  <Col span={12}>
                    <Statistic title="导入文件" value={data.totalImportFiles} suffix="个" />
                  </Col>
                  <Col span={12}>
                    <Statistic title="写入项目" value={data.totalProjectsImported} suffix="条" />
                  </Col>
                  <Col span={12}>
                    <Statistic title="导入失败" value={data.failedImports} suffix="次" />
                  </Col>
                  <Col span={12}>
                    <Statistic title="解析错误" value={data.parseErrors} suffix="条" />
                  </Col>
                  <Col span={12}>
                    <Statistic title="校验失败" value={data.validationErrors} suffix="条" />
                  </Col>
                  <Col span={12}>
                    <Statistic
                      title="查询平均耗时"
                      value={data.queryDurationAverageMs ? Math.round(data.queryDurationAverageMs) : 0}
                      suffix="ms"
                    />
                  </Col>
                </Row>
              </ProCard>
            </Col>
            <Col xs={24} xl={12}>
              <ProCard title="Amazon 采集" bordered>
                <Row gutter={[12, 12]}>
                  <Col span={12}>
                    <Statistic title="榜单快照" value={data.amazonSnapshots} suffix="次" />
                  </Col>
                  <Col span={12}>
                    <Statistic title="趋势分析任务" value={data.amazonTrendJobs} suffix="次" />
                  </Col>
                  <Col span={12}>
                    <Statistic title="采集失败" value={data.amazonFailures} suffix="次" />
                  </Col>
                </Row>
              </ProCard>
            </Col>
          </Row>
          {data.alerts.length === 0 ? (
            <Alert type="success" showIcon message="监控指标正常，未发现异常告警。" />
          ) : (
            <Space direction="vertical" style={{ width: '100%' }}>
              {data.alerts.map((alert, index) => (
                <Alert key={`${alert.level}-${index}`} type={STATUS_COLOR_MAP[alert.level]} showIcon message={alert.message} />
              ))}
            </Space>
          )}
        </Space>
      )}
    </ProCard>
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
