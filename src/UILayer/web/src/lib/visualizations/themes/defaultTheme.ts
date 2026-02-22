import { VisualizationTheme } from '../types/visualization';

/**
 * Light mode visualization theme.
 * Optimized for readability on white backgrounds with sufficient contrast
 * to meet WCAG 2.1 AA guidelines for non-text elements.
 */
export const defaultTheme: VisualizationTheme = {
  background: '#ffffff',
  text: '#333333',
  grid: '#e0e0e0',
  primary: '#005a9e',
  secondary: '#6c757d',
  success: '#28a745',
  warning: '#f59e0b',
  error: '#dc3545',
};
