import { Menu } from 'antd';
import { AppstoreOutlined, DashboardOutlined, ExperimentOutlined } from '@ant-design/icons';
import { useLocation, useNavigate } from 'react-router-dom';

const menuItems = [
  {
    key: '/',
    icon: <DashboardOutlined />,
    label: '概览仪表盘',
  },
  {
    key: '/projects',
    icon: <AppstoreOutlined />,
    label: '项目浏览',
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

  return (
    <Menu
      theme="dark"
      mode="inline"
      selectedKeys={[menuItems.find((item) => location.pathname.startsWith(item.key))?.key ?? '/']}
      onClick={({ key }) => navigate(key)}
      items={menuItems}
    />
  );
};
