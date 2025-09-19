import { useMemo, useState } from 'react';
import { Button, Card, message, Space, Statistic, Table, Tag, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { ProjectFilters } from '@/components/projects/ProjectFilters';
import { useProjects } from '@/hooks/useProjects';
import { useProjectFilters } from '@/hooks/useProjectFilters';
import { Project, ProjectQueryParams } from '@/types/project';
import { DEFAULT_PAGE_SIZE, PAGE_SIZE_OPTIONS } from '@/constants/projectOptions';

const columns: ColumnsType<Project> = [
  {
    title: '项目名称',
    dataIndex: 'name',
    key: 'name',
    render: (value, record) => (
      <Space direction="vertical" size={0}>
        <Typography.Link>{value}</Typography.Link>
        <Typography.Text type="secondary" ellipsis style={{ maxWidth: 320 }}>
          {record.blurb}
        </Typography.Text>
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
    title: '目标金额',
    dataIndex: 'goal',
    key: 'goal',
    render: (goal, record) => `${record.currency} ${goal.toLocaleString()}`,
  },
  {
    title: '已筹金额',
    dataIndex: 'pledged',
    key: 'pledged',
    render: (pledged, record) => `${record.currency} ${pledged.toLocaleString()}`,
  },
  {
    title: '达成率',
    dataIndex: 'percentFunded',
    key: 'percentFunded',
    render: (percent) => `${percent.toFixed(1)}%`,
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
];

export const ProjectsPage = () => {
  const [filters, setFilters] = useState<ProjectQueryParams>({
    page: 1,
    pageSize: DEFAULT_PAGE_SIZE,
  });

  const { data, isFetching } = useProjects(filters);
  const { data: filterOptions, isLoading: filtersLoading } = useProjectFilters();

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

  const stats = useMemo(() => {
    if (!data) {
      return null;
    }

    const fundedAverage = data.items.reduce((sum, project) => sum + project.percentFunded, 0) /
      (data.items.length || 1);
    return (
      <Space size="large">
        <Statistic title="匹配项目" value={data.total} suffix="个" />
        <Statistic title="平均达成率" value={fundedAverage} suffix="%" precision={1} />
      </Space>
    );
  }, [data]);

  const handleExport = () => {
    if (!data || data.items.length === 0) {
      message.info('当前无可导出的项目。');
      return;
    }

    const headers = ['Id', 'Name', 'Category', 'Country', 'State', 'Goal', 'Pledged', 'PercentFunded', 'Backers', 'Currency', 'LaunchedAt', 'Deadline'];
    const rows = data.items.map((project) => [
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
            if (typeof cell === 'string' && cell.includes(',')) {
              return `"${cell.replace(/"/g, '""')}"`;
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

  return (
    <Space direction="vertical" size={24} style={{ width: '100%' }}>
      <Card title="筛选条件">
        <ProjectFilters
          value={filters}
          onChange={handleFiltersChange}
          isLoading={isFetching || filtersLoading}
          options={filterOptions}
        />
      </Card>
      <Card
        title="项目列表"
        extra={
          <Space size="middle">
            {stats}
            <Button onClick={handleExport}>导出 CSV</Button>
          </Space>
        }
      >
        <Table<Project>
          columns={columns}
          dataSource={data?.items ?? []}
          rowKey={(record) => record.id}
          loading={isFetching}
          pagination={{
            current: filters.page,
            pageSize: filters.pageSize,
            total: data?.total ?? 0,
            onChange: handlePageChange,
            showSizeChanger: true,
            pageSizeOptions: PAGE_SIZE_OPTIONS.map(String),
          }}
        />
      </Card>
    </Space>
  );
};
