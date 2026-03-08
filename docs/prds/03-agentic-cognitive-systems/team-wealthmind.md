# PRD: WealthMind Agent Team

**Project:** WealthMind
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Purpose

Provide financial intelligence capabilities including portfolio analysis, risk modeling, market intelligence gathering, regulatory compliance monitoring, and automated financial reporting to support investment decisions and portfolio management.

---

## 2. Team Composition

| # | Agent | Role |
|---|-------|------|
| 1 | PortfolioAnalyzer | Analyzes investment portfolio composition, performance attribution, diversification metrics, and rebalancing opportunities |
| 2 | RiskModeler | Models financial risk across dimensions of market risk, credit risk, liquidity risk, and concentration risk using quantitative methods |
| 3 | MarketIntelligence | Gathers and synthesizes market data including sector trends, macroeconomic indicators, and emerging opportunity signals |
| 4 | ComplianceMonitor | Monitors financial activities against regulatory requirements, flags potential compliance issues, and tracks regulatory changes |
| 5 | ReportGenerator | Generates structured financial reports combining portfolio metrics, risk assessments, market context, and compliance status |

---

## 3. Workflow

1. **MarketIntelligence** continuously gathers market data, sector performance, and macroeconomic signals to establish the current market context.
2. **PortfolioAnalyzer** evaluates the current portfolio against market context, computing performance attribution, concentration metrics, and drift from target allocation.
3. **RiskModeler** runs risk scenarios (stress tests, Monte Carlo simulations, correlation analysis) on the portfolio given current market conditions.
4. **ComplianceMonitor** checks portfolio positions and proposed changes against applicable regulatory frameworks and internal policy constraints.
5. **ReportGenerator** consolidates all agent outputs into a comprehensive financial intelligence report with executive summary, detailed analytics, and recommended actions.

---

## 4. Integration Points

- **StartupSim** — financial projections from startup simulations feed into portfolio-level modeling
- **ContractAuditor** — financial terms in contracts are cross-referenced with portfolio risk models

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
