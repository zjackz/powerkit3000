import { Layout } from 'antd';
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
  return (
    <Layout className={styles.appLayout}>
      <AppHeader />
      <Layout>
        <Sider width={240} className={styles.appSider}>
          <AppSidebar />
        </Sider>
        <Layout>
          <Content className={styles.appContent}>
            <Routes>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/demo" element={<DemoPage />} />
              <Route path="/projects" element={<ProjectsPage />} />
              <Route path="/favorites" element={<FavoritesPage />} />
              <Route path="/amazon" element={<AmazonPage />} />
              <Route path="/amazon/tasks" element={<AmazonTasksPage />} />
            </Routes>
          </Content>
        </Layout>
      </Layout>
    </Layout>
  );
};

export default App;
