import { useCallback, useEffect, useState } from "react";
import {
  type UserStoragePreferences,
  PreferencesService,
} from "@/services/preferences";

export type WizardStep =
  | "welcome"
  | "use-case"
  | "primary-store"
  | "vector-provider"
  | "cache-layer"
  | "review"
  | "complete";

const STEP_ORDER: WizardStep[] = [
  "welcome",
  "use-case",
  "primary-store",
  "vector-provider",
  "cache-layer",
  "review",
  "complete",
];

export function useSetupWizard() {
  const [currentStep, setCurrentStep] = useState<WizardStep>("welcome");
  const [preferences, setPreferences] = useState<UserStoragePreferences>(
    PreferencesService.getInstance().getPreferences()
  );
  const [useCase, setUseCase] = useState<
    "development" | "production" | "cloud" | "testing" | null
  >(null);
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
      const defaults =
        PreferencesService.getInstance().getRecommendedConfig(uc);
      setPreferences((prev) => ({ ...prev, ...defaults }));
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

  const completeSetup = useCallback(() => {
    const service = PreferencesService.getInstance();
    const finalPrefs = { ...preferences, setupCompleted: true };
    service.savePreferences(finalPrefs);
    setPreferences(finalPrefs);
    setCurrentStep("complete");
  }, [preferences]);

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
    useCase,
    isVisible,
    goNext,
    goBack,
    applyUseCaseDefaults,
    updatePreference,
    completeSetup,
    dismiss,
    reopen,
  };
}
