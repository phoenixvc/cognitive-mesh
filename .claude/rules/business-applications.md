---
paths:
  - "src/BusinessApplications/**/*.cs"
---

# Business Applications Layer Rules

## Compliance
- **GDPR**: `GDPRComplianceAdapter` — data subject rights (access, erasure, portability)
- **EU AI Act**: `EUAIActComplianceAdapter` — risk classification for AI systems
- Audit events must be immutable with 7-year retention
- Event types: PolicyApproved, GovernanceViolation, EthicalAssessmentPerformed

## Decision Support
- `DecisionSupportManager` coordinates reasoning recipes for business decisions
- Always include provenance chain (which data → which reasoning → which decision)

## Customer Intelligence
- `CustomerIntelligenceManager` handles customer behavioral analysis
- Never store raw PII — use pseudonymization or aggregation
- All customer data operations must pass Informational Dignity checks
