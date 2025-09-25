'use client';

import { ProCard } from '@ant-design/pro-components';
import { ProShell } from '@/layouts/ProShell';

export default function AmazonPage() {
  return (
    <ProShell
      title="Amazon 榜单"
      description="监控重点类目趋势、抓取任务与库存健康。"
    >
      <ProCard ghost style={{ minHeight: 480 }}>
        Amazon 仪表盘迁移中，预计展示类目热力、渠道对比与库存走势。
      </ProCard>
    </ProShell>
  );
}
