import { Menu } from 'antd';
import type { MenuProps } from 'antd';
import {
  AppstoreOutlined,
  DashboardOutlined,
  ShopOutlined,
  StarFilled,
  ThunderboltFilled,
} from '@ant-design/icons';
import { useLocation, useNavigate } from 'react-router-dom';

const CONTROL_TOWER_URL = import.meta.env.VITE_CONTROL_TOWER_URL ?? 'http://localhost:3000';

const menuItems: MenuProps['items'] = [
  {
    key: '/',
    icon: <DashboardOutlined />,
    label: 'Legacy 仪表盘',
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
    label: 'Amazon 榜单 (Legacy)',
  },
  {
    type: 'divider',
  },
  {
    key: 'control-tower',
    icon: <ThunderboltFilled />,
    label: '打开新版 Control Tower',
  },
];

export const AppSidebar = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const routeKeys = ['/', '/projects', '/favorites', '/amazon'];
  const activeKey = routeKeys.reduce((matched, key) => {
    if (location.pathname.startsWith(key) && key.length > matched.length) {
      return key;
    }
    return matched;
  }, '');

  const handleClick: MenuProps['onClick'] = ({ key }) => {
    if (key === 'control-tower') {
      window.open(CONTROL_TOWER_URL, '_blank', 'noopener');
      return;
    }
    navigate(key);
  };

  return (
    <Menu
      theme="dark"
      mode="inline"
      selectedKeys={activeKey ? [activeKey] : []}
      onClick={handleClick}
      items={menuItems}
    />
  );
};
