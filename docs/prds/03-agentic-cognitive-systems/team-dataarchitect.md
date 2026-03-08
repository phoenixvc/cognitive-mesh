# PRD: DataArchitect Agent Team

**Project:** DataArchitect
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Purpose

DataArchitect is a data architecture team that designs database schemas, plans data migrations, maps cross-service data flows, optimizes query performance, and enforces data governance standards. The team ensures that data infrastructure across the Cognitive Mesh ecosystem is well-structured, performant, and compliant with governance policies.

---

## 2. Team Composition

| # | Agent | Role |
|---|-------|------|
| 1 | SchemaDesigner | Designs database schemas for CosmosDB, DuckDB, Redis, and Qdrant, ensuring appropriate modeling for each storage paradigm (document, columnar, key-value, vector) |
| 2 | MigrationPlanner | Plans and validates data migrations including schema evolution, data transformation, backfill strategies, and zero-downtime migration procedures |
| 3 | DataFlowMapper | Maps data flows across services and layers, documenting how data moves through the system from ingestion to storage, processing, and presentation |
| 4 | PerformanceTuner | Optimizes query performance by analyzing execution plans, recommending indexes, partition strategies, caching layers, and query rewrites |
| 5 | DataGovernanceAdvisor | Ensures compliance with data governance policies including data classification, retention rules, access controls, PII handling, and GDPR data subject rights |

---

## 3. Workflow

1. **Design**: SchemaDesigner creates or evolves database schemas based on domain requirements, selecting appropriate storage engines and modeling patterns.
2. **Map**: DataFlowMapper documents how the proposed schema changes affect data flows across services, identifying upstream and downstream impacts.
3. **Plan**: MigrationPlanner develops a migration strategy including rollback procedures, data validation checks, and deployment sequencing.
4. **Tune**: PerformanceTuner evaluates the schema and query patterns for performance, recommending indexes, partitioning, and caching strategies.
5. **Govern**: DataGovernanceAdvisor reviews the design for compliance with data classification, retention, and privacy requirements.
6. **Deliver**: A comprehensive data architecture document is produced with schema definitions, migration scripts, performance benchmarks, and governance annotations.

---

## 4. Integration Points

- **cognitive-mesh**: Manages schemas for CosmosDB (persistence), DuckDB (analytics), Redis (caching/memory), and Qdrant (vector search) used across the foundation layer.
- **chaufher**: Designs data models for the mobility platform including real-time location data, booking records, and fleet management.
- **Mystira**: Architects data storage for content management, user profiles, and analytics data.
- **SecurityAudit**: Coordinates on data access controls, encryption at rest, and PII identification.
- **ReleasePilot**: Provides migration readiness assessments as part of release validation.

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
