namespace CognitiveMesh.ReasoningLayer.LLMReasoning
{
    /// <summary>
    /// Defines the use-case categories for which different LLM models can be assigned.
    /// Users can configure a different model per use case in the setup wizard.
    /// </summary>
    public enum LLMUseCase
    {
        /// <summary>General-purpose reasoning and analysis (default fallback).</summary>
        GeneralReasoning,

        /// <summary>ConclAIve structured reasoning: debate, sequential, strategic simulation.</summary>
        StructuredReasoning,

        /// <summary>Code generation, review, and refactoring.</summary>
        CodeGeneration,

        /// <summary>Embedding generation for vector search and RAG retrieval.</summary>
        Embeddings,

        /// <summary>Ethical reasoning (Brandom inferentialism + Floridi information ethics).</summary>
        EthicalReasoning,

        /// <summary>Creative tasks: content generation, brainstorming, copywriting.</summary>
        CreativeWriting,

        /// <summary>Long-context document analysis and summarization.</summary>
        DocumentAnalysis,

        /// <summary>Real-time chat and interactive conversation.</summary>
        ChatConversation,

        /// <summary>Domain-specific threat intelligence and security analysis.</summary>
        ThreatIntelligence,

        /// <summary>Multi-agent orchestration planning and task decomposition.</summary>
        AgentOrchestration
    }

    /// <summary>
    /// Metadata describing an LLM model's capabilities, used to render the
    /// setup wizard's model selection UI with weighted ADR scores.
    /// </summary>
    public class LLMModelInfo
    {
        /// <summary>Machine-readable model key (e.g., "claude-opus-4", "gpt-4o").</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>Human-readable display name.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Short description of the model.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Provider family (e.g., "anthropic", "openai", "google", "minimax", "meta").</summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>List of advantages.</summary>
        public List<string> Pros { get; set; } = new();

        /// <summary>List of disadvantages or limitations.</summary>
        public List<string> Cons { get; set; } = new();

        /// <summary>Recommended use-case labels.</summary>
        public List<LLMUseCase> RecommendedUseCases { get; set; } = new();

        /// <summary>Whether this model supports embedding generation.</summary>
        public bool SupportsEmbeddings { get; set; }

        /// <summary>Whether this model supports function/tool calling.</summary>
        public bool SupportsToolCalling { get; set; }

        /// <summary>Whether this model supports vision/image input.</summary>
        public bool SupportsVision { get; set; }

        /// <summary>Maximum context window size in tokens.</summary>
        public int MaxContextTokens { get; set; }

        /// <summary>Maximum output tokens per request.</summary>
        public int MaxOutputTokens { get; set; }

        /// <summary>Whether the provider requires an API key.</summary>
        public bool RequiresApiKey { get; set; } = true;

        /// <summary>Default API endpoint (can be overridden per user).</summary>
        public string DefaultEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Weighted ADR decision score (0-5) based on: reasoning quality,
        /// context length, cost efficiency, latency, tool calling, ecosystem maturity.
        /// </summary>
        public double DecisionScore { get; set; }
    }

    /// <summary>
    /// User-level LLM preferences supporting per-use-case model assignment
    /// with polyglot model routing (different models for different tasks).
    /// </summary>
    public class UserLLMPreferences
    {
        /// <summary>Unique user identifier.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Per-use-case model assignments. Key is the <see cref="LLMUseCase"/> name,
        /// value is the model key from the registry.
        /// </summary>
        public Dictionary<string, string> ModelAssignments { get; set; } = new();

        /// <summary>
        /// Provider-level API key configuration. Key is provider name (e.g., "anthropic"),
        /// value is the API key. Stored securely, never exposed to frontend.
        /// </summary>
        public Dictionary<string, string> ProviderApiKeys { get; set; } = new();

        /// <summary>
        /// Provider-level custom endpoint overrides. Key is provider name,
        /// value is the custom endpoint URL.
        /// </summary>
        public Dictionary<string, string> ProviderEndpoints { get; set; } = new();

        /// <summary>Whether the user has completed LLM configuration.</summary>
        public bool LlmSetupCompleted { get; set; }
    }

    /// <summary>
    /// Static registry of all available LLM models with weighted ADR decision scores.
    /// Scores are weighted across: reasoning quality (25%), context length (15%),
    /// cost efficiency (20%), latency (15%), tool calling (10%), ecosystem maturity (15%).
    /// </summary>
    public static class LLMModelRegistry
    {
        /// <summary>
        /// Returns metadata for all available LLM models, scored and ranked.
        /// </summary>
        public static IReadOnlyList<LLMModelInfo> GetAllModels() => new List<LLMModelInfo>
        {
            // ── Anthropic ───────────────────────────────────────────────────
            new()
            {
                Key = "claude-opus-4",
                DisplayName = "Claude Opus 4",
                Description = "Anthropic's most capable model. Exceptional at complex reasoning, nuanced analysis, and long-form structured output.",
                Provider = "anthropic",
                Pros = new() { "Best-in-class reasoning", "200K context window", "Excellent instruction following", "Strong ethical guardrails", "Superior structured output" },
                Cons = new() { "Higher latency than smaller models", "Premium pricing", "No native embeddings" },
                RecommendedUseCases = new() { LLMUseCase.StructuredReasoning, LLMUseCase.EthicalReasoning, LLMUseCase.DocumentAnalysis, LLMUseCase.AgentOrchestration },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 200_000,
                MaxOutputTokens = 32_000,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.anthropic.com",
                DecisionScore = 4.65
            },
            new()
            {
                Key = "claude-sonnet-4",
                DisplayName = "Claude Sonnet 4",
                Description = "Best balance of intelligence and speed. Ideal for most production workloads requiring strong reasoning at moderate cost.",
                Provider = "anthropic",
                Pros = new() { "Excellent reasoning-to-cost ratio", "200K context window", "Fast response times", "Strong tool calling", "Good at code" },
                Cons = new() { "Slightly less capable than Opus on hardest tasks", "No native embeddings" },
                RecommendedUseCases = new() { LLMUseCase.GeneralReasoning, LLMUseCase.CodeGeneration, LLMUseCase.ChatConversation, LLMUseCase.ThreatIntelligence },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 200_000,
                MaxOutputTokens = 16_000,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.anthropic.com",
                DecisionScore = 4.42
            },
            new()
            {
                Key = "claude-haiku-4",
                DisplayName = "Claude Haiku 4",
                Description = "Fastest Claude model. Near-instant responses for high-throughput, cost-sensitive workloads.",
                Provider = "anthropic",
                Pros = new() { "Very fast responses", "Low cost per token", "200K context window", "Good for classification/extraction", "Tool calling support" },
                Cons = new() { "Less capable on complex reasoning", "Shorter output limit", "May miss nuance" },
                RecommendedUseCases = new() { LLMUseCase.ChatConversation, LLMUseCase.CreativeWriting },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 200_000,
                MaxOutputTokens = 8_192,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.anthropic.com",
                DecisionScore = 3.78
            },

            // ── OpenAI ──────────────────────────────────────────────────────
            new()
            {
                Key = "gpt-4o",
                DisplayName = "GPT-4o",
                Description = "OpenAI's flagship multimodal model. Strong all-around performer with native vision, audio, and tool calling.",
                Provider = "openai",
                Pros = new() { "Strong multimodal capabilities", "128K context window", "Native vision + audio", "Fast for its capability tier", "Excellent tool calling" },
                Cons = new() { "128K context (less than Claude)", "Higher cost than GPT-4o-mini", "Can be verbose" },
                RecommendedUseCases = new() { LLMUseCase.GeneralReasoning, LLMUseCase.CodeGeneration, LLMUseCase.ChatConversation },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 128_000,
                MaxOutputTokens = 16_384,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.openai.com/v1",
                DecisionScore = 4.18
            },
            new()
            {
                Key = "gpt-4o-mini",
                DisplayName = "GPT-4o Mini",
                Description = "Cost-efficient OpenAI model. Good performance at a fraction of GPT-4o cost for simpler tasks.",
                Provider = "openai",
                Pros = new() { "Very low cost", "Fast responses", "128K context", "Good for simple tasks", "Tool calling support" },
                Cons = new() { "Weaker on complex reasoning", "Less reliable on nuanced tasks", "Shorter output quality" },
                RecommendedUseCases = new() { LLMUseCase.ChatConversation, LLMUseCase.CreativeWriting },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 128_000,
                MaxOutputTokens = 16_384,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.openai.com/v1",
                DecisionScore = 3.55
            },
            new()
            {
                Key = "o3",
                DisplayName = "OpenAI o3",
                Description = "OpenAI's reasoning model with chain-of-thought. Excels at math, science, and complex multi-step problems.",
                Provider = "openai",
                Pros = new() { "Superior chain-of-thought reasoning", "Excellent at math/science", "Strong coding ability", "Thinks before answering", "200K context" },
                Cons = new() { "Slower due to thinking time", "Higher cost", "May overthink simple tasks", "Limited streaming" },
                RecommendedUseCases = new() { LLMUseCase.StructuredReasoning, LLMUseCase.CodeGeneration, LLMUseCase.EthicalReasoning },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 200_000,
                MaxOutputTokens = 100_000,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.openai.com/v1",
                DecisionScore = 4.35
            },
            new()
            {
                Key = "text-embedding-3-large",
                DisplayName = "text-embedding-3-large",
                Description = "OpenAI's best embedding model. 3072 dimensions with optional dimension reduction via Matryoshka representation.",
                Provider = "openai",
                Pros = new() { "Best-in-class embeddings", "3072 dimensions (reducible)", "Matryoshka representation", "Excellent retrieval quality", "Low cost per token" },
                Cons = new() { "Embedding-only (no generation)", "Requires OpenAI API key", "Higher dimensional cost" },
                RecommendedUseCases = new() { LLMUseCase.Embeddings },
                SupportsEmbeddings = true,
                SupportsToolCalling = false,
                SupportsVision = false,
                MaxContextTokens = 8_191,
                MaxOutputTokens = 0,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.openai.com/v1",
                DecisionScore = 4.50
            },

            // ── Google ──────────────────────────────────────────────────────
            new()
            {
                Key = "gemini-2.5-pro",
                DisplayName = "Gemini 2.5 Pro",
                Description = "Google's most capable model with 1M token context. Excellent for massive document analysis and multi-modal tasks.",
                Provider = "google",
                Pros = new() { "1M token context window", "Strong multimodal (text, image, video, audio)", "Native code execution", "Competitive reasoning", "Google ecosystem integration" },
                Cons = new() { "Variable availability", "Google Cloud dependency", "Occasional inconsistency on edge cases" },
                RecommendedUseCases = new() { LLMUseCase.DocumentAnalysis, LLMUseCase.GeneralReasoning, LLMUseCase.CreativeWriting },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 1_000_000,
                MaxOutputTokens = 65_536,
                RequiresApiKey = true,
                DefaultEndpoint = "https://generativelanguage.googleapis.com/v1beta",
                DecisionScore = 4.22
            },
            new()
            {
                Key = "gemini-2.5-flash",
                DisplayName = "Gemini 2.5 Flash",
                Description = "Google's speed-optimized model with 1M context. Best for high-throughput workloads needing large context.",
                Provider = "google",
                Pros = new() { "1M token context", "Very fast responses", "Low cost", "Good multimodal", "Thinking mode available" },
                Cons = new() { "Less capable than Pro on hard tasks", "Google Cloud dependency", "Newer ecosystem" },
                RecommendedUseCases = new() { LLMUseCase.ChatConversation, LLMUseCase.DocumentAnalysis },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 1_000_000,
                MaxOutputTokens = 65_536,
                RequiresApiKey = true,
                DefaultEndpoint = "https://generativelanguage.googleapis.com/v1beta",
                DecisionScore = 3.88
            },

            // ── MiniMax ─────────────────────────────────────────────────────
            new()
            {
                Key = "minimax-m1",
                DisplayName = "MiniMax-M1",
                Description = "MiniMax's reasoning model with hybrid chain-of-thought. Strong performance on AIME/GPQA benchmarks at low cost.",
                Provider = "minimax",
                Pros = new() { "Competitive reasoning benchmarks", "Low cost per token", "Hybrid thinking mode", "Good math/science", "OpenAI-compatible API" },
                Cons = new() { "Smaller ecosystem", "Limited multimodal", "Newer provider", "Less enterprise support" },
                RecommendedUseCases = new() { LLMUseCase.StructuredReasoning, LLMUseCase.CodeGeneration },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = false,
                MaxContextTokens = 1_000_000,
                MaxOutputTokens = 16_384,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.minimax.chat/v1",
                DecisionScore = 3.72
            },
            new()
            {
                Key = "minimax-m2.5",
                DisplayName = "MiniMax-M2.5",
                Description = "MiniMax's latest flagship model (Feb 2025). Top-tier performance on LiveCodeBench and MMLU-Pro with 1M context and native tool calling.",
                Provider = "minimax",
                Pros = new() { "1M token context window", "Top-5 on LiveCodeBench/MMLU-Pro", "Native tool/function calling", "OpenAI-compatible API", "Very competitive pricing", "Strong at code generation" },
                Cons = new() { "Newer provider — less battle-tested", "Limited vision capabilities", "Smaller community", "Enterprise support still growing" },
                RecommendedUseCases = new() { LLMUseCase.CodeGeneration, LLMUseCase.GeneralReasoning, LLMUseCase.AgentOrchestration, LLMUseCase.StructuredReasoning },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = false,
                MaxContextTokens = 1_000_000,
                MaxOutputTokens = 32_000,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.minimax.chat/v1",
                DecisionScore = 4.08
            },

            // ── DeepSeek ────────────────────────────────────────────────────
            new()
            {
                Key = "deepseek-r1",
                DisplayName = "DeepSeek-R1",
                Description = "Open-weight reasoning model rivaling o1 on math/code. MIT-licensed with chain-of-thought transparency.",
                Provider = "deepseek",
                Pros = new() { "Open-weight (MIT license)", "Strong math/code reasoning", "Chain-of-thought transparent", "Self-hostable", "Low API cost" },
                Cons = new() { "Slower due to reasoning chain", "128K context (less than competitors)", "Newer provider", "Less enterprise support" },
                RecommendedUseCases = new() { LLMUseCase.StructuredReasoning, LLMUseCase.CodeGeneration },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = false,
                MaxContextTokens = 128_000,
                MaxOutputTokens = 16_384,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.deepseek.com/v1",
                DecisionScore = 3.85
            },
            new()
            {
                Key = "deepseek-v3",
                DisplayName = "DeepSeek-V3",
                Description = "DeepSeek's general-purpose MoE model. Cost-effective with strong multilingual and coding performance.",
                Provider = "deepseek",
                Pros = new() { "Very low cost", "MoE architecture (efficient)", "Strong multilingual", "Open weights available", "Good coding" },
                Cons = new() { "Less capable than R1 on hard reasoning", "128K context", "Variable availability" },
                RecommendedUseCases = new() { LLMUseCase.GeneralReasoning, LLMUseCase.ChatConversation, LLMUseCase.CreativeWriting },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = false,
                MaxContextTokens = 128_000,
                MaxOutputTokens = 8_192,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.deepseek.com/v1",
                DecisionScore = 3.58
            },

            // ── Meta (Llama) ────────────────────────────────────────────────
            new()
            {
                Key = "llama-4-maverick",
                DisplayName = "Llama 4 Maverick",
                Description = "Meta's Llama 4 Maverick — 400B MoE model with 128 experts. Best open-weight model for self-hosted enterprise deployments.",
                Provider = "meta",
                Pros = new() { "Open-weight (self-hostable)", "400B MoE (17B active)", "1M context window", "Strong multilingual (12 languages)", "No API costs when self-hosted" },
                Cons = new() { "Requires significant GPU infrastructure", "No hosted API by default", "Community support only", "Complex deployment" },
                RecommendedUseCases = new() { LLMUseCase.GeneralReasoning, LLMUseCase.CodeGeneration, LLMUseCase.ChatConversation },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 1_000_000,
                MaxOutputTokens = 16_384,
                RequiresApiKey = false,
                DefaultEndpoint = "http://localhost:11434/v1",
                DecisionScore = 3.92
            },

            // ── xAI ─────────────────────────────────────────────────────────
            new()
            {
                Key = "grok-3",
                DisplayName = "Grok 3",
                Description = "xAI's flagship model trained on Colossus cluster. Strong reasoning with real-time data access and 'think' mode.",
                Provider = "xai",
                Pros = new() { "Strong reasoning benchmarks", "Real-time data access", "Think mode for deep analysis", "128K context", "Competitive pricing" },
                Cons = new() { "xAI platform dependency", "Newer ecosystem", "Less enterprise adoption", "API stability still maturing" },
                RecommendedUseCases = new() { LLMUseCase.GeneralReasoning, LLMUseCase.ThreatIntelligence, LLMUseCase.CreativeWriting },
                SupportsEmbeddings = false,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 128_000,
                MaxOutputTokens = 16_384,
                RequiresApiKey = true,
                DefaultEndpoint = "https://api.x.ai/v1",
                DecisionScore = 3.82
            },

            // ── Azure OpenAI ────────────────────────────────────────────────
            new()
            {
                Key = "azure-openai",
                DisplayName = "Azure OpenAI Service",
                Description = "Microsoft-hosted OpenAI models with enterprise SLA, VNet integration, and content filtering. Existing integration in this codebase.",
                Provider = "azure-openai",
                Pros = new() { "Enterprise SLA and compliance", "VNet/private endpoint support", "Content safety built-in", "Existing codebase integration", "Azure AD authentication" },
                Cons = new() { "Azure subscription required", "Model availability varies by region", "Higher cost than direct OpenAI", "Deployment provisioning delays" },
                RecommendedUseCases = new() { LLMUseCase.GeneralReasoning, LLMUseCase.CodeGeneration, LLMUseCase.ChatConversation, LLMUseCase.EthicalReasoning },
                SupportsEmbeddings = true,
                SupportsToolCalling = true,
                SupportsVision = true,
                MaxContextTokens = 128_000,
                MaxOutputTokens = 16_384,
                RequiresApiKey = true,
                DefaultEndpoint = "",
                DecisionScore = 4.12
            },
        };

        /// <summary>
        /// Returns models filtered by a specific use case, ordered by decision score.
        /// </summary>
        public static IReadOnlyList<LLMModelInfo> GetModelsForUseCase(LLMUseCase useCase) =>
            GetAllModels()
                .Where(m => m.RecommendedUseCases.Contains(useCase))
                .OrderByDescending(m => m.DecisionScore)
                .ToList();

        /// <summary>
        /// Returns the recommended default model assignments per use case.
        /// </summary>
        public static Dictionary<string, string> GetDefaultAssignments(string profile = "balanced")
        {
            return profile.ToLower() switch
            {
                "performance" => new()
                {
                    [nameof(LLMUseCase.GeneralReasoning)] = "claude-opus-4",
                    [nameof(LLMUseCase.StructuredReasoning)] = "claude-opus-4",
                    [nameof(LLMUseCase.CodeGeneration)] = "claude-opus-4",
                    [nameof(LLMUseCase.Embeddings)] = "text-embedding-3-large",
                    [nameof(LLMUseCase.EthicalReasoning)] = "claude-opus-4",
                    [nameof(LLMUseCase.CreativeWriting)] = "claude-opus-4",
                    [nameof(LLMUseCase.DocumentAnalysis)] = "gemini-2.5-pro",
                    [nameof(LLMUseCase.ChatConversation)] = "claude-sonnet-4",
                    [nameof(LLMUseCase.ThreatIntelligence)] = "claude-opus-4",
                    [nameof(LLMUseCase.AgentOrchestration)] = "claude-opus-4",
                },
                "cost-optimized" => new()
                {
                    [nameof(LLMUseCase.GeneralReasoning)] = "deepseek-v3",
                    [nameof(LLMUseCase.StructuredReasoning)] = "minimax-m2.5",
                    [nameof(LLMUseCase.CodeGeneration)] = "minimax-m2.5",
                    [nameof(LLMUseCase.Embeddings)] = "text-embedding-3-large",
                    [nameof(LLMUseCase.EthicalReasoning)] = "deepseek-r1",
                    [nameof(LLMUseCase.CreativeWriting)] = "gpt-4o-mini",
                    [nameof(LLMUseCase.DocumentAnalysis)] = "gemini-2.5-flash",
                    [nameof(LLMUseCase.ChatConversation)] = "claude-haiku-4",
                    [nameof(LLMUseCase.ThreatIntelligence)] = "deepseek-v3",
                    [nameof(LLMUseCase.AgentOrchestration)] = "minimax-m2.5",
                },
                "open-source" => new()
                {
                    [nameof(LLMUseCase.GeneralReasoning)] = "llama-4-maverick",
                    [nameof(LLMUseCase.StructuredReasoning)] = "deepseek-r1",
                    [nameof(LLMUseCase.CodeGeneration)] = "deepseek-r1",
                    [nameof(LLMUseCase.Embeddings)] = "text-embedding-3-large",
                    [nameof(LLMUseCase.EthicalReasoning)] = "deepseek-r1",
                    [nameof(LLMUseCase.CreativeWriting)] = "llama-4-maverick",
                    [nameof(LLMUseCase.DocumentAnalysis)] = "llama-4-maverick",
                    [nameof(LLMUseCase.ChatConversation)] = "llama-4-maverick",
                    [nameof(LLMUseCase.ThreatIntelligence)] = "deepseek-v3",
                    [nameof(LLMUseCase.AgentOrchestration)] = "deepseek-r1",
                },
                // "balanced" is the default
                _ => new()
                {
                    [nameof(LLMUseCase.GeneralReasoning)] = "claude-sonnet-4",
                    [nameof(LLMUseCase.StructuredReasoning)] = "claude-opus-4",
                    [nameof(LLMUseCase.CodeGeneration)] = "minimax-m2.5",
                    [nameof(LLMUseCase.Embeddings)] = "text-embedding-3-large",
                    [nameof(LLMUseCase.EthicalReasoning)] = "claude-opus-4",
                    [nameof(LLMUseCase.CreativeWriting)] = "gemini-2.5-flash",
                    [nameof(LLMUseCase.DocumentAnalysis)] = "gemini-2.5-pro",
                    [nameof(LLMUseCase.ChatConversation)] = "claude-haiku-4",
                    [nameof(LLMUseCase.ThreatIntelligence)] = "claude-sonnet-4",
                    [nameof(LLMUseCase.AgentOrchestration)] = "minimax-m2.5",
                },
            };
        }

        /// <summary>
        /// Returns a list of unique provider keys from all models.
        /// </summary>
        public static IReadOnlyList<string> GetProviderKeys() =>
            GetAllModels().Select(m => m.Provider).Distinct().ToList();
    }
}
