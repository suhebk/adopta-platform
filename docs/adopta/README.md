# Adopta — Digital Adoption Platform Specifications

Production-grade, multi-tenant SaaS Digital Adoption Platform (DAP) for in-app guidance, AI self-service, product analytics, governance, and closed-loop adoption improvement.

This pack is the source of truth for the new standalone GitHub repository:

```text
https://github.com/suhebk/adopta-platform
```

The Adopta repository is separate from:

```text
https://github.com/suhebk/property-income-mtd-platform
```

Property Income MTD remains undisturbed while its remaining sprints continue. Adopta will integrate with Property MTD later through a controlled first-party runtime SDK and safe adoption context contract.

## Read order

| # | Document | Purpose |
|---|---|---|
| 00 | Product Vision & Competitive Analysis | Product direction, competitor benchmark, DAP capability model, differentiation. |
| 01 | User Requirements | Personas, journeys, user requirements, acceptance criteria. |
| 02 | Functional Requirements | Functional and non-functional requirements, traceability, definition of done. |
| 03 | Solution Architecture | Logical, physical, security, identity, event, AI, analytics, data, DevSecOps architecture. |
| 04 | Wireframes | Admin Studio and runtime UX wireframes. |
| 05 | Codex + Visual Studio Build Prompt | Implementation prompt for Codex, Visual Studio Code, GitHub, Azure, and .NET-first delivery. |
| 06 | Retrofit Decisions & Spec Updates | Decisions that supersede baseline assumptions, including Codex, .NET-first, first-party SDK mode, and maximum five sprints. |
| setup | Local Setup Steps | Exact local and GitHub setup steps for the user's chosen path. |
| sprints | ADOPTA-SPRINT-1 | Sprint 1 scope, acceptance criteria, Codex prompt, and guardrails. |
| persistence | Migration & Operations Planning | Migration strategy, database-boundary tenant isolation design, and persistence operations runbook. |
| operations | Operational Readiness | Deployment readiness, observability/logging guidance, incident response, rollback, and tenant-isolation validation checklists. |
| production-readiness | Sprint 5 Production Readiness Closeout | Controlled production-readiness closeout, persistence enablement, route authorization, and migration approval checklists. |
| studio | Studio Read API Activation Readiness | Secure configuration and preflight readiness for controlled read-only Studio API activation. |
| studio | Studio Read API Environment Validation | Placeholder-only environment validation guidance for controlled read-only Studio API activation. |
| studio | Studio Read API Activation Rehearsal | Test-only activation rehearsal guidance for the read-only Studio API path. |
| studio | Studio Read API Status Surface | Operator-facing readiness surface for controlled read-only Studio API activation. |
| sprints | ADOPTA-SPRINT-5 | Controlled production enablement foundations, starting with reviewable EF schema baseline generation. |
| sprints | ADOPTA-SPRINT-6 | Runtime delivery API and bundle retrieval foundation. |
| sprints | ADOPTA-SPRINT-8 | Adoption Studio UI foundation, starting with read-only content list and view. |
| sprints | ADOPTA-SPRINT-9 | Controlled authenticated Studio API integration and read-only activation readiness closeout. |
| sprints | ADOPTA-SPRINT-10 | Controlled Studio read API activation validation, starting with safe preflight readiness. |

## Important superseding decisions

The baseline research documents are retained because they contain valuable DAP market and architecture analysis. Where older wording conflicts with the latest direction, apply these decisions:

1. Use Codex + Visual Studio Code, not Claude Code.
2. Use a standalone `adopta-platform` repository.
3. Keep `property-income-mtd-platform` undisturbed until the later controlled integration branch.
4. Use .NET-first backend/control-plane architecture.
5. Use TypeScript for the runtime SDK and browser extension.
6. Use first-party SDK mode for owned applications such as Property MTD.
7. Use browser extension mode later for third-party applications.
8. Keep delivery to a maximum of five production-grade sprints.
9. Use Microsoft Entra ID and Microsoft Entra External ID.
10. Do not use Azure AD B2C for new greenfield identity.
11. Do not capture sensitive Property MTD values by default.

## Current sprint

Current sprint: `ADOPTA-SPRINT-1 — SaaS Foundation, Security, Tenant Model, RBAC, Audit, and Repository Structure`.

Codex must plan first, list exact files to add/change, and wait for approval before applying changes.
