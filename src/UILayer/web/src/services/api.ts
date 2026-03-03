// API Service Layer for Cognitive Mesh Dashboard
// Stub implementations that can be replaced with real API calls

export interface Layer {
  id: string;
  name: string;
  icon: string; // Icon name that can be mapped to component
  color: string;
  uptime: number;
  description: string;
}

export interface Metric {
  id: string;
  label: string;
  value: string;
  change: string;
  status: "up" | "stable" | "down";
  energy: number;
  icon: string;
}

export interface Agent {
  name: string;
  status: "active" | "idle";
  tasks: number;
  energy: number;
}

export interface Activity {
  time: string;
  event: string;
  type: string;
}

export interface SystemStatus {
  power: number;
  load: number;
  neuralNetwork: boolean;
  quantumProcessing: boolean;
}

// Simulate API delay
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export class DashboardAPI {
  private static instance: DashboardAPI;
  
  private constructor() {}
  
  static getInstance(): DashboardAPI {
    if (!DashboardAPI.instance) {
      DashboardAPI.instance = new DashboardAPI();
    }
    return DashboardAPI.instance;
  }

  async getLayers(): Promise<Layer[]> {
    await delay(300); // Simulate network delay
    return [
      {
        id: "foundation",
        name: "Foundation Layer",
        icon: "Shield",
        color: "cyan",
        uptime: 99.9,
        description: "Core infrastructure, security, and data persistence",
      },
      {
        id: "reasoning",
        name: "Reasoning Layer",
        icon: "Brain",
        color: "blue",
        uptime: 94.2,
        description: "Cognitive engines for analytical and creative reasoning",
      },
      {
        id: "metacognitive",
        name: "Metacognitive Layer",
        icon: "Eye",
        color: "purple",
        uptime: 87.5,
        description: "Self-monitoring and continuous learning systems",
      },
      {
        id: "agency",
        name: "Agency Layer",
        icon: "Users",
        color: "green",
        uptime: 91.8,
        description: "Autonomous agents executing tasks and workflows",
      },
      {
        id: "business",
        name: "Business Applications",
        icon: "BarChart3",
        color: "orange",
        uptime: 96.3,
        description: "Business-specific APIs and application logic",
      },
    ];
  }

  async getMetrics(): Promise<Metric[]> {
    await delay(200);
    return [
      {
        id: "active-agents",
        label: "Active Agents",
        value: "247",
        change: "+12%",
        status: "up",
        energy: 0.8,
        icon: "Users",
      },
      {
        id: "processing-rate",
        label: "Processing Rate",
        value: "1.2M/s",
        change: "+5.3%",
        status: "up",
        energy: 0.9,
        icon: "Cpu",
      },
      {
        id: "security-score",
        label: "Security Score",
        value: "99.8%",
        change: "0%",
        status: "stable",
        energy: 0.6,
        icon: "Shield",
      },
      {
        id: "compliance",
        label: "Compliance",
        value: "100%",
        change: "0%",
        status: "stable",
        energy: 0.5,
        icon: "CheckCircle",
      },
    ];
  }

  async getAgents(): Promise<Agent[]> {
    await delay(250);
    return [
      { name: "Threat Intelligence Agent", status: "active", tasks: 12, energy: 0.9 },
      { name: "Data Processing Agent", status: "active", tasks: 8, energy: 0.7 },
      { name: "Compliance Monitor", status: "active", tasks: 3, energy: 0.4 },
      { name: "Performance Optimizer", status: "idle", tasks: 0, energy: 0.1 },
      { name: "Security Auditor", status: "active", tasks: 5, energy: 0.6 },
    ];
  }

  async getActivities(): Promise<Activity[]> {
    await delay(150);
    return [
      { time: "2 min ago", event: "Security scan completed", type: "security" },
      { time: "5 min ago", event: "New agent deployed", type: "deployment" },
      { time: "12 min ago", event: "Performance optimization applied", type: "optimization" },
      { time: "18 min ago", event: "Compliance check passed", type: "compliance" },
      { time: "25 min ago", event: "Data backup completed", type: "backup" },
    ];
  }

  async getSystemStatus(): Promise<SystemStatus> {
    await delay(100);
    return {
      power: 98,
      load: 67,
      neuralNetwork: true,
      quantumProcessing: true,
    };
  }

  // Real-time updates (for future WebSocket integration)
  async subscribeToUpdates(callback: (data: any) => void): Promise<() => void> {
    // Simulate real-time updates
    const interval = setInterval(async () => {
      const updates = await this.getSystemStatus();
      callback(updates);
    }, 5000);

    return () => clearInterval(interval);
  }
}

// Export singleton instance
export const dashboardAPI = DashboardAPI.getInstance(); 