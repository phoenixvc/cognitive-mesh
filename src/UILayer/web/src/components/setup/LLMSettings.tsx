"use client";

import React, { useCallback, useState } from "react";
import {
  Brain,
  ChevronDown,
  ChevronUp,
  Key,
  Save,
  Check,
  RotateCcw,
  Shield,
  Eye,
  EyeOff,
} from "lucide-react";
import {
  type UserLLMPreferences,
  type LLMUseCase,
  type LLMProfile,
  LLM_MODELS,
  LLM_PROVIDERS,
  LLM_USE_CASE_META,
  LLM_DEFAULT_ASSIGNMENTS,
  LLMPreferencesService,
} from "@/services/llmPreferences";
import LLMModelCard from "./LLMModelCard";

const LLMSettings: React.FC = () => {
  const [preferences, setPreferences] = useState<UserLLMPreferences>(
    LLMPreferencesService.getInstance().getPreferences()
  );
  const [hasChanges, setHasChanges] = useState(false);
  const [savedFeedback, setSavedFeedback] = useState(false);
  const [expandedUseCase, setExpandedUseCase] = useState<string | null>(null);
  const [expandedProviders, setExpandedProviders] = useState(false);
  const [visibleKeys, setVisibleKeys] = useState<Set<string>>(new Set());
  const [activeProfile, setActiveProfile] = useState<LLMProfile | null>(null);

  const useCases = Object.keys(LLM_USE_CASE_META) as LLMUseCase[];

  const updateAssignment = useCallback(
    (useCase: LLMUseCase, modelKey: string) => {
      setPreferences((prev) => ({
        ...prev,
        modelAssignments: { ...prev.modelAssignments, [useCase]: modelKey },
      }));
      setHasChanges(true);
      setActiveProfile(null);
    },
    []
  );

  const updateApiKey = useCallback((provider: string, key: string) => {
    setPreferences((prev) => ({
      ...prev,
      providerApiKeys: { ...prev.providerApiKeys, [provider]: key },
    }));
    setHasChanges(true);
  }, []);

  const updateEndpoint = useCallback((provider: string, endpoint: string) => {
    setPreferences((prev) => ({
      ...prev,
      providerEndpoints: { ...prev.providerEndpoints, [provider]: endpoint },
    }));
    setHasChanges(true);
  }, []);

  const applyProfile = useCallback((profile: LLMProfile) => {
    setActiveProfile(profile);
    setPreferences((prev) => ({
      ...prev,
      modelAssignments: { ...LLM_DEFAULT_ASSIGNMENTS[profile] },
    }));
    setHasChanges(true);
  }, []);

  const saveAll = useCallback(() => {
    const final = { ...preferences, llmSetupCompleted: true };
    LLMPreferencesService.getInstance().savePreferences(final);
    setPreferences(final);
    setHasChanges(false);
    setSavedFeedback(true);
    setTimeout(() => setSavedFeedback(false), 2000);
  }, [preferences]);

  const resetDefaults = useCallback(() => {
    LLMPreferencesService.getInstance().resetToDefaults();
    const fresh = LLMPreferencesService.getInstance().getPreferences();
    setPreferences(fresh);
    setHasChanges(true);
    setActiveProfile(null);
  }, []);

  const toggleKeyVisibility = (provider: string) => {
    setVisibleKeys((prev) => {
      const next = new Set(prev);
      if (next.has(provider)) next.delete(provider);
      else next.add(provider);
      return next;
    });
  };

  // Determine which providers are actually in use
  const activeProviders = new Set(
    Object.values(preferences.modelAssignments)
      .map((modelKey) => LLM_MODELS.find((m) => m.key === modelKey)?.provider)
      .filter(Boolean) as string[]
  );

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-violet-500/20 to-cyan-500/20 flex items-center justify-center border border-violet-500/20">
            <Brain className="w-5 h-5 text-violet-400" />
          </div>
          <div>
            <h2 className="text-lg font-bold text-white">
              LLM Model Configuration
            </h2>
            <p className="text-xs text-slate-400">
              Assign different models per use case &mdash; polyglot AI routing
            </p>
          </div>
        </div>
      </div>

      {/* Profile presets */}
      <div className="flex flex-wrap gap-2">
        {(
          [
            { id: "balanced", label: "Balanced", desc: "Best mix of quality & cost" },
            { id: "performance", label: "Performance", desc: "Top-tier models everywhere" },
            { id: "cost-optimized", label: "Cost-Optimized", desc: "Budget-friendly choices" },
            { id: "open-source", label: "Open Source", desc: "Self-hostable models" },
          ] as const
        ).map((p) => (
          <button
            key={p.id}
            onClick={() => applyProfile(p.id)}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-all border ${
              activeProfile === p.id
                ? "bg-violet-500/20 text-violet-300 border-violet-500/40"
                : "bg-slate-800/40 text-slate-400 border-slate-700/40 hover:text-white hover:bg-slate-700/40"
            }`}
            title={p.desc}
          >
            {p.label}
          </button>
        ))}
      </div>

      {/* Per-use-case model assignments */}
      <div className="space-y-2">
        {useCases.map((uc) => {
          const meta = LLM_USE_CASE_META[uc];
          const assignedKey = preferences.modelAssignments[uc];
          const assignedModel = LLM_MODELS.find((m) => m.key === assignedKey);
          const isExpanded = expandedUseCase === uc;
          const recommendedModels = LLM_MODELS.filter((m) =>
            m.recommendedUseCases.includes(uc)
          ).sort((a, b) => b.decisionScore - a.decisionScore);
          const otherModels = LLM_MODELS.filter(
            (m) => !m.recommendedUseCases.includes(uc)
          );

          return (
            <div
              key={uc}
              className="border border-slate-700/40 rounded-xl overflow-hidden"
            >
              <button
                onClick={() => setExpandedUseCase(isExpanded ? null : uc)}
                className="w-full flex items-center justify-between p-3 bg-slate-800/30 hover:bg-slate-800/50 transition-colors"
              >
                <div className="flex items-center gap-3">
                  <div className="text-left">
                    <h3 className="text-sm font-semibold text-white">
                      {meta.label}
                    </h3>
                    <p className="text-[11px] text-slate-400">
                      {meta.description}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  {assignedModel && (
                    <span
                      className={`text-xs font-medium px-2 py-0.5 rounded-full ${
                        LLM_PROVIDERS[assignedModel.provider]
                          ? `bg-slate-700/60 text-slate-300`
                          : "bg-slate-700/60 text-slate-400"
                      }`}
                    >
                      {assignedModel.displayName}
                    </span>
                  )}
                  {isExpanded ? (
                    <ChevronUp className="w-4 h-4 text-slate-400" />
                  ) : (
                    <ChevronDown className="w-4 h-4 text-slate-400" />
                  )}
                </div>
              </button>

              {isExpanded && (
                <div className="p-3 space-y-3">
                  {recommendedModels.length > 0 && (
                    <div>
                      <div className="text-[10px] uppercase tracking-wider text-emerald-400/80 font-semibold mb-2">
                        Recommended for {meta.label}
                      </div>
                      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
                        {recommendedModels.map((model) => (
                          <LLMModelCard
                            key={model.key}
                            model={model}
                            isSelected={assignedKey === model.key}
                            onSelect={(key) => updateAssignment(uc, key)}
                            compact
                          />
                        ))}
                      </div>
                    </div>
                  )}
                  {otherModels.length > 0 && (
                    <div>
                      <div className="text-[10px] uppercase tracking-wider text-slate-500 font-semibold mb-2">
                        Other Models
                      </div>
                      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
                        {otherModels.map((model) => (
                          <LLMModelCard
                            key={model.key}
                            model={model}
                            isSelected={assignedKey === model.key}
                            onSelect={(key) => updateAssignment(uc, key)}
                            compact
                          />
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* Provider API Keys */}
      <div className="border border-slate-700/40 rounded-xl overflow-hidden">
        <button
          onClick={() => setExpandedProviders(!expandedProviders)}
          className="w-full flex items-center justify-between p-4 bg-slate-800/30 hover:bg-slate-800/50 transition-colors"
        >
          <div className="flex items-center gap-3">
            <Key className="w-5 h-5 text-amber-400" />
            <div className="text-left">
              <h3 className="text-sm font-semibold text-white">
                Provider API Keys &amp; Endpoints
              </h3>
              <p className="text-xs text-slate-400">
                Configure credentials for {activeProviders.size} active
                provider{activeProviders.size !== 1 ? "s" : ""}
              </p>
            </div>
          </div>
          {expandedProviders ? (
            <ChevronUp className="w-4 h-4 text-slate-400" />
          ) : (
            <ChevronDown className="w-4 h-4 text-slate-400" />
          )}
        </button>

        {expandedProviders && (
          <div className="p-4 space-y-3">
            <div className="flex items-center gap-2 text-xs text-amber-400/80 bg-amber-500/10 border border-amber-500/20 rounded-lg p-2.5">
              <Shield className="w-4 h-4 shrink-0" />
              <span>
                API keys are stored in your browser&apos;s localStorage only.
                For production, use environment variables or a secrets manager.
              </span>
            </div>

            {Array.from(activeProviders)
              .sort()
              .map((providerKey) => {
                const pInfo = LLM_PROVIDERS[providerKey];
                const currentKey =
                  preferences.providerApiKeys[providerKey] ?? "";
                const currentEndpoint =
                  preferences.providerEndpoints[providerKey] ?? "";
                const defaultEndpoint =
                  LLM_MODELS.find((m) => m.provider === providerKey)
                    ?.defaultEndpoint ?? "";
                const isKeyVisible = visibleKeys.has(providerKey);
                const requiresKey =
                  LLM_MODELS.find((m) => m.provider === providerKey)
                    ?.requiresApiKey ?? true;

                return (
                  <div
                    key={providerKey}
                    className="p-3 rounded-lg bg-slate-800/30 border border-slate-700/30 space-y-2"
                  >
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

                    {requiresKey && (
                      <div className="relative">
                        <input
                          type={isKeyVisible ? "text" : "password"}
                          value={currentKey}
                          onChange={(e) =>
                            updateApiKey(providerKey, e.target.value)
                          }
                          placeholder={`${pInfo?.displayName ?? providerKey} API key...`}
                          className="w-full px-3 py-2 pr-10 rounded-lg bg-slate-900/60 border border-slate-700/50 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500/50 font-mono"
                        />
                        <button
                          onClick={() => toggleKeyVisibility(providerKey)}
                          className="absolute right-2 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white"
                        >
                          {isKeyVisible ? (
                            <EyeOff className="w-4 h-4" />
                          ) : (
                            <Eye className="w-4 h-4" />
                          )}
                        </button>
                      </div>
                    )}

                    <input
                      type="text"
                      value={currentEndpoint}
                      onChange={(e) =>
                        updateEndpoint(providerKey, e.target.value)
                      }
                      placeholder={defaultEndpoint || "Custom endpoint (optional)"}
                      className="w-full px-3 py-2 rounded-lg bg-slate-900/60 border border-slate-700/50 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500/50 font-mono"
                    />
                  </div>
                );
              })}
          </div>
        )}
      </div>

      {/* Save / Reset */}
      <div className="flex items-center justify-between pt-2">
        <button
          onClick={resetDefaults}
          className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 transition-colors"
        >
          <RotateCcw className="w-3.5 h-3.5" /> Reset to Defaults
        </button>

        <button
          onClick={saveAll}
          disabled={!hasChanges}
          className={`flex items-center gap-1.5 px-5 py-2 rounded-lg text-sm font-medium transition-all ${
            savedFeedback
              ? "bg-emerald-500/20 text-emerald-300 border border-emerald-500/30"
              : hasChanges
                ? "bg-cyan-500/20 text-cyan-300 border border-cyan-500/30 hover:bg-cyan-500/30"
                : "bg-slate-800/40 text-slate-500 border border-slate-700/40 cursor-not-allowed"
          }`}
        >
          {savedFeedback ? (
            <>
              <Check className="w-4 h-4" /> Saved
            </>
          ) : (
            <>
              <Save className="w-4 h-4" /> Save LLM Config
            </>
          )}
        </button>
      </div>
    </div>
  );
};

export default LLMSettings;
