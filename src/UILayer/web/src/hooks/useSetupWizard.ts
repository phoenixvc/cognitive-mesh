import { useCallback, useEffect, useState } from "react";
import {
  type UserStoragePreferences,
  PreferencesService,
} from "@/services/preferences";
import {
  type UserLLMPreferences,
  type LLMProfile,
  LLM_DEFAULT_ASSIGNMENTS,
  LLMPreferencesService,
} from "@/services/llmPreferences";

export type WizardStep =
  | "welcome"
  | "use-case"
  | "primary-store"
  | "vector-provider"
  | "cache-layer"
  | "llm-profile"
  | "llm-models"
  | "llm-api-keys"
  | "review"
  | "complete";

const STEP_ORDER: WizardStep[] = [
  "welcome",
  "use-case",
  "primary-store",
  "vector-provider",
  "cache-layer",
  "llm-profile",
  "llm-models",
  "llm-api-keys",
  "review",
  "complete",
];

export function useSetupWizard() {
  const [currentStep, setCurrentStep] = useState<WizardStep>("welcome");
  const [preferences, setPreferences] = useState<UserStoragePreferences>(
    PreferencesService.getInstance().getPreferences()
  );
  const [llmPreferences, setLlmPreferences] = useState<UserLLMPreferences>(
    LLMPreferencesService.getInstance().getPreferences()
  );
  const [useCase, setUseCase] = useState<
    "development" | "production" | "cloud" | "testing" | null
  >(null);
  const [llmProfile, setLlmProfile] = useState<LLMProfile>("balanced");
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const service = PreferencesService.getInstance();
    if (!service.isSetupCompleted()) {
      setIsVisible(true);
    }
  }, []);

  const currentStepIndex = STEP_ORDER.indexOf(currentStep);
  const totalSteps = STEP_ORDER.length;
  const progress = ((currentStepIndex + 1) / totalSteps) * 100;

  const goNext = useCallback(() => {
    const idx = STEP_ORDER.indexOf(currentStep);
    if (idx < STEP_ORDER.length - 1) {
      setCurrentStep(STEP_ORDER[idx + 1]);
    }
  }, [currentStep]);

  const goBack = useCallback(() => {
    const idx = STEP_ORDER.indexOf(currentStep);
    if (idx > 0) {
      setCurrentStep(STEP_ORDER[idx - 1]);
    }
  }, [currentStep]);

  const applyUseCaseDefaults = useCallback(
    (uc: "development" | "production" | "cloud" | "testing") => {
      setUseCase(uc);
      const storageDefaults =
        PreferencesService.getInstance().getRecommendedConfig(uc);
      setPreferences((prev) => ({ ...prev, ...storageDefaults }));

      // Map use case to LLM profile
      const profileMap: Record<string, LLMProfile> = {
        development: "balanced",
        production: "performance",
        cloud: "performance",
        testing: "cost-optimized",
      };
      const profile = profileMap[uc] ?? "balanced";
      setLlmProfile(profile);
      setLlmPreferences((prev) => ({
        ...prev,
        modelAssignments: { ...LLM_DEFAULT_ASSIGNMENTS[profile] },
      }));
    },
    []
  );

  const updatePreference = useCallback(
    <K extends keyof UserStoragePreferences>(
      key: K,
      value: UserStoragePreferences[K]
    ) => {
      setPreferences((prev) => ({ ...prev, [key]: value }));
    },
    []
  );

  const updateLlmPreference = useCallback(
    <K extends keyof UserLLMPreferences>(
      key: K,
      value: UserLLMPreferences[K]
    ) => {
      setLlmPreferences((prev) => ({ ...prev, [key]: value }));
    },
    []
  );

  const applyLlmProfile = useCallback((profile: LLMProfile) => {
    setLlmProfile(profile);
    setLlmPreferences((prev) => ({
      ...prev,
      modelAssignments: { ...LLM_DEFAULT_ASSIGNMENTS[profile] },
    }));
  }, []);

  const updateModelAssignment = useCallback(
    (useCase: string, modelKey: string) => {
      setLlmPreferences((prev) => ({
        ...prev,
        modelAssignments: { ...prev.modelAssignments, [useCase]: modelKey },
      }));
    },
    []
  );

  const updateProviderApiKey = useCallback(
    (provider: string, apiKey: string) => {
      setLlmPreferences((prev) => ({
        ...prev,
        providerApiKeys: { ...prev.providerApiKeys, [provider]: apiKey },
      }));
    },
    []
  );

  const completeSetup = useCallback(() => {
    // Save storage preferences
    const storageService = PreferencesService.getInstance();
    const finalStoragePrefs = { ...preferences, setupCompleted: true };
    storageService.savePreferences(finalStoragePrefs);
    setPreferences(finalStoragePrefs);

    // Save LLM preferences
    const llmService = LLMPreferencesService.getInstance();
    const finalLlmPrefs = { ...llmPreferences, llmSetupCompleted: true };
    llmService.savePreferences(finalLlmPrefs);
    setLlmPreferences(finalLlmPrefs);

    setCurrentStep("complete");
  }, [preferences, llmPreferences]);

  const dismiss = useCallback(() => {
    setIsVisible(false);
  }, []);

  const reopen = useCallback(() => {
    setCurrentStep("welcome");
    setIsVisible(true);
  }, []);

  return {
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
    updateLlmPreference,
    applyLlmProfile,
    updateModelAssignment,
    updateProviderApiKey,
    completeSetup,
    dismiss,
    reopen,
  };
}
