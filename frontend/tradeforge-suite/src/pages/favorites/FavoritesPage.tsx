import { useMemo, useState } from 'react';
import {
  Button,
  Card,
  Empty,
  Form,
  Input,
  List,
  Modal,
  Space,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import { StarFilled } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useNavigate } from 'react-router-dom';
import { ProjectDetailDrawer } from '@/components/projects/ProjectDetailDrawer';
import { useProjectFavorites } from '@/hooks/useProjectFavorites';
import type { Project, ProjectFavoriteRecord } from '@/types/project';
const { Title, Text } = Typography;

const formatCurrencyValue = (value: number, currency: string) => `${currency} ${value.toLocaleString()}`;

export const FavoritesPage = () => {
  const navigate = useNavigate();
  const { favorites, removeFavorite, clearFavorites, updateFavoriteNote, isLoading } = useProjectFavorites();
  const [activeProject, setActiveProject] = useState<Project | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [noteModalState, setNoteModalState] = useState<{ open: boolean; record?: ProjectFavoriteRecord }>({ open: false });
  const [noteForm] = Form.useForm();
  const sortedFavorites = useMemo(
    () =>
      [...favorites].sort((a, b) => {
        if (b.project.percentFunded === a.project.percentFunded) {
          return b.project.pledged - a.project.pledged;
        }
        return b.project.percentFunded - a.project.percentFunded;
      }),
    [favorites],
  );
  const filteredFavorites = useMemo(() => {
    const term = searchTerm.trim().toLowerCase();
    if (!term) {
      return sortedFavorites;
    }

    return sortedFavorites.filter(({ project, note }) => {
      const fields = [
        project.name,
        project.nameCn ?? '',
        project.categoryName,
        project.country,
        note ?? '',
      ];
      return fields.some((value) => value.toLowerCase().includes(term));
    });
  }, [sortedFavorites, searchTerm]);

  const handleOpenNoteModal = (record: ProjectFavoriteRecord) => {
    noteForm.setFieldsValue({ note: record.note ?? '' });
    setNoteModalState({ open: true, record });
  };

  const handleSaveNote = async () => {
    try {
      const { note } = await noteForm.validateFields();
      if (!noteModalState.record) {
        return;
      }

      await updateFavoriteNote(noteModalState.record.project.id, note);
      message.success('备注已更新');
      setNoteModalState({ open: false });
      noteForm.resetFields();
    } catch (error: any) {
      if (error?.errorFields) {
        return;
      }
      console.error(error);
      message.error('更新备注失败，请稍后重试');
    }
  };

  const handleExportFavorites = () => {
    if (favorites.length === 0) {
      message.info('暂无收藏可导出');
      return;
    }

    const headers = ['Id', 'Name', 'NameCn', 'Category', 'Country', 'State', 'Goal', 'Pledged', 'PercentFunded', 'Backers', 'Currency', 'LaunchedAt', 'Deadline', 'Note'];
    const rows = favorites.map(({ project, note }) => [
      project.id,
      project.name,
      project.nameCn ?? '',
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
      note ?? '',
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
    link.setAttribute('download', `tradeforge-favorites-${dayjs().format('YYYYMMDD-HHmmss')}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);

    message.success('已导出收藏清单');
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <Card
        title={
          <Space align="center" size={10}>
            <StarFilled style={{ color: '#faad14' }} />
            <Title level={4} style={{ margin: 0 }}>
              我的收藏
            </Title>
          </Space>
        }
        extra={
          favorites.length > 0 ? (
            <Space size={8}>
              <Button size="small" onClick={handleExportFavorites}>
                导出 CSV
              </Button>
              <Tooltip title="清空收藏">
                <Button
                  danger
                  size="small"
                  onClick={async () => {
                    try {
                      await clearFavorites();
                      message.success('已清空收藏');
                    } catch (error) {
                      console.error(error);
                      message.error('清空收藏失败，请稍后重试');
                    }
                  }}
                >
                  清空
                </Button>
              </Tooltip>
            </Space>
          ) : null
        }
        loading={isLoading}
      >
        {favorites.length === 0 ? (
          <Empty description="暂无收藏项目">
            <Button type="primary" onClick={() => navigate('/projects')}>
              去浏览项目
            </Button>
          </Empty>
        ) : (
          <Space direction="vertical" size={16} style={{ width: '100%' }}>
            <Input.Search
              placeholder="按项目名、品类或备注搜索"
              allowClear
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
            />
            {filteredFavorites.length === 0 ? (
              <Empty description="未找到符合条件的收藏" />
            ) : (
              <List
                itemLayout="vertical"
                dataSource={filteredFavorites}
                renderItem={(item) => (
                  <List.Item
                    key={item.project.id}
                    actions={[
                      <Button
                        type="link"
                        onClick={() => setActiveProject(item.project)}
                        key="view"
                      >
                        查看详情
                      </Button>,
                      <Button
                        type="link"
                        onClick={() => handleOpenNoteModal(item)}
                        key="note"
                      >
                        编辑备注
                      </Button>,
                      <Button
                        type="link"
                        danger
                        onClick={async () => {
                          try {
                            await removeFavorite(item.project.id);
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
                    ]}
                  >
                    <List.Item.Meta
                      title={
                        <Space size={8} wrap>
                          <Text strong>{item.project.nameCn ?? item.project.name}</Text>
                          {item.project.nameCn && (
                            <Text type="secondary">{item.project.name}</Text>
                          )}
                          <Tag color="blue">{item.project.categoryName}</Tag>
                          <Tag>{item.project.country}</Tag>
                        </Space>
                      }
                      description={
                        <Space direction="vertical" size={2}>
                          <Text type="secondary">
                            上线时间：{dayjs(item.project.launchedAt).format('YYYY-MM-DD')} · 截止：
                            {dayjs(item.project.deadline).format('YYYY-MM-DD')}
                          </Text>
                          <Text>
                            达成率 {item.project.percentFunded.toFixed(1)}% · 支持者{' '}
                            {item.project.backersCount.toLocaleString()}
                          </Text>
                          <Text>
                            已筹 {formatCurrencyValue(item.project.pledged, item.project.currency)} · 目标{' '}
                            {formatCurrencyValue(item.project.goal, item.project.currency)}
                          </Text>
                          <Text type="secondary" italic>
                            备注：{item.note ?? '暂无备注'}
                          </Text>
                        </Space>
                      }
                    />
                  </List.Item>
                )}
              />
            )}
          </Space>
        )}
      </Card>
      <ProjectDetailDrawer
        project={activeProject}
        open={Boolean(activeProject)}
        onClose={() => setActiveProject(null)}
      />
      <Modal
        title="编辑收藏备注"
        open={noteModalState.open}
        okText="保存"
        cancelText="取消"
        onOk={handleSaveNote}
        onCancel={() => {
          setNoteModalState({ open: false });
          noteForm.resetFields();
        }}
        destroyOnClose
      >
        <Form form={noteForm} layout="vertical">
          <Form.Item
            label="备注"
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

export default FavoritesPage;
