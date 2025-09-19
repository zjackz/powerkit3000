import { Layout, Space, Typography, theme } from 'antd';
import { ThunderboltFilled } from '@ant-design/icons';
import styles from './AppHeader.module.css';

const { Header } = Layout;

export const AppHeader = () => {
  const {
    token: { colorPrimary },
  } = theme.useToken();

  return (
    <Header className={styles.header}>
      <Space size="middle" align="center">
        <ThunderboltFilled style={{ color: colorPrimary, fontSize: 24 }} />
        <Typography.Title level={4} style={{ margin: 0, color: '#f5f7fa' }}>
          TradeForge Suite
        </Typography.Title>
      </Space>
    </Header>
  );
};
