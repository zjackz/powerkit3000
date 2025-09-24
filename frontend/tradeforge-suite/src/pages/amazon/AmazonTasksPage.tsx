import { DeleteOutlined, PlusOutlined, SaveOutlined } from '@ant-design/icons';
import {
  Badge,
  Button,
  Card,
  Empty,
  Form,
  FormListFieldData,
  Input,
  InputNumber,
  List,
  message,
  Select,
  Skeleton,
  Space,
  Tag,
  Tooltip,
  Typography,
} from 'antd';
import dayjs from 'dayjs';
import { useEffect, useMemo, useState } from 'react';
import {
  createAmazonTaskDraft,
  fetchAmazonTasks,
  summarizeTask as summarizeTaskFromService,
  upsertAmazonTask,
} from '@/services/amazonTasksService';
import type {
  AmazonTask,
  AmazonTaskCategorySelector,
  AmazonTaskFilterRules,
  AmazonTaskKeywordRules,
  AmazonTaskLimits,
  AmazonTaskPriceRange,
  AmazonTaskSchedule,
  AmazonTaskStatus,
} from '@/types/amazon';
import styles from './AmazonTasksPage.module.css';

interface CategoryFormValue extends AmazonTaskCategorySelector {}

interface AmazonTaskFormValues {
  name: string;
  site: string;
  categories: CategoryFormValue[];
  leaderboards: string[];
  priceRange: AmazonTaskPriceRange;
  keywords: AmazonTaskKeywordRules;
  filters: AmazonTaskFilterRules;
  schedule: AmazonTaskSchedule;
  limits: AmazonTaskLimits;
  proxyPolicy: string;
  status: AmazonTaskStatus;
  notes?: string;
}

const leaderboardOptions = [
  { value: 'BestSellers', label: 'Best Sellers' },
  { value: 'NewReleases', label: 'New Releases' },
  { value: 'MoversAndShakers', label: 'Movers & Shakers' },
];

const statusColor: Record<AmazonTaskStatus, string> = {
  draft: 'default',
  active: 'success',
  paused: 'warning',
};

const statusLabel: Record<AmazonTaskStatus, string> = {
  draft: '草稿',
  active: '生效中',
  paused: '暂停',
};

const categoryTypeOptions = [
  { value: 'url', label: '类目 URL' },
  { value: 'node', label: '类目节点 ID' },
];

const ensureCategories = (categories?: CategoryFormValue[]): CategoryFormValue[] => {
  if (!categories || categories.length === 0) {
    return [{ type: 'url', value: '' }];
  }
  return categories;
};

const convertTaskToFormValues = (task: AmazonTask): AmazonTaskFormValues => ({
  name: task.name,
  site: task.site,
  categories: ensureCategories(task.categories),
  leaderboards: [...task.leaderboards],
  priceRange: { ...task.priceRange },
  keywords: {
    include: [...task.keywords.include],
    exclude: [...task.keywords.exclude],
  },
  filters: { ...task.filters },
  schedule: { ...task.schedule },
  limits: { ...task.limits },
  proxyPolicy: task.proxyPolicy,
  status: task.status,
  notes: task.notes ?? '',
});

const mergeFormValuesToTask = (
  task: AmazonTask,
  values: AmazonTaskFormValues,
): AmazonTask => ({
  ...task,
  name: values.name.trim(),
  site: values.site.trim(),
  categories: values.categories
    .map((item) => ({ ...item, value: item.value.trim() }))
    .filter((item) => item.value.length > 0),
  leaderboards: values.leaderboards,
  priceRange: values.priceRange,
  keywords: {
    include: values.keywords.include.map((item) => item.trim()).filter(Boolean),
    exclude: values.keywords.exclude.map((item) => item.trim()).filter(Boolean),
  },
  filters: values.filters,
  schedule: values.schedule,
  limits: values.limits,
  proxyPolicy: values.proxyPolicy.trim(),
  status: values.status,
  notes: values.notes?.trim() ?? '',
  updatedAt: new Date().toISOString(),
});

const buildSummaryFromValues = (values: AmazonTaskFormValues | AmazonTask): string => {
  const leaderboards = Array.isArray(values.leaderboards)
    ? values.leaderboards
    : (values as AmazonTask).leaderboards;
  const categories = Array.isArray(values.categories)
    ? values.categories
    : (values as AmazonTask).categories;

  const price = values.priceRange ?? (values as AmazonTask).priceRange;
  const filters = values.filters ?? (values as AmazonTask).filters;

  const lbText = leaderboards.join('、') || '未选择榜单';
  const priceText = `${price?.min ?? '0'} - ${price?.max ?? '∞'}`;
  const ratingText = filters?.minRating ? `${filters.minRating}★` : '不限评分';
  const reviewText = filters?.minReviews ? `评论 ≥ ${filters.minReviews}` : '不限评论';

  return `聚焦 ${lbText}，价位 ${priceText} 美元，筛选 ${ratingText} / ${reviewText}。类目共 ${categories.length} 项，建议定期审查数据质量。`;
};

const AmazonTasksPage = () => {
  const [tasks, setTasks] = useState<AmazonTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [selectedTaskId, setSelectedTaskId] = useState<string>();
  const [form] = Form.useForm<AmazonTaskFormValues>();

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        const data = await fetchAmazonTasks();
        setTasks(data);
        if (data.length > 0) {
          setSelectedTaskId(data[0].id);
          form.setFieldsValue(convertTaskToFormValues(data[0]));
        }
      } catch (error) {
        message.error('加载任务配置失败');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [form]);

  const selectedTask = useMemo(
    () => tasks.find((task) => task.id === selectedTaskId),
    [tasks, selectedTaskId],
  );

  useEffect(() => {
    if (selectedTask) {
      form.setFieldsValue(convertTaskToFormValues(selectedTask));
    }
  }, [selectedTask, form]);

  const handleSelectTask = (task: AmazonTask) => {
    setSelectedTaskId(task.id);
    form.setFieldsValue(convertTaskToFormValues(task));
  };

  const handleCreateTask = () => {
    const newTask = createAmazonTaskDraft({
      status: 'draft',
      llmSummary: buildSummaryFromValues({
        leaderboards: ['BestSellers'],
        categories: [{ type: 'url', value: '' }],
        priceRange: { min: 30, max: 50 },
        filters: { minRating: 4, minReviews: 25 },
      } as AmazonTaskFormValues),
    });
    setTasks((prev) => [newTask, ...prev]);
    setSelectedTaskId(newTask.id);
    form.setFieldsValue(convertTaskToFormValues(newTask));
    message.success('已创建任务草稿，完善配置后保存。');
  };

  const handleSave = async () => {
    if (!selectedTask) {
      return;
    }
    try {
      setSaving(true);
      const values = await form.validateFields();
      const merged = mergeFormValuesToTask(selectedTask, values);
      merged.llmSummary = summarizeTaskFromService(merged) || buildSummaryFromValues(values);

      await upsertAmazonTask(merged);
      setTasks((prev) => {
        const next = prev.filter((item) => item.id !== merged.id);
        return [merged, ...next];
      });
      form.setFieldsValue(convertTaskToFormValues(merged));
      message.success('任务已保存');
    } catch (error) {
      if (!(error as { errorFields?: unknown }).errorFields) {
        message.error('保存任务失败，请稍后重试');
      }
    } finally {
      setSaving(false);
    }
  };

  const handleResetToOriginal = () => {
    if (selectedTask) {
      form.setFieldsValue(convertTaskToFormValues(selectedTask));
    }
  };

  return (
    <div className={styles.wrapper}>
      <Typography.Title level={3}>Amazon 采集任务配置</Typography.Title>
      <Typography.Paragraph className={styles.subtitle}>
        配置要跟踪的类目、榜单与筛选规则，系统会按计划自动抓取。保存后即可由调度与 CLI 使用。
      </Typography.Paragraph>
      <div className={styles.layout}>
        <Card
          className={styles.taskListCard}
          title={<div className={styles.taskListHeader}><span>任务列表</span><Badge count={tasks.length} /></div>}
          extra={
            <Button type="primary" icon={<PlusOutlined />} onClick={handleCreateTask}>
              新建任务
            </Button>
          }
        >
          <div className={styles.taskListContent}>
            {loading ? (
              <Skeleton active paragraph={{ rows: 6 }} />
            ) : tasks.length === 0 ? (
              <Empty description="暂无任务配置" />
            ) : (
              <List
                dataSource={tasks}
                renderItem={(task) => {
                  const isActive = task.id === selectedTaskId;
                  return (
                    <List.Item
                      key={task.id}
                      className={`${styles.taskItem} ${isActive ? styles.taskItemActive : ''}`}
                      onClick={() => handleSelectTask(task)}
                    >
                      <Space direction="vertical" size={4} style={{ width: '100%' }}>
                        <Space align="center" style={{ justifyContent: 'space-between', width: '100%' }}>
                          <Typography.Text strong>{task.name}</Typography.Text>
                          <Tag color={statusColor[task.status]}>{statusLabel[task.status]}</Tag>
                        </Space>
                        <Typography.Text className={styles.taskItemMeta}>
                          {task.leaderboards.join(' / ')} · {task.site}
                        </Typography.Text>
                        <Typography.Text className={styles.taskItemMeta}>
                          更新于 {dayjs(task.updatedAt).format('MM-DD HH:mm')}
                        </Typography.Text>
                      </Space>
                    </List.Item>
                  );
                }}
              />
            )}
          </div>
        </Card>

        <div className={styles.formWrapper}>
          {selectedTask ? (
            <>
              <Form<AmazonTaskFormValues> form={form} layout="vertical">
                <Card title="基础信息" className={styles.formCard} size="small">
                  <div className={styles.formGrid}>
                    <Form.Item
                      label="任务名称"
                      name="name"
                      rules={[{ required: true, message: '请输入任务名称' }]}
                    >
                      <Input placeholder="例如 home_new_releases_30_50" />
                    </Form.Item>
                    <Form.Item
                      label="目标站点"
                      name="site"
                      rules={[{ required: true, message: '请输入站点域名，例如 amazon.com' }]}
                    >
                      <Input placeholder="例如 amazon.com" />
                    </Form.Item>
                    <Form.Item label="任务状态" name="status">
                      <Select
                        options={[
                          { value: 'draft', label: '草稿' },
                          { value: 'active', label: '生效中' },
                          { value: 'paused', label: '暂停' },
                        ]}
                      />
                    </Form.Item>
                    <Form.Item label="代理策略" name="proxyPolicy">
                      <Select
                        options={[
                          { value: 'default', label: '默认策略' },
                          { value: 'residential', label: '住宅代理' },
                          { value: 'datacenter', label: '数据中心代理' },
                        ]}
                      />
                    </Form.Item>
                  </div>
                  <Form.Item label="备注" name="notes">
                    <Input.TextArea rows={2} placeholder="用于说明该任务的运营目标或注意事项" />
                  </Form.Item>
                </Card>

                <Card title="类目与榜单" className={styles.formCard} size="small">
                  <Form.Item label="类目来源" required>
                    <Form.List
                      name="categories"
                      rules={[
                        {
                          validator: async (_, value) => {
                            if (!value || value.length === 0 || value.every((item: CategoryFormValue) => !item.value)) {
                              return Promise.reject(new Error('请至少添加一个类目来源'));
                            }
                            return Promise.resolve();
                          },
                        },
                      ]}
                    >
                      {(fields, { add, remove }) => (
                        <div className={styles.categoriesGrid}>
                          {fields.map((field: FormListFieldData, index) => (
                            <Space key={field.key} className={styles.categoryRow} align="baseline">
                              <Form.Item
                                {...field}
                                name={[field.name, 'type']}
                                fieldKey={[field.fieldKey as number, 'type']}
                                label={index === 0 ? '类型' : ''}
                                rules={[{ required: true, message: '请选择来源类型' }]}
                              >
                                <Select options={categoryTypeOptions} style={{ width: 140 }} />
                              </Form.Item>
                              <Form.Item
                                {...field}
                                name={[field.name, 'value']}
                                fieldKey={[field.fieldKey as number, 'value']}
                                label={index === 0 ? '值' : ''}
                                rules={[{ required: true, message: '请输入类目 URL 或节点 ID' }]}
                              >
                                <Input placeholder="粘贴类目 URL 或节点 ID" />
                              </Form.Item>
                              {fields.length > 1 && (
                                <Tooltip title="移除">
                                  <Button
                                    type="text"
                                    icon={<DeleteOutlined />}
                                    danger
                                    onClick={() => remove(field.name)}
                                  />
                                </Tooltip>
                              )}
                            </Space>
                          ))}
                          <Button type="dashed" icon={<PlusOutlined />} onClick={() => add({ type: 'url', value: '' })}>
                            添加类目
                          </Button>
                        </div>
                      )}
                    </Form.List>
                  </Form.Item>
                  <Form.Item
                    label="榜单类型"
                    name="leaderboards"
                    rules={[{ required: true, message: '请至少选择一个榜单' }]}
                  >
                    <Select mode="multiple" options={leaderboardOptions} placeholder="选择要采集的榜单" />
                  </Form.Item>
                </Card>

                <Card title="筛选与限制" className={styles.formCard} size="small">
                  <div className={styles.formGrid}>
                    <Form.Item label="最低评分" name={['filters', 'minRating']}>
                      <InputNumber min={0} max={5} step={0.1} style={{ width: '100%' }} placeholder="例如 4.0" />
                    </Form.Item>
                    <Form.Item label="最低评论数" name={['filters', 'minReviews']}>
                      <InputNumber min={0} step={10} style={{ width: '100%' }} placeholder="例如 25" />
                    </Form.Item>
                    <Form.Item label="最低价格" name={['priceRange', 'min']}>
                      <InputNumber min={0} step={1} style={{ width: '100%' }} placeholder="例如 30" />
                    </Form.Item>
                    <Form.Item label="最高价格" name={['priceRange', 'max']}>
                      <InputNumber min={0} step={1} style={{ width: '100%' }} placeholder="例如 50" />
                    </Form.Item>
                    <Form.Item label="最大商品数" name={['limits', 'maxProducts']}>
                      <InputNumber min={50} step={50} style={{ width: '100%' }} placeholder="例如 200" />
                    </Form.Item>
                    <Form.Item label="每小时请求上限" name={['limits', 'maxRequestsPerHour']}>
                      <InputNumber min={100} step={50} style={{ width: '100%' }} placeholder="例如 400" />
                    </Form.Item>
                  </div>
                  <Form.Item label="关键字（包含）" name={['keywords', 'include']}>
                    <Select mode="tags" placeholder="输入后回车即可添加关键字" />
                  </Form.Item>
                  <Form.Item label="关键字（排除）" name={['keywords', 'exclude']}>
                    <Select mode="tags" placeholder="输入后回车即可排除关键字" />
                  </Form.Item>
                </Card>

                <Card title="调度计划" className={styles.formCard} size="small">
                  <div className={styles.formGrid}>
                    <Form.Item label="调度类型" name={['schedule', 'type']}>
                      <Select
                        options={[
                          { value: 'recurring', label: '周期执行' },
                          { value: 'once', label: '单次执行' },
                        ]}
                      />
                    </Form.Item>
                    <Form.Item
                      label="Cron 表达式"
                      name={['schedule', 'cron']}
                      tooltip="使用 cron 表达式指定时间，例如 0 30 2 * * * 表示每日 02:30 UTC"
                      rules={[{ required: true, message: '请输入 cron 表达式' }]}
                    >
                      <Input placeholder="0 30 2 * * *" />
                    </Form.Item>
                    <Form.Item label="时区" name={['schedule', 'timezone']}>
                      <Select
                        showSearch
                        optionFilterProp="label"
                        options={[
                          { value: 'UTC', label: 'UTC' },
                          { value: 'America/Los_Angeles', label: 'America/Los_Angeles' },
                          { value: 'America/New_York', label: 'America/New_York' },
                          { value: 'Europe/Berlin', label: 'Europe/Berlin' },
                          { value: 'Asia/Shanghai', label: 'Asia/Shanghai' },
                        ]}
                        placeholder="选择时区"
                      />
                    </Form.Item>
                  </div>
                </Card>
              </Form>

              <div className={styles.actionsBar}>
                <Space>
                  <Button onClick={handleResetToOriginal}>还原当前任务</Button>
                  <Button type="primary" icon={<SaveOutlined />} loading={saving} onClick={handleSave}>
                    保存任务
                  </Button>
                </Space>
                <div className={styles.aiPanel}>
                  <Typography.Text className={styles.aiPanelTitle}>AI 建议</Typography.Text>
                  <Typography.Paragraph className={styles.aiPanelText}>
                    {selectedTask.llmSummary?.trim() || buildSummaryFromValues(form.getFieldsValue(true))}
                  </Typography.Paragraph>
                  <Tag color="processing">自动生成</Tag>
                </div>
              </div>
            </>
          ) : loading ? (
            <Skeleton active paragraph={{ rows: 8 }} />
          ) : (
            <Empty description="请选择左侧任务或新建任务" />
          )}
        </div>
      </div>
    </div>
  );
};

export default AmazonTasksPage;
