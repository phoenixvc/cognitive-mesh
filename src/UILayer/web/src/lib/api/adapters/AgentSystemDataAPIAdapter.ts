import { v4 as uuidv4 } from 'uuid';

// --- Type Definitions (assuming these are in a shared models file) ---

interface ErrorEnvelope {
  errorCode: string;
  message: string;
  correlationId?: string;
  details?: any;
  canRetry?: boolean;
}

interface WidgetState<T> {
  data: T | null;
  isStale: boolean;
  lastSyncTimestamp: Date;
  lastError: ErrorEnvelope | null;
}

interface AgentViewModel {
  agentId: string;
  agentType: string;
  description: string;
  status: 'Active' | 'Deprecated' | 'Retired';
  version: string;
  defaultAutonomy: 'RecommendOnly' | 'ActWithConfirmation' | 'FullyAutonomous';
}

interface AuthorityScopeViewModel {
  allowedApiEndpoints: string[];
  maxResourceConsumption?: number;
  maxBudget?: number;
  dataAccessPolicies: string[];
}

interface AuditEventViewModel {
  auditId: string;
  agentId?: string;
  actionType: string;
  userId?: string;
  timestamp: Date;
  outcome: 'Success' | 'Failure' | 'Escalated' | 'Info';
  correlationId?: string;
  eventData: any;
}

// Assume other view models like AgencyModeStatus, PolicyDecision, etc., are defined elsewhere
// We will stub the methods that use them.
interface AgencyModeStatus {}
interface PolicyDecision {}
interface ValueDiagnosticViewModel {}
interface OrgBlindnessTrendViewModel {}
interface EmployabilityScoreViewModel {}

// --- Abstract Interface (from IAgencyWidgetAdapters.cs) ---

interface IDataAPIAdapterPort {
  getAgentRegistryAsync(includeRetired: boolean, tenantId: string): Promise<WidgetState<AgentViewModel[]>>;
  getAgentDetailsAsync(agentId: string, tenantId: string): Promise<WidgetState<AgentViewModel>>;
  getAuthorityScopeAsync(agentId: string, tenantId: string): Promise<WidgetState<AuthorityScopeViewModel>>;
  updateAuthorityScopeAsync(agentId: string, newScope: AuthorityScopeViewModel, tenantId: string, reason: string): Promise<boolean>;
  overrideAuthorityAsync(agentId: string, overrideScope: AuthorityScopeViewModel, duration: number, tenantId: string, reason: string): Promise<boolean>;
  validateActionAuthorityAsync(agentId: string, actionType: string, parameters: Record<string, any>, tenantId: string): Promise<boolean>;
  getAgentAuditEventsAsync(agentId: string | undefined, since: Date | undefined, pageSize: number, tenantId: string, correlationId?: string, eventType?: string, userId?: string): Promise<WidgetState<AuditEventViewModel[]>>;
  
  // Stubbed methods not relevant to this adapter
  getAgencyModeStatusAsync(taskId: string): Promise<WidgetState<AgencyModeStatus>>;
  getPolicyDecisionTableAsync(taskId: string): Promise<WidgetState<PolicyDecision[]>>;
  getValueDiagnosticDataAsync(targetId: string, targetType: string, tenantId: string): Promise<WidgetState<ValueDiagnosticViewModel>>;
  getOrgBlindnessTrendsAsync(organizationId: string, departmentFilters: string[], tenantId: string): Promise<WidgetState<OrgBlindnessTrendViewModel>>;
  getEmployabilityScoreAsync(userId: string, tenantId: string): Promise<WidgetState<EmployabilityScoreViewModel>>;
  submitTwoHundredDollarTestAsync(userId: string, responses: Record<string, any>, tenantId: string): Promise<boolean>;
}

// --- Mock OpenAPI Client ---
// In a real application, this would be auto-generated from the agentic-ai.yaml spec.

class MockAgenticSystemApiClient {
  private async simulateRequest<T>(data: T, shouldFail: boolean = false): Promise<T> {
    await new Promise(resolve => setTimeout(resolve, 500 + Math.random() * 1000));
    if (shouldFail) {
      throw new Error('Simulated API failure.');
    }
    return data;
  }

  async listAgents(includeRetired: boolean, headers: Record<string, string>): Promise<AgentViewModel[]> {
    const mockAgents: AgentViewModel[] = [
        { agentId: 'agent-001', agentType: 'ChampionNudger', description: 'Nudges potential champions.', status: 'Active', version: '1.2.0', defaultAutonomy: 'ActWithConfirmation' },
        { agentId: 'agent-002', agentType: 'VelocityRecalibrator', description: 'Suggests project recalibrations.', status: 'Active', version: '1.0.5', defaultAutonomy: 'RecommendOnly' },
        { agentId: 'agent-003', agentType: 'DataCleansingBot', description: 'Cleans datasets.', status: 'Deprecated', version: '0.9.0', defaultAutonomy: 'FullyAutonomous' },
        { agentId: 'agent-004', agentType: 'ComplianceAuditor', description: 'Performs compliance checks.', status: 'Retired', version: '1.0.0', defaultAutonomy: 'ActWithConfirmation' },
    ];
    const data = includeRetired ? mockAgents : mockAgents.filter(a => a.status !== 'Retired');
    return this.simulateRequest(data);
  }
  
  async getAgentDetails(agentId: string, headers: Record<string, string>): Promise<AgentViewModel> {
      const agent = (await this.listAgents(true, headers)).find(a => a.agentId === agentId);
      if (!agent) throw new Error('Agent not found');
      return this.simulateRequest(agent);
  }

  async getAuthorityScope(agentId: string, headers: Record<string, string>): Promise<AuthorityScopeViewModel> {
      const scope: AuthorityScopeViewModel = {
          allowedApiEndpoints: ['/data/read', '/report/generate'],
          maxResourceConsumption: 100,
          maxBudget: 500,
          dataAccessPolicies: ['read:public', 'read:operational-data'],
      };
      return this.simulateRequest(scope);
  }
  
  async updateAuthorityScope(agentId: string, body: any, headers: Record<string, string>): Promise<void> {
      await this.simulateRequest(undefined);
  }
  
  async getAuditEvents(params: any, headers: Record<string, string>): Promise<AuditEventViewModel[]> {
      const events: AuditEventViewModel[] = [
          { auditId: uuidv4(), agentId: params.agentId || 'agent-001', actionType: 'AgentActionExecuted', userId: 'user-x', timestamp: new Date(), outcome: 'Success', correlationId: uuidv4(), eventData: { action: 'GetData' } },
          { auditId: uuidv4(), agentId: params.agentId || 'agent-001', actionType: 'AuthorityOverridden', userId: 'admin-y', timestamp: new Date(Date.now() - 3600000), outcome: 'Escalated', correlationId: uuidv4(), eventData: { reason: 'Emergency fix' } },
      ];
      return this.simulateRequest(events.slice(0, params.pageSize));
  }
}


// --- Concrete Adapter Implementation ---

export class AgentSystemDataAPIAdapter implements IDataAPIAdapterPort {
  private readonly apiClient: MockAgenticSystemApiClient;
  private readonly logger: Console; // Replace with a proper logger interface
  private readonly authTokenProvider: () => Promise<string>;
  private readonly maxRetries = 3;

  constructor(
    logger: Console,
    authTokenProvider: () => Promise<string>
  ) {
    this.apiClient = new MockAgenticSystemApiClient();
    this.logger = logger;
    this.authTokenProvider = authTokenProvider;
  }

  private async getAuthHeaders(correlationId: string): Promise<Record<string, string>> {
    const token = await this.authTokenProvider();
    return {
      'Authorization': `Bearer ${token}`,
      'X-Correlation-ID': correlationId,
      'Content-Type': 'application/json',
    };
  }

  private handleApiError(error: any, correlationId: string): ErrorEnvelope {
    this.logger.error(`API Error (Correlation ID: ${correlationId}):`, error);
    return {
      errorCode: error.name === 'AbortError' ? 'TIMEOUT' : 'API_ERROR',
      message: error.message || 'An unexpected error occurred.',
      correlationId,
      canRetry: true,
    };
  }

  private async executeWithRetry<T>(
    apiCall: (headers: Record<string, string>) => Promise<T>,
    correlationId: string
  ): Promise<WidgetState<T>> {
    for (let attempt = 1; attempt <= this.maxRetries; attempt++) {
      try {
        const headers = await this.getAuthHeaders(correlationId);
        const data = await apiCall(headers);
        return {
          data,
          isStale: false,
          lastSyncTimestamp: new Date(),
          lastError: null,
        };
      } catch (error) {
        if (attempt === this.maxRetries) {
          this.logger.error(`API call failed after ${this.maxRetries} attempts.`);
          return {
            data: null,
            isStale: false,
            lastSyncTimestamp: new Date(),
            lastError: this.handleApiError(error, correlationId),
          };
        }
        const delay = Math.pow(2, attempt) * 100 + Math.random() * 100; // Exponential backoff with jitter
        this.logger.warn(`API call failed. Retrying in ${delay.toFixed(0)}ms... (Attempt ${attempt}/${this.maxRetries})`);
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }
    // This part should be unreachable, but TypeScript needs it for return type safety.
    return { data: null, isStale: false, lastSyncTimestamp: new Date(), lastError: this.handleApiError(new Error("Max retries exceeded."), correlationId) };
  }

  async getAgentRegistryAsync(includeRetired: boolean, tenantId: string): Promise<WidgetState<AgentViewModel[]>> {
    const correlationId = uuidv4();
    return this.executeWithRetry(
      (headers) => this.apiClient.listAgents(includeRetired, headers),
      correlationId
    );
  }

  async getAgentDetailsAsync(agentId: string, tenantId: string): Promise<WidgetState<AgentViewModel>> {
    const correlationId = uuidv4();
    return this.executeWithRetry(
      (headers) => this.apiClient.getAgentDetails(agentId, headers),
      correlationId
    );
  }

  async getAuthorityScopeAsync(agentId: string, tenantId: string): Promise<WidgetState<AuthorityScopeViewModel>> {
    const correlationId = uuidv4();
    return this.executeWithRetry(
      (headers) => this.apiClient.getAuthorityScope(agentId, headers),
      correlationId
    );
  }

  async updateAuthorityScopeAsync(agentId: string, newScope: AuthorityScopeViewModel, tenantId: string, reason: string): Promise<boolean> {
    const correlationId = uuidv4();
    try {
      const headers = await this.getAuthHeaders(correlationId);
      await this.apiClient.updateAuthorityScope(agentId, { scope: newScope, reason }, headers);
      return true;
    } catch (error) {
      this.handleApiError(error, correlationId);
      return false;
    }
  }
  
  async overrideAuthorityAsync(agentId: string, overrideScope: AuthorityScopeViewModel, duration: number, tenantId: string, reason: string): Promise<boolean> {
      // This would call the backend's override endpoint.
      this.logger.warn("overrideAuthorityAsync is not fully implemented in this mock.");
      return Promise.resolve(true);
  }
  
  async validateActionAuthorityAsync(agentId: string, actionType: string, parameters: Record<string, any>, tenantId: string): Promise<boolean> {
      // This would call the backend's validation endpoint.
      this.logger.warn("validateActionAuthorityAsync is not fully implemented in this mock.");
      return Promise.resolve(true);
  }

  async getAgentAuditEventsAsync(agentId: string | undefined, since: Date | undefined, pageSize: number, tenantId: string, correlationId?: string, eventType?: string, userId?: string): Promise<WidgetState<AuditEventViewModel[]>> {
    const requestCorrelationId = correlationId || uuidv4();
    return this.executeWithRetry(
      (headers) => this.apiClient.getAuditEvents({ agentId, since, pageSize, tenantId, eventType, userId }, headers),
      requestCorrelationId
    );
  }

  // --- Stubbed Methods ---
  private async notImplemented<T>(): Promise<WidgetState<T>> {
    return Promise.resolve({
      data: null,
      isStale: false,
      lastSyncTimestamp: new Date(),
      lastError: {
        errorCode: 'NOT_IMPLEMENTED',
        message: 'This method is not implemented by the AgentSystemDataAPIAdapter.',
        canRetry: false
      }
    });
  }

  getAgencyModeStatusAsync(taskId: string): Promise<WidgetState<AgencyModeStatus>> { return this.notImplemented<AgencyModeStatus>(); }
  getPolicyDecisionTableAsync(taskId: string): Promise<WidgetState<PolicyDecision[]>> { return this.notImplemented<PolicyDecision[]>(); }
  getValueDiagnosticDataAsync(targetId: string, targetType: string, tenantId: string): Promise<WidgetState<ValueDiagnosticViewModel>> { return this.notImplemented<ValueDiagnosticViewModel>(); }
  getOrgBlindnessTrendsAsync(organizationId: string, departmentFilters: string[], tenantId: string): Promise<WidgetState<OrgBlindnessTrendViewModel>> { return this.notImplemented<OrgBlindnessTrendViewModel>(); }
  getEmployabilityScoreAsync(userId: string, tenantId: string): Promise<WidgetState<EmployabilityScoreViewModel>> { return this.notImplemented<EmployabilityScoreViewModel>(); }
  submitTwoHundredDollarTestAsync(userId: string, responses: Record<string, any>, tenantId: string): Promise<boolean> { return Promise.resolve(false); }
}
