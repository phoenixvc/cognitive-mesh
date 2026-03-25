# Release Notes — cognitive-mesh — 2026-03-25 (DRAFT)

> Covers changes since repository creation — 14 PRs over the last 30 days  
> Review and edit before publishing.

## Features
- Frontend Phase 13: ApiHost, route decorators, build fixes across 15 projects (#348)
- Phase 14: Core UX — 5 Zustand stores, navigation, routing, SignalR, real API integration (#352)
- Phase 14b: UI component library integration — 48 shadcn/ui components, design tokens, Storybook (#358)
- Phase 15 Batch A: Settings page (language, GDPR toggles), notifications page, profile enhancements (#359)
- Phase 16: 4 widget dashboards, role-based UI with RoleGuard, compliance page, frontend tests (#361)
- Policy Store DB for self-healing remediation policies with decision engine in AgencyLayer — PHO-5 (#377)
- AI-written implementations for core interfaces (#421)
- Phase 14 follow-up: Zustand stores, navigation, routing, SignalR, skeleton loaders (#426)

## Bug Fixes
- Fix gh-pages deploy permissions for `peaceiris/actions-gh-pages` (#357)
- Fix code scanning alert: remove unused `handleDragStart` variable (#392)
- Fix stale ecosystem names in README (ai-flume→sluice, ai-gauge→docket, ai-cadence→phoenix-flow) (#423)

## Infra / DevOps
- Add kernel.sh cloud browser MCP server to agent tooling stack (#388)
- Reduce CodeQL to weekly + manual only to cut CI costs (#384)
- Update project badges (#420)
