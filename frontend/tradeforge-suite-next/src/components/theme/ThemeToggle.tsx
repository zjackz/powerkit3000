'use client';

import { Button, Tooltip } from 'antd';
import { MoonFilled, SunFilled } from '@ant-design/icons';
import { useThemeMode } from '@/contexts/ThemeContext';

export const ThemeToggle = () => {
  const { mode, toggleMode } = useThemeMode();
  const isDark = mode === 'dark';

  return (
    <Tooltip title={isDark ? '切换到浅色主题' : '切换到深色主题'}>
      <Button
        type="text"
        shape="circle"
        aria-label={isDark ? '切换到浅色主题' : '切换到深色主题'}
        icon={isDark ? <SunFilled style={{ color: '#facc15' }} /> : <MoonFilled style={{ color: '#2563eb' }} />}
        onClick={toggleMode}
      />
    </Tooltip>
  );
};
