# ADOPTA-SPRINT-1 — SaaS Foundation, Security, Tenant Model, RBAC, Audit, and Repository Structure

## Objective

Create the production-grade foundation for the standalone Adopta SaaS platform without touching the Property Income MTD repository.

This sprint is not a prototype. It must establish the architecture, security, tenancy, repository structure, coding conventions, test foundation, and documentation discipline needed for a successful five-sprint delivery.

## Repository boundary

Adopta repository:

```text
suhebk/adopta-platform
```

Property Income MTD repository:

```text
suhebk/property-income-mtd-platform
```

Do not modify Property Income MTD in Sprint 1.

## Scope

Sprint 1 shall deliver:

1. Production-grade solution/repository structure.
2. .NET-first backend/control-plane foundation.
3. Admin web shell placeholder or project skeleton.
4. Tenant model.
5. TenantApplication model.
6. AdoptionUser model.
7. Role and Permission model.
8. AuditEvent model.
9. DeploymentEnvironment model.
10. Tenant context conventions.
11. RBAC conventions.
12. Audit logging conventions.
13. Automated tenant isolation test foundation.
14. GitHub Actions baseline.
15. Documentation and ADRs.

## Explicitly out of scope

Do not build these in Sprint 1:

- Runtime SDK
- Browser extension
- Walkthrough renderer
- Tooltip engine
- Adoption Studio authoring UI
- AI assistant
- RAG pipeline
- Event Hubs integration
- ClickHouse analytics
- Property MTD integration
- HMRC workflow changes
- Tax workflow changes

## Recommended .NET solution structure

```text
src/Adopta.Api
src/Adopta.Application
src/Adopta.Domain
src/Adopta.Infrastructure
src/Adopta.Web
tests/Adopta.UnitTests
tests/Adopta.IntegrationTests
```

## Initial service interfaces

Codex should create or propose these interfaces where appropriate:

```text
IAdoptionTenantContext
IAdoptionAuthorizationService
IAdoptionAuditService
IApplicationRegistrationService
```

## Initial model list

```text
Tenant
TenantApplication
AdoptionUser
Role
Permission
AuditEvent
DeploymentEnvironment
```

## Sprint 1 acceptance criteria

Sprint 1 is complete only when:

1. The repository builds.
2. The solution structure is clear and production-grade.
3. Tenant-scoped models are present.
4. RBAC model is present.
5. Audit model is present.
6. Tenant context conventions are documented and testable.
7. Tenant isolation tests exist, even if implemented against an in-memory/test foundation initially.
8. GitHub Actions baseline exists.
9. No secrets are committed.
10. No Property MTD repository files are touched.
11. Documentation is updated.
12. Codex provides a log of files changed, tests run, assumptions, and known limitations.

## Codex Sprint 1 prompt

Paste this into Codex:

```text
You are working in the new adopta-platform repository.

Before making any changes, read:

- AGENTS.md
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

We are starting ADOPTA-SPRINT-1 — SaaS Foundation, Security, Tenant Model, RBAC, Audit, and Repository Structure.

Important context:
- This is not a prototype.
- The solution must be production-ready SaaS from the beginning.
- This is a separate repository from Property Income MTD.
- Do not modify the Property Income MTD repository.
- Keep to a maximum of five major implementation sprints.
- Use a .NET-first backend architecture.
- Use TypeScript later for the runtime SDK, but do not build the SDK in Sprint 1.
- Microsoft Entra security, tenant isolation, RBAC, audit logging, observability, and tests are mandatory.
- Do not use Azure AD B2C.
- Do not capture sensitive Property MTD values by default.

Sprint 1 scope:
1. Review the repository and specification documents.
2. Propose the safest production-grade solution structure for Adopta.
3. Add a .NET solution structure suitable for SaaS:
   - src/Adopta.Api
   - src/Adopta.Application
   - src/Adopta.Domain
   - src/Adopta.Infrastructure
   - src/Adopta.Web
   - tests/Adopta.UnitTests
   - tests/Adopta.IntegrationTests
4. Add production-grade foundation models:
   - Tenant
   - TenantApplication
   - AdoptionUser
   - Role
   - Permission
   - AuditEvent
   - DeploymentEnvironment
5. Add server-side tenant isolation conventions and tests.
6. Add initial service interfaces:
   - IAdoptionTenantContext
   - IAdoptionAuthorizationService
   - IAdoptionAuditService
   - IApplicationRegistrationService
7. Add documentation for Sprint 1 and ADRs.
8. Do not build the runtime SDK, walkthroughs, AI, analytics, Event Hubs, ClickHouse, browser extension, or Property MTD integration in this sprint.

Before coding:
- Produce a concise implementation plan.
- List exact files to add/change.
- Wait for my approval before applying changes.
```

## Recommended working discipline

1. Ask Codex to plan only.
2. Approve one slice at a time.
3. Run build and tests locally.
4. Commit only intended files.
5. Keep the PR focused.
6. Do not start Sprint 2 until Sprint 1 acceptance criteria pass.
