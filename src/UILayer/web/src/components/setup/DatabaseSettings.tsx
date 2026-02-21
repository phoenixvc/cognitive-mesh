"use client";

import React, { useCallback, useEffect, useState } from "react";
import {
  Database,
  Search,
  Layers,
  Settings,
  Save,
  RotateCcw,
  Check,
  ChevronDown,
  ChevronUp,
  Sparkles,
} from "lucide-react";
import {
  type UserStoragePreferences,
  type StorageProviderInfo,
  MEMORY_STORE_PROVIDERS,
  VECTOR_SEARCH_PROVIDERS,
  PreferencesService,
} from "@/services/preferences";
import StorageProviderCard from "./StorageProviderCard";

interface DatabaseSettingsProps {
  onRerunWizard?: () => void;
  onStartTour?: () => void;
}

const DatabaseSettings: React.FC<DatabaseSettingsProps> = ({
  onRerunWizard,
  onStartTour,
}) => {
  const [preferences, setPreferences] = useState<UserStoragePreferences>(
    PreferencesService.getInstance().getPreferences()
  );
  const [hasChanges, setHasChanges] = useState(false);
  const [savedFeedback, setSavedFeedback] = useState(false);
  const [expandedSection, setExpandedSection] = useState<string | null>(
    "primary"
  );

  const updatePreference = useCallback(
    <K extends keyof UserStoragePreferences>(
      key: K,
      value: UserStoragePreferences[K]
    ) => {
      setPreferences((prev) => ({ ...prev, [key]: value }));
      setHasChanges(true);
    },
    []
  );

  const savePreferences = useCallback(() => {
    PreferencesService.getInstance().savePreferences(preferences);
    setHasChanges(false);
    setSavedFeedback(true);
    setTimeout(() => setSavedFeedback(false), 2000);
  }, [preferences]);

  const resetToDefaults = useCallback(() => {
    PreferencesService.getInstance().resetSetup();
    const fresh = PreferencesService.getInstance().getPreferences();
    setPreferences(fresh);
    setHasChanges(true);
  }, []);

  const toggleSection = (section: string) => {
    setExpandedSection(expandedSection === section ? null : section);
  };

  const renderSection = (
    id: string,
    title: string,
    icon: React.ElementType,
    iconColor: string,
    description: string,
    providers: StorageProviderInfo[],
    selectedKey: string,
    onSelect: (key: string) => void
  ) => {
    const Icon = icon;
    const isExpanded = expandedSection === id;

    return (
      <div className="border border-slate-700/40 rounded-xl overflow-hidden">
        <button
          onClick={() => toggleSection(id)}
          className="w-full flex items-center justify-between p-4 bg-slate-800/30 hover:bg-slate-800/50 transition-colors"
        >
          <div className="flex items-center gap-3">
            <Icon className={`w-5 h-5 ${iconColor}`} />
            <div className="text-left">
              <h3 className="text-sm font-semibold text-white">{title}</h3>
              <p className="text-xs text-slate-400">{description}</p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs font-medium px-2 py-0.5 rounded-full bg-slate-700/60 text-slate-300">
              {providers.find((p) => p.key === selectedKey)?.displayName ??
                selectedKey}
            </span>
            {isExpanded ? (
              <ChevronUp className="w-4 h-4 text-slate-400" />
            ) : (
              <ChevronDown className="w-4 h-4 text-slate-400" />
            )}
          </div>
        </button>

        {isExpanded && (
          <div className="p-4 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
            {providers.map((provider) => (
              <StorageProviderCard
                key={provider.key}
                provider={provider}
                isSelected={selectedKey === provider.key}
                onSelect={onSelect}
                compact
              />
            ))}
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-500/20 to-violet-500/20 flex items-center justify-center border border-cyan-500/20">
            <Settings className="w-5 h-5 text-cyan-400" />
          </div>
          <div>
            <h2 className="text-lg font-bold text-white">
              Database Configuration
            </h2>
            <p className="text-xs text-slate-400">
              Polyglot persistence &mdash; select databases for each role
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          {onStartTour && (
            <button
              onClick={onStartTour}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs text-slate-400 hover:text-white hover:bg-slate-800 transition-colors border border-slate-700/40"
            >
              <Sparkles className="w-3.5 h-3.5" /> Tour
            </button>
          )}
          {onRerunWizard && (
            <button
              onClick={onRerunWizard}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs text-slate-400 hover:text-white hover:bg-slate-800 transition-colors border border-slate-700/40"
            >
              <RotateCcw className="w-3.5 h-3.5" /> Re-run Wizard
            </button>
          )}
        </div>
      </div>

      {/* Sections */}
      {renderSection(
        "primary",
        "Primary Memory Store",
        Database,
        "text-emerald-400",
        "Main database for context persistence and key-value storage",
        MEMORY_STORE_PROVIDERS,
        preferences.primaryStore,
        (key) => updatePreference("primaryStore", key)
      )}

      {renderSection(
        "vector",
        "Vector Search Provider",
        Search,
        "text-violet-400",
        "Semantic similarity search, RAG retrieval, and embedding lookups",
        VECTOR_SEARCH_PROVIDERS,
        preferences.vectorSearchProvider,
        (key) => updatePreference("vectorSearchProvider", key)
      )}

      {/* Hybrid mode section */}
      <div className="border border-slate-700/40 rounded-xl overflow-hidden">
        <button
          onClick={() => toggleSection("hybrid")}
          className="w-full flex items-center justify-between p-4 bg-slate-800/30 hover:bg-slate-800/50 transition-colors"
        >
          <div className="flex items-center gap-3">
            <Layers className="w-5 h-5 text-amber-400" />
            <div className="text-left">
              <h3 className="text-sm font-semibold text-white">
                Cache &amp; Hybrid Mode
              </h3>
              <p className="text-xs text-slate-400">
                Optional cache layer with dual-write for low-latency reads
              </p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <span
              className={`text-xs font-medium px-2 py-0.5 rounded-full ${
                preferences.enableHybridMode
                  ? "bg-amber-500/20 text-amber-300"
                  : "bg-slate-700/60 text-slate-400"
              }`}
            >
              {preferences.enableHybridMode ? "Enabled" : "Disabled"}
            </span>
            {expandedSection === "hybrid" ? (
              <ChevronUp className="w-4 h-4 text-slate-400" />
            ) : (
              <ChevronDown className="w-4 h-4 text-slate-400" />
            )}
          </div>
        </button>

        {expandedSection === "hybrid" && (
          <div className="p-4 space-y-3">
            <div
              onClick={() =>
                updatePreference(
                  "enableHybridMode",
                  !preferences.enableHybridMode
                )
              }
              className={`p-3 rounded-lg border cursor-pointer transition-all ${
                preferences.enableHybridMode
                  ? "bg-amber-500/10 border-amber-500/40"
                  : "bg-slate-800/40 border-slate-700/50"
              }`}
            >
              <div className="flex items-center justify-between">
                <span className="text-sm text-white">Enable Hybrid Mode</span>
                <div
                  className={`w-10 h-6 rounded-full transition-colors flex items-center px-1 ${
                    preferences.enableHybridMode
                      ? "bg-amber-500"
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
              <div
                onClick={() =>
                  updatePreference(
                    "preferCacheForRetrieval",
                    !preferences.preferCacheForRetrieval
                  )
                }
                className={`p-3 rounded-lg border cursor-pointer transition-all ${
                  preferences.preferCacheForRetrieval
                    ? "bg-amber-500/10 border-amber-500/30"
                    : "bg-slate-800/40 border-slate-700/50"
                }`}
              >
                <div className="flex items-center justify-between">
                  <span className="text-sm text-white">
                    Prefer cache for reads
                  </span>
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
          </div>
        )}
      </div>

      {/* Save / Reset bar */}
      <div className="flex items-center justify-between pt-2">
        <button
          onClick={resetToDefaults}
          className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 transition-colors"
        >
          <RotateCcw className="w-3.5 h-3.5" /> Reset to Defaults
        </button>

        <button
          onClick={savePreferences}
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
              <Save className="w-4 h-4" /> Save Changes
            </>
          )}
        </button>
      </div>
    </div>
  );
};

export default DatabaseSettings;
