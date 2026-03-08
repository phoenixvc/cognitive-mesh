# PRD: DocsCrew Agent Team

**Project:** DocsCrew
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Purpose

DocsCrew is a documentation team that generates, maintains, and validates comprehensive documentation across the Cognitive Mesh ecosystem. The team ensures that API references, architecture documentation, user guides, and changelogs remain accurate, consistent, and up to date with the evolving codebase.

---

## 2. Team Composition

| # | Agent | Role |
|---|-------|------|
| 1 | APIDocWriter | Generates API documentation from code annotations, XML doc comments, and OpenAPI specifications, ensuring all public endpoints and types are documented |
| 2 | ArchitectureDocWriter | Documents system architecture including layer boundaries, dependency flows, hexagonal port/adapter patterns, and deployment topology diagrams |
| 3 | UserGuideAuthor | Writes user-facing documentation including setup guides, tutorials, workflow explanations, and troubleshooting instructions |
| 4 | ChangelogGenerator | Generates changelogs from commit history, pull request descriptions, and release tags, organizing entries by category (features, fixes, breaking changes) |
| 5 | DocLinter | Validates documentation quality and consistency by checking for broken links, outdated references, style guide adherence, and completeness against the codebase |

---

## 3. Workflow

1. **Discover**: APIDocWriter and ArchitectureDocWriter scan the codebase for undocumented or poorly documented public types, endpoints, and architectural patterns.
2. **Generate**: APIDocWriter produces reference documentation while ArchitectureDocWriter creates or updates architectural overviews and diagrams.
3. **Author**: UserGuideAuthor writes narrative documentation for onboarding, configuration, and common workflows based on the codebase and existing docs.
4. **Compile**: ChangelogGenerator processes recent commits and PRs to produce structured release notes.
5. **Validate**: DocLinter reviews all generated and existing documentation for accuracy, consistency, broken links, and style compliance.
6. **Publish**: Validated documentation is committed to the repository and published to the documentation site.

---

## 4. Integration Points

- **All repositories**: DocsCrew generates and maintains documentation across every Cognitive Mesh repository.
- **CI/CD Pipelines**: DocLinter runs as part of the PR review process to catch documentation regressions.
- **CLAUDE.md files**: ArchitectureDocWriter ensures project instructions and conventions documented in CLAUDE.md files remain synchronized with the actual codebase.
- **Release process**: ChangelogGenerator integrates with ReleasePilot to produce release notes as part of the release workflow.

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
