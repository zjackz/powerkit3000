import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Button,
  Card,
  Col,
  Drawer,
  Empty,
  Form,
  Input,
  List,
  Modal,
  message,
  Popconfirm,
  Progress,
  Row,
  Space,
  Statistic,
  Switch,
  Table,
  Tag,
  Tooltip,
  Typography,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { StarFilled, StarOutlined } from '@ant-design/icons';
import { ProjectFilters } from '@/components/projects/ProjectFilters';
import { ProjectDetailDrawer } from '@/components/projects/ProjectDetailDrawer';
import { useProjects } from '@/hooks/useProjects';
import { useProjectFilters } from '@/hooks/useProjectFilters';
import { useProjectFavorites } from '@/hooks/useProjectFavorites';
import { Project, ProjectQueryParams } from '@/types/project';
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

export const ProjectsPage = () => {
  const [filters, setFilters] = useState<ProjectQueryParams>({
    page: 1,
    pageSize: DEFAULT_PAGE_SIZE,
  });
  const [activeProject, setActiveProject] = useState<Project | null>(null);
  const {
    favorites,
    addFavorite,
    isFavorite,
    removeFavorite,
    clearFavorites,
  } = useProjectFavorites();
  const [showFavoritesOnly, setShowFavoritesOnly] = useState(false);
  const [favoritesDrawerOpen, setFavoritesDrawerOpen] = useState(false);
  const [favoriteModalProject, setFavoriteModalProject] = useState<Project | null>(null);
  const [favoriteModalSaving, setFavoriteModalSaving] = useState(false);
  const [favoriteForm] = Form.useForm();
  const favoritesCount = favorites.length;
  const favoriteProjects = useMemo(() => favorites.map((item) => item.project), [favorites]);
  const favoriteNotes = useMemo(
    () => new Map(favorites.map((item) => [item.project.id, item.note])),
    [favorites],
  );

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
    } catch (error: any) {
      if (error?.errorFields) {
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
          const starred = isFavorite(record.id);
          const note = favoriteNotes.get(record.id);
          const icon = starred ? <StarFilled style={{ color: '#faad14' }} /> : <StarOutlined />;
          const tooltipTitle = starred
            ? note
              ? `收藏备注：${note}`
              : '已收藏'
            : '收藏';
          return (
            <Tooltip title={tooltipTitle}>
              <Button
                type="text"
                size="small"
                icon={icon}
                onClick={async (event) => {
                  event.stopPropagation();

                  if (starred) {
                    try {
                      await removeFavorite(record.id);
                      message.success('已取消收藏');
                    } catch (error) {
                      console.error(error);
                      message.error('取消收藏失败，请稍后重试');
                    }
                  } else {
                    favoriteForm.setFieldsValue({ note: favoriteNotes.get(record.id) ?? '' });
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
        render: (value, record) => (
          <Space direction="vertical" size={0}>
            <Typography.Link>{record.nameCn ?? value}</Typography.Link>
            {record.nameCn && (
              <Typography.Text type="secondary">{value}</Typography.Text>
            )}
            {(record.blurbCn || record.blurb) && (
              <Typography.Text type="secondary" ellipsis style={{ maxWidth: 320 }}>
                {record.blurbCn ?? record.blurb}
              </Typography.Text>
            )}
          </Space>
        ),
      },
      {
        title: '状态',
        dataIndex: 'state',
        key: 'state',
        render: (state) => {
          const stateColor: Record<string, string> = {
            successful: 'green',
            live: 'blue',
            canceled: 'red',
            failed: 'volcano',
          };
          return <Tag color={stateColor[state] ?? 'default'}>{state}</Tag>;
        },
      },
      {
        title: '热度',
        dataIndex: 'percentFunded',
        key: 'momentum',
        width: 110,
        render: (percent: number) => {
          const meta = getMomentumMeta(Number(percent));
          return (
            <Tooltip title={`达成率 ${percent.toFixed(1)}%`}>
              <Tag color={meta.color}>{meta.label}</Tag>
            </Tooltip>
          );
        },
        responsive: ['md'],
      },
      {
        title: '品类',
        dataIndex: 'categoryName',
        key: 'categoryName',
      },
      {
        title: '国家',
        dataIndex: 'country',
        key: 'country',
      },
      {
        title: '创作者 / 地点',
        key: 'creator',
        render: (_, record) => (
          <Space direction="vertical" size={0}>
            <Typography.Text>{record.creatorName || '未提供'}</Typography.Text>
            {record.locationName && (
              <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                {record.locationName}
              </Typography.Text>
            )}
          </Space>
        ),
        responsive: ['lg'],
      },
      {
        title: '目标金额',
        dataIndex: 'goal',
        key: 'goal',
        align: 'right',
        render: (goal, record) => formatCurrencyValue(goal, record.currency),
      },
      {
        title: '已筹金额',
        dataIndex: 'pledged',
        key: 'pledged',
        align: 'right',
        render: (pledged, record) => (
          <Typography.Text strong>{formatCurrencyValue(pledged, record.currency)}</Typography.Text>
        ),
      },
      {
        title: '筹资速度',
        dataIndex: 'fundingVelocity',
        key: 'fundingVelocity',
        align: 'right',
        render: (velocity, record) => `${record.currency} ${velocity.toFixed(2)}/天`,
        sorter: (a, b) => a.fundingVelocity - b.fundingVelocity,
      },
      {
        title: '达成率',
        dataIndex: 'percentFunded',
        key: 'percentFunded',
        width: 200,
        render: (percent: number) => {
          const capped = Math.min(Number(percent), 100);
          const strokeColor = percent >= 100 ? '#52c41a' : '#1890ff';
          return (
            <Space size={8} align="center">
              <Tooltip title={`达成率 ${percent.toFixed(1)}%`}>
                <Progress
                  percent={capped}
                  size="small"
                  strokeColor={strokeColor}
                  showInfo={false}
                  style={{ width: 90 }}
                />
              </Tooltip>
              <Typography.Text>{percent.toFixed(1)}%</Typography.Text>
            </Space>
          );
        },
      },
      {
        title: '支持者',
        dataIndex: 'backersCount',
        key: 'backersCount',
        render: (value) => value.toLocaleString(),
      },
      {
        title: '上线时间',
        dataIndex: 'launchedAt',
        key: 'launchedAt',
        render: (value) => dayjs(value).format('YYYY-MM-DD'),
      },
      {
        title: '截止时间',
        dataIndex: 'deadline',
        key: 'deadline',
        render: (value) => dayjs(value).format('YYYY-MM-DD'),
        responsive: ['md'],
      },
      {
        title: '筹资周期',
        key: 'campaignDuration',
        render: (_, record) => {
          const launched = dayjs(record.launchedAt);
          const deadline = dayjs(record.deadline);
          const duration = Math.max(1, Math.round(deadline.diff(launched, 'day', true)));
          return `${duration} 天`;
        },
        responsive: ['lg'],
      },
    ],
    [favoriteNotes, favoriteForm, isFavorite, removeFavorite],
  );

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
      <Row gutter={[12, 12]} align="stretch">
        <Col xs={24} xl={10}>
          <Card
            title="筛选条件"
            size="small"
            bodyStyle={CARD_BODY_STYLE}
            headStyle={{ fontSize: 14, fontWeight: 600, padding: '8px 16px' }}
          >
            <ProjectFilters
              value={filters}
              onChange={handleFiltersChange}
              isLoading={isFetching || filtersLoading}
              options={filterOptions}
            />
          </Card>
        </Col>
        <Col xs={24} xl={14}>
          <Card
            title="数据概览"
            size="small"
            bodyStyle={CARD_BODY_STYLE}
            headStyle={{ fontSize: 14, fontWeight: 600, padding: '8px 16px' }}
          >
            <Row gutter={[12, 12]}>
              <Col xs={12} md={8}>
                <Statistic title="匹配项目" value={totalCount} suffix="个" valueStyle={{ color: '#1f6feb' }} />
              </Col>
              <Col xs={12} md={8}>
                <Statistic title="成功项目" value={aggregated?.successfulCount ?? 0} suffix="个" />
              </Col>
              <Col xs={12} md={8}>
                <Statistic
                  title="平均达成率"
                  value={aggregated?.averagePercentFunded ?? 0}
                  suffix="%"
                  precision={1}
                />
              </Col>
              <Col xs={12} md={8}>
                <Statistic
                  title="总支持者"
                  value={aggregated?.totalBackers ?? 0}
                  formatter={(value) => Number(value).toLocaleString()}
                />
              </Col>
              <Col xs={12} md={8}>
                <Statistic
                  title="总筹资"
                  value={aggregated?.totalPledged ?? 0}
                  formatter={(value) => Number(value).toLocaleString()}
                />
              </Col>
              <Col xs={12} md={8}>
                <Statistic
                  title="平均目标金额"
                  value={aggregated?.averageGoal ?? 0}
                  formatter={(value) =>
                    Number(value).toLocaleString(undefined, { maximumFractionDigits: 2 })
                  }
                />
              </Col>
            </Row>
            {aggregated?.topProject && (
              <Card
                type="inner"
                size="small"
                title="最高热度项目"
                style={{ marginTop: 12 }}
                bodyStyle={{ padding: 12 }}
              >
                <Space direction="vertical" size={4}>
                  <Typography.Link>{aggregated.topProject.nameCn ?? aggregated.topProject.name}</Typography.Link>
                  {aggregated.topProject.nameCn && (
                    <Typography.Text type="secondary">{aggregated.topProject.name}</Typography.Text>
                  )}
                  <Typography.Text type="secondary">
                    {aggregated.topProject.categoryName} · {aggregated.topProject.country}
                  </Typography.Text>
                  <Typography.Text>
                    达成率 {aggregated.topProject.percentFunded.toFixed(1)}% · 支持者{' '}
                    {aggregated.topProject.backersCount.toLocaleString()}
                  </Typography.Text>
                </Space>
              </Card>
            )}
          </Card>
        </Col>
      </Row>
      <Card
        title="项目列表"
        size="small"
        bodyStyle={{ padding: 0 }}
        headStyle={{ fontSize: 14, fontWeight: 600, padding: '8px 16px' }}
        extra={
          <Space size={8}>
            <Tooltip title={showFavoritesOnly ? '查看全部项目' : '仅查看收藏项目'}>
              <Switch
                size="small"
                checked={showFavoritesOnly}
                onChange={setShowFavoritesOnly}
                checkedChildren="收藏"
                unCheckedChildren="全部"
              />
            </Tooltip>
            <Button
              size="small"
              icon={<StarFilled style={{ color: '#faad14' }} />}
              onClick={() => setFavoritesDrawerOpen(true)}
            >
              我的收藏 ({favoritesCount})
            </Button>
            <Button size="small" onClick={handleExport}>
              导出 CSV
            </Button>
          </Space>
        }
      >
        <Table<Project>
          columns={columns}
          dataSource={tableData}
          rowKey={(record) => record.id}
          loading={isFetching && !showFavoritesOnly}
          size="middle"
          onRow={(record) => ({
            onClick: () => setActiveProject(record),
          })}
          pagination={
            showFavoritesOnly
              ? false
              : {
                  current: filters.page,
                  pageSize: filters.pageSize,
                  total: totalCount,
                  onChange: handlePageChange,
                  showSizeChanger: true,
                  pageSizeOptions: PAGE_SIZE_OPTIONS.map(String),
                }
          }
        />
      </Card>
      <ProjectDetailDrawer
        project={activeProject}
        open={Boolean(activeProject)}
        onClose={() => setActiveProject(null)}
      />
      <Drawer
        title={`我的收藏 (${favoritesCount})`}
        width={420}
        open={favoritesDrawerOpen}
        onClose={() => setFavoritesDrawerOpen(false)}
        extra={
          favoritesCount > 0 ? (
            <Popconfirm
              title="清空所有收藏?"
              okText="清空"
              cancelText="取消"
              onConfirm={async () => {
                try {
                  await clearFavorites();
                  message.success('已清空收藏');
                } catch (error) {
                  console.error(error);
                  message.error('清空收藏失败，请稍后重试');
                }
              }}
              placement="bottomRight"
            >
              <Button type="link" danger size="small">
                清空
              </Button>
            </Popconfirm>
          ) : null
        }
      >
        {favoritesCount === 0 ? (
          <Empty description="暂无收藏项目" />
        ) : (
          <List
            dataSource={favorites}
            renderItem={({ project, note }) => (
              <List.Item
                key={project.id}
                actions={[
                  <Button
                    type="link"
                    size="small"
                    onClick={async () => {
                      try {
                        await removeFavorite(project.id);
                        message.success('已取消收藏');
                      } catch (error) {
                        console.error(error);
                        message.error('取消收藏失败，请稍后重试');
                      }
                    }}
                    key="remove"
                  >
                    取消收藏
                  </Button>,
                  <Button
                    type="link"
                    size="small"
                    onClick={() => {
                      setActiveProject(project);
                      setFavoritesDrawerOpen(false);
                    }}
                    key="view"
                  >
                    查看
                  </Button>,
                ]}
              >
                <List.Item.Meta
                  title={
                    <Space size={6} wrap>
                      <Typography.Text strong>
                        {project.nameCn ?? project.name}
                      </Typography.Text>
                      <Tag>{project.categoryName}</Tag>
                      <Tag>{project.country}</Tag>
                    </Space>
                  }
                  description={
                    <Typography.Text type="secondary">
                      达成率 {project.percentFunded.toFixed(1)}% · 已筹{' '}
                      {formatCurrencyValue(project.pledged, project.currency)}
                    </Typography.Text>
                  }
                />
                {note && (
                  <Typography.Text type="secondary" italic>
                    备注：{note}
                  </Typography.Text>
                )}
              </List.Item>
            )}
          />
        )}
      </Drawer>
      <Modal
        title="添加收藏"
        open={Boolean(favoriteModalProject)}
        okText="保存"
        cancelText="取消"
        onOk={handleFavoriteModalOk}
        onCancel={handleFavoriteModalClose}
        destroyOnClose
        confirmLoading={favoriteModalSaving}
      >
        <Form
          form={favoriteForm}
          layout="vertical"
          initialValues={{ note: '' }}
        >
          <Form.Item
            label="收藏备注"
            name="note"
            rules={[{ max: 200, message: '备注长度需在 200 字以内' }]}
          >
            <Input.TextArea rows={4} placeholder="记录收藏理由，方便后续复盘（可留空）" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};
