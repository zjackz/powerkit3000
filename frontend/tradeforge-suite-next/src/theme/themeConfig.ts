import type { ThemeConfig } from 'antd';

export const darkThemeConfig: ThemeConfig = {
  token: {
    colorPrimary: '#38bdf8',
    colorBgBase: '#0f172a',
    colorTextBase: '#e2e8f0',
    colorTextSecondary: 'rgba(148, 163, 184, 0.85)',
    fontSize: 14,
    borderRadius: 10,
  },
  components: {
    Layout: {
      headerBg: 'rgba(8, 15, 35, 0.92)',
      siderBg: 'rgba(11, 23, 42, 0.95)',
      triggerBg: 'rgba(30, 64, 89, 0.7)',
    },
    Menu: {
      colorItemBg: 'transparent',
      colorItemText: 'rgba(226, 232, 240, 0.7)',
      colorItemTextSelected: '#f8fafc',
      colorItemBgSelected: 'rgba(56, 189, 248, 0.16)',
    },
    Card: {
      colorBgContainer: 'rgba(16, 27, 49, 0.88)',
      colorBorderSecondary: 'rgba(51, 65, 85, 0.35)',
      borderRadiusLG: 18,
    },
  },
};

export const lightThemeConfig: ThemeConfig = {
  token: {
    colorPrimary: '#2563eb',
    colorBgBase: '#f8fafc',
    colorTextBase: '#0f172a',
    colorTextSecondary: 'rgba(71, 85, 105, 0.88)',
    fontSize: 14,
    borderRadius: 10,
  },
  components: {
    Layout: {
      headerBg: '#ffffffdd',
      siderBg: '#ffffff',
      triggerBg: '#e2e8f0',
    },
    Menu: {
      colorItemBg: 'transparent',
      colorItemText: 'rgba(30, 41, 59, 0.75)',
      colorItemTextSelected: '#1d4ed8',
      colorItemBgSelected: 'rgba(37, 99, 235, 0.12)',
    },
    Card: {
      colorBgContainer: '#ffffff',
      colorBorderSecondary: 'rgba(148, 163, 184, 0.12)',
      borderRadiusLG: 18,
    },
  },
};
