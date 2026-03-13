import { usePreferencesStore } from "./usePreferencesStore";
import { act } from "@testing-library/react";

// Reset store between tests — clear persisted state
beforeEach(() => {
  localStorage.clear();
  act(() => {
    usePreferencesStore.getState().resetDefaults();
  });
});

describe("usePreferencesStore", () => {
  it("should have correct default values", () => {
    const state = usePreferencesStore.getState();
    expect(state.theme).toBe("dark");
    expect(state.language).toBe("en-US");
    expect(state.sidebarCollapsed).toBe(false);
    expect(state.fontSize).toBe("medium");
    expect(state.soundEnabled).toBe(true);
    expect(state.notificationsEnabled).toBe(true);
    expect(state.reducedMotion).toBe(false);
    expect(state.highContrast).toBe(false);
  });

  it("should set theme", () => {
    act(() => {
      usePreferencesStore.getState().setTheme("light");
    });
    expect(usePreferencesStore.getState().theme).toBe("light");

    act(() => {
      usePreferencesStore.getState().setTheme("system");
    });
    expect(usePreferencesStore.getState().theme).toBe("system");
  });

  it("should set language", () => {
    act(() => {
      usePreferencesStore.getState().setLanguage("de-DE");
    });
    expect(usePreferencesStore.getState().language).toBe("de-DE");
  });

  it("should toggle sidebar", () => {
    expect(usePreferencesStore.getState().sidebarCollapsed).toBe(false);
    act(() => {
      usePreferencesStore.getState().toggleSidebar();
    });
    expect(usePreferencesStore.getState().sidebarCollapsed).toBe(true);
    act(() => {
      usePreferencesStore.getState().toggleSidebar();
    });
    expect(usePreferencesStore.getState().sidebarCollapsed).toBe(false);
  });

  it("should set privacy consent and update timestamp", () => {
    const before = Date.now();
    act(() => {
      usePreferencesStore.getState().setPrivacyConsent({
        analytics: true,
        telemetry: true,
      });
    });
    const consent = usePreferencesStore.getState().privacyConsent;
    expect(consent.analytics).toBe(true);
    expect(consent.telemetry).toBe(true);
    expect(consent.personalizedContent).toBe(false); // unchanged default
    expect(consent.updatedAt).toBeGreaterThanOrEqual(before);
  });

  it("should set font size", () => {
    act(() => {
      usePreferencesStore.getState().setFontSize("large");
    });
    expect(usePreferencesStore.getState().fontSize).toBe("large");
  });

  it("should set GDPR consent and replace existing consent of same type", () => {
    act(() => {
      usePreferencesStore.getState().setGdprConsent("marketing", true);
    });
    expect(usePreferencesStore.getState().gdprConsents).toHaveLength(1);
    expect(usePreferencesStore.getState().gdprConsents[0].type).toBe("marketing");
    expect(usePreferencesStore.getState().gdprConsents[0].granted).toBe(true);

    // Update same type
    act(() => {
      usePreferencesStore.getState().setGdprConsent("marketing", false);
    });
    expect(usePreferencesStore.getState().gdprConsents).toHaveLength(1);
    expect(usePreferencesStore.getState().gdprConsents[0].granted).toBe(false);
  });

  it("should set notification channel individually", () => {
    act(() => {
      usePreferencesStore.getState().setNotificationChannel("sms", true);
    });
    expect(usePreferencesStore.getState().notificationPreferences.channels.sms).toBe(true);
    // Other channels unchanged
    expect(usePreferencesStore.getState().notificationPreferences.channels.email).toBe(true);
  });

  it("should reset to defaults", () => {
    act(() => {
      usePreferencesStore.getState().setTheme("light");
      usePreferencesStore.getState().setLanguage("fr-FR");
      usePreferencesStore.getState().setFontSize("large");
    });
    expect(usePreferencesStore.getState().theme).toBe("light");

    act(() => {
      usePreferencesStore.getState().resetDefaults();
    });
    const state = usePreferencesStore.getState();
    expect(state.theme).toBe("dark");
    expect(state.language).toBe("en-US");
    expect(state.fontSize).toBe("medium");
  });
});
