<template>
  <div class="page">
    <n-card size="small" :bordered="false" class="overview-card">
      <n-space justify="space-between" align="center" wrap>
        <n-space :size="16" wrap>
          <n-statistic
            v-for="metric in summaryCards"
            :key="metric.key"
            :label="metric.label"
            :value="metric.value"
          />
        </n-space>
        <n-space :size="8">
          <n-button secondary :loading="loading" @click="refresh">刷新</n-button>
          <n-button tertiary :disabled="!tableData.length" @click="exportCsv">导出 CSV</n-button>
        </n-space>
      </n-space>
      <div class="sub-info">
        <span>最新数据：</span>
        <span>{{ lastUpdatedLabel }}</span>
      </div>
    </n-card>

    <n-card size="small" :bordered="false">
      <n-space :size="12" wrap>
        <n-segmented v-model:value="filters.issueType" :options="issueTypeSegments" />
        <n-segmented v-model:value="filters.severity" :options="severitySegments" />
        <n-input v-model:value="filters.search" placeholder="ASIN / 商品名" clearable style="width: 240px" />
        <n-button text :disabled="loading" @click="resetFilters">重置</n-button>
      </n-space>
    </n-card>

    <n-card class="mt-12" size="small" :bordered="false">
      <n-spin :show="loading">
        <n-data-table
          size="small"
          :columns="columns"
          :data="tableData"
          :row-key="rowKey"
          :pagination="false"
          :scroll-x="980"
        />
      </n-spin>
      <div class="table-footer">
        <n-pagination
          :page="filters.page"
          :page-size="filters.pageSize"
          :page-sizes="[20, 50, 100]"
          :item-count="total"
          show-size-picker
          @update:page="handlePageChange"
          @update:page-size="handlePageSizeChange"
        />
      </div>
    </n-card>

    <n-drawer v-model:show="detailVisible" width="420">
      <n-drawer-content :title="activeIssue?.title ?? '风险详情'">
        <n-descriptions v-if="activeIssue" label-placement="left" :column="1">
          <n-descriptions-item label="ASIN">
            <n-text copyable>{{ activeIssue.asin }}</n-text>
          </n-descriptions-item>
          <n-descriptions-item label="问题类型">{{ issueTypeLabel[activeIssue.issueType] }}</n-descriptions-item>
          <n-descriptions-item label="严重度">{{ severityLabel[activeIssue.severity] }}</n-descriptions-item>
          <n-descriptions-item label="采集时间">
            {{ dayjs(activeIssue.capturedAt).format('YYYY-MM-DD HH:mm:ss') }}
          </n-descriptions-item>
          <n-descriptions-item label="建议动作">{{ activeIssue.recommendation }}</n-descriptions-item>
        </n-descriptions>
      </n-drawer-content>
    </n-drawer>
  </div>
</template>

<script setup lang="ts">
import { computed, h, onMounted, reactive, ref, watch } from 'vue';
import type { DataTableColumns } from 'naive-ui';
import { useDebounceFn } from '@vueuse/core';
import { useMessage } from 'naive-ui';
import dayjs from 'dayjs';
import { httpClient } from '@/api/http';

type IssueType = 'LowStock' | 'NegativeReview';
type Severity = 'High' | 'Medium' | 'Low';

type Issue = {
  asin: string;
  title: string;
  issueType: IssueType;
  severity: Severity;
  capturedAt: string;
  recommendation: string;
};

type IssuesResponse = {
  items: Issue[];
  total: number;
};

type OperationalSummary = {
  lastUpdatedAt?: string | null;
  lowStock: { total: number; high?: number; medium?: number; low?: number };
  negativeReview: { total: number; high?: number; medium?: number; low?: number };
  amazonFailures: number;
};

type SummaryCard = {
  key: string;
  label: string;
  value: string | number;
};

const message = useMessage();

const filters = reactive({
  issueType: 'ALL' as 'ALL' | IssueType,
  severity: 'ALL' as 'ALL' | Severity,
  search: '',
  page: 1,
  pageSize: 20,
});

const issues = ref<Issue[]>([]);
const total = ref(0);
const loading = ref(false);
const summary = ref<OperationalSummary | null>(null);
const summaryCards = ref<SummaryCard[]>([
  { key: 'stock', label: '库存风险', value: '-' },
  { key: 'review', label: '差评风险', value: '-' },
  { key: 'failure', label: '采集失败', value: '-' },
]);
const activeIssue = ref<Issue | null>(null);
const detailVisible = ref(false);

const issueTypeLabel: Record<IssueType, string> = {
  LowStock: '库存告警',
  NegativeReview: '差评风险',
};
const severityLabel: Record<Severity, string> = {
  High: '高',
  Medium: '中',
  Low: '低',
};

const severitySegments = [
  { label: '全部', value: 'ALL' },
  { label: '高风险', value: 'High' },
  { label: '中风险', value: 'Medium' },
  { label: '低风险', value: 'Low' },
];
const issueTypeSegments = [
  { label: '全部类型', value: 'ALL' },
  { label: '库存告警', value: 'LowStock' },
  { label: '差评风险', value: 'NegativeReview' },
];

const columns: DataTableColumns<Issue> = [
  {
    title: 'ASIN',
    key: 'asin',
    width: 160,
    render(row) {
      return h('span', { class: 'mono' }, row.asin);
    },
  },
  {
    title: '商品',
    key: 'title',
    minWidth: 220,
  },
  {
    title: '问题类型',
    key: 'issueType',
    width: 120,
    render(row) {
      return issueTypeLabel[row.issueType];
    },
  },
  {
    title: '严重度',
    key: 'severity',
    width: 100,
    render(row) {
      const typeMap: Record<Severity, 'success' | 'warning' | 'error'> = {
        High: 'error',
        Medium: 'warning',
        Low: 'success',
      };
      return h(
        'span',
        { class: ['severity', row.severity.toLowerCase()] },
        severityLabel[row.severity],
      );
    },
  },
  {
    title: '采集时间',
    key: 'capturedAt',
    width: 160,
    render(row) {
      return dayjs(row.capturedAt).format('YYYY-MM-DD HH:mm');
    },
  },
  {
    title: '建议动作',
    key: 'recommendation',
    minWidth: 240,
  },
  {
    title: '操作',
    key: 'actions',
    width: 100,
    align: 'center',
    render(row) {
      return h(
        'a',
        {
          class: 'action-link',
          onClick: () => openDetail(row),
        },
        '查看',
      );
    },
  },
];

const activeController = ref<AbortController | null>(null);

const fetchIssues = async () => {
  if (activeController.value) {
    activeController.value.abort();
  }
  const controller = new AbortController();
  activeController.value = controller;
  loading.value = true;
  try {
    const response = await httpClient.get<IssuesResponse>('/amazon/operations/issues', {
      signal: controller.signal,
      params: {
        issueType: filters.issueType === 'ALL' ? undefined : filters.issueType,
        severity: filters.severity === 'ALL' ? undefined : filters.severity,
        search: filters.search || undefined,
        page: filters.page,
        pageSize: filters.pageSize,
      },
    });
    issues.value = response.items ?? [];
    total.value = response.total ?? issues.value.length;
  } catch (error) {
    if ((error as { name?: string }).name !== 'CanceledError') {
      console.error(error);
      message.error('加载风险数据失败');
    }
  } finally {
    if (activeController.value === controller) {
      activeController.value = null;
    }
    loading.value = false;
  }
};

const fetchSummary = async () => {
  try {
    const response = await httpClient.get<OperationalSummary>('/amazon/operations/summary');
    summary.value = response;
    summaryCards.value = [
      {
        key: 'stock',
        label: '库存风险',
        value: formatNumber(response.lowStock.total ?? 0),
      },
      {
        key: 'review',
        label: '差评风险',
        value: formatNumber(response.negativeReview.total ?? 0),
      },
      {
        key: 'fail',
        label: '采集失败',
        value: formatNumber(response.amazonFailures ?? 0),
      },
    ];
  } catch (error) {
    console.error(error);
  }
};

const resetFilters = () => {
  filters.issueType = 'ALL';
  filters.severity = 'ALL';
  filters.search = '';
  filters.page = 1;
  filters.pageSize = 20;
  fetchIssues();
};

const exportCsv = () => {
  if (!tableData.value.length) {
    message.info('暂无数据可导出');
    return;
  }
  const headers = ['ASIN', '标题', '问题类型', '严重度', '采集时间', '建议'];
  const rows = tableData.value.map((item) => [
    item.asin,
    item.title,
    issueTypeLabel[item.issueType],
    severityLabel[item.severity],
    dayjs(item.capturedAt).format('YYYY-MM-DD HH:mm'),
    item.recommendation,
  ]);
  const csv = [headers, ...rows]
    .map((row) => row.map((cell) => String(cell ?? '')).join(','))
    .join('\n');
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', `amazon-operations-${Date.now()}.csv`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
};

const openDetail = (issue: Issue) => {
  activeIssue.value = issue;
  detailVisible.value = true;
};

const formatNumber = (value: number) => value.toLocaleString('en-US');

const tableData = computed(() => issues.value);
const rowKey = (row: Issue) => `${row.asin}-${row.issueType}`;

const debouncedFetch = useDebounceFn(() => {
  filters.page = 1;
  fetchIssues();
}, 300);

watch(
  () => [filters.issueType, filters.severity, filters.search],
  () => debouncedFetch(),
);

const handlePageChange = (page: number) => {
  filters.page = page;
  fetchIssues();
};

const handlePageSizeChange = (pageSize: number) => {
  filters.pageSize = pageSize;
  filters.page = 1;
  fetchIssues();
};

const refresh = () => {
  fetchSummary();
  fetchIssues();
};

const lastUpdatedLabel = computed(() => {
  if (!summary.value?.lastUpdatedAt) {
    return '暂无数据';
  }
  return dayjs(summary.value.lastUpdatedAt).format('YYYY-MM-DD HH:mm:ss');
});

onMounted(refresh);
</script>

<style scoped>
.page {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.overview-card {
  padding-bottom: 12px;
}
.mt-12 {
  margin-top: 12px;
}
.sub-info {
  margin-top: 12px;
  font-size: 12px;
  color: #6b7280;
}
.table-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}
.mono {
  font-family: 'JetBrains Mono', monospace;
}
.severity {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
}
.severity.high {
  color: #b91c1c;
  background: rgba(220, 38, 38, 0.12);
}
.severity.medium {
  color: #b45309;
  background: rgba(251, 191, 36, 0.12);
}
.severity.low {
  color: #047857;
  background: rgba(16, 185, 129, 0.12);
}
.action-link {
  color: #1f2937;
  font-size: 12px;
  cursor: pointer;
}
</style>
