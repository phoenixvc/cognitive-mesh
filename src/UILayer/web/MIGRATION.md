# Frontend Migration Tracker — Phase 14b

> Final status of all frontend assets after Phase 14b (UI Component Library Integration).
> Updated: 2026-03-11 — **Phase 14b complete**

## Status Legend

| Status | Meaning |
|--------|---------|
| **done** | Migrated and passing TypeScript checks |
| **migrating** | Work in progress, may have TS errors |
| **blocked** | Depends on missing backend or external work |
| **deferred** | Intentionally skipped for a later phase |
| **removed** | Dead code deleted during cleanup |
| **legacy** | Working but should be replaced in a future phase |

---

## Shadcn/UI Components (48 files)

All moved from root `components/ui/` → `src/components/ui/`. Radix-UI deps installed. TS validation enabled (removed from tsconfig exclude).

| Component | Status | Notes |
|-----------|--------|-------|
| accordion | done | |
| alert | done | |
| alert-dialog | done | |
| aspect-ratio | done | |
| avatar | done | |
| badge | done | |
| breadcrumb | done | |
| button | done | |
| calendar | done | Rewritten for react-day-picker v9 (Chevron API) |
| card | done | |
| carousel | done | |
| chart | done | Rewritten for recharts v3 (explicit payload types) |
| checkbox | done | |
| collapsible | done | |
| command | done | cmdk installed |
| context-menu | done | |
| dialog | done | |
| drawer | done | vaul installed |
| dropdown-menu | done | |
| form | done | react-hook-form installed |
| hover-card | done | |
| input | done | |
| input-otp | done | input-otp installed |
| label | done | |
| menubar | done | |
| navigation-menu | done | |
| pagination | done | |
| popover | done | |
| progress | done | |
| radio-group | done | |
| resizable | done | Rewritten for react-resizable-panels v4 (Group/Separator API) |
| scroll-area | done | |
| select | done | |
| separator | done | |
| sheet | done | |
| sidebar | done | |
| skeleton | done | Shadcn version (simple). Custom `Skeleton/` is the primary system |
| slider | done | |
| sonner | done | sonner installed, not yet wired into layout |
| switch | done | |
| table | done | |
| tabs | done | |
| textarea | done | |
| toast | done | Radix toast primitives. Legacy `Toast/` is the active system |
| toaster | done | Radix toaster. Not yet replacing legacy Toast |
| toggle | done | |
| toggle-group | done | |
| tooltip | done | |

---

## App Components

| Component | Status | Notes |
|-----------|--------|-------|
| AdvancedDraggableModule | done | |
| AgentActionAuditTrail | done | Fixed ThemeSettings type |
| AgentControlCenter | done | Fixed AgentDetailsModal prop types |
| AgentStatusBanner | done | |
| AgentStatusIndicator | done | |
| AuditEventLogOverlay | done | Fixed onChange handler |
| AuthorityConsentModal | done | |
| AuthorityLevelBadge | done | |
| EmployabilityScoreWidget | done | |
| OrgBlindnessTrends | done | |
| RegistryViewer | done | Fixed CSS `as const`, aria-sort values |
| TwoHundredDollarTestWidget | done | |
| ValueDiagnosticDashboard | done | |
| ApiBootstrap | done | |
| BackgroundEffects | done | |
| BridgeHeader | removed | Dead code — unreferenced |
| CognitiveMeshButton | done | |
| CognitiveMeshCard | done | |
| DashboardLayout | done | |
| DesignSystemShowcase | done | |
| DockZone | done | |
| DraggableComponent | done | |
| DraggableModule | done | |
| DraggableModuleContent | done | Added LucideIcon import |
| EnergyFlow | done | |
| ErrorBoundary | done | |
| ExtensionErrorSuppressor | done | |
| FXModePanel | removed | Dead code — only used by removed BridgeHeader |
| GuidedTour | done | |
| LayoutToolsPanel | removed | Dead code — only used by removed BridgeHeader |
| LoadingSpinner | done | |
| Navigation/Breadcrumbs | done | |
| Navigation/ConnectionIndicator | done | |
| Navigation/MobileMenu | done | |
| Navigation/Sidebar | done | |
| Navigation/TopBar | done | |
| Nexus | done | Fixed drag handler redeclarations, useNexusDrag destructuring |
| ParticleField | done | |
| ProtectedRoute | done | |
| Skeleton/Skeleton | done | Primary skeleton system (SkeletonCard, SkeletonTable, etc.) |
| theme-provider | done | Moved from root `components/` to `src/components/` |
| Toast/Toast | legacy | Active toast system. Should migrate to sonner/shadcn in future |
| VoiceFeedback | removed | Dead code — unreferenced |
| setup/DatabaseSettings | done | |
| setup/LLMModelCard | done | |
| setup/LLMSettings | done | |
| setup/SetupWizard | done | |
| setup/StorageProviderCard | done | |

---

## Visualizations

| Component | Status | Notes |
|-----------|--------|-------|
| AgentNetworkGraph | done | Fixed import paths for useD3, types, themes |
| AuditTimeline | done | Fixed module imports |
| MetricsChart | done | Fixed module imports |

---

## Pages

| Route | Status | Notes |
|-------|--------|-------|
| `/` (root) | done | Redirects to `/dashboard` |
| `/login` | done | |
| `/setup` | done | |
| `/forbidden` | done | |
| `/dashboard` | done | (app) route group with ProtectedRoute |
| `/agents` | done | |
| `/analytics` | done | |
| `/compliance` | done | |
| `/marketplace` | done | |
| `/settings` (app) | done | Inside (app) route group |
| `/settings` (root) | removed | Deleted — duplicate route conflict with (app)/settings |

---

## Layouts & Error Boundaries

| File | Status | Notes |
|------|--------|-------|
| app/layout.tsx | done | Root layout with ThemeProvider, AuthProvider, ToastProvider |
| app/(app)/layout.tsx | done | App layout with Sidebar, TopBar, MobileMenu |
| app/(app)/loading.tsx | done | SkeletonDashboard |
| app/(app)/error.tsx | done | Error boundary per route group |

---

## Hooks

| Hook | Status | Notes |
|------|--------|-------|
| useAudioSystem | done | |
| use-mobile | done | Moved from root `hooks/` to `src/hooks/`. Duplicate in ui/ removed |
| useNexusDrag | done | Fixed exports, destructuring in Nexus |
| useSetupWizard | done | |
| useSignalR | done | @microsoft/signalr v10 |
| use-toast | done | Moved from root `hooks/` to `src/hooks/`. Duplicate in ui/ removed |

---

## Stores (Zustand)

| Store | Status | Notes |
|-------|--------|-------|
| useAgentStore | done | |
| useAuthStore | done | |
| useDashboardStore | done | |
| useNotificationStore | done | |
| usePreferencesStore | done | |

---

## Contexts

| Context | Status | Notes |
|---------|--------|-------|
| AuthContext | done | |
| DragDropContext | done | |

---

## Lib / Infrastructure

| Module | Status | Notes |
|--------|--------|-------|
| lib/utils | done | Created — standard shadcn `cn()` helper |
| lib/fonts | done | |
| lib/api/client | done | |
| lib/api/interceptors | done | |
| lib/accessibility/index | done | Fixed TS errors |
| lib/accessibility/axeConfig | done | |
| lib/code-splitting/LazyWidgetLoader | done | |
| lib/code-splitting/registry/lazyWidgets | done | Fixed lazy import paths |
| lib/code-splitting/components/* | done | ErrorBoundary, WidgetErrorFallback, WidgetSkeleton |
| lib/i18n/i18nConfig | done | |
| lib/i18n/index | done | |
| lib/i18n/hooks/useTranslation | done | Fixed TS error |
| lib/i18n/components/LanguageSelector | done | |
| lib/i18n/locales/* | done | en-US, fr-FR, de-DE |
| lib/service-worker/sw | done | Fixed service worker types, Background Sync cast |
| lib/service-worker/register | done | Fixed TS error |
| lib/service-worker/offlineManager | done | |
| lib/service-worker/index | done | |
| lib/visualizations/useD3 | done | D3 hook exists here (AgentNetworkGraph imports wrong path) |

---

## Design Tokens

| Item | Status | Notes |
|------|--------|-------|
| tokens/colors.json | done | Cognitive theme colors |
| tokens/dimensions.json | done | |
| tokens/spacing.json | done | |
| tokens/text.json | done | |
| tokens/typography.json | done | |
| tokens/object-values.json | done | |
| style-dictionary build | done | Generates `build/css/_variables.css` |
| CSS import in globals.css | done | `@import "../../build/css/_variables.css"` |
| prebuild/predev scripts | done | Auto-generate tokens before build/dev |

---

## Storybook

| Item | Status | Notes |
|------|--------|-------|
| Core (storybook@10) | done | |
| @storybook/react-webpack5@10 | done | |
| @storybook/addon-links@10 | done | |
| @storybook/addon-essentials | removed | No v10 release — essentials bundled into core |
| @storybook/addon-interactions | removed | No v10 release |
| @storybook/blocks | removed | No v10 release |
| .storybook/main.ts | done | Updated story paths, removed dead addons |
| .storybook/preview.ts | done | |

### Existing Stories

| Story | Status | Notes |
|-------|--------|-------|
| TwoHundredDollarTestWidget.stories | done | Fixed stale interface mocks, Storybook 10 imports |
| CognitiveMeshButton.stories | done | |
| CognitiveMeshCard.stories | done | |
| DesignSystemShowcase.stories | done | |

---

## Tests

| Test | Status | Notes |
|------|--------|-------|
| CognitiveMeshButton.test.tsx | done | Fixed jest matcher type |

---

## Dependencies Installed (Phase 14b)

**Runtime (35 packages):**
- 27 @radix-ui/* packages (all at latest)
- cmdk, embla-carousel-react, input-otp, recharts@3, sonner, vaul
- react-day-picker@9, react-hook-form, react-resizable-panels@4

**Dev:**
- Storybook aligned to v10 (core + react + react-webpack5 + addon-links)
- style-dictionary@5 (token generation)

---

## Summary

| Category | Total | Done | Migrating | Legacy | Removed | Blocked |
|----------|-------|------|-----------|--------|---------|---------|
| Shadcn/UI components | 48 | 48 | 0 | 0 | 0 | 0 |
| App components | 48 | 43 | 0 | 1 | 4 | 0 |
| Visualizations | 3 | 3 | 0 | 0 | 0 | 0 |
| Pages | 11 | 10 | 0 | 0 | 1 | 0 |
| Hooks | 6 | 6 | 0 | 0 | 0 | 0 |
| Stores | 5 | 5 | 0 | 0 | 0 | 0 |
| Lib modules | 17 | 17 | 0 | 0 | 0 | 0 |
| Design tokens | 8 | 8 | 0 | 0 | 0 | 0 |
| Stories | 4 | 4 | 0 | 0 | 0 | 0 |
| Tests | 1 | 1 | 0 | 0 | 0 | 0 |
| **Totals** | **151** | **145** | **0** | **1** | **5** | **0** |

**Migration progress: 100% complete (0 TypeScript errors, Next.js 16 build passing)**

### Remaining legacy items (1)

- Toast/Toast — Active toast system. Should migrate to sonner/shadcn in a future phase.

### Phase 14b highlights

- Tailwind CSS v3 → v4 migration (`@tailwindcss/postcss` + CSS-first `@config`)
- Next.js 16 SSR hardening (Suspense boundaries, `typeof window` guards, env fallbacks)
- All 48 shadcn/ui components integrated with Radix UI deps
- Design tokens generating CSS custom properties via style-dictionary
- Storybook aligned to v10
