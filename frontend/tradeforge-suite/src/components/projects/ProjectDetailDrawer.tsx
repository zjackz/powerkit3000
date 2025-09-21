import { Drawer, Progress, Space, Tag, Typography, Divider, Descriptions } from 'antd';
import dayjs from 'dayjs';
import type { Project } from '@/types/project';

export interface ProjectDetailDrawerProps {
  project?: Project | null;
  open: boolean;
  onClose: () => void;
}

const getStateTagColor = (state: string) => {
  const stateColor: Record<string, string> = {
    successful: 'green',
    live: 'blue',
    canceled: 'red',
    failed: 'volcano',
  };
  return stateColor[state] ?? 'default';
};

export const ProjectDetailDrawer = ({ project, open, onClose }: ProjectDetailDrawerProps) => {
  if (!project) {
    return (
      <Drawer open={open} onClose={onClose} title="项目详情" width={420} destroyOnClose />
    );
  }

  const launchedAt = dayjs(project.launchedAt);
  const deadline = dayjs(project.deadline);
  const duration = Math.max(1, Math.round(deadline.diff(launchedAt, 'day', true)));
  const percent = Number(project.percentFunded);

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title={project.name}
      width={420}
      destroyOnClose
    >
      <Space direction="vertical" size={16} style={{ width: '100%' }}>
        <Space size={8} wrap>
          <Tag color={getStateTagColor(project.state)}>{project.state}</Tag>
          <Tag>{project.categoryName}</Tag>
          <Tag>{project.country}</Tag>
        </Space>
        {project.blurb && (
          <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
            {project.blurb}
          </Typography.Paragraph>
        )}
        <div>
          <Typography.Text strong>达成率</Typography.Text>
          <Progress
            percent={Math.min(percent, 100)}
            format={() => `${percent.toFixed(1)}%`}
            status={percent >= 100 ? 'success' : 'active'}
          />
        </div>
        <Descriptions column={1} size="small" labelStyle={{ width: 120 }} bordered>
          <Descriptions.Item label="创作者">{project.creatorName || '未提供'}</Descriptions.Item>
          <Descriptions.Item label="地点">{project.locationName || '未提供'}</Descriptions.Item>
          <Descriptions.Item label="目标金额">
            {`${project.currency} ${project.goal.toLocaleString()}`}
          </Descriptions.Item>
          <Descriptions.Item label="已筹金额">
            <Typography.Text strong>
              {`${project.currency} ${project.pledged.toLocaleString()}`}
            </Typography.Text>
          </Descriptions.Item>
          <Descriptions.Item label="支持者">
            {project.backersCount.toLocaleString()}
          </Descriptions.Item>
          <Descriptions.Item label="上线时间">{launchedAt.format('YYYY-MM-DD')}</Descriptions.Item>
          <Descriptions.Item label="截止时间">{deadline.format('YYYY-MM-DD')}</Descriptions.Item>
          <Descriptions.Item label="筹资周期">{duration} 天</Descriptions.Item>
        </Descriptions>
        <Divider style={{ margin: '12px 0' }} />
        <Typography.Text type="secondary">
          项目 ID：{project.id}
        </Typography.Text>
      </Space>
    </Drawer>
  );
};

export default ProjectDetailDrawer;
