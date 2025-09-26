'use client';

import { ReactNode } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { ProLayout, PageContainer, ProCard } from '@ant-design/pro-components';
import { Avatar, Badge, Space, Typography } from 'antd';
import { ThunderboltFilled } from '@ant-design/icons';
import { navigationConfig } from '@/config/navigation';

interface ProShellProps {
  title?: string;
  description?: string;
  actions?: ReactNode;
  overview?: ReactNode;
  children: ReactNode;
}

const Branding = () => (
  <Space align="center" size="small">
    <Badge count={2024} style={{ backgroundColor: '#22d3ee' }}>
      <Avatar shape="square" size="large" icon={<ThunderboltFilled />} />
    </Badge>
    <div>
      <Typography.Text strong style={{ color: '#e2e8f0' }}>
        TradeForge Control Tower
      </Typography.Text>
      <Typography.Paragraph style={{ margin: 0, color: '#94a3b8' }}>
        跨境情报实时调度中心
      </Typography.Paragraph>
    </div>
  </Space>
);

export const ProShell = ({ title, description, actions, overview, children }: ProShellProps) => {
  const pathname = usePathname();

  return (
    <ProLayout
      layout="mix"
      navTheme="realDark"
      fixSiderbar
      fixedHeader
      route={navigationConfig}
      location={{ pathname }}
      logo={<ThunderboltFilled style={{ color: '#38bdf8', fontSize: 24 }} />}
      menuHeaderRender={() => <Branding />}
      token={{
        layout: {
          siderMenuType: 'group',
          colorMenuBackground: 'rgba(15,23,42,0.88)',
          colorBgHeader: 'rgba(2,6,23,0.85)',
          colorTextMenu: 'rgba(226,232,240,0.75)',
        },
      }}
      menuItemRender={(item, dom) => {
        if (!item.path) return dom;
        return <Link href={item.path}>{dom}</Link>;
      }}
      avatarProps={{
        src: 'https://avatars.githubusercontent.com/u/000?v=4',
        size: 'small',
        title: '运营值班',
      }}
      actionsRender={() => [actions ?? null].filter(Boolean)}
      breadcrumbRender={(routers = []) => routers?.filter((route) => route?.name)}
      collapsedButtonRender={false}
    >
      <PageContainer
        title={title || '全局驾驶舱'}
        content={description}
        ghost
        extra={actions}
      >
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          {overview}
          <ProCard ghost gutter={16} wrap>
            {children}
          </ProCard>
        </Space>
      </PageContainer>
    </ProLayout>
  );
};
