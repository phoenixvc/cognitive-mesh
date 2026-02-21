"use client";

import React, { useCallback } from "react";
import { ArrowLeft } from "lucide-react";
import DatabaseSettings from "@/components/setup/DatabaseSettings";

export default function SettingsPage() {
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

        <DatabaseSettings
          onRerunWizard={handleRerunWizard}
          onStartTour={handleStartTour}
        />
      </div>
    </div>
  );
}
