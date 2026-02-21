"use client";

import React from "react";
import { Check, Star, Wrench, Eye, Layers, Zap } from "lucide-react";
import type { LLMModelInfo } from "@/services/llmPreferences";
import { LLM_PROVIDERS } from "@/services/llmPreferences";

interface LLMModelCardProps {
  model: LLMModelInfo;
  isSelected: boolean;
  onSelect: (key: string) => void;
  compact?: boolean;
}

const providerColorMap: Record<string, { bg: string; border: string; text: string }> = {
  anthropic: { bg: "bg-amber-500/20", border: "border-amber-500/40", text: "text-amber-300" },
  openai: { bg: "bg-emerald-500/20", border: "border-emerald-500/40", text: "text-emerald-300" },
  google: { bg: "bg-blue-500/20", border: "border-blue-500/40", text: "text-blue-300" },
  minimax: { bg: "bg-violet-500/20", border: "border-violet-500/40", text: "text-violet-300" },
  deepseek: { bg: "bg-cyan-500/20", border: "border-cyan-500/40", text: "text-cyan-300" },
  meta: { bg: "bg-indigo-500/20", border: "border-indigo-500/40", text: "text-indigo-300" },
  xai: { bg: "bg-rose-500/20", border: "border-rose-500/40", text: "text-rose-300" },
  "azure-openai": { bg: "bg-sky-500/20", border: "border-sky-500/40", text: "text-sky-300" },
};

function formatTokens(n: number): string {
  if (n >= 1_000_000) return `${(n / 1_000_000).toFixed(0)}M`;
  if (n >= 1_000) return `${(n / 1_000).toFixed(0)}K`;
  return `${n}`;
}

const LLMModelCard: React.FC<LLMModelCardProps> = ({
  model,
  isSelected,
  onSelect,
  compact = false,
}) => {
  const scoreColor =
    model.decisionScore >= 4.3
      ? "text-emerald-400"
      : model.decisionScore >= 3.8
        ? "text-cyan-400"
        : model.decisionScore >= 3.4
          ? "text-amber-400"
          : "text-slate-400";

  const scoreBarWidth = (model.decisionScore / 5) * 100;
  const pColors = providerColorMap[model.provider] ?? providerColorMap.openai;

  return (
    <div
      onClick={() => onSelect(model.key)}
      className={`relative p-4 rounded-xl border cursor-pointer transition-all duration-300 ${
        isSelected
          ? "bg-cyan-500/15 border-cyan-500/60 shadow-lg shadow-cyan-500/10 ring-1 ring-cyan-500/30"
          : "bg-slate-800/40 border-slate-700/50 hover:bg-slate-700/40 hover:border-slate-600/60"
      }`}
    >
      {isSelected && (
        <div className="absolute top-3 right-3 w-6 h-6 rounded-full bg-cyan-500 flex items-center justify-center">
          <Check className="w-4 h-4 text-white" />
        </div>
      )}

      {/* Header */}
      <div className="flex items-start gap-3 mb-2">
        <div className={`px-2 py-0.5 rounded-full text-[10px] font-semibold ${pColors.bg} ${pColors.border} ${pColors.text} border`}>
          {LLM_PROVIDERS[model.provider]?.displayName ?? model.provider}
        </div>
      </div>

      <h3 className="text-sm font-semibold text-white mb-1">{model.displayName}</h3>
      <p className="text-xs text-slate-400 line-clamp-2 mb-2">{model.description}</p>

      {/* Score bar */}
      <div className="mb-2">
        <div className="flex items-center justify-between mb-1">
          <span className="text-xs text-slate-500 flex items-center gap-1">
            <Star className="w-3 h-3" /> ADR Score
          </span>
          <span className={`text-xs font-mono font-bold ${scoreColor}`}>
            {model.decisionScore.toFixed(2)}
          </span>
        </div>
        <div className="h-1.5 bg-slate-700/60 rounded-full overflow-hidden">
          <div
            className={`h-full rounded-full transition-all duration-500 ${
              model.decisionScore >= 4.3
                ? "bg-gradient-to-r from-emerald-500 to-emerald-400"
                : model.decisionScore >= 3.8
                  ? "bg-gradient-to-r from-cyan-500 to-cyan-400"
                  : model.decisionScore >= 3.4
                    ? "bg-gradient-to-r from-amber-500 to-amber-400"
                    : "bg-gradient-to-r from-slate-500 to-slate-400"
            }`}
            style={{ width: `${scoreBarWidth}%` }}
          />
        </div>
      </div>

      {/* Capabilities badges */}
      <div className="flex flex-wrap gap-1.5 mb-2">
        <span className="px-1.5 py-0.5 text-[10px] rounded-full bg-slate-700/60 text-slate-300 border border-slate-600/40">
          {formatTokens(model.maxContextTokens)} ctx
        </span>
        {model.supportsToolCalling && (
          <span className="px-1.5 py-0.5 text-[10px] rounded-full bg-violet-500/15 text-violet-300 border border-violet-500/25 flex items-center gap-0.5">
            <Wrench className="w-2.5 h-2.5" /> Tools
          </span>
        )}
        {model.supportsVision && (
          <span className="px-1.5 py-0.5 text-[10px] rounded-full bg-blue-500/15 text-blue-300 border border-blue-500/25 flex items-center gap-0.5">
            <Eye className="w-2.5 h-2.5" /> Vision
          </span>
        )}
        {model.supportsEmbeddings && (
          <span className="px-1.5 py-0.5 text-[10px] rounded-full bg-emerald-500/15 text-emerald-300 border border-emerald-500/25 flex items-center gap-0.5">
            <Layers className="w-2.5 h-2.5" /> Embed
          </span>
        )}
      </div>

      {/* Pros / Cons */}
      {!compact && (
        <div className="space-y-1.5 mt-2 pt-2 border-t border-slate-700/40">
          <div>
            <ul className="space-y-0.5">
              {model.pros.slice(0, 3).map((pro) => (
                <li key={pro} className="flex items-start gap-1.5 text-xs text-slate-300">
                  <Check className="w-3 h-3 text-emerald-400 mt-0.5 shrink-0" />
                  <span>{pro}</span>
                </li>
              ))}
            </ul>
          </div>
          <ul className="space-y-0.5">
            {model.cons.slice(0, 2).map((con) => (
              <li key={con} className="flex items-start gap-1.5 text-xs text-slate-400">
                <Zap className="w-3 h-3 text-amber-400 mt-0.5 shrink-0" />
                <span>{con}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};

export default LLMModelCard;
