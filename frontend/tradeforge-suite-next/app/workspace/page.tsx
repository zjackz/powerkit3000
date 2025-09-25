'use client';

import { ProCard } from '@ant-design/pro-components';
import { ProShell } from '@/layouts/ProShell';

export default function WorkspacePage() {
  return (
    <ProShell
      title="分析工作台"
      description="搭建自定义指标组合，串联跨域项目洞察。"
    >
      <ProCard ghost style={{ minHeight: 480 }}>
        工作台内容建设中，欢迎定义模块需求。
      </ProCard>
    </ProShell>
  );
}
