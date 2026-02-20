import React from "react";

interface VoiceFeedbackProps {
  isVoiceActive: boolean;
  voiceFeedback: string;
  particleEffectsEnabled: boolean;
  effectSpeed: number;
}

const VoiceFeedback: React.FC<VoiceFeedbackProps> = ({
  isVoiceActive,
  voiceFeedback,
  particleEffectsEnabled,
  effectSpeed,
}) => {
  return (
    <>
      {/* Voice Activation Feedback */}
      {isVoiceActive && (
        <div className="fixed top-1/2 left-8 transform -translate-y-1/2 z-50">
          <div className="backdrop-blur-md bg-red-500/20 border border-red-500/50 rounded-xl p-4 flex items-center space-x-3">
            <div
              className={`w-3 h-3 bg-red-400 rounded-full ${particleEffectsEnabled ? 'animate-pulse' : ''}`}
              style={{
                animationDuration: particleEffectsEnabled ? `${1 / effectSpeed}s` : undefined,
              }}
            />
            <span className="text-red-400 font-semibold">VOICE RECOGNITION ACTIVE</span>
          </div>
        </div>
      )}

      {/* Voice Feedback Message */}
      {voiceFeedback && (
        <div className="fixed bottom-8 left-1/2 transform -translate-x-1/2 z-50">
          <div className="backdrop-blur-md bg-slate-900/90 border border-cyan-500/50 rounded-lg px-4 py-2">
            <span className="text-cyan-400 text-sm">{voiceFeedback}</span>
          </div>
        </div>
      )}
    </>
  );
};

export default VoiceFeedback; 