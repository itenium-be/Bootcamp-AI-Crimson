import { create } from 'zustand';
import { persist } from 'zustand/middleware';

type Theme = 'light' | 'dark' | 'system';

interface ThemeState {
  theme: Theme;
  resolvedTheme: 'light' | 'dark';
  setTheme: (theme: Theme) => void;
}

function getSystemTheme(): 'light' | 'dark' {
  if (typeof window === 'undefined') return 'light';
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function resolveTheme(theme: Theme): 'light' | 'dark' {
  if (theme === 'system') {
    return getSystemTheme();
  }
  return theme;
}

function applyTheme(resolvedTheme: 'light' | 'dark') {
  const root = window.document.documentElement;
  root.classList.remove('light', 'dark');
  root.classList.add(resolvedTheme);
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set, get) => ({
      theme: 'system',
      resolvedTheme: getSystemTheme(),

      setTheme: (theme: Theme) => {
        const resolvedTheme = resolveTheme(theme);
        applyTheme(resolvedTheme);
        set({ theme, resolvedTheme });
      },
    }),
    {
      name: 'theme-storage',
      onRehydrateStorage: () => (state) => {
        if (state) {
          const resolvedTheme = resolveTheme(state.theme);
          applyTheme(resolvedTheme);
          state.resolvedTheme = resolvedTheme;
        }
      },
    }
  )
);

// Listen for system theme changes
if (typeof window !== 'undefined') {
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
    const state = useThemeStore.getState();
    if (state.theme === 'system') {
      const resolvedTheme = getSystemTheme();
      applyTheme(resolvedTheme);
      useThemeStore.setState({ resolvedTheme });
    }
  });
}
