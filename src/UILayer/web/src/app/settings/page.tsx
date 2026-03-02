"use client";

import React, { useCallback, useState } from "react";
import { ArrowLeft, Database, Brain } from "lucide-react";
import DatabaseSettings from "@/components/setup/DatabaseSettings";
import LLMSettings from "@/components/setup/LLMSettings";

type SettingsTab = "storage" | "llm";

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState<SettingsTab>("storage");

  const handleRerunWizard = useCallback(() => {
    window.location.href = "/setup";
  }, []);

  const handleStartTour = useCallback(() => {
    window.dispatchEvent(new CustomEvent("cognitive-mesh:start-tour"));
  }, []);

  return (
    <div className="min-h-screen bg-slate-950 p-6">
      <div className="max-w-4xl mx-auto">
        <a
          href="/"
          className="inline-flex items-center gap-1.5 text-sm text-slate-400 hover:text-white transition-colors mb-6"
        >
          <ArrowLeft className="w-4 h-4" /> Back to Dashboard
        </a>

        <h1 className="text-2xl font-bold text-white mb-6">Settings</h1>

        {/* Tab Navigation */}
        <div className="flex gap-1 mb-6 bg-slate-900/50 rounded-lg p-1 w-fit">
          <button
            onClick={() => setActiveTab("storage")}
            className={`flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              activeTab === "storage"
                ? "bg-blue-600 text-white"
                : "text-slate-400 hover:text-white hover:bg-slate-800"
            }`}
          >
            <Database className="w-4 h-4" />
            Storage & Databases
          </button>
          <button
            onClick={() => setActiveTab("llm")}
            className={`flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              activeTab === "llm"
                ? "bg-purple-600 text-white"
                : "text-slate-400 hover:text-white hover:bg-slate-800"
            }`}
          >
            <Brain className="w-4 h-4" />
            LLM Models & Providers
          </button>
        </div>

        {/* Tab Content */}
        {activeTab === "storage" && (
          <DatabaseSettings
            onRerunWizard={handleRerunWizard}
            onStartTour={handleStartTour}
          />
        )}
        {activeTab === "llm" && <LLMSettings />}
      </div>
    </div>
  );
}
