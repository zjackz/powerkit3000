import { message } from 'antd';

const fallbackNotified = new Set<string>();
const errorNotified = new Map<string, number>();

const COOLDOWN_MS = 30_000;

const getLabel = (label: string) => label.trim() || '未知模块';

export const notifyApiFallback = (label: string) => {
  const normalized = getLabel(label);
  if (fallbackNotified.has(normalized)) {
    return;
  }
  fallbackNotified.add(normalized);
  message.warning(`${normalized}：后端暂不可用，已切换为演示数据。`);
};

export const notifyApiError = (label: string, error: unknown) => {
  const normalized = getLabel(label);
  const now = Date.now();
  const last = errorNotified.get(normalized) ?? 0;
  if (now - last < COOLDOWN_MS) {
    return;
  }
  errorNotified.set(normalized, now);
  const detail = error instanceof Error ? error.message : String(error);
  message.error(`${normalized} 加载失败：${detail}`);
};

export const resetApiNotifications = () => {
  fallbackNotified.clear();
  errorNotified.clear();
};
