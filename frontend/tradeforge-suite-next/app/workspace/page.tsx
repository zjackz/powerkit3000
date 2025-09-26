'use client';

import Link from 'next/link';
import { ArrowRightOutlined, StarFilled, TableOutlined } from '@ant-design/icons';
import { ProCard } from '@ant-design/pro-components';
import { Space, Typography } from 'antd';
import { ProShell } from '@/layouts/ProShell';
import { TeamSwitcher } from '@/components/team/TeamSwitcher';
import { useTeamContext } from '@/contexts/TeamContext';
import { useProjectFavorites } from '@/hooks/useProjectFavorites';

const WorkspaceContent = () => {
  const { team } = useTeamContext();
  const { favorites } = useProjectFavorites();

  const overview = (
    <ProCard bordered>
      <Space direction="vertical" size={12} style={{ width: '100%' }}>
        <Typography.Text type="secondary">团队视角</Typography.Text>
        <TeamSwitcher />
        <Typography.Text type="secondary" style={{ fontSize: 12 }}>
          {team.description}
        </Typography.Text>
      </Space>
    </ProCard>
  );

  return (
    <ProShell
      title="分析工作台"
      description="整合项目巡检、收藏复盘与趋势调度，帮助运营团队快速进入高价值页面。"
      overview={overview}
    >
      <ProCard
        colSpan={{ xs: 24, xl: 12 }}
        bordered
        hoverable
        title="项目巡检"
        extra={<Link href="/workspace/projects">查看 <ArrowRightOutlined /></Link>}
      >
        <Space direction="vertical" size={8}>
          <Typography.Text type="secondary">
            按团队偏好预置筛选条件，可查看达成率、筹资指标与收藏状态，并支持导出 CSV。
          </Typography.Text>
          <Space align="center" size={6}>
            <TableOutlined style={{ color: '#38bdf8' }} />
            <Typography.Text>实时巡检 Kickstarter 项目，结合收藏抽屉快速复盘。</Typography.Text>
          </Space>
        </Space>
      </ProCard>
      <ProCard
        colSpan={{ xs: 24, xl: 12 }}
        bordered
        hoverable
        title="重点跟进"
        extra={<Link href="/workspace/favorites">进入 <ArrowRightOutlined /></Link>}
      >
        <Space direction="vertical" size={8}>
          <Typography.Text type="secondary">
            维护团队共享收藏与备注。目前收藏项目：{favorites.length} 个。
          </Typography.Text>
          <Space align="center" size={6}>
            <StarFilled style={{ color: '#faad14' }} />
            <Typography.Text>支持关键词搜索、快速编辑备注与一键导出收藏清单。</Typography.Text>
          </Space>
        </Space>
      </ProCard>
    </ProShell>
  );
};

export default function WorkspacePage() {
  return <WorkspaceContent />;
}
