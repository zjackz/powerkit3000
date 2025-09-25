import type { ProLayoutProps } from '@ant-design/pro-components';
import {
  ThunderboltFilled,
  FundFilled,
  LineChartOutlined,
  TagsFilled,
  StarFilled,
  CloudOutlined,
} from '@ant-design/icons';

export const navigationConfig: ProLayoutProps['route'] = {
  routes: [
    {
      path: '/dashboard',
      name: '全局驾驶舱',
      icon: <ThunderboltFilled />,
    },
    {
      path: '/workspace',
      name: '分析工作台',
      icon: <LineChartOutlined />,
      routes: [
        {
          path: '/workspace/projects',
          name: '项目巡检',
          icon: <FundFilled />,
        },
        {
          path: '/workspace/favorites',
          name: '重点跟进',
          icon: <StarFilled />,
        },
      ],
    },
    {
      path: '/amazon',
      name: 'Amazon 榜单',
      icon: <TagsFilled />,
      routes: [
        {
          path: '/amazon/trends',
          name: '趋势雷达',
          icon: <CloudOutlined />,
        },
        {
          path: '/amazon/tasks',
          name: '抓取调度',
          icon: <ThunderboltFilled />,
        },
      ],
    },
  ],
};
