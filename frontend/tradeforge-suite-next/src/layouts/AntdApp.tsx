'use client';

import { ReactNode, useState } from 'react';
import { App as AntdApp } from 'antd';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '@/contexts/ThemeContext';
import { TeamProvider } from '@/contexts/TeamContext';

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

  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <TeamProvider>
          <AntdApp>{children}</AntdApp>
        </TeamProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
};
