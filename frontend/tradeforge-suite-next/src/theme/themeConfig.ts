import type { ThemeConfig } from 'antd';

export const themeConfig: ThemeConfig = {
  token: {
    colorPrimary: '#5b8ff9',
    colorBgBase: '#020617',
    colorTextBase: '#f8fafc',
    fontSize: 14,
    borderRadius: 8,
  },
  components: {
    Layout: {
      headerBg: 'rgba(2, 6, 23, 0.9)',
      siderBg: 'rgba(15, 23, 42, 0.9)',
      triggerBg: 'rgba(30, 41, 59, 0.7)',
    },
    Menu: {
      colorItemBg: 'transparent',
      colorItemText: 'rgba(248, 250, 252, 0.65)',
      colorItemTextSelected: '#f8fafc',
    },
    Card: {
      colorBgContainer: 'rgba(15, 23, 42, 0.85)',
      colorBorderSecondary: 'rgba(148, 163, 184, 0.2)',
      borderRadiusLG: 16,
    },
  },
};
