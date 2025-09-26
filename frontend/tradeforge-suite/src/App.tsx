import { Alert, Layout } from 'antd';
import { Route, Routes } from 'react-router-dom';
import { AppHeader } from './components/layout/AppHeader';
import { AppSidebar } from './components/layout/AppSidebar';
import { DashboardPage } from './pages/dashboard/DashboardPage';
import { FavoritesPage } from './pages/favorites/FavoritesPage';
import { ProjectsPage } from './pages/projects/ProjectsPage';
import { AmazonPage } from './pages/amazon/AmazonPage';
import AmazonTasksPage from './pages/amazon/AmazonTasksPage';
import DemoPage from './pages/demo/DemoPage';
import styles from './App.module.css';

const { Sider, Content } = Layout;

const App = () => {
  const controlTowerUrl = import.meta.env.VITE_CONTROL_TOWER_URL ?? 'http://localhost:3000';

  return (
    <Layout className={styles.appLayout}>
      <AppHeader />
      <Layout>
        <Sider width={240} className={styles.appSider}>
          <AppSidebar />
        </Sider>
        <Layout>
          <Content className={styles.appContent}>
            <Alert
              type="info"
              showIcon
              message="提醒：Legacy Vite 版本仅供回溯，最新体验请使用 Next.js Control Tower"
              description={
                <span>
                  您可以通过侧边栏的“打开新版 Control Tower”链接或访问{' '}
                  <a href={controlTowerUrl} target="_blank" rel="noreferrer">
                    {controlTowerUrl}
                  </a>{' '}
                  获取完整功能。
                </span>
              }
              style={{ marginBottom: 16 }}
            />
            <Routes>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/projects" element={<ProjectsPage />} />
              <Route path="/favorites" element={<FavoritesPage />} />
              <Route path="/amazon" element={<AmazonPage />} />
              <Route path="/amazon/tasks" element={<AmazonTasksPage />} />
              <Route path="/demo" element={<DemoPage />} />
            </Routes>
          </Content>
        </Layout>
      </Layout>
    </Layout>
  );
};

export default App;
