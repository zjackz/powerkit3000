'use client';

import {
  ReactNode,
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';
import { ConfigProvider, theme as antdTheme } from 'antd';
import zhCN from 'antd/locale/zh_CN';
import { StyleProvider, legacyLogicalPropertiesTransformer } from '@ant-design/cssinjs';
import { darkThemeConfig, lightThemeConfig } from '@/theme/themeConfig';

type ThemeMode = 'dark' | 'light';

interface ThemeContextValue {
  mode: ThemeMode;
  toggleMode: () => void;
  setMode: (mode: ThemeMode) => void;
  primaryColor: string;
  setPrimaryColor: (color: string) => void;
}

const STORAGE_KEY_MODE = 'tf-theme-mode';
const STORAGE_KEY_PRIMARY = 'tf-theme-primary';

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

interface ThemeProviderProps {
  children: ReactNode;
}

export const ThemeProvider = ({ children }: ThemeProviderProps) => {
  const [mode, setModeState] = useState<ThemeMode>('dark');
  const [primaryColor, setPrimaryColorState] = useState<string>('#38bdf8');

  useEffect(() => {
    if (typeof window === 'undefined') {
      return;
    }
    const storedMode = window.localStorage.getItem(STORAGE_KEY_MODE);
    if (storedMode === 'dark' || storedMode === 'light') {
      setModeState(storedMode);
    }
    const storedPrimary = window.localStorage.getItem(STORAGE_KEY_PRIMARY);
    if (storedPrimary) {
      setPrimaryColorState(storedPrimary);
    }
  }, []);

  const setMode = useCallback((next: ThemeMode) => {
    setModeState(next);
    if (typeof window !== 'undefined') {
      window.localStorage.setItem(STORAGE_KEY_MODE, next);
    }
  }, []);

  const toggleMode = useCallback(() => {
    setMode(mode === 'dark' ? 'light' : 'dark');
  }, [mode, setMode]);

  const setPrimaryColor = useCallback((color: string) => {
    setPrimaryColorState(color);
    if (typeof window !== 'undefined') {
      window.localStorage.setItem(STORAGE_KEY_PRIMARY, color);
    }
  }, []);

  const themeConfig = useMemo(() => {
    const base = mode === 'dark' ? darkThemeConfig : lightThemeConfig;
    return {
      ...base,
      token: {
        ...base.token,
        colorPrimary: primaryColor,
      },
      algorithm:
        mode === 'dark'
          ? [antdTheme.darkAlgorithm, antdTheme.compactAlgorithm]
          : [antdTheme.defaultAlgorithm, antdTheme.compactAlgorithm],
    };
  }, [mode, primaryColor]);

  const contextValue = useMemo<ThemeContextValue>(
    () => ({ mode, toggleMode, setMode, primaryColor, setPrimaryColor }),
    [mode, toggleMode, setMode, primaryColor, setPrimaryColor],
  );

  return (
    <ThemeContext.Provider value={contextValue}>
      <StyleProvider transformers={[legacyLogicalPropertiesTransformer]} hashPriority="high">
        <ConfigProvider locale={zhCN} theme={themeConfig} componentSize="middle">
          {children}
        </ConfigProvider>
      </StyleProvider>
    </ThemeContext.Provider>
  );
};

export const useThemeMode = (): ThemeContextValue => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useThemeMode must be used within ThemeProvider');
  }
  return context;
};
