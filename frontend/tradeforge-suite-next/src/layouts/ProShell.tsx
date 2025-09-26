'use client';

import { ReactNode, useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { ProLayout, PageContainer, ProCard, SettingDrawer, type ProSettings } from '@ant-design/pro-components';
import { Avatar, Badge, Space, Typography } from 'antd';
import { ThunderboltFilled } from '@ant-design/icons';
import { navigationConfig } from '@/config/navigation';
import { theme as antdTheme } from 'antd';
import { useThemeMode } from '@/contexts/ThemeContext';
import { ThemeToggle } from '@/components/theme/ThemeToggle';

interface ProShellProps {
  title?: string;
  description?: string;
  actions?: ReactNode;
  overview?: ReactNode;
  children: ReactNode;
}

const Branding = ({ mode }: { mode: 'dark' | 'light' }) => (
  <Space align="center" size="small">
    <Badge count={2024} style={{ backgroundColor: mode === 'dark' ? '#22d3ee' : '#2563eb' }}>
      <Avatar shape="square" size="large" style={{ background: mode === 'dark' ? '#0f172a' : '#e0f2fe' }} icon={<ThunderboltFilled style={{ color: mode === 'dark' ? '#38bdf8' : '#2563eb' }} />} />
    </Badge>
    <div>
      <Typography.Text strong style={{ color: mode === 'dark' ? '#e2e8f0' : '#0f172a' }}>
        TradeForge Control Tower
      </Typography.Text>
      <Typography.Paragraph style={{ margin: 0, color: mode === 'dark' ? '#94a3b8' : '#475569' }}>
        跨境情报实时调度中心
      </Typography.Paragraph>
    </div>
  </Space>
);

export const ProShell = ({ title, description, actions, overview, children }: ProShellProps) => {
  const pathname = usePathname();
  const { mode, setMode, primaryColor, setPrimaryColor } = useThemeMode();
  const isDark = mode === 'dark';
  const { token } = antdTheme.useToken();
  const [settings, setSettings] = useState<Partial<ProSettings>>({
    navTheme: isDark ? 'realDark' : 'light',
    colorPrimary: primaryColor,
    layout: 'mix',
  });

  useEffect(() => {
    setSettings((prev) => ({
      ...prev,
      navTheme: isDark ? 'realDark' : 'light',
      colorPrimary: primaryColor,
    }));
  }, [isDark, primaryColor]);

  const layoutTokens = useMemo(
    () => ({
      layout: {
        siderMenuType: 'group',
        colorMenuBackground: isDark ? 'rgba(15,23,42,0.9)' : '#ffffff',
        colorBgHeader: isDark ? 'rgba(8,15,35,0.92)' : '#ffffffdd',
        colorTextMenu: isDark ? 'rgba(226,232,240,0.76)' : 'rgba(30, 41, 59, 0.78)',
        colorTextMenuSelected: token.colorPrimary,
      },
    }),
    [isDark, token.colorPrimary],
  );

  return (
    <>
      <ProLayout
      layout="mix"
      navTheme={isDark ? 'realDark' : 'light'}
      fixSiderbar
      fixedHeader
      route={navigationConfig}
      location={{ pathname }}
      logo={<ThunderboltFilled style={{ color: token.colorPrimary, fontSize: 24 }} />}
      menuHeaderRender={() => <Branding mode={mode} />}
      token={layoutTokens}
      settings={settings}
      menuItemRender={(item, dom) => {
        if (!item.path) return dom;
        return <Link href={item.path}>{dom}</Link>;
      }}
      avatarProps={{
        src: 'https://avatars.githubusercontent.com/u/000?v=4',
        size: 'small',
        title: '运营值班',
      }}
      actionsRender={() => [<ThemeToggle key="theme-toggle" />, actions ?? null].filter(Boolean)}
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
      <SettingDrawer
        settings={settings}
        disableUrlParams
        enableDarkTheme
        hideCopyButton
        getContainer={() => document.body}
        onSettingChange={(nextSettings) => {
          setSettings(nextSettings);
          if (nextSettings.navTheme) {
            setMode(nextSettings.navTheme === 'realDark' ? 'dark' : 'light');
          }
          if (nextSettings.isDarkTheme !== undefined) {
            setMode(nextSettings.isDarkTheme ? 'dark' : 'light');
          }
          if (nextSettings.colorPrimary) {
            setPrimaryColor(nextSettings.colorPrimary as string);
          }
        }}
      />
    </>
  );
};
