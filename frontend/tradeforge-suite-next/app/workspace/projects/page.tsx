'use client';

import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Button,
  Card,
  Drawer,
  Empty,
  Form,
  Input,
  List,
  Modal,
  Popconfirm,
  Progress,
  Space,
  Statistic,
  Switch,
  Table,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { StarFilled, StarOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { ProCard } from '@ant-design/pro-components';
import { ProShell } from '@/layouts/ProShell';
import { ProjectFilters } from '@/components/projects/ProjectFilters';
import { ProjectDetailDrawer } from '@/components/projects/ProjectDetailDrawer';
import { useProjects } from '@/hooks/useProjects';
import { useProjectFilters } from '@/hooks/useProjectFilters';
import { useProjectFavorites } from '@/hooks/useProjectFavorites';
import type { Project, ProjectQueryParams } from '@/types/project';
import { DEFAULT_PAGE_SIZE, PAGE_SIZE_OPTIONS } from '@/constants/projectOptions';

const formatCurrencyValue = (value: number, currency: string) => `${currency} ${value.toLocaleString()}`;

const getMomentumMeta = (percent: number) => {
  if (percent >= 200) {
    return { color: 'magenta', label: '爆款' };
  }
  if (percent >= 120) {
    return { color: 'gold', label: '高潜' };
  }
  if (percent >= 80) {
    return { color: 'blue', label: '稳健' };
  }
  return { color: 'default', label: '观察' };
};

const CARD_BODY_STYLE = { padding: 14 };

const DEFAULT_PROJECT_FILTERS: ProjectQueryParams = { minPercentFunded: 200 };

const ProjectsContent = () => {
  const [filters, setFilters] = useState<ProjectQueryParams>({
    page: 1,
    pageSize: DEFAULT_PAGE_SIZE,
    ...DEFAULT_PROJECT_FILTERS,
  });
  const [activeProject, setActiveProject] = useState<Project | null>(null);
  const { favorites, addFavorite, isFavorite, removeFavorite, clearFavorites } = useProjectFavorites();
  const [showFavoritesOnly, setShowFavoritesOnly] = useState(false);
  const [favoritesDrawerOpen, setFavoritesDrawerOpen] = useState(false);
  const [favoriteModalProject, setFavoriteModalProject] = useState<Project | null>(null);
  const [favoriteModalSaving, setFavoriteModalSaving] = useState(false);
  const [favoriteForm] = Form.useForm();

  const favoritesCount = favorites.length;
  const favoriteProjects = useMemo(() => favorites.map((item) => item.project), [favorites]);
  const favoriteNotes = useMemo(() => new Map(favorites.map((item) => [item.project.id, item.note])), [favorites]);

  const { data, isFetching } = useProjects(filters);
  const { data: filterOptions, isLoading: filtersLoading } = useProjectFilters();

  useEffect(() => {
    if (favoritesCount === 0 && showFavoritesOnly) {
      setShowFavoritesOnly(false);
    }
  }, [favoritesCount, showFavoritesOnly]);

  useEffect(() => {
    if (favoriteModalProject) {
      favoriteForm.setFieldsValue({ note: favoriteNotes.get(favoriteModalProject.id) ?? '' });
    }
  }, [favoriteForm, favoriteModalProject, favoriteNotes]);

  const handleFavoriteModalClose = useCallback(() => {
    setFavoriteModalProject(null);
    favoriteForm.resetFields();
    setFavoriteModalSaving(false);
  }, [favoriteForm]);

  const handleFavoriteModalOk = useCallback(async () => {
    try {
      const values = await favoriteForm.validateFields();
      if (!favoriteModalProject) {
        return;
      }
      setFavoriteModalSaving(true);
      await addFavorite(favoriteModalProject, values.note);
      message.success('已添加收藏');
      handleFavoriteModalClose();
    } catch (error) {
      if ((error as { errorFields?: unknown }).errorFields) {
        return;
      }
      console.error(error);
      message.error('收藏失败，请稍后重试');
    } finally {
      setFavoriteModalSaving(false);
    }
  }, [addFavorite, favoriteForm, favoriteModalProject, handleFavoriteModalClose]);

  const handleFiltersChange = (nextFilters: ProjectQueryParams) => {
    setFilters((prev) => ({
      ...prev,
      ...nextFilters,
      page: 1,
    }));
  };

  const handlePageChange = (page: number, pageSize: number) => {
    setFilters((prev) => ({
      ...prev,
      page,
      pageSize,
    }));
  };

  const currentItems = data?.items ?? [];
  const tableData = showFavoritesOnly ? favoriteProjects : currentItems;
  const totalCount = showFavoritesOnly ? tableData.length : data?.total ?? 0;
  const aggregated = data?.stats;

  const handleExport = () => {
    if (!tableData.length) {
      message.info('当前无可导出的项目。');
      return;
    }

    const headers = ['Id', 'Name', 'Category', 'Country', 'State', 'Goal', 'Pledged', 'PercentFunded', 'Backers', 'Currency', 'LaunchedAt', 'Deadline'];
    const rows = tableData.map((project) => [
      project.id,
      project.name,
      project.categoryName,
      project.country,
      project.state,
      project.goal,
      project.pledged,
      project.percentFunded,
      project.backersCount,
      project.currency,
      dayjs(project.launchedAt).format('YYYY-MM-DD'),
      dayjs(project.deadline).format('YYYY-MM-DD'),
    ]);

    const csv = [headers, ...rows]
      .map((row) =>
        row
          .map((cell) => {
            if (cell === null || cell === undefined) {
              return '';
            }
            if (typeof cell === 'string') {
              const escaped = cell.replace(/"/g, '""');
              return /[",\n]/.test(escaped) ? `"${escaped}"` : escaped;
            }
            return cell;
          })
          .join(','),
      )
      .join('\n');

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `tradeforge-projects-${dayjs().format('YYYYMMDD-HHmmss')}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);

    message.success('已导出当前筛选的项目。');
  };

  const columns = useMemo<ColumnsType<Project>>(
    () => [
      {
        title: '收藏',
        dataIndex: 'favorite',
        key: 'favorite',
        width: 70,
        align: 'center',
        render: (_, record) => {
          const favorite = isFavorite(record.id);
          return (
            <Tooltip title={favorite ? '取消收藏' : '加入收藏'}>
              <Button
                type="text"
                icon={favorite ? <StarFilled style={{ color: '#fbbf24' }} /> : <StarOutlined />}
                onClick={async (event) => {
                  event.stopPropagation();
                  if (favorite) {
                    await removeFavorite(record.id);
                    message.success('已取消收藏');
                  } else {
                    setFavoriteModalProject(record);
                  }
                }}
              />
            </Tooltip>
          );
        },
      },
      {
        title: '项目名称',
        dataIndex: 'name',
        key: 'name',
        width: 260,
        render: (_, record) => (
          <Space direction="vertical" size={2}>
            <Typography.Text strong>{record.nameCn ?? record.name}</Typography.Text>
            {record.nameCn && <Typography.Text type="secondary">{record.name}</Typography.Text>}
            <Typography.Text type="secondary" style={{ fontSize: 12 }}>
              {record.categoryName} · {record.country}
            </Typography.Text>
          </Space>
        ),
      },
      {
        title: '筹资进度',
        dataIndex: 'percentFunded',
        key: 'percentFunded',
        width: 220,
        render: (_, record) => {
          const percent = Number(record.percentFunded.toFixed(1));
          const momentum = getMomentumMeta(percent);
          return (
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Progress percent={Math.min(percent, 100)} format={() => `${percent.toFixed(1)}%`} status={percent >= 100 ? 'success' : 'active'} />
              <Space size={6}>
                <Tag color={momentum.color}>{momentum.label}</Tag>
                <Typography.Text type="secondary">{record.backersCount.toLocaleString()} 支持者</Typography.Text>
              </Space>
            </Space>
          );
        },
      },
      {
        title: '筹资金额',
        dataIndex: 'pledged',
        key: 'pledged',
        width: 160,
        render: (_, record) => (
          <Space direction="vertical" size={2}>
            <Typography.Text strong>{formatCurrencyValue(record.pledged, record.currency)}</Typography.Text>
            <Typography.Text type="secondary" style={{ fontSize: 12 }}>
              目标 {formatCurrencyValue(record.goal, record.currency)}
            </Typography.Text>
          </Space>
        ),
      },
      {
        title: '上线时间',
        dataIndex: 'launchedAt',
        key: 'launchedAt',
        width: 140,
        render: (value: string) => dayjs(value).format('YYYY-MM-DD'),
      },
    ],
    [isFavorite, removeFavorite],
  );

  const overview = (
    <Card bordered={false} bodyStyle={CARD_BODY_STYLE} style={{ background: 'rgba(15,23,42,0.85)' }}>
      <Space direction="vertical" size={12} style={{ width: '100%' }}>
        <Typography.Text type="secondary">视图说明</Typography.Text>
        <Typography.Text type="secondary" style={{ fontSize: 12 }}>
          默认聚焦高热 Kickstarter 项目，可通过筛选器细化条件并导出结果。
        </Typography.Text>
      </Space>
    </Card>
  );

  return (
    <>
      <ProShell
        title="项目巡检"
        description="统一管理 Kickstarter 项目筛选、收藏及导出，支撑跨团队的高效协同。"
        overview={overview}
      >
        <ProCard colSpan={{ xs: 24, xl: 8 }} bordered>
          <Space direction="vertical" size={12} style={{ width: '100%' }}>
            <Statistic title="当前结果" value={tableData.length} suffix="个" valueStyle={{ color: '#38bdf8' }} />
            <Statistic title="收藏数量" value={favoritesCount} suffix="个" valueStyle={{ color: '#fbbf24' }} />
            {aggregated ? (
              <Space direction="vertical" size={4}>
                <Typography.Text type="secondary">成功项目：{aggregated.successfulCount.toLocaleString()}</Typography.Text>
                <Typography.Text type="secondary">总筹资：{aggregated.totalPledged.toLocaleString()}</Typography.Text>
                <Typography.Text type="secondary">平均达成率：{aggregated.averagePercentFunded.toFixed(1)}%</Typography.Text>
              </Space>
            ) : (
              <Typography.Text type="secondary">暂无聚合数据</Typography.Text>
            )}
            <Space>
              <Button type="primary" onClick={handleExport} disabled={!tableData.length}>
                导出 CSV
              </Button>
              <Button onClick={() => setFavoritesDrawerOpen(true)} disabled={!favoritesCount}>
                查看收藏
              </Button>
            </Space>
          </Space>
        </ProCard>
        <ProCard colSpan={{ xs: 24, xl: 16 }} bordered>
          <ProjectFilters
            value={filters}
            onChange={handleFiltersChange}
            isLoading={filtersLoading}
            options={filterOptions}
          />
        </ProCard>
        <ProCard colSpan={24} bordered>
          <Space direction="vertical" size={12} style={{ width: '100%' }}>
            <Space wrap align="center">
              <Switch
                checked={showFavoritesOnly}
                onChange={setShowFavoritesOnly}
                checkedChildren="仅看收藏"
                unCheckedChildren="全部项目"
                disabled={!favoritesCount}
              />
              <Tooltip title="重置页码">
                <Button
                  size="small"
                  onClick={() => setFilters((prev) => ({ ...prev, page: 1 }))}
                >
                  回到第 1 页
                </Button>
              </Tooltip>
            </Space>
            <Table<Project>
              rowKey={(project) => project.id}
              dataSource={tableData}
              columns={columns}
              loading={isFetching}
              pagination={{
                current: filters.page,
                pageSize: filters.pageSize,
                total: totalCount,
                showSizeChanger: true,
                pageSizeOptions: PAGE_SIZE_OPTIONS.map(String),
                onChange: handlePageChange,
              }}
              onRow={(record) => ({
                onClick: () => setActiveProject(record),
                style: { cursor: 'pointer' },
              })}
            />
          </Space>
        </ProCard>
      </ProShell>

      <ProjectDetailDrawer project={activeProject} open={Boolean(activeProject)} onClose={() => setActiveProject(null)} />

      <Drawer
        title="收藏项目"
        width={420}
        open={favoritesDrawerOpen}
        onClose={() => setFavoritesDrawerOpen(false)}
        destroyOnClose
      >
        {favoritesCount === 0 ? (
          <Empty description="暂无收藏项目" />
        ) : (
          <Space direction="vertical" size={12} style={{ width: '100%' }}>
            <List
              dataSource={favorites}
              renderItem={(item) => (
                <List.Item
                  actions={[
                    <Tooltip title="查看详情" key="view">
                      <Button type="link" onClick={() => setActiveProject(item.project)}>
                        查看
                      </Button>
                    </Tooltip>,
                    <Popconfirm
                      key="remove"
                      title="确认取消收藏？"
                      onConfirm={() => removeFavorite(item.project.id)}
                    >
                      <Button type="link" danger>
                        移除
                      </Button>
                    </Popconfirm>,
                  ]}
                >
                  <List.Item.Meta
                    title={
                      <Space>
                        <Typography.Text strong>{item.project.nameCn ?? item.project.name}</Typography.Text>
                        <Tag color="blue">{item.project.categoryName}</Tag>
                      </Space>
                    }
                    description={item.note ?? '暂无备注'}
                  />
                </List.Item>
              )}
            />
            <Popconfirm title="确认清空所有收藏？" onConfirm={clearFavorites}>
              <Button danger block>
                清空收藏
              </Button>
            </Popconfirm>
          </Space>
        )}
      </Drawer>

      <Modal
        title="添加收藏备注"
        open={Boolean(favoriteModalProject)}
        onOk={handleFavoriteModalOk}
        onCancel={handleFavoriteModalClose}
        confirmLoading={favoriteModalSaving}
        okText="保存"
        cancelText="取消"
        destroyOnClose
      >
        <Form form={favoriteForm} layout="vertical">
          <Form.Item name="note" label="备注" rules={[{ max: 120, message: '备注需在 120 字以内' }]}>
            <Input.TextArea rows={4} placeholder="记录项目亮点、下一步动作等信息（可选）" />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
};

export default function ProjectsPage() {
  return <ProjectsContent />;
}
