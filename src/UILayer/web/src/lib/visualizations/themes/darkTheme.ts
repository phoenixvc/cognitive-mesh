import { VisualizationTheme } from '../types/visualization';

/**
 * Dark mode visualization theme.
 * Uses muted backgrounds with high-contrast foreground colors to ensure
 * readability in dark UI contexts while maintaining WCAG 2.1 AA contrast ratios.
 */
export const darkTheme: VisualizationTheme = {
  background: '#1e1e2e',
  text: '#e0e0e0',
  grid: '#3a3a4a',
  primary: '#87ceeb',
  secondary: '#a0a0b0',
  success: '#4ade80',
  warning: '#fbbf24',
  error: '#f87171',
};
