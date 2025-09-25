import { Menu } from 'antd';
import {
  AppstoreOutlined,
  DashboardOutlined,
  ExperimentOutlined,
  PlayCircleOutlined,
  ProfileOutlined,
  ShopOutlined,
  StarFilled,
} from '@ant-design/icons';
import { useLocation, useNavigate } from 'react-router-dom';

const menuItems = [
  {
    key: '/',
    icon: <DashboardOutlined />,
    label: '概览仪表盘',
  },
  {
    key: '/demo',
    icon: <PlayCircleOutlined />,
    label: '客户演示',
  },
  {
    key: '/projects',
    icon: <AppstoreOutlined />,
    label: '项目浏览',
  },
  {
    key: '/favorites',
    icon: <StarFilled />,
    label: '我的收藏',
  },
  {
    key: '/amazon',
    icon: <ShopOutlined />,
    label: 'Amazon 榜单',
  },
  {
    key: '/amazon/tasks',
    icon: <ProfileOutlined />,
    label: 'Amazon 任务配置',
  },
  {
    key: 'experiments',
    icon: <ExperimentOutlined />,
    label: '实验室 (规划中)',
    disabled: true,
  },
];

export const AppSidebar = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const activeKey = menuItems.reduce((matched, item) => {
    if (location.pathname.startsWith(item.key) && item.key.length > matched.length) {
      return item.key;
    }
    return matched;
  }, '/');

  return (
    <Menu
      theme="dark"
      mode="inline"
      selectedKeys={[activeKey]}
      onClick={({ key }) => navigate(key)}
      items={menuItems}
    />
  );
};
