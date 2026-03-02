"use client";

import React, { useState } from "react";
import {
  Database,
  Server,
  Layers,
  Rocket,
  ChevronRight,
  ChevronLeft,
  Sparkles,
  TestTube,
  Cloud,
  Code,
  Factory,
  Search,
  HardDrive,
  ArrowRight,
  Check,
  X,
  Brain,
  Key,
  Eye,
  EyeOff,
  Shield,
} from "lucide-react";
import { useSetupWizard } from "@/hooks/useSetupWizard";
import {
  MEMORY_STORE_PROVIDERS,
  VECTOR_SEARCH_PROVIDERS,
} from "@/services/preferences";
import {
  LLM_MODELS,
  LLM_PROVIDERS,
  LLM_USE_CASE_META,
  type LLMUseCase,
  type LLMProfile,
} from "@/services/llmPreferences";
import StorageProviderCard from "./StorageProviderCard";
import LLMModelCard from "./LLMModelCard";

const SetupWizard: React.FC = () => {
  const {
    currentStep,
    currentStepIndex,
    totalSteps,
    progress,
    preferences,
    llmPreferences,
    llmProfile,
    useCase,
    isVisible,
    goNext,
    goBack,
    applyUseCaseDefaults,
    updatePreference,
    applyLlmProfile,
    updateModelAssignment,
    updateProviderApiKey,
    completeSetup,
    dismiss,
  } = useSetupWizard();

  const [expandedLlmUseCase, setExpandedLlmUseCase] = useState<string | null>(null);
  const [visibleApiKeys, setVisibleApiKeys] = useState<Set<string>>(new Set());

  if (!isVisible) return null;

  const toggleApiKeyVisibility = (provider: string) => {
    setVisibleApiKeys((prev) => {
      const next = new Set(prev);
      if (next.has(provider)) next.delete(provider);
      else next.add(provider);
      return next;
    });
  };

  const stepIndicators = [
    { id: "welcome", label: "Welcome", icon: Sparkles },
    { id: "use-case", label: "Use Case", icon: Rocket },
    { id: "primary-store", label: "Primary DB", icon: Database },
    { id: "vector-provider", label: "Vector", icon: Search },
    { id: "cache-layer", label: "Cache", icon: Layers },
    { id: "llm-profile", label: "LLM Profile", icon: Brain },
    { id: "llm-models", label: "Models", icon: Brain },
    { id: "llm-api-keys", label: "API Keys", icon: Key },
    { id: "review", label: "Review", icon: Check },
    { id: "complete", label: "Done", icon: Sparkles },
  ];

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/80 backdrop-blur-sm"
        onClick={dismiss}
      />

      {/* Wizard panel */}
      <div className="relative w-full max-w-4xl max-h-[90vh] mx-4 bg-slate-900/95 border border-slate-700/60 rounded-2xl shadow-2xl shadow-cyan-500/5 overflow-hidden flex flex-col">
        {/* Progress bar */}
        <div className="h-1 bg-slate-800">
          <div
            className="h-full bg-gradient-to-r from-cyan-500 via-blue-500 to-violet-500 transition-all duration-500"
            style={{ width: `${progress}%` }}
          />
        </div>

        {/* Step indicators */}
        <div className="flex items-center justify-center gap-1 px-6 py-3 border-b border-slate-800/60">
          {stepIndicators.map((step, idx) => {
            const Icon = step.icon;
            const isActive = idx === currentStepIndex;
            const isCompleted = idx < currentStepIndex;
            return (
              <React.Fragment key={step.id}>
                {idx > 0 && (
                  <div
                    className={`w-8 h-px ${isCompleted ? "bg-cyan-500/60" : "bg-slate-700/40"}`}
                  />
                )}
                <div
                  className={`flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs transition-all ${
                    isActive
                      ? "bg-cyan-500/20 text-cyan-300 border border-cyan-500/30"
                      : isCompleted
                        ? "text-cyan-400/70"
                        : "text-slate-500"
                  }`}
                >
                  <Icon className="w-3.5 h-3.5" />
                  <span className="hidden sm:inline">{step.label}</span>
                </div>
              </React.Fragment>
            );
          })}
        </div>

        {/* Content area */}
        <div className="flex-1 overflow-y-auto p-6 sm:p-8">
          {/* Step: Welcome */}
          {currentStep === "welcome" && (
            <div className="text-center max-w-lg mx-auto space-y-6">
              <div className="w-20 h-20 mx-auto rounded-2xl bg-gradient-to-br from-cyan-500/30 to-violet-500/30 flex items-center justify-center border border-cyan-500/20">
                <Database className="w-10 h-10 text-cyan-400" />
              </div>
              <h2 className="text-2xl font-bold text-white">
                Configure Your Storage Layer
              </h2>
              <p className="text-slate-400 leading-relaxed">
                Cognitive Mesh supports{" "}
                <span className="text-cyan-400 font-semibold">
                  polyglot persistence
                </span>{" "}
                &mdash; you can use multiple databases simultaneously, each
                optimized for its role: primary storage, caching, vector search,
                and analytics.
              </p>
              <div className="grid grid-cols-3 gap-3 pt-4">
                <div className="p-3 rounded-lg bg-slate-800/50 border border-slate-700/40">
                  <Database className="w-6 h-6 text-emerald-400 mx-auto mb-2" />
                  <div className="text-xs font-medium text-slate-300">
                    8 Store Backends
                  </div>
                </div>
                <div className="p-3 rounded-lg bg-slate-800/50 border border-slate-700/40">
                  <Search className="w-6 h-6 text-violet-400 mx-auto mb-2" />
                  <div className="text-xs font-medium text-slate-300">
                    5 Vector Providers
                  </div>
                </div>
                <div className="p-3 rounded-lg bg-slate-800/50 border border-slate-700/40">
                  <Layers className="w-6 h-6 text-amber-400 mx-auto mb-2" />
                  <div className="text-xs font-medium text-slate-300">
                    Hybrid Modes
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Step: Use Case */}
          {currentStep === "use-case" && (
            <div className="space-y-6">
              <div className="text-center">
                <h2 className="text-xl font-bold text-white">
                  What best describes your use case?
                </h2>
                <p className="text-sm text-slate-400 mt-1">
                  We&apos;ll pre-select optimal defaults. You can customize
                  everything in the next steps.
                </p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-2xl mx-auto">
                {(
                  [
                    {
                      id: "development",
                      icon: Code,
                      title: "Development",
                      desc: "Local development with embedded databases. No external services needed.",
                      color: "emerald",
                    },
                    {
                      id: "production",
                      icon: Factory,
                      title: "Production",
                      desc: "PostgreSQL + Redis + Qdrant for high-performance workloads.",
                      color: "cyan",
                    },
                    {
                      id: "cloud",
                      icon: Cloud,
                      title: "Cloud / Azure",
                      desc: "Cosmos DB + Redis + Milvus for globally distributed systems.",
                      color: "violet",
                    },
                    {
                      id: "testing",
                      icon: TestTube,
                      title: "Testing",
                      desc: "In-memory stores for fast, isolated unit and integration tests.",
                      color: "amber",
                    },
                  ] as const
                ).map((uc) => {
                  const Icon = uc.icon;
                  const isSelected = useCase === uc.id;
                  const colorClasses = {
                    emerald: {
                      bg: "bg-emerald-500/15",
                      border: "border-emerald-500/60",
                      ring: "ring-emerald-500/30",
                      iconBg: "bg-emerald-500/20",
                      iconText: "text-emerald-400",
                    },
                    cyan: {
                      bg: "bg-cyan-500/15",
                      border: "border-cyan-500/60",
                      ring: "ring-cyan-500/30",
                      iconBg: "bg-cyan-500/20",
                      iconText: "text-cyan-400",
                    },
                    violet: {
                      bg: "bg-violet-500/15",
                      border: "border-violet-500/60",
                      ring: "ring-violet-500/30",
                      iconBg: "bg-violet-500/20",
                      iconText: "text-violet-400",
                    },
                    amber: {
                      bg: "bg-amber-500/15",
                      border: "border-amber-500/60",
                      ring: "ring-amber-500/30",
                      iconBg: "bg-amber-500/20",
                      iconText: "text-amber-400",
                    },
                  };
                  const colors = colorClasses[uc.color];

                  return (
                    <div
                      key={uc.id}
                      onClick={() => applyUseCaseDefaults(uc.id)}
                      className={`p-5 rounded-xl border cursor-pointer transition-all duration-300 ${
                        isSelected
                          ? `${colors.bg} ${colors.border} ring-1 ${colors.ring} shadow-lg`
                          : "bg-slate-800/40 border-slate-700/50 hover:bg-slate-700/40"
                      }`}
                    >
                      <div
                        className={`w-12 h-12 rounded-xl ${colors.iconBg} flex items-center justify-center mb-3`}
                      >
                        <Icon className={`w-6 h-6 ${colors.iconText}`} />
                      </div>
                      <h3 className="font-semibold text-white">{uc.title}</h3>
                      <p className="text-xs text-slate-400 mt-1">{uc.desc}</p>
                      {isSelected && (
                        <div className="mt-3 flex items-center gap-1 text-xs text-cyan-400">
                          <Check className="w-3.5 h-3.5" />
                          <span>Selected</span>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* Step: Primary Store */}
          {currentStep === "primary-store" && (
            <div className="space-y-5">
              <div className="text-center">
                <h2 className="text-xl font-bold text-white flex items-center justify-center gap-2">
                  <HardDrive className="w-5 h-5 text-cyan-400" />
                  Choose Primary Memory Store
                </h2>
                <p className="text-sm text-slate-400 mt-1">
                  Your primary store handles all context persistence (read/write
                  of session data, key-value pairs).
                </p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                {MEMORY_STORE_PROVIDERS.map((provider) => (
                  <StorageProviderCard
                    key={provider.key}
                    provider={provider}
                    isSelected={preferences.primaryStore === provider.key}
                    onSelect={(key) => updatePreference("primaryStore", key)}
                  />
                ))}
              </div>
            </div>
          )}

          {/* Step: Vector Search Provider */}
          {currentStep === "vector-provider" && (
            <div className="space-y-5">
              <div className="text-center">
                <h2 className="text-xl font-bold text-white flex items-center justify-center gap-2">
                  <Search className="w-5 h-5 text-violet-400" />
                  Choose Vector Search Provider
                </h2>
                <p className="text-sm text-slate-400 mt-1">
                  Vector search powers semantic similarity queries, RAG
                  retrieval, and embedding-based lookups.
                </p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                {VECTOR_SEARCH_PROVIDERS.map((provider) => (
                  <StorageProviderCard
                    key={provider.key}
                    provider={provider}
                    isSelected={
                      preferences.vectorSearchProvider === provider.key
                    }
                    onSelect={(key) =>
                      updatePreference("vectorSearchProvider", key)
                    }
                  />
                ))}
              </div>
            </div>
          )}

          {/* Step: Cache Layer */}
          {currentStep === "cache-layer" && (
            <div className="space-y-5">
              <div className="text-center">
                <h2 className="text-xl font-bold text-white flex items-center justify-center gap-2">
                  <Layers className="w-5 h-5 text-amber-400" />
                  Configure Cache &amp; Hybrid Mode
                </h2>
                <p className="text-sm text-slate-400 mt-1">
                  Optionally add a cache layer for low-latency reads with
                  dual-write to your primary store.
                </p>
              </div>

              {/* Hybrid mode toggle */}
              <div className="max-w-lg mx-auto space-y-4">
                <div
                  onClick={() =>
                    updatePreference(
                      "enableHybridMode",
                      !preferences.enableHybridMode
                    )
                  }
                  className={`p-4 rounded-xl border cursor-pointer transition-all ${
                    preferences.enableHybridMode
                      ? "bg-cyan-500/15 border-cyan-500/50"
                      : "bg-slate-800/40 border-slate-700/50"
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="font-semibold text-white text-sm">
                        Enable Hybrid Mode
                      </h3>
                      <p className="text-xs text-slate-400 mt-0.5">
                        Dual-write to primary store + cache for optimal
                        durability and speed
                      </p>
                    </div>
                    <div
                      className={`w-10 h-6 rounded-full transition-colors flex items-center px-1 ${
                        preferences.enableHybridMode
                          ? "bg-cyan-500"
                          : "bg-slate-600"
                      }`}
                    >
                      <div
                        className={`w-4 h-4 rounded-full bg-white transition-transform ${
                          preferences.enableHybridMode
                            ? "translate-x-4"
                            : "translate-x-0"
                        }`}
                      />
                    </div>
                  </div>
                </div>

                {preferences.enableHybridMode && (
                  <>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                      {(
                        [
                          {
                            key: "redis",
                            name: "Redis",
                            desc: "Sub-ms latency, ideal cache layer",
                          },
                          {
                            key: null,
                            name: "No Cache",
                            desc: "Use primary store only",
                          },
                        ] as const
                      ).map((opt) => (
                        <div
                          key={opt.key ?? "none"}
                          onClick={() =>
                            updatePreference("cacheStore", opt.key)
                          }
                          className={`p-4 rounded-xl border cursor-pointer transition-all ${
                            preferences.cacheStore === opt.key
                              ? "bg-amber-500/15 border-amber-500/50"
                              : "bg-slate-800/40 border-slate-700/50 hover:bg-slate-700/40"
                          }`}
                        >
                          <h4 className="font-semibold text-white text-sm">
                            {opt.name}
                          </h4>
                          <p className="text-xs text-slate-400 mt-0.5">
                            {opt.desc}
                          </p>
                        </div>
                      ))}
                    </div>

                    {preferences.cacheStore && (
                      <div
                        onClick={() =>
                          updatePreference(
                            "preferCacheForRetrieval",
                            !preferences.preferCacheForRetrieval
                          )
                        }
                        className={`p-4 rounded-xl border cursor-pointer transition-all ${
                          preferences.preferCacheForRetrieval
                            ? "bg-amber-500/10 border-amber-500/40"
                            : "bg-slate-800/40 border-slate-700/50"
                        }`}
                      >
                        <div className="flex items-center justify-between">
                          <div>
                            <h4 className="text-sm font-medium text-white">
                              Prefer cache for reads
                            </h4>
                            <p className="text-xs text-slate-400">
                              Read from cache first, fall back to primary store
                            </p>
                          </div>
                          <div
                            className={`w-10 h-6 rounded-full transition-colors flex items-center px-1 ${
                              preferences.preferCacheForRetrieval
                                ? "bg-amber-500"
                                : "bg-slate-600"
                            }`}
                          >
                            <div
                              className={`w-4 h-4 rounded-full bg-white transition-transform ${
                                preferences.preferCacheForRetrieval
                                  ? "translate-x-4"
                                  : "translate-x-0"
                              }`}
                            />
                          </div>
                        </div>
                      </div>
                    )}
                  </>
                )}
              </div>
            </div>
          )}

          {/* Step: LLM Profile */}
          {currentStep === "llm-profile" && (
            <div className="space-y-6">
              <div className="text-center">
                <h2 className="text-xl font-bold text-white flex items-center justify-center gap-2">
                  <Brain className="w-5 h-5 text-violet-400" />
                  Choose LLM Configuration Profile
                </h2>
                <p className="text-sm text-slate-400 mt-1">
                  Select a preset that assigns optimal models per use case. You can
                  customize individual assignments in the next step.
                </p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-2xl mx-auto">
                {([
                  { id: "balanced" as LLMProfile, title: "Balanced", desc: "Claude Sonnet for general, Opus for reasoning, MiniMax M2.5 for code, Gemini for docs. Best mix of quality & cost.", color: "cyan" },
                  { id: "performance" as LLMProfile, title: "Performance", desc: "Claude Opus 4 across all reasoning tasks. Maximum quality, premium pricing.", color: "emerald" },
                  { id: "cost-optimized" as LLMProfile, title: "Cost-Optimized", desc: "DeepSeek V3, MiniMax M2.5, GPT-4o Mini, Haiku. Budget-friendly without sacrificing too much quality.", color: "amber" },
                  { id: "open-source" as LLMProfile, title: "Open Source", desc: "Llama 4 Maverick + DeepSeek R1. Self-hostable with no API costs. Requires GPU infrastructure.", color: "violet" },
                ]).map((p) => {
                  const isSelected = llmProfile === p.id;
                  return (
                    <div
                      key={p.id}
                      onClick={() => applyLlmProfile(p.id)}
                      className={`p-5 rounded-xl border cursor-pointer transition-all duration-300 ${
                        isSelected
                          ? `bg-${p.color}-500/15 border-${p.color}-500/60 ring-1 ring-${p.color}-500/30 shadow-lg`
                          : "bg-slate-800/40 border-slate-700/50 hover:bg-slate-700/40"
                      }`}
                    >
                      <h3 className="font-semibold text-white">{p.title}</h3>
                      <p className="text-xs text-slate-400 mt-1">{p.desc}</p>
                      {isSelected && (
                        <div className="mt-3 flex items-center gap-1 text-xs text-cyan-400">
                          <Check className="w-3.5 h-3.5" /> Selected
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* Step: LLM Models per use case */}
          {currentStep === "llm-models" && (() => {
            const useCases = Object.keys(LLM_USE_CASE_META) as LLMUseCase[];
            return (
              <div className="space-y-4">
                <div className="text-center">
                  <h2 className="text-xl font-bold text-white flex items-center justify-center gap-2">
                    <Brain className="w-5 h-5 text-violet-400" />
                    Model Per Use Case
                  </h2>
                  <p className="text-sm text-slate-400 mt-1">
                    Fine-tune which model handles each task type. Click to expand and change.
                  </p>
                </div>
                <div className="space-y-2">
                  {useCases.map((uc) => {
                    const meta = LLM_USE_CASE_META[uc];
                    const assignedKey = llmPreferences.modelAssignments[uc];
                    const assignedModel = LLM_MODELS.find((m) => m.key === assignedKey);
                    const isExpanded = expandedLlmUseCase === uc;
                    const recommended = LLM_MODELS.filter((m) =>
                      m.recommendedUseCases.includes(uc)
                    ).sort((a, b) => b.decisionScore - a.decisionScore);

                    return (
                      <div key={uc} className="border border-slate-700/40 rounded-xl overflow-hidden">
                        <button
                          onClick={() => setExpandedLlmUseCase(isExpanded ? null : uc)}
                          className="w-full flex items-center justify-between p-3 bg-slate-800/30 hover:bg-slate-800/50 transition-colors"
                        >
                          <div className="text-left">
                            <h3 className="text-sm font-semibold text-white">{meta.label}</h3>
                            <p className="text-[11px] text-slate-400">{meta.description}</p>
                          </div>
                          <span className="text-xs font-medium px-2 py-0.5 rounded-full bg-slate-700/60 text-slate-300 shrink-0 ml-2">
                            {assignedModel?.displayName ?? assignedKey}
                          </span>
                        </button>
                        {isExpanded && (
                          <div className="p-3 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
                            {recommended.map((model) => (
                              <LLMModelCard
                                key={model.key}
                                model={model}
                                isSelected={assignedKey === model.key}
                                onSelect={(key) => updateModelAssignment(uc, key)}
                                compact
                              />
                            ))}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            );
          })()}

          {/* Step: API Keys */}
          {currentStep === "llm-api-keys" && (() => {
            const activeProviders = new Set(
              Object.values(llmPreferences.modelAssignments)
                .map((mk) => LLM_MODELS.find((m) => m.key === mk)?.provider)
                .filter(Boolean) as string[]
            );
            return (
              <div className="space-y-5 max-w-lg mx-auto">
                <div className="text-center">
                  <h2 className="text-xl font-bold text-white flex items-center justify-center gap-2">
                    <Key className="w-5 h-5 text-amber-400" />
                    Provider API Keys
                  </h2>
                  <p className="text-sm text-slate-400 mt-1">
                    Enter API keys for the {activeProviders.size} provider{activeProviders.size !== 1 ? "s" : ""} you selected. You can add these later in Settings.
                  </p>
                </div>

                <div className="flex items-center gap-2 text-xs text-amber-400/80 bg-amber-500/10 border border-amber-500/20 rounded-lg p-2.5">
                  <Shield className="w-4 h-4 shrink-0" />
                  <span>Keys are stored in localStorage only. Use environment variables for production.</span>
                </div>

                <div className="space-y-3">
                  {Array.from(activeProviders).sort().map((providerKey) => {
                    const pInfo = LLM_PROVIDERS[providerKey];
                    const model = LLM_MODELS.find((m) => m.provider === providerKey);
                    const currentKey = llmPreferences.providerApiKeys[providerKey] ?? "";
                    const isVisible = visibleApiKeys.has(providerKey);

                    if (!model?.requiresApiKey) return null;

                    return (
                      <div key={providerKey} className="p-3 rounded-lg bg-slate-800/30 border border-slate-700/30 space-y-2">
                        <div className="flex items-center justify-between">
                          <span className="text-sm font-semibold text-white">
                            {pInfo?.displayName ?? providerKey}
                          </span>
                          {currentKey && (
                            <span className="text-[10px] px-2 py-0.5 rounded-full bg-emerald-500/20 text-emerald-300 border border-emerald-500/30">
                              Configured
                            </span>
                          )}
                        </div>
                        <div className="relative">
                          <input
                            type={isVisible ? "text" : "password"}
                            value={currentKey}
                            onChange={(e) => updateProviderApiKey(providerKey, e.target.value)}
                            placeholder={`${pInfo?.displayName ?? providerKey} API key...`}
                            className="w-full px-3 py-2 pr-10 rounded-lg bg-slate-900/60 border border-slate-700/50 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500/50 font-mono"
                          />
                          <button
                            onClick={() => toggleApiKeyVisibility(providerKey)}
                            className="absolute right-2 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white"
                          >
                            {isVisible ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                          </button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            );
          })()}

          {/* Step: Review */}
          {currentStep === "review" && (() => {
            const topAssignments = Object.entries(llmPreferences.modelAssignments).slice(0, 5);
            return (
              <div className="space-y-6 max-w-2xl mx-auto">
                <div className="text-center">
                  <h2 className="text-xl font-bold text-white">
                    Review Your Configuration
                  </h2>
                  <p className="text-sm text-slate-400 mt-1">
                    Storage + LLM setup. Change anytime in Settings.
                  </p>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  {/* Storage summary */}
                  <div className="space-y-2">
                    <div className="text-[10px] uppercase tracking-wider text-slate-500 font-semibold">Storage Layer</div>
                    <div className="p-3 rounded-xl bg-slate-800/50 border border-slate-700/40">
                      <div className="flex items-center gap-2">
                        <Database className="w-4 h-4 text-emerald-400" />
                        <div>
                          <div className="text-[10px] text-slate-500">Primary Store</div>
                          <div className="text-xs font-semibold text-white">
                            {MEMORY_STORE_PROVIDERS.find((p) => p.key === preferences.primaryStore)?.displayName ?? preferences.primaryStore}
                          </div>
                        </div>
                      </div>
                    </div>
                    <div className="p-3 rounded-xl bg-slate-800/50 border border-slate-700/40">
                      <div className="flex items-center gap-2">
                        <Search className="w-4 h-4 text-violet-400" />
                        <div>
                          <div className="text-[10px] text-slate-500">Vector Search</div>
                          <div className="text-xs font-semibold text-white">
                            {VECTOR_SEARCH_PROVIDERS.find((p) => p.key === preferences.vectorSearchProvider)?.displayName ?? preferences.vectorSearchProvider}
                          </div>
                        </div>
                      </div>
                    </div>
                    <div className="p-3 rounded-xl bg-slate-800/50 border border-slate-700/40">
                      <div className="flex items-center gap-2">
                        <Layers className="w-4 h-4 text-amber-400" />
                        <div>
                          <div className="text-[10px] text-slate-500">Hybrid Mode</div>
                          <div className="text-xs font-semibold text-white">
                            {preferences.enableHybridMode ? `Enabled (${preferences.cacheStore ?? "none"})` : "Disabled"}
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* LLM summary */}
                  <div className="space-y-2">
                    <div className="text-[10px] uppercase tracking-wider text-slate-500 font-semibold">LLM Models</div>
                    {topAssignments.map(([uc, modelKey]) => {
                      const meta = LLM_USE_CASE_META[uc as LLMUseCase];
                      const model = LLM_MODELS.find((m) => m.key === modelKey);
                      return (
                        <div key={uc} className="p-3 rounded-xl bg-slate-800/50 border border-slate-700/40">
                          <div className="flex items-center justify-between">
                            <div>
                              <div className="text-[10px] text-slate-500">{meta?.label ?? uc}</div>
                              <div className="text-xs font-semibold text-white">{model?.displayName ?? modelKey}</div>
                            </div>
                            <span className="text-[10px] text-slate-400">{model?.provider}</span>
                          </div>
                        </div>
                      );
                    })}
                    {Object.keys(llmPreferences.modelAssignments).length > 5 && (
                      <div className="text-[10px] text-slate-500 text-center">
                        +{Object.keys(llmPreferences.modelAssignments).length - 5} more use cases configured
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })()}

          {/* Step: Complete */}
          {currentStep === "complete" && (
            <div className="text-center max-w-md mx-auto space-y-6 py-8">
              <div className="w-20 h-20 mx-auto rounded-2xl bg-gradient-to-br from-emerald-500/30 to-cyan-500/30 flex items-center justify-center border border-emerald-500/20">
                <Check className="w-10 h-10 text-emerald-400" />
              </div>
              <h2 className="text-2xl font-bold text-white">
                Setup Complete!
              </h2>
              <p className="text-slate-400">
                Your polyglot persistence layer is configured. You can change
                these settings anytime from the Settings panel.
              </p>
              <button
                onClick={dismiss}
                className="px-6 py-3 rounded-xl bg-gradient-to-r from-cyan-500 to-blue-500 text-white font-semibold text-sm hover:from-cyan-400 hover:to-blue-400 transition-all shadow-lg shadow-cyan-500/25"
              >
                Launch Dashboard
              </button>
            </div>
          )}
        </div>

        {/* Footer navigation */}
        {currentStep !== "complete" && (
          <div className="flex items-center justify-between px-6 py-4 border-t border-slate-800/60 bg-slate-900/50">
            <button
              onClick={currentStep === "welcome" ? dismiss : goBack}
              className="flex items-center gap-1.5 px-4 py-2 rounded-lg text-sm text-slate-400 hover:text-white hover:bg-slate-800/60 transition-colors"
            >
              {currentStep === "welcome" ? (
                <>
                  <X className="w-4 h-4" /> Skip Setup
                </>
              ) : (
                <>
                  <ChevronLeft className="w-4 h-4" /> Back
                </>
              )}
            </button>

            <span className="text-xs text-slate-500">
              Step {currentStepIndex + 1} of {totalSteps}
            </span>

            <button
              onClick={currentStep === "review" ? completeSetup : goNext}
              className="flex items-center gap-1.5 px-5 py-2 rounded-lg text-sm font-medium bg-cyan-500/20 text-cyan-300 border border-cyan-500/30 hover:bg-cyan-500/30 transition-colors"
            >
              {currentStep === "review" ? (
                <>
                  <Check className="w-4 h-4" /> Apply Configuration
                </>
              ) : (
                <>
                  Continue <ChevronRight className="w-4 h-4" />
                </>
              )}
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default SetupWizard;
