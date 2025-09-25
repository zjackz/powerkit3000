import './globals.css';
import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { AntdAppShell } from '@/layouts/AntdApp';

export const metadata: Metadata = {
  title: 'TradeForge Control Tower',
  description: '跨境电商情报驾驶舱',
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="zh">
      <body>
        <AntdAppShell>{children}</AntdAppShell>
      </body>
    </html>
  );
}
