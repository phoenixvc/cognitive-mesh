"use client";

import React from "react";
import { Check, X, Server, HardDrive, Star, Zap } from "lucide-react";
import type { StorageProviderInfo } from "@/services/preferences";

interface StorageProviderCardProps {
  provider: StorageProviderInfo;
  isSelected: boolean;
  onSelect: (key: string) => void;
  compact?: boolean;
}

const StorageProviderCard: React.FC<StorageProviderCardProps> = ({
  provider,
  isSelected,
  onSelect,
  compact = false,
}) => {
  const scoreColor =
    provider.decisionScore >= 3.8
      ? "text-emerald-400"
      : provider.decisionScore >= 3.4
        ? "text-cyan-400"
        : provider.decisionScore >= 3.0
          ? "text-amber-400"
          : "text-slate-400";

  const scoreBarWidth = (provider.decisionScore / 5) * 100;

  return (
    <div
      onClick={() => onSelect(provider.key)}
      className={`relative p-4 rounded-xl border cursor-pointer transition-all duration-300 group ${
        isSelected
          ? "bg-cyan-500/15 border-cyan-500/60 shadow-lg shadow-cyan-500/10 ring-1 ring-cyan-500/30"
          : "bg-slate-800/40 border-slate-700/50 hover:bg-slate-700/40 hover:border-slate-600/60"
      }`}
    >
      {/* Selection indicator */}
      {isSelected && (
        <div className="absolute top-3 right-3 w-6 h-6 rounded-full bg-cyan-500 flex items-center justify-center">
          <Check className="w-4 h-4 text-white" />
        </div>
      )}

      {/* Header */}
      <div className="flex items-start gap-3 mb-3">
        <div
          className={`w-10 h-10 rounded-lg flex items-center justify-center ${
            provider.requiresExternalService
              ? "bg-purple-500/20 text-purple-400"
              : "bg-emerald-500/20 text-emerald-400"
          }`}
        >
          {provider.requiresExternalService ? (
            <Server className="w-5 h-5" />
          ) : (
            <HardDrive className="w-5 h-5" />
          )}
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-sm font-semibold text-white truncate">
            {provider.displayName}
          </h3>
          <p className="text-xs text-slate-400 mt-0.5 line-clamp-2">
            {provider.description}
          </p>
        </div>
      </div>

      {/* Score bar */}
      <div className="mb-3">
        <div className="flex items-center justify-between mb-1">
          <span className="text-xs text-slate-500 flex items-center gap-1">
            <Star className="w-3 h-3" /> ADR Score
          </span>
          <span className={`text-xs font-mono font-bold ${scoreColor}`}>
            {provider.decisionScore.toFixed(2)}
          </span>
        </div>
        <div className="h-1.5 bg-slate-700/60 rounded-full overflow-hidden">
          <div
            className={`h-full rounded-full transition-all duration-500 ${
              provider.decisionScore >= 3.8
                ? "bg-gradient-to-r from-emerald-500 to-emerald-400"
                : provider.decisionScore >= 3.4
                  ? "bg-gradient-to-r from-cyan-500 to-cyan-400"
                  : provider.decisionScore >= 3.0
                    ? "bg-gradient-to-r from-amber-500 to-amber-400"
                    : "bg-gradient-to-r from-slate-500 to-slate-400"
            }`}
            style={{ width: `${scoreBarWidth}%` }}
          />
        </div>
      </div>

      {/* Tags */}
      <div className="flex flex-wrap gap-1.5 mb-3">
        {provider.recommendedFor.map((tag) => (
          <span
            key={tag}
            className="px-2 py-0.5 text-[10px] font-medium rounded-full bg-slate-700/60 text-slate-300 border border-slate-600/40"
          >
            {tag}
          </span>
        ))}
        {provider.supportsVectorSearch && (
          <span className="px-2 py-0.5 text-[10px] font-medium rounded-full bg-violet-500/20 text-violet-300 border border-violet-500/30 flex items-center gap-1">
            <Zap className="w-2.5 h-2.5" /> Vector
          </span>
        )}
      </div>

      {/* Pros / Cons (collapsible in compact mode) */}
      {!compact && (
        <div className="space-y-2 mt-3 pt-3 border-t border-slate-700/40">
          <div>
            <span className="text-[10px] uppercase tracking-wider text-emerald-400/80 font-semibold">
              Advantages
            </span>
            <ul className="mt-1 space-y-0.5">
              {provider.pros.slice(0, 3).map((pro) => (
                <li
                  key={pro}
                  className="flex items-start gap-1.5 text-xs text-slate-300"
                >
                  <Check className="w-3 h-3 text-emerald-400 mt-0.5 shrink-0" />
                  <span>{pro}</span>
                </li>
              ))}
            </ul>
          </div>
          <div>
            <span className="text-[10px] uppercase tracking-wider text-rose-400/80 font-semibold">
              Limitations
            </span>
            <ul className="mt-1 space-y-0.5">
              {provider.cons.slice(0, 2).map((con) => (
                <li
                  key={con}
                  className="flex items-start gap-1.5 text-xs text-slate-400"
                >
                  <X className="w-3 h-3 text-rose-400 mt-0.5 shrink-0" />
                  <span>{con}</span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}
    </div>
  );
};

export default StorageProviderCard;
