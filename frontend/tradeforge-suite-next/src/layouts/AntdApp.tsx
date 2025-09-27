'use client';

import { ReactNode, useState } from 'react';
import { App as AntdApp } from 'antd';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '@/contexts/ThemeContext';

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
        <AntdApp>{children}</AntdApp>
      </QueryClientProvider>
    </ThemeProvider>
  );
};
