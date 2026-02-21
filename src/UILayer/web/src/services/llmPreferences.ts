// LLM Model Preferences Service
// Manages polyglot model selection per use case, API key configuration, and defaults

export type LLMUseCase =
  | "GeneralReasoning"
  | "StructuredReasoning"
  | "CodeGeneration"
  | "Embeddings"
  | "EthicalReasoning"
  | "CreativeWriting"
  | "DocumentAnalysis"
  | "ChatConversation"
  | "ThreatIntelligence"
  | "AgentOrchestration";

export const LLM_USE_CASE_META: Record<
  LLMUseCase,
  { label: string; description: string; icon: string }
> = {
  GeneralReasoning: {
    label: "General Reasoning",
    description: "Default fallback for analysis and general-purpose tasks",
    icon: "Brain",
  },
  StructuredReasoning: {
    label: "Structured Reasoning",
    description:
      "ConclAIve debate, sequential, and strategic simulation engines",
    icon: "GitBranch",
  },
  CodeGeneration: {
    label: "Code Generation",
    description: "Writing, reviewing, and refactoring code",
    icon: "Code",
  },
  Embeddings: {
    label: "Embeddings",
    description: "Vector embeddings for semantic search and RAG retrieval",
    icon: "Layers",
  },
  EthicalReasoning: {
    label: "Ethical Reasoning",
    description:
      "Brandom inferentialism + Floridi information ethics evaluation",
    icon: "Scale",
  },
  CreativeWriting: {
    label: "Creative Writing",
    description: "Content generation, brainstorming, and copywriting",
    icon: "Pen",
  },
  DocumentAnalysis: {
    label: "Document Analysis",
    description: "Long-context document summarization and extraction",
    icon: "FileText",
  },
  ChatConversation: {
    label: "Chat / Conversation",
    description: "Interactive real-time dialogue and Q&A",
    icon: "MessageSquare",
  },
  ThreatIntelligence: {
    label: "Threat Intelligence",
    description: "Security analysis and threat assessment",
    icon: "Shield",
  },
  AgentOrchestration: {
    label: "Agent Orchestration",
    description: "Multi-agent planning and task decomposition",
    icon: "Network",
  },
};

export interface LLMModelInfo {
  key: string;
  displayName: string;
  description: string;
  provider: string;
  pros: string[];
  cons: string[];
  recommendedUseCases: LLMUseCase[];
  supportsEmbeddings: boolean;
  supportsToolCalling: boolean;
  supportsVision: boolean;
  maxContextTokens: number;
  maxOutputTokens: number;
  requiresApiKey: boolean;
  defaultEndpoint: string;
  decisionScore: number;
}

export interface UserLLMPreferences {
  modelAssignments: Record<string, string>;
  providerApiKeys: Record<string, string>;
  providerEndpoints: Record<string, string>;
  llmSetupCompleted: boolean;
}

export interface ProviderConfig {
  key: string;
  displayName: string;
  requiresApiKey: boolean;
  defaultEndpoint: string;
  hasApiKey: boolean;
  customEndpoint: string;
}

const LLM_PREFERENCES_KEY = "cognitive-mesh-llm-preferences";

// ── Complete LLM Model Registry ──────────────────────────────────────────────

export const LLM_MODELS: LLMModelInfo[] = [
  // Anthropic
  {
    key: "claude-opus-4",
    displayName: "Claude Opus 4",
    description:
      "Anthropic's most capable model. Exceptional at complex reasoning, nuanced analysis, and long-form structured output.",
    provider: "anthropic",
    pros: [
      "Best-in-class reasoning",
      "200K context window",
      "Excellent instruction following",
      "Strong ethical guardrails",
      "Superior structured output",
    ],
    cons: [
      "Higher latency than smaller models",
      "Premium pricing",
      "No native embeddings",
    ],
    recommendedUseCases: [
      "StructuredReasoning",
      "EthicalReasoning",
      "DocumentAnalysis",
      "AgentOrchestration",
    ],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 200_000,
    maxOutputTokens: 32_000,
    requiresApiKey: true,
    defaultEndpoint: "https://api.anthropic.com",
    decisionScore: 4.65,
  },
  {
    key: "claude-sonnet-4",
    displayName: "Claude Sonnet 4",
    description:
      "Best balance of intelligence and speed. Ideal for most production workloads.",
    provider: "anthropic",
    pros: [
      "Excellent reasoning-to-cost ratio",
      "200K context window",
      "Fast response times",
      "Strong tool calling",
      "Good at code",
    ],
    cons: [
      "Slightly less capable than Opus on hardest tasks",
      "No native embeddings",
    ],
    recommendedUseCases: [
      "GeneralReasoning",
      "CodeGeneration",
      "ChatConversation",
      "ThreatIntelligence",
    ],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 200_000,
    maxOutputTokens: 16_000,
    requiresApiKey: true,
    defaultEndpoint: "https://api.anthropic.com",
    decisionScore: 4.42,
  },
  {
    key: "claude-haiku-4",
    displayName: "Claude Haiku 4",
    description:
      "Fastest Claude model. Near-instant responses for high-throughput workloads.",
    provider: "anthropic",
    pros: [
      "Very fast responses",
      "Low cost per token",
      "200K context window",
      "Good for classification/extraction",
      "Tool calling support",
    ],
    cons: [
      "Less capable on complex reasoning",
      "Shorter output limit",
      "May miss nuance",
    ],
    recommendedUseCases: ["ChatConversation", "CreativeWriting"],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 200_000,
    maxOutputTokens: 8_192,
    requiresApiKey: true,
    defaultEndpoint: "https://api.anthropic.com",
    decisionScore: 3.78,
  },
  // OpenAI
  {
    key: "gpt-4o",
    displayName: "GPT-4o",
    description:
      "OpenAI's flagship multimodal model with native vision, audio, and tool calling.",
    provider: "openai",
    pros: [
      "Strong multimodal capabilities",
      "128K context window",
      "Native vision + audio",
      "Fast for its capability tier",
      "Excellent tool calling",
    ],
    cons: [
      "128K context (less than Claude)",
      "Higher cost than GPT-4o-mini",
      "Can be verbose",
    ],
    recommendedUseCases: [
      "GeneralReasoning",
      "CodeGeneration",
      "ChatConversation",
    ],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 128_000,
    maxOutputTokens: 16_384,
    requiresApiKey: true,
    defaultEndpoint: "https://api.openai.com/v1",
    decisionScore: 4.18,
  },
  {
    key: "gpt-4o-mini",
    displayName: "GPT-4o Mini",
    description:
      "Cost-efficient OpenAI model for simpler tasks at a fraction of GPT-4o cost.",
    provider: "openai",
    pros: [
      "Very low cost",
      "Fast responses",
      "128K context",
      "Good for simple tasks",
      "Tool calling support",
    ],
    cons: [
      "Weaker on complex reasoning",
      "Less reliable on nuanced tasks",
      "Shorter output quality",
    ],
    recommendedUseCases: ["ChatConversation", "CreativeWriting"],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 128_000,
    maxOutputTokens: 16_384,
    requiresApiKey: true,
    defaultEndpoint: "https://api.openai.com/v1",
    decisionScore: 3.55,
  },
  {
    key: "o3",
    displayName: "OpenAI o3",
    description:
      "OpenAI's reasoning model with chain-of-thought. Excels at math, science, and multi-step problems.",
    provider: "openai",
    pros: [
      "Superior chain-of-thought reasoning",
      "Excellent at math/science",
      "Strong coding ability",
      "Thinks before answering",
      "200K context",
    ],
    cons: [
      "Slower due to thinking time",
      "Higher cost",
      "May overthink simple tasks",
      "Limited streaming",
    ],
    recommendedUseCases: [
      "StructuredReasoning",
      "CodeGeneration",
      "EthicalReasoning",
    ],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 200_000,
    maxOutputTokens: 100_000,
    requiresApiKey: true,
    defaultEndpoint: "https://api.openai.com/v1",
    decisionScore: 4.35,
  },
  {
    key: "text-embedding-3-large",
    displayName: "text-embedding-3-large",
    description:
      "OpenAI's best embedding model. 3072 dimensions with Matryoshka representation.",
    provider: "openai",
    pros: [
      "Best-in-class embeddings",
      "3072 dimensions (reducible)",
      "Matryoshka representation",
      "Excellent retrieval quality",
      "Low cost per token",
    ],
    cons: [
      "Embedding-only (no generation)",
      "Requires OpenAI API key",
      "Higher dimensional cost",
    ],
    recommendedUseCases: ["Embeddings"],
    supportsEmbeddings: true,
    supportsToolCalling: false,
    supportsVision: false,
    maxContextTokens: 8_191,
    maxOutputTokens: 0,
    requiresApiKey: true,
    defaultEndpoint: "https://api.openai.com/v1",
    decisionScore: 4.5,
  },
  // Google
  {
    key: "gemini-2.5-pro",
    displayName: "Gemini 2.5 Pro",
    description:
      "Google's most capable model with 1M token context. Excellent for massive documents.",
    provider: "google",
    pros: [
      "1M token context window",
      "Strong multimodal",
      "Native code execution",
      "Competitive reasoning",
      "Google ecosystem integration",
    ],
    cons: [
      "Variable availability",
      "Google Cloud dependency",
      "Occasional inconsistency on edge cases",
    ],
    recommendedUseCases: [
      "DocumentAnalysis",
      "GeneralReasoning",
      "CreativeWriting",
    ],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 1_000_000,
    maxOutputTokens: 65_536,
    requiresApiKey: true,
    defaultEndpoint: "https://generativelanguage.googleapis.com/v1beta",
    decisionScore: 4.22,
  },
  {
    key: "gemini-2.5-flash",
    displayName: "Gemini 2.5 Flash",
    description:
      "Speed-optimized Google model with 1M context for high-throughput workloads.",
    provider: "google",
    pros: [
      "1M token context",
      "Very fast responses",
      "Low cost",
      "Good multimodal",
      "Thinking mode available",
    ],
    cons: [
      "Less capable than Pro on hard tasks",
      "Google Cloud dependency",
      "Newer ecosystem",
    ],
    recommendedUseCases: ["ChatConversation", "DocumentAnalysis"],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 1_000_000,
    maxOutputTokens: 65_536,
    requiresApiKey: true,
    defaultEndpoint: "https://generativelanguage.googleapis.com/v1beta",
    decisionScore: 3.88,
  },
  // MiniMax
  {
    key: "minimax-m1",
    displayName: "MiniMax-M1",
    description:
      "MiniMax's reasoning model with hybrid chain-of-thought. Strong AIME/GPQA benchmarks at low cost.",
    provider: "minimax",
    pros: [
      "Competitive reasoning benchmarks",
      "Low cost per token",
      "Hybrid thinking mode",
      "Good math/science",
      "OpenAI-compatible API",
    ],
    cons: [
      "Smaller ecosystem",
      "Limited multimodal",
      "Newer provider",
      "Less enterprise support",
    ],
    recommendedUseCases: ["StructuredReasoning", "CodeGeneration"],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: false,
    maxContextTokens: 1_000_000,
    maxOutputTokens: 16_384,
    requiresApiKey: true,
    defaultEndpoint: "https://api.minimax.chat/v1",
    decisionScore: 3.72,
  },
  {
    key: "minimax-m2.5",
    displayName: "MiniMax-M2.5",
    description:
      "MiniMax's latest flagship (Feb 2025). Top-tier LiveCodeBench/MMLU-Pro scores with 1M context and tool calling.",
    provider: "minimax",
    pros: [
      "1M token context window",
      "Top-5 on LiveCodeBench/MMLU-Pro",
      "Native tool/function calling",
      "OpenAI-compatible API",
      "Very competitive pricing",
      "Strong at code generation",
    ],
    cons: [
      "Newer provider \u2014 less battle-tested",
      "Limited vision capabilities",
      "Smaller community",
      "Enterprise support still growing",
    ],
    recommendedUseCases: [
      "CodeGeneration",
      "GeneralReasoning",
      "AgentOrchestration",
      "StructuredReasoning",
    ],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: false,
    maxContextTokens: 1_000_000,
    maxOutputTokens: 32_000,
    requiresApiKey: true,
    defaultEndpoint: "https://api.minimax.chat/v1",
    decisionScore: 4.08,
  },
  // DeepSeek
  {
    key: "deepseek-r1",
    displayName: "DeepSeek-R1",
    description:
      "Open-weight reasoning model rivaling o1. MIT-licensed with chain-of-thought transparency.",
    provider: "deepseek",
    pros: [
      "Open-weight (MIT license)",
      "Strong math/code reasoning",
      "Chain-of-thought transparent",
      "Self-hostable",
      "Low API cost",
    ],
    cons: [
      "Slower due to reasoning chain",
      "128K context",
      "Newer provider",
      "Less enterprise support",
    ],
    recommendedUseCases: ["StructuredReasoning", "CodeGeneration"],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: false,
    maxContextTokens: 128_000,
    maxOutputTokens: 16_384,
    requiresApiKey: true,
    defaultEndpoint: "https://api.deepseek.com/v1",
    decisionScore: 3.85,
  },
  {
    key: "deepseek-v3",
    displayName: "DeepSeek-V3",
    description:
      "Cost-effective MoE model with strong multilingual and coding performance.",
    provider: "deepseek",
    pros: [
      "Very low cost",
      "MoE architecture (efficient)",
      "Strong multilingual",
      "Open weights available",
      "Good coding",
    ],
    cons: [
      "Less capable than R1 on hard reasoning",
      "128K context",
      "Variable availability",
    ],
    recommendedUseCases: [
      "GeneralReasoning",
      "ChatConversation",
      "CreativeWriting",
    ],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: false,
    maxContextTokens: 128_000,
    maxOutputTokens: 8_192,
    requiresApiKey: true,
    defaultEndpoint: "https://api.deepseek.com/v1",
    decisionScore: 3.58,
  },
  // Meta
  {
    key: "llama-4-maverick",
    displayName: "Llama 4 Maverick",
    description:
      "Meta's 400B MoE model (17B active). Best open-weight model for self-hosted enterprise.",
    provider: "meta",
    pros: [
      "Open-weight (self-hostable)",
      "400B MoE (17B active)",
      "1M context window",
      "Strong multilingual (12 languages)",
      "No API costs when self-hosted",
    ],
    cons: [
      "Requires significant GPU infrastructure",
      "No hosted API by default",
      "Community support only",
      "Complex deployment",
    ],
    recommendedUseCases: [
      "GeneralReasoning",
      "CodeGeneration",
      "ChatConversation",
    ],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 1_000_000,
    maxOutputTokens: 16_384,
    requiresApiKey: false,
    defaultEndpoint: "http://localhost:11434/v1",
    decisionScore: 3.92,
  },
  // xAI
  {
    key: "grok-3",
    displayName: "Grok 3",
    description:
      "xAI's flagship model. Strong reasoning with real-time data access and 'think' mode.",
    provider: "xai",
    pros: [
      "Strong reasoning benchmarks",
      "Real-time data access",
      "Think mode for deep analysis",
      "128K context",
      "Competitive pricing",
    ],
    cons: [
      "xAI platform dependency",
      "Newer ecosystem",
      "Less enterprise adoption",
      "API stability still maturing",
    ],
    recommendedUseCases: [
      "GeneralReasoning",
      "ThreatIntelligence",
      "CreativeWriting",
    ],
    supportsEmbeddings: false,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 128_000,
    maxOutputTokens: 16_384,
    requiresApiKey: true,
    defaultEndpoint: "https://api.x.ai/v1",
    decisionScore: 3.82,
  },
  // Azure OpenAI
  {
    key: "azure-openai",
    displayName: "Azure OpenAI Service",
    description:
      "Microsoft-hosted OpenAI models with enterprise SLA and VNet integration. Existing codebase integration.",
    provider: "azure-openai",
    pros: [
      "Enterprise SLA and compliance",
      "VNet/private endpoint support",
      "Content safety built-in",
      "Existing codebase integration",
      "Azure AD authentication",
    ],
    cons: [
      "Azure subscription required",
      "Model availability varies by region",
      "Higher cost than direct OpenAI",
      "Deployment provisioning delays",
    ],
    recommendedUseCases: [
      "GeneralReasoning",
      "CodeGeneration",
      "ChatConversation",
      "EthicalReasoning",
    ],
    supportsEmbeddings: true,
    supportsToolCalling: true,
    supportsVision: true,
    maxContextTokens: 128_000,
    maxOutputTokens: 16_384,
    requiresApiKey: true,
    defaultEndpoint: "",
    decisionScore: 4.12,
  },
];

// ── Provider metadata ────────────────────────────────────────────────────────

export const LLM_PROVIDERS: Record<
  string,
  { displayName: string; color: string }
> = {
  anthropic: { displayName: "Anthropic", color: "amber" },
  openai: { displayName: "OpenAI", color: "emerald" },
  google: { displayName: "Google", color: "blue" },
  minimax: { displayName: "MiniMax", color: "violet" },
  deepseek: { displayName: "DeepSeek", color: "cyan" },
  meta: { displayName: "Meta (Llama)", color: "indigo" },
  xai: { displayName: "xAI", color: "rose" },
  "azure-openai": { displayName: "Azure OpenAI", color: "sky" },
};

// ── Default assignments per profile ──────────────────────────────────────────

export type LLMProfile =
  | "balanced"
  | "performance"
  | "cost-optimized"
  | "open-source";

export const LLM_DEFAULT_ASSIGNMENTS: Record<
  LLMProfile,
  Record<LLMUseCase, string>
> = {
  balanced: {
    GeneralReasoning: "claude-sonnet-4",
    StructuredReasoning: "claude-opus-4",
    CodeGeneration: "minimax-m2.5",
    Embeddings: "text-embedding-3-large",
    EthicalReasoning: "claude-opus-4",
    CreativeWriting: "gemini-2.5-flash",
    DocumentAnalysis: "gemini-2.5-pro",
    ChatConversation: "claude-haiku-4",
    ThreatIntelligence: "claude-sonnet-4",
    AgentOrchestration: "minimax-m2.5",
  },
  performance: {
    GeneralReasoning: "claude-opus-4",
    StructuredReasoning: "claude-opus-4",
    CodeGeneration: "claude-opus-4",
    Embeddings: "text-embedding-3-large",
    EthicalReasoning: "claude-opus-4",
    CreativeWriting: "claude-opus-4",
    DocumentAnalysis: "gemini-2.5-pro",
    ChatConversation: "claude-sonnet-4",
    ThreatIntelligence: "claude-opus-4",
    AgentOrchestration: "claude-opus-4",
  },
  "cost-optimized": {
    GeneralReasoning: "deepseek-v3",
    StructuredReasoning: "minimax-m2.5",
    CodeGeneration: "minimax-m2.5",
    Embeddings: "text-embedding-3-large",
    EthicalReasoning: "deepseek-r1",
    CreativeWriting: "gpt-4o-mini",
    DocumentAnalysis: "gemini-2.5-flash",
    ChatConversation: "claude-haiku-4",
    ThreatIntelligence: "deepseek-v3",
    AgentOrchestration: "minimax-m2.5",
  },
  "open-source": {
    GeneralReasoning: "llama-4-maverick",
    StructuredReasoning: "deepseek-r1",
    CodeGeneration: "deepseek-r1",
    Embeddings: "text-embedding-3-large",
    EthicalReasoning: "deepseek-r1",
    CreativeWriting: "llama-4-maverick",
    DocumentAnalysis: "llama-4-maverick",
    ChatConversation: "llama-4-maverick",
    ThreatIntelligence: "deepseek-v3",
    AgentOrchestration: "deepseek-r1",
  },
};

// ── Preferences Service ──────────────────────────────────────────────────────

const DEFAULT_LLM_PREFS: UserLLMPreferences = {
  modelAssignments: { ...LLM_DEFAULT_ASSIGNMENTS.balanced },
  providerApiKeys: {},
  providerEndpoints: {},
  llmSetupCompleted: false,
};

export class LLMPreferencesService {
  private static instance: LLMPreferencesService;

  private constructor() {}

  static getInstance(): LLMPreferencesService {
    if (!LLMPreferencesService.instance) {
      LLMPreferencesService.instance = new LLMPreferencesService();
    }
    return LLMPreferencesService.instance;
  }

  getPreferences(): UserLLMPreferences {
    if (typeof window === "undefined") return { ...DEFAULT_LLM_PREFS };
    const stored = localStorage.getItem(LLM_PREFERENCES_KEY);
    if (!stored) return { ...DEFAULT_LLM_PREFS };
    try {
      return JSON.parse(stored) as UserLLMPreferences;
    } catch {
      return { ...DEFAULT_LLM_PREFS };
    }
  }

  savePreferences(prefs: UserLLMPreferences): void {
    if (typeof window === "undefined") return;
    localStorage.setItem(LLM_PREFERENCES_KEY, JSON.stringify(prefs));
  }

  applyProfile(profile: LLMProfile): UserLLMPreferences {
    const prefs = this.getPreferences();
    prefs.modelAssignments = { ...LLM_DEFAULT_ASSIGNMENTS[profile] };
    return prefs;
  }

  getModelForUseCase(useCase: LLMUseCase): LLMModelInfo | undefined {
    const prefs = this.getPreferences();
    const modelKey = prefs.modelAssignments[useCase];
    return LLM_MODELS.find((m) => m.key === modelKey);
  }

  getModelsForUseCase(useCase: LLMUseCase): LLMModelInfo[] {
    return LLM_MODELS.filter((m) =>
      m.recommendedUseCases.includes(useCase)
    ).sort((a, b) => b.decisionScore - a.decisionScore);
  }

  getConfiguredProviders(): ProviderConfig[] {
    const prefs = this.getPreferences();
    const providerKeys = [
      ...new Set(LLM_MODELS.map((m) => m.provider)),
    ];
    return providerKeys.map((key) => ({
      key,
      displayName: LLM_PROVIDERS[key]?.displayName ?? key,
      requiresApiKey:
        LLM_MODELS.find((m) => m.provider === key)?.requiresApiKey ?? true,
      defaultEndpoint:
        LLM_MODELS.find((m) => m.provider === key)?.defaultEndpoint ?? "",
      hasApiKey: !!prefs.providerApiKeys[key],
      customEndpoint: prefs.providerEndpoints[key] ?? "",
    }));
  }

  resetToDefaults(): void {
    if (typeof window === "undefined") return;
    localStorage.removeItem(LLM_PREFERENCES_KEY);
  }
}
