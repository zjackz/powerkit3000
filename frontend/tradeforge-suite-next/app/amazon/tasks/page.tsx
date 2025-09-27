'use client';

import { Button, Card, List, Space, Tag, Typography } from 'antd';
import { ThunderboltOutlined } from '@ant-design/icons';
import { ProCard } from '@ant-design/pro-components';
import { ProShell } from '@/layouts/ProShell';

interface TaskItem {
  id: string;
  name: string;
  status: 'online' | 'paused' | 'draft';
  site: string;
  categories: string[];
  schedule: string;
  owner: string;
  successRate: number;
}

const buildMockTasks = (): TaskItem[] => [
  {
    id: 'task-1',
    name: 'MISSION X | 家居 Top100 日更',
    status: 'online',
    site: 'amazon.com',
    categories: ['Home & Kitchen'],
    schedule: '每日 02:30 UTC',
    owner: '采集中台',
    successRate: 99.1,
  },
  {
    id: 'task-2',
    name: 'MISSION X | 工具飙升榜监控',
    status: 'online',
    site: 'amazon.com',
    categories: ['Tools & Home Improvement'],
    schedule: '每 3 小时',
    owner: '运营自动化',
    successRate: 97.6,
  },
  {
    id: 'task-3',
    name: 'MISSION X | 评论洞察周报',
    status: 'paused',
    site: 'amazon.co.uk',
    categories: ['Home & Kitchen'],
    schedule: '每周一 08:00 UTC',
    owner: '客服中台',
    successRate: 92.3,
  },
];

const TaskStatusTag = ({ status }: { status: TaskItem['status'] }) => {
  const map: Record<TaskItem['status'], { color: string; label: string }> = {
    online: { color: 'green', label: '运行中' },
    paused: { color: 'orange', label: '已暂停' },
    draft: { color: 'default', label: '草稿' },
  };
  const { color, label } = map[status];
  return <Tag color={color}>{label}</Tag>;
};

const TasksContent = () => {
  const tasks = buildMockTasks();

  const overview = (
    <Card bordered={false} style={{ background: 'rgba(15,23,42,0.85)' }}>
      <Space direction="vertical" size={12} style={{ width: '100%' }}>
        <Typography.Text type="secondary">调度概览</Typography.Text>
        <Typography.Text type="secondary" style={{ fontSize: 12 }}>
          规划 Amazon 榜单采集任务，掌握运行状态与后续计划。
        </Typography.Text>
      </Space>
    </Card>
  );

  return (
    <ProShell
      title="抓取调度"
      description="查看与管理 Amazon 榜单采集任务，监控成功率并快速派发新的采集计划。"
      overview={overview}
    >
      <ProCard colSpan={{ xs: 24, xl: 16 }} bordered title="任务列表">
        <List
          itemLayout="vertical"
          dataSource={tasks}
          renderItem={(item) => (
            <List.Item
              key={item.id}
              actions={[
                <Button key="edit" type="link">配置</Button>,
                <Button key="run" type="link">手动触发</Button>,
              ]}
            >
              <List.Item.Meta
                avatar={<ThunderboltOutlined style={{ fontSize: 20, color: '#38bdf8' }} />}
                title={
                  <Space size={8} wrap>
                    <Typography.Text strong>{item.name}</Typography.Text>
                    <TaskStatusTag status={item.status} />
                    <Tag>{item.site}</Tag>
                  </Space>
                }
                description={
                  <Space direction="vertical" size={2}>
                    <Typography.Text type="secondary">类目：{item.categories.join(' / ')}</Typography.Text>
                    <Typography.Text type="secondary">调度：{item.schedule}</Typography.Text>
                    <Typography.Text type="secondary">负责人：{item.owner}</Typography.Text>
                  </Space>
                }
              />
              <Typography.Text type="secondary">近 30 日成功率：{item.successRate}%</Typography.Text>
            </List.Item>
          )}
        />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 8 }} bordered title="下一步计划">
        <Space direction="vertical" size={12} style={{ width: '100%' }}>
          <Typography.Text>· 接入代理池与重试策略，提升抓取稳定性。</Typography.Text>
          <Typography.Text>· 与监控中心对接，异常任务自动告警至 Slack。</Typography.Text>
          <Typography.Text>· 支持按团队模板快速创建任务，降低配置成本。</Typography.Text>
          <Button type="primary">新建抓取任务</Button>
        </Space>
      </ProCard>
    </ProShell>
  );
};

export default function AmazonTasksPage() {
  return <TasksContent />;
}
