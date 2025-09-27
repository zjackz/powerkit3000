import type { ProLayoutProps } from '@ant-design/pro-components';
import {
  AppstoreOutlined,
  BarChartOutlined,
  CloudOutlined,
  DashboardOutlined,
  FundFilled,
  LineChartOutlined,
  StarFilled,
  TagsFilled,
  ThunderboltFilled,
} from '@ant-design/icons';

export const navigationConfig: ProLayoutProps['route'] = {
  routes: [
    {
      path: '/dashboard',
      name: 'Kickstarter 分析',
      icon: <LineChartOutlined />,
      routes: [
        {
          path: '/dashboard',
          name: '全局驾驶舱',
          icon: <BarChartOutlined />,
        },
        {
          path: '/workspace',
          name: '分析工作台',
          icon: <AppstoreOutlined />,
        },
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
      path: '/amazon/trends',
      name: '亚马逊榜单',
      icon: <TagsFilled />,
      routes: [
        {
          path: '/amazon/trends',
          name: '趋势雷达',
          icon: <CloudOutlined />,
        },
      ],
    },
    {
      path: '/amazon/operations',
      name: '亚马逊运营仪表盘',
      icon: <DashboardOutlined />,
      routes: [
        {
          path: '/amazon/operations',
          name: '运营中控台',
          icon: <DashboardOutlined />,
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
