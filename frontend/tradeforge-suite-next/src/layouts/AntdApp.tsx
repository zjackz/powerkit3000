'use client';

import { ReactNode, useMemo, useState } from 'react';
import { App as AntdApp, ConfigProvider, theme as antdTheme } from 'antd';
import zhCN from 'antd/locale/zh_CN';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { StyleProvider, legacyLogicalPropertiesTransformer } from '@ant-design/cssinjs';
import { themeConfig } from '@/theme/themeConfig';

interface Props {
  children: ReactNode;
}

export const AntdAppShell = ({ children }: Props) => {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            refetchOnWindowFocus: false,
            retry: 1,
          },
        },
      }),
  );

  const mergedTheme = useMemo(() => ({
    ...themeConfig,
    algorithm: [antdTheme.darkAlgorithm, antdTheme.compactAlgorithm],
  }), []);

  return (
    <StyleProvider transformers={[legacyLogicalPropertiesTransformer]} hashPriority="high">
      <ConfigProvider locale={zhCN} theme={mergedTheme} componentSize="middle">
        <QueryClientProvider client={queryClient}>
          <AntdApp>{children}</AntdApp>
        </QueryClientProvider>
      </ConfigProvider>
    </StyleProvider>
  );
};
