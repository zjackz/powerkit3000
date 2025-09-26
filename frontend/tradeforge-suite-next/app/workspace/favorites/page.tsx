'use client';

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
  Typography,
  message,
} from 'antd';
import dayjs from 'dayjs';
import { StarFilled } from '@ant-design/icons';
import { ProCard } from '@ant-design/pro-components';
import { ProShell } from '@/layouts/ProShell';
import { TeamSwitcher } from '@/components/team/TeamSwitcher';
import { useTeamContext } from '@/contexts/TeamContext';
import { ProjectDetailDrawer } from '@/components/projects/ProjectDetailDrawer';
import { useProjectFavorites } from '@/hooks/useProjectFavorites';
import type { Project, ProjectFavoriteRecord } from '@/types/project';

const formatCurrencyValue = (value: number, currency: string) => `${currency} ${value.toLocaleString()}`;

const FavoritesContent = () => {
  const { team } = useTeamContext();
  const { favorites, removeFavorite, clearFavorites, updateFavoriteNote, isLoading } = useProjectFavorites();
  const [searchTerm, setSearchTerm] = useState('');
  const [activeProject, setActiveProject] = useState<Project | null>(null);
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
      const fields = [project.name, project.nameCn ?? '', project.categoryName, project.country, note ?? '' ];
      return fields.some((value) => value.toLowerCase().includes(term));
    });
  }, [searchTerm, sortedFavorites]);

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
    } catch (error) {
      if ((error as { errorFields?: unknown }).errorFields) {
        return;
      }
      console.error(error);
      message.error('更新备注失败，请稍后重试');
    }
  };

  const handleExportFavorites = () => {
    if (!favorites.length) {
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

  const overview = (
    <Card bordered={false} style={{ background: 'rgba(15,23,42,0.85)' }}>
      <Space direction="vertical" size={12} style={{ width: '100%' }}>
        <Typography.Text type="secondary">团队视角</Typography.Text>
        <TeamSwitcher />
        <Typography.Text type="secondary" style={{ fontSize: 12 }}>
          {team.description}
        </Typography.Text>
      </Space>
    </Card>
  );

  return (
    <>
      <ProShell
        title="重点跟进"
        description="维护跨团队共享的收藏项目、备注与导出清单，支撑会议复盘与执行落地。"
        overview={overview}
      >
        <ProCard colSpan={{ xs: 24, xl: 8 }} bordered>
          <Space direction="vertical" size={12} style={{ width: '100%' }}>
            <Space align="center" size={10}>
              <StarFilled style={{ color: '#faad14' }} />
              <Typography.Title level={4} style={{ margin: 0 }}>
                我的收藏
              </Typography.Title>
            </Space>
            <Typography.Text type="secondary">
              收藏数量：{favorites.length} 个 · 团队：{team.name}
            </Typography.Text>
            <Space>
              <Button type="primary" onClick={handleExportFavorites} disabled={!favorites.length}>
                导出 CSV
              </Button>
              <Button danger disabled={!favorites.length} onClick={clearFavorites}>
                清空收藏
              </Button>
            </Space>
            <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
              使用右侧列表快速筛选、维护备注，并通过「查看详情」进入项目抽屉。
            </Typography.Paragraph>
          </Space>
        </ProCard>
        <ProCard colSpan={{ xs: 24, xl: 16 }} bordered>
          <Space direction="vertical" size={12} style={{ width: '100%' }}>
            <Input.Search
              placeholder="按项目名、品类、国家或备注搜索"
              allowClear
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
            />
            {isLoading ? (
              <Empty description="加载中" />
            ) : filteredFavorites.length === 0 ? (
              <Empty description={favorites.length === 0 ? '暂无收藏项目' : '未找到符合条件的收藏'} />
            ) : (
              <List
                itemLayout="vertical"
                dataSource={filteredFavorites}
                renderItem={(item) => (
                  <List.Item
                    key={item.project.id}
                    actions={[
                      <Button type="link" key="view" onClick={() => setActiveProject(item.project)}>
                        查看详情
                      </Button>,
                      <Button type="link" key="note" onClick={() => handleOpenNoteModal(item)}>
                        编辑备注
                      </Button>,
                      <Button
                        type="link"
                        key="remove"
                        danger
                        onClick={async () => {
                          await removeFavorite(item.project.id);
                          message.success('已取消收藏');
                        }}
                      >
                        取消收藏
                      </Button>,
                    ]}
                  >
                    <List.Item.Meta
                      title={
                        <Space size={8} wrap>
                          <Typography.Text strong>{item.project.nameCn ?? item.project.name}</Typography.Text>
                          {item.project.nameCn && (
                            <Typography.Text type="secondary">{item.project.name}</Typography.Text>
                          )}
                          <Tag color="blue">{item.project.categoryName}</Tag>
                          <Tag>{item.project.country}</Tag>
                        </Space>
                      }
                      description={
                        <Space direction="vertical" size={2}>
                          <Typography.Text type="secondary">
                            上线：{dayjs(item.project.launchedAt).format('YYYY-MM-DD')} · 截止：
                            {dayjs(item.project.deadline).format('YYYY-MM-DD')}
                          </Typography.Text>
                          <Typography.Text>
                            达成率 {item.project.percentFunded.toFixed(1)}% · 支持者 {item.project.backersCount.toLocaleString()}
                          </Typography.Text>
                          <Typography.Text>
                            已筹 {formatCurrencyValue(item.project.pledged, item.project.currency)} · 目标 {formatCurrencyValue(item.project.goal, item.project.currency)}
                          </Typography.Text>
                          <Typography.Text type="secondary" italic>
                            备注：{item.note ?? '暂无备注'}
                          </Typography.Text>
                        </Space>
                      }
                    />
                  </List.Item>
                )}
              />
            )}
          </Space>
        </ProCard>
      </ProShell>

      <ProjectDetailDrawer project={activeProject} open={Boolean(activeProject)} onClose={() => setActiveProject(null)} />

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
          <Form.Item name="note" label="备注" rules={[{ max: 200, message: '备注长度需在 200 字以内' }]}>
            <Input.TextArea rows={4} placeholder="记录收藏理由，方便后续复盘（可留空）" />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
};

export default function FavoritesPage() {
  return <FavoritesContent />;
}
