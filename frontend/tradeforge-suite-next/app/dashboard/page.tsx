'use client';

import { ProCard } from '@ant-design/pro-components';
import { ProShell } from '@/layouts/ProShell';
import { InsightMetricCard } from '@/components/metrics/InsightMetricCard';
import { TrendAreaChart } from '@/components/metrics/TrendAreaChart';
import { CategoryDonut } from '@/components/metrics/CategoryDonut';
import { TopListCard } from '@/components/metrics/TopListCard';

const mockTrend = [
  { month: '02-01', value: 12 },
  { month: '02-08', value: 18 },
  { month: '02-15', value: 26 },
  { month: '02-22', value: 32 },
  { month: '02-29', value: 30 },
  { month: '03-07', value: 40 },
  { month: '03-14', value: 52 },
];

const categoryData = [
  { name: '家居生活', value: 124 },
  { name: '消费电子', value: 96 },
  { name: '户外运动', value: 88 },
  { name: '设计创意', value: 72 },
  { name: '桌游玩具', value: 56 },
];

const hypeProjects = [
  {
    key: '1',
    title: 'Nebula Projector',
    subtitle: '消费电子 | 美国',
    metric: '$1.2M',
    delta: '+320%',
    trend: 'up' as const,
  },
  {
    key: '2',
    title: 'Lumos Bike 2',
    subtitle: '户外运动 | 英国',
    metric: '$620K',
    delta: '+110%',
    trend: 'up' as const,
  },
  {
    key: '3',
    title: 'Zen Home Station',
    subtitle: '家居生活 | 加拿大',
    metric: '$540K',
    delta: '+65%',
    trend: 'up' as const,
  },
];

export default function DashboardPage() {
  return (
    <ProShell
      title="全局驾驶舱"
      description="整合 Kickstarter & Amazon 双域数据，实时洞察跨境热点动向。"
    >
      <ProCard colSpan={{ xs: 24, md: 12, xxl: 8 }} ghost>
        <InsightMetricCard
          title="导入任务成功率"
          value={98.7}
          unit="%"
          trendLabel="较昨日"
          trendValue="+1.5%"
          caption="覆盖 12 条抓取线路，自动重试策略线上运行"
          accent="green"
        />
      </ProCard>
      <ProCard colSpan={{ xs: 24, md: 12, xxl: 8 }} ghost>
        <InsightMetricCard
          title="爆款信号累计"
          value={312}
          unit=" 条"
          trendLabel="近 7 天"
          trendValue="+46"
          caption="含 Kickstarter 爆款潜力榜与 Amazon 趋势榜单"
          accent="blue"
        />
      </ProCard>
      <ProCard colSpan={{ xs: 24, md: 12, xxl: 8 }} ghost>
        <InsightMetricCard
          title="翻译覆盖率"
          value={94.2}
          unit="%"
          trendLabel="LLM 成功率"
          trendValue="98.1%"
          caption="调用 Azure OpenAI + 内置缓存，延迟控制在 1.2s"
          accent="orange"
        />
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 14 }} ghost direction="column" gutter={[16, 16]}>
        <ProCard ghost title="筹资速度趋势" colSpan={24} style={{ minHeight: 280 }}>
          <TrendAreaChart data={mockTrend} />
        </ProCard>
        <ProCard ghost title="热点项目跃升" colSpan={24} style={{ minHeight: 320 }}>
          <TopListCard title="爆款潜力榜" items={hypeProjects} />
        </ProCard>
      </ProCard>
      <ProCard colSpan={{ xs: 24, xl: 10 }} ghost title="品类成功率分布" style={{ minHeight: 600 }}>
        <CategoryDonut data={categoryData} />
      </ProCard>
    </ProShell>
  );
}
