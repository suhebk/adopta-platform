# Adopta Platform — Codex Instructions

This repository contains the Adopta Digital Adoption Platform.

Adopta is a production-grade, multi-tenant SaaS Digital Adoption Platform designed to integrate with Property Income MTD and future applications through a first-party runtime SDK, secure APIs, contextual guidance, authoring, analytics, AI assistance, governance, and Microsoft SaaS security.

## Source-of-truth documents

Codex must read these documents before making architecture or implementation changes:

- docs/adopta/README.md
- docs/adopta/00-product-vision-and-competitive-analysis.md
- docs/adopta/01-user-requirements.md
- docs/adopta/02-functional-requirements.md
- docs/adopta/03-solution-architecture.md
- docs/adopta/04-wireframes.md
- docs/adopta/05-codex-visual-studio-build-prompt.md
- docs/adopta/06-review-retrofit-decisions-and-spec-updates.md
- docs/adopta/setup/LOCAL-SETUP-STEPS.md
- docs/adopta/sprints/ADOPTA-SPRINT-1.md

## Repository relationship

This is a separate repository from Property Income MTD.

Property Income MTD repo:
- suhebk/property-income-mtd-platform

Adopta repo:
- suhebk/adopta-platform

Do not modify the Property Income MTD repository from this repo.

Property MTD integration must happen later through a controlled SDK/API contract.

## Non-negotiable principles

1. This is production-grade SaaS work, not a prototype.
2. Use a maximum of five implementation sprints.
3. Microsoft Entra security is mandatory.
4. Do not use Azure AD B2C.
5. Tenant isolation is mandatory from the first sprint.
6. RBAC, audit logging, observability, automated tests, and secure defaults are mandatory.
7. Use .NET-first backend architecture unless explicitly agreed otherwise.
8. Use TypeScript for the runtime SDK and browser-delivered client libraries.
9. Do not capture sensitive Property MTD values by default.
10. Every PR must reference the relevant requirement IDs from the Adopta specification pack.
11. Every code change must include or update automated tests.
12. Build and tests must pass before commit.

## Sprint plan

Sprint 1 — SaaS Foundation, Security, Tenant Model, RBAC, Audit, Repo Structure  
Sprint 2 — Runtime SDK, Element Anchoring, Property MTD Integration Contract  
Sprint 3 — Adoption Studio, Authoring, Governance, Versioning, Publishing  
Sprint 4 — Analytics, Segmentation, AI Help, Closed-Loop Insights  
Sprint 5 — Enterprise Hardening, Extension, Observability, Production Readiness

Current sprint:
ADOPTA-SPRINT-1 — SaaS Foundation, Security, Tenant Model, RBAC, Audit, and Repository Structure.

Codex must not start Sprint 2 until Sprint 1 acceptance criteria are satisfied.
