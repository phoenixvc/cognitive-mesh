# PRD: TestForge Agent Team

**Project:** TestForge
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Purpose

TestForge is an automated testing team that designs, generates, and executes comprehensive test suites across the Cognitive Mesh ecosystem. The team ensures quality through strategic test planning, automated test creation, end-to-end validation using VS Code 1.110 agentic browser tools, and continuous coverage improvement.

---

## 2. Team Composition

| # | Agent | Role |
|---|-------|------|
| 1 | TestStrategist | Designs test plans by analyzing code changes, risk areas, and business-critical paths to determine optimal test coverage strategy |
| 2 | UnitTestWriter | Generates unit tests following project conventions (xUnit + Moq + FluentAssertions), targeting uncovered code paths and edge cases |
| 3 | IntegrationTestDesigner | Designs integration tests that validate interactions between layers, services, and external dependencies using appropriate test doubles |
| 4 | E2ETestRunner | Orchestrates end-to-end tests using VS Code 1.110 agentic browser tools to simulate real user workflows across the full application stack |
| 5 | CoverageAnalyzer | Tracks code coverage metrics, identifies coverage gaps, prioritizes untested areas by risk, and recommends targeted test additions |

---

## 3. Workflow

1. **Plan**: TestStrategist analyzes recent code changes, PR diffs, and architectural risk areas to produce a prioritized test plan.
2. **Generate**: UnitTestWriter and IntegrationTestDesigner work in parallel to create tests aligned with the test plan, following the `MethodName_Scenario_ExpectedResult` naming convention.
3. **Execute**: E2ETestRunner orchestrates browser-based end-to-end tests for user-facing features, capturing screenshots and interaction logs.
4. **Analyze**: CoverageAnalyzer evaluates the results, computes coverage deltas, and identifies remaining gaps.
5. **Iterate**: Gaps identified by CoverageAnalyzer feed back into the generation phase for targeted test creation.
6. **Report**: A consolidated test report is produced with pass/fail results, coverage metrics, and recommendations.

---

## 4. Integration Points

- **All repositories**: TestForge generates and runs tests across all Cognitive Mesh repositories, respecting each project's test framework and conventions.
- **DebugSquad**: Collaborates with DebugSquad to reproduce reported bugs as failing tests and to validate proposed fixes.
- **CI/CD Pipelines**: Integrates with GitHub Actions to run tests on pull requests and provide automated feedback.
- **VS Code 1.110**: Leverages agentic browser tools for end-to-end testing of web-based interfaces and API interactions.

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
