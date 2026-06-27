# 05 — Codex + Visual Studio Master Build Prompt

**Project:** Adopta Platform — Digital Adoption Platform SaaS  
**Purpose:** Production-grade implementation prompt for Codex working in Visual Studio, GitHub, and Azure.  
**Principle:** This is not a prototype. Every sprint delivers production-grade, secure, tested, multi-tenant SaaS capability.

---

## 1. Mission for Codex

You are the lead engineering agent building a production-grade, multi-tenant SaaS Digital Adoption Platform that can be integrated with Property MTD later and reused across future products.

You must use a Microsoft-secured architecture:

- Microsoft Entra ID for workforce/admin users.
- Microsoft Entra External ID for customer/external users.
- No Azure AD B2C for new greenfield work.
- Azure Key Vault for secrets.
- Managed identities for service-to-service Azure access.
- Tenant isolation enforced in API, database, storage, analytics, cache, and AI retrieval.
- RBAC and audit logging from the first sprint.

You must work efficiently in:

- Visual Studio / Visual Studio Code as appropriate.
- Codex.
- GitHub.
- GitHub Actions.
- Azure.

You must build in small, reviewable PRs. Every PR must include:

- requirement IDs implemented;
- changed files summary;
- tests added/updated;
- security/tenant-isolation impact;
- acceptance criteria evidence;
- commands run;
- any assumptions made.

Never weaken tenant isolation, never collect sensitive Property MTD data by default, and never alter HMRC submission, authentication, tax calculation, or compliance workflows unless explicitly instructed.

---

## 2. Authoritative product scope

The platform shall provide:

- embeddable runtime SDK;
- Property MTD Blazor integration contract;
- no-code authoring studio;
- walkthroughs;
- tooltips;
- smart tips;
- banners;
- modals;
- launchers;
- onboarding checklists;
- resource centre;
- surveys;
- segmentation;
- targeting;
- content governance;
- analytics;
- friction detection;
- AI-assisted authoring;
- RAG-grounded self-help assistant;
- tenant administration;
- RBAC;
- audit;
- environments;
- versioning;
- rollback;
- data export;
- extension mode for third-party apps.

The platform shall support two delivery modes:

1. **First-party SDK mode** for Property MTD and future owned applications.
2. **Browser extension mode** for third-party applications where source access is not available.

For Property MTD, SDK mode is the primary integration path, but Property MTD repository changes occur later through a controlled integration branch.

---

## 3. Technology stack

Use this default stack unless a repository-specific constraint requires otherwise.

### Backend/control plane

- .NET 10 / ASP.NET Core.
- Clean Architecture or modular vertical-slice architecture.
- EF Core.
- Azure SQL with Row-Level Security or Azure PostgreSQL with RLS.
- OpenAPI/Swagger.
- FluentValidation or equivalent.
- xUnit/NUnit for tests.
- OpenTelemetry.

### Admin UI

- Blazor Web App preferred for Visual Studio and Property MTD alignment.
- Use a componentised design system.
- WCAG 2.2 AA.
- If React is chosen for a specific screen, isolate it and document why.

### Runtime SDK

- TypeScript.
- Vite/esbuild.
- Shadow DOM or equivalent style isolation.
- No render blocking.
- Async loading.
- CDN-delivered versioned bundles.
- Framework-agnostic core.
- Optional Blazor wrapper for Property MTD.

### Browser extension

- TypeScript.
- Manifest V3.
- Shares runtime SDK core.
- Client-side URL/domain matching.

### Azure services

- Azure Front Door / WAF.
- Azure API Management.
- Azure App Service or Azure Container Apps.
- Azure SQL or Azure Database for PostgreSQL.
- Azure Blob Storage.
- Azure CDN/Front Door delivery.
- Azure Event Hubs.
- Azure Cache for Redis.
- Azure AI Search.
- Azure OpenAI / OpenAI provider abstraction.
- Azure Key Vault.
- Azure App Configuration.
- Azure Monitor / Application Insights / Managed Grafana.

### DevSecOps

- GitHub Actions.
- CodeQL.
- Dependabot.
- Secret scanning.
- Dependency scanning.
- Infrastructure as Code using Bicep.
- Environment gates for test and production.

---

## 4. Non-negotiable invariants

### 4.1 Tenant isolation is sacred

Every persisted record must be tenant-scoped unless deliberately global and read-only.

The system must derive tenant context from verified identity, never from untrusted client input.

Every data path must be covered by automated cross-tenant isolation tests.

### 4.2 Property MTD data safety

Do not capture, store, transmit, embed, or expose by default:

- HMRC OAuth tokens;
- Government Gateway data;
- submission payloads;
- UTRs;
- National Insurance numbers;
- property addresses;
- bank transaction values;
- tax amounts;
- client identifiers;
- personally sensitive form values.

The adoption platform may use safe status metadata such as:

- HMRC authorised: yes/no;
- onboarding status;
- quarterly update status;
- validation error category;
- count of unresolved items;
- role;
- app route;
- feature flags;
- completion events.

### 4.3 The element engine is the moat

Do not use brittle CSS selectors as the only anchor strategy.

Implement stable first-party anchors for Property MTD:

```html
<button data-adopt-id="mtd.quarterly.submit">Submit</button>
```

Use multi-signal descriptors for all other apps.

### 4.4 The runtime must never break the host app

The SDK must:

- load asynchronously;
- catch all internal errors;
- degrade silently;
- not modify host app business state;
- not block rendering;
- not leak styles;
- be removable without side effects.

---

## 5. Required future Property MTD integration contract

Do not modify Property MTD in Sprint 1. The future integration contract is:

```text
Property MTD Blazor/Web App
  ├── Adoption runtime initialised in app shell
  ├── Blazor wrapper for runtime SDK
  ├── data-adopt-id anchors on key components
  ├── route/screen tracking
  ├── safe context updates
  └── runtime guide rendering

Property MTD Backend
  ├── /api/adoption/context
  ├── adoption-safe domain event adapter
  ├── no raw tax-data event payloads
  ├── no direct adoption access to HMRC credentials
  └── workflow status exposure only
```

Initial anchor set:

```text
mtd.dashboard.next-action
mtd.setup.start
mtd.eligibility.check
mtd.hmrc.authorise
mtd.hmrc.authorisation-status
mtd.property.add
mtd.property.edit
mtd.records.import
mtd.records.manual-entry
mtd.transaction.category
mtd.transaction.uncategorised-list
mtd.quarterly.start
mtd.quarterly.review-income
mtd.quarterly.review-expenses
mtd.quarterly.validation-errors
mtd.quarterly.submit
mtd.quarterly.hmrc-confirmation
mtd.submission.history
mtd.agent.client-list
mtd.agent.client-mtd-status
mtd.agent.request-authorisation
```

Initial safe events:

```text
mtd.onboarding.started
mtd.onboarding.completed
mtd.hmrc_authorisation.started
mtd.hmrc_authorisation.completed
mtd.hmrc_authorisation.failed
mtd.property.created
mtd.records.imported
mtd.transaction.categorisation.started
mtd.transaction.categorisation.completed
mtd.quarterly_update.started
mtd.quarterly_update.validation_failed
mtd.quarterly_update.submitted
mtd.quarterly_update.accepted_by_hmrc
mtd.quarterly_update.rejected_by_hmrc
```

---

## 6. Maximum five production sprints

### Sprint 1 — SaaS Foundation, Security & Platform Skeleton

Build:

- Visual Studio solution.
- .NET control plane API.
- Admin shell.
- Entra ID workforce authentication.
- Entra External ID architecture support.
- Tenant model.
- Row-level tenant isolation.
- RBAC.
- Audit logging.
- Bicep Azure infrastructure.
- GitHub Actions CI/CD.
- CodeQL/dependency/secret scanning.
- OpenTelemetry baseline.
- Cross-tenant isolation test harness.

Acceptance:

- Tenant admin can sign in.
- Tenant context is enforced.
- Cross-tenant tests fail safely.
- CI/CD passes.
- No secrets in source.

### Sprint 2 — Runtime SDK, Element Anchoring, Property MTD Integration Contract & Core Guidance

Build:

- TypeScript runtime SDK.
- Blazor wrapper.
- SDK initialisation contract for Property MTD app shell.
- Safe `/api/adoption/context` endpoint contract.
- `data-adopt-id` anchor contract.
- Content model.
- CDN/versioned bundle delivery.
- Client-side targeting.
- Walkthroughs, tooltips, banners, checklists, launchers, resource centre.
- Element engine v1.
- Property MTD first guide pack.

Acceptance:

- Guide runs inside an Adopta-hosted sample app and is ready for later Property MTD integration.
- Completion/dismissal persists.
- Runtime never breaks host app.
- No sensitive MTD data captured.
- Build/test pass.

### Sprint 3 — Authoring Studio, Governance & Content Lifecycle

Build:

- No-code authoring studio.
- In-app editor.
- Element capture.
- True runtime preview.
- Draft/review/approval/publish.
- Compliance approval for tax-sensitive content.
- Versioning.
- Rollback.
- Environment promotion.
- Theme/accessibility settings.
- Anchor-health v1.

Acceptance:

- Author can create, preview, approve, publish, and roll back a guide.
- Audit trail records all changes.
- Compliance gate blocks tax-sensitive content until approved.

### Sprint 4 — Analytics, Segmentation, AI Help & Closed Loop

Build:

- Runtime event collection.
- Event ingestion pipeline.
- Structural autocapture.
- Property MTD workflow event adapter.
- Funnel analytics.
- Guide performance analytics.
- Segment builder.
- Cohort-to-segment targeting.
- Self-help widget.
- RAG answer agent.
- AI co-author.
- AI insights agent.
- AI guardrails.

Acceptance:

- Product team sees drop-off in Property MTD workflows.
- Segment can be created from behaviour.
- Guidance can be targeted to that segment.
- AI answers only from approved knowledge with citations.
- Unsupported tax advice is refused/escalated.

### Sprint 5 — Enterprise Hardening, Extension & Production Readiness

Build:

- Custom roles.
- SCIM provisioning.
- Data residency controls.
- Silo tenant seam.
- Surveys/NPS.
- Data export.
- Webhooks.
- Browser extension mode.
- Element Library v1.
- Vision fallback behind opt-in.
- Performance/load tests.
- Security hardening.
- Backup/restore/DR runbooks.
- Production dashboards and alerts.

Acceptance:

- Platform is production-ready for Property MTD and future owned apps.
- Extension mode works for approved third-party domains.
- Load/security/isolation/accessibility tests pass.
- Operational runbooks exist.

---

## 7. PR definition of done

A PR is complete only when:

- requirements and acceptance criteria are referenced;
- code is production quality;
- build passes;
- unit tests pass;
- integration tests pass where relevant;
- tenant isolation tests pass where relevant;
- accessibility tests pass for UI changes;
- security scans pass;
- documentation is updated;
- observability is added where relevant;
- no unrelated files are changed;
- no sensitive Property MTD data is collected;
- assumptions are recorded in the PR description.

---

## 8. First Codex kickoff prompt

```text
Read the specification documents in /docs, especially 06-review-retrofit-decisions-and-spec-updates.md and this Codex build prompt.

Begin Sprint 1: SaaS Foundation, Security & Platform Skeleton.

Use Visual Studio and .NET 10 / ASP.NET Core as the control-plane default. Use GitHub Actions and Bicep for Azure infrastructure. Create a production-ready foundation, not a prototype.

Implement in small PR-sized slices:
1. solution/repo structure;
2. Entra authentication shell;
3. tenant model and database isolation;
4. RBAC and audit;
5. CI/CD and security gates;
6. observability;
7. cross-tenant isolation test harness.

Do not touch the Property MTD repository in Sprint 1. Future Property MTD integration must be handled through a separate controlled branch after the SDK contract is ready.

Every PR must state requirement IDs, files changed, tests added, commands run, assumptions, and tenant/security impact.
```
