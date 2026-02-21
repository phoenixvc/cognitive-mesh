import { Activity, Agent, Layer, Metric, SystemStatus, dashboardAPI } from '@/services/api';
import { useCallback, useEffect, useState } from 'react';

interface DashboardData {
  layers: Layer[];
  metrics: Metric[];
  agents: Agent[];
  activities: Activity[];
  systemStatus: SystemStatus;
}

interface UseDashboardDataReturn {
  data: DashboardData | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  refreshMetrics: () => Promise<void>;
  refreshActivities: () => Promise<void>;
}

export const useDashboardData = (): UseDashboardDataReturn => {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAllData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const [layers, metrics, agents, activities, systemStatus] = await Promise.all([
        dashboardAPI.getLayers(),
        dashboardAPI.getMetrics(),
        dashboardAPI.getAgents(),
        dashboardAPI.getActivities(),
        dashboardAPI.getSystemStatus(),
      ]);

      setData({
        layers,
        metrics,
        agents,
        activities,
        systemStatus,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch dashboard data');
    } finally {
      setLoading(false);
    }
  }, []);

  const refreshMetrics = useCallback(async () => {
    try {
      const metrics = await dashboardAPI.getMetrics();
      setData(prev => prev ? { ...prev, metrics } : null);
    } catch (err) {
      console.error('Failed to refresh metrics:', err);
    }
  }, []);

  const refreshActivities = useCallback(async () => {
    try {
      const activities = await dashboardAPI.getActivities();
      setData(prev => prev ? { ...prev, activities } : null);
    } catch (err) {
      console.error('Failed to refresh activities:', err);
    }
  }, []);

  useEffect(() => {
    fetchAllData();
  }, [fetchAllData]);

  return {
    data,
    loading,
    error,
    refetch: fetchAllData,
    refreshMetrics,
    refreshActivities,
  };
}; 