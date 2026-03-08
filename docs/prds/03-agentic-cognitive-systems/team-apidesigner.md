# PRD: APIDesigner Agent Team

**Project:** APIDesigner
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Purpose

APIDesigner is an API design team that ensures all service interfaces across the Cognitive Mesh ecosystem follow consistent, well-versioned, and developer-friendly contract patterns. The team handles contract-first design, endpoint architecture, versioning strategy, mock generation, and cross-cutting API quality review.

---

## 2. Team Composition

| # | Agent | Role |
|---|-------|------|
| 1 | ContractDesigner | Designs API contracts and schemas using OpenAPI, Protocol Buffers, or JSON Schema, ensuring type safety and clear domain modeling |
| 2 | EndpointArchitect | Designs REST and GraphQL endpoints following resource-oriented patterns, consistent naming conventions, and appropriate HTTP semantics |
| 3 | VersioningStrategist | Manages API versioning strategy including backward compatibility analysis, deprecation timelines, and migration paths between API versions |
| 4 | MockGenerator | Creates API mocks and stubs from contracts to enable parallel development, integration testing, and client SDK prototyping |
| 5 | APIReviewer | Reviews API designs for consistency, adherence to organizational standards, usability, error handling patterns, and alignment with industry best practices |

---

## 3. Workflow

1. **Design**: ContractDesigner collaborates with domain experts to define API schemas and data models based on business requirements.
2. **Architect**: EndpointArchitect maps domain operations to endpoints, selecting appropriate HTTP methods, URL structures, and query/body patterns.
3. **Version**: VersioningStrategist evaluates breaking vs. non-breaking changes and determines the versioning approach for each API evolution.
4. **Mock**: MockGenerator produces functional mocks from the finalized contracts, enabling consumers to begin integration before implementation is complete.
5. **Review**: APIReviewer conducts a comprehensive review of the full API surface for consistency, usability, security, and adherence to standards.
6. **Deliver**: Approved contracts are published as artifacts for consuming teams and integrated into CI validation.

---

## 4. Integration Points

- **cognitive-mesh**: Designs internal APIs for inter-layer communication following hexagonal architecture port/adapter patterns.
- **ai-gateway**: Defines the public API surface for AI model routing, prompt management, and inference endpoints.
- **chaufher**: Designs APIs for the mobility platform including real-time data streams and booking interfaces.
- **DocsCrew**: Provides finalized API contracts to DocsCrew for automated reference documentation generation.
- **TestForge**: Supplies API contracts and mocks to TestForge for contract testing and integration test design.

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
