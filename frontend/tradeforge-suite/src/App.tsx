import { Layout } from 'antd';
import { Route, Routes } from 'react-router-dom';
import { AppHeader } from './components/layout/AppHeader';
import { AppSidebar } from './components/layout/AppSidebar';
import { DashboardPage } from './pages/dashboard/DashboardPage';
import { ProjectsPage } from './pages/projects/ProjectsPage';
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
              <Route path="/projects" element={<ProjectsPage />} />
            </Routes>
          </Content>
        </Layout>
      </Layout>
    </Layout>
  );
};

export default App;
