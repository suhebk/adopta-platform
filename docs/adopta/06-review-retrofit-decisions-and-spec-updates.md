# 06 — Review, Retrofit Decisions & Specification Updates

**Project:** Property MTD Adoption Platform / Digital Adoption Platform SaaS  
**Purpose:** Review of the uploaded Adopta specification pack and the concrete decisions to retrofit into the Property MTD adoption solution.  
**Decision:** Adopt the uploaded pack as a strong baseline, but amend it for Property MTD, Codex + Visual Studio delivery, .NET/Blazor alignment, and a maximum five-sprint production build.

---

## 1. Executive decision

The uploaded specification pack is strong and should become the baseline for the DAP solution, with amendments. Its strongest contributions are:

- a clear DAP capability model;
- strong competitor-derived thinking from VisualSP, WalkMe, Whatfix, and Pendo;
- a serious resilient element-engine design;
- Microsoft Entra-first identity;
- production-grade SaaS multi-tenancy from the outset;
- analytics and guidance built on one unified data model;
- strong UI/UX wireframes;
- a structured AI-native product direction;
- a build prompt that can be converted into a Codex + Visual Studio implementation playbook.

The main changes required are:

- adapt from **Claude Code** to **Codex + Visual Studio + GitHub**;
- adapt from **TypeScript-first/NestJS-first** to **.NET-first for the SaaS control plane**, while keeping TypeScript for the runtime SDK and browser extension;
- adapt from **zero-code everywhere** to **dual delivery**: first-party SDK for Property MTD and future owned apps, browser extension for third-party apps;
- collapse the six-phase plan into **maximum five production sprints**;
- move mobile, VDI/Citrix, session replay, simulation training, and full cross-app agent orchestration to later roadmap unless specifically required for the first commercial release;
- add a dedicated **Property MTD integration specification** covering HMRC-safe workflow context, no tax-data leakage, Blazor anchors, and backend domain events.

---

## 2. Beneficial items to retrofit into our solution

### 2.1 Product strategy and positioning

- Adopt the four-layer DAP capability model:
  - guidance;
  - self-service support;
  - analytics;
  - engagement and feedback.
- Keep the product vision of a production-grade multi-tenant SaaS DAP.
- Keep the product principles: privacy by construction, performance as a feature, one data model, AI as human-in-the-loop, and multi-tenant secure from commit one.
- Keep the competitor benchmark framing:
  - WalkMe = durable guidance and enterprise app coverage;
  - Whatfix = AI-native guidance and analytics;
  - Pendo = product analytics and retroactive capture;
  - VisualSP = Microsoft-native contextual help.
- Keep the strategic differentiation of a Microsoft-secured, AI-native, analytics-led DAP.
- Keep the principle that the solution should become reusable IP, not just a Property MTD feature.

### 2.2 Microsoft identity and SaaS security

- Use Microsoft Entra ID for workforce/admin users.
- Use Microsoft Entra External ID for customer/consumer identities.
- Do not use Azure AD B2C for greenfield work.
- Support Conditional Access and MFA.
- Support RBAC from the first sprint.
- Add SCIM provisioning as an enterprise requirement.
- Enforce strict tenant isolation in application code, database, analytics, cache, storage, and AI retrieval.
- Include an automated tenant-isolation test harness as a non-negotiable CI gate.
- Include complete audit logging from the start.
- Include tenant/domain/app scoping so the runtime only executes where allowed.

### 2.3 Architecture

- Keep the four-plane architecture:
  - Runtime/Edge plane;
  - Experience/Admin plane;
  - Control plane;
  - Data plane.
- Keep Azure Front Door/WAF, API Management, Key Vault, App Configuration, Blob/CDN, Event Hubs, Azure AI Search, and OpenTelemetry.
- Keep Azure Container Apps or App Service as the primary deployment model, with an AKS seam later if needed.
- Keep Infrastructure as Code using Bicep unless Terraform is preferred for portability.
- Keep GitHub Actions with CodeQL, dependency scanning, secret scanning, tests, and deployment gates.
- Keep OpenTelemetry and tenant-aware observability.
- Keep the split between OLTP configuration/content storage and OLAP event analytics.
- Keep a silo seam for enterprise tenants that require dedicated infrastructure.

### 2.4 Runtime SDK and delivery

- Keep the JavaScript/TypeScript runtime SDK.
- Keep async, non-blocking CDN delivery.
- Keep versioned content bundles.
- Keep client-side targeting/rule evaluation for speed and resilience.
- Keep Shadow DOM or equivalent style isolation.
- Keep safe failure: the adoption runtime must never break Property MTD or a host application.
- Keep preview/QA channels separate from production content.
- Keep browser extension support as the zero-code route for third-party apps, but not as the primary route for Property MTD.

### 2.5 Element engine

- Adopt the multi-signal element descriptor:
  - stable attributes;
  - `data-*` identifiers;
  - ARIA and accessible names;
  - role;
  - text fingerprint;
  - class tokens;
  - parent/sibling structure;
  - geometry;
  - nearby labels.
- Adopt confidence-scored runtime resolution rather than single CSS selectors.
- Add graceful suppression when anchor confidence is too low.
- Add MutationObserver and SPA route-change support.
- Add same-origin iframe and Shadow DOM support where technically feasible.
- Add anchor-health telemetry.
- Add one-action re-capture and bulk find/replace.
- Add a maintained Element Library as a strategic differentiator, starting with Property MTD first and Microsoft/Dynamics later.
- Keep vision/computer-use fallback as a privacy-gated, last-resort recovery mechanism.

### 2.6 Authoring Studio

- Keep the no-code in-app editor.
- Keep live target-app capture and true-to-runtime preview.
- Keep walkthroughs, tooltips, smart tips, launchers, checklists, banners, modals, surveys, and resource centre.
- Keep AI-assisted authoring, but require human review and approval before publication.
- Keep localisation and theming.
- Keep content templates, folders, tags, and search.
- Keep governance states: draft, review, approved, published, archived.
- Keep versioning and rollback.

### 2.7 Analytics and closed loop

- Adopt autocapture, but structure-only by default.
- Adopt retroactive tagging where possible.
- Adopt funnels, paths, retention, cohorts, and guide-performance analytics.
- Adopt cohort-to-segment workflow.
- Adopt friction detection: rage clicks, repeated validation errors, dead ends, high drop-off, and repeated backtracking.
- Adopt guide-to-outcome conversion metrics.
- Adopt data export to warehouse/object storage.
- Adopt an insights agent for natural-language analytics, with transparent generated queries.

### 2.8 AI and RAG

- Adopt RAG-grounded answer agent with citations.
- Adopt AI co-author for draft guidance.
- Adopt AI insights agent.
- Adopt strict AI guardrails:
  - no unsupported tax advice;
  - no hallucinated answers;
  - no cross-tenant retrieval;
  - no sensitive screen values by default;
  - prompt-injection defences;
  - source citations;
  - human escalation.
- Add model-provider abstraction so the platform can use Azure OpenAI/OpenAI first while allowing other models later.
- Keep MCP-style interface as a future integration pattern, but initially position it as a secure internal/Codex automation interface.

### 2.9 Wireframes and UX

- Keep the Admin Studio global shell.
- Keep tenant onboarding and identity setup wireframes.
- Keep content library wireframe.
- Keep in-app editor wireframe; this is one of the signature product experiences.
- Keep segment builder wireframe.
- Keep analytics funnel and insights-agent wireframes.
- Keep governance review wireframe.
- Keep runtime self-help widget, answer agent, walkthrough step, checklist, and survey wireframes.
- Add dedicated Property MTD runtime examples:
  - HMRC authorisation guidance;
  - property setup checklist;
  - income/expense categorisation tips;
  - quarterly update walkthrough;
  - agent/client readiness dashboard guidance.

### 2.10 Build process

- Keep traceability from user stories to functional requirements to tests.
- Keep production definition of done.
- Keep small PRs with GitHub CI gates.
- Keep code/spec lockstep.
- Keep explicit acceptance criteria for every sprint.
- Convert the build prompt from Claude Code to Codex + Visual Studio.

---

## 3. Items I would reject, amend, or defer

### 3.1 Reject or amend

- **Absolute “zero-code on target app”** should be amended. For Property MTD and our own future apps, use a first-party SDK and stable `data-adopt-id` anchors. Zero-code/browser-extension mode should be for third-party apps where we cannot change source.
- **Claude Code as the delivery tool** should be replaced by Codex + Visual Studio + GitHub.
- **TypeScript-first/NestJS-first as the default backend** should be amended. For this project, make the SaaS control plane .NET-first because Property MTD is already .NET/Blazor/EF-oriented and you want Visual Studio delivery. Keep TypeScript where it is clearly the right tool: runtime SDK, browser extension, and possibly small front-end packages.
- **Six phases / Phase 0–5 plan** should be collapsed into five production sprints to match the stated maximum.
- **AI provider hardcoding** should be amended. Do not hardwire Claude as primary. Use a model-provider abstraction, with Azure OpenAI/OpenAI as the default path for Codex/OpenAI alignment and Azure governance.
- **Market-size figures sourced via vendor reporting** should remain internal/directional unless independently verified before external sales or investor material.
- **Full form interaction autocapture** should be heavily constrained. For Property MTD, capture structural behaviour only by default; do not collect tax values, addresses, bank values, UTRs, NI numbers, HMRC tokens, or client identifiers.
- **MCP as a public customer feature from day one** should be amended. Build the seam, then expose it only after RBAC, audit, rate limits, and tenant boundaries are mature.

### 3.2 Defer to later roadmap unless explicitly required

- Native mobile SDK.
- Citrix/VDI/desktop guidance.
- Session replay.
- Whatfix Mirror-style simulation/sandbox training.
- Full cross-app AI-agent orchestration.
- Full enterprise connector marketplace.
- Tenant-managed storage for every tenant.
- Maintained element libraries for many third-party apps. Start with Property MTD and Microsoft/Dynamics; expand gradually.
- Browser extension as the primary Property MTD delivery mechanism.
- Vision/computer-use fallback as a default runtime path. Keep it gated, opt-in, expensive-path only.

---

## 4. Updated specification decisions by document

### 4.1 Update `00-product-vision-and-competitive-analysis.md`

Add:

- Product mode distinction:
  - **First-party mode** for Property MTD and owned apps: SDK + stable anchors + domain events.
  - **Third-party mode** for external SaaS apps: browser extension + resilient selector graph.
- Strategic wedge:
  - “Best Microsoft-secured DAP for regulated SaaS and MTD-style compliance workflows.”
- Property MTD as the first reference implementation.
- Codex + Visual Studio as the build workflow instead of Claude Code.
- Maximum five production sprints.

Amend:

- “Zero-code on the target app” to “zero-code where required, first-party SDK where we own the app.”
- Competitor/market statistics should be marked as internal benchmark material unless independently verified before public use.

### 4.2 Update `01-user-requirements.md`

Add personas:

- **Property MTD Product Owner** — owns adoption outcomes across HMRC authorisation, property setup, records, and submissions.
- **Landlord End User** — needs in-flow help with MTD setup and quarterly updates.
- **Agent/Accountant User** — manages multiple clients and needs readiness, exception, and deadline guidance.
- **Support Agent** — needs adoption history and failed-help-search context to resolve tickets faster.
- **Compliance Reviewer** — reviews tax-sensitive guidance before publication.

Add journeys:

- Property MTD setup journey.
- HMRC authorisation journey.
- Digital records journey.
- Quarterly update journey.
- Agent/client readiness journey.
- Error recovery journey.

### 4.3 Update `02-functional-requirements.md`

Add requirements:

- `FR-MTD-001`: The Property MTD app shall initialise the adoption runtime in the Blazor/Web app shell.
- `FR-MTD-002`: The Property MTD backend shall expose a safe `/api/adoption/context` endpoint.
- `FR-MTD-003`: The Property MTD UI shall include stable `data-adopt-id` anchors on guided controls.
- `FR-MTD-004`: The Property MTD backend shall emit adoption-safe domain events for workflow status changes.
- `FR-MTD-005`: The adoption platform shall not store or transmit HMRC tokens, submission payloads, UTRs, NI numbers, property addresses, bank values, or tax values by default.
- `FR-MTD-006`: Tax-sensitive content shall require compliance approval before production publication.
- `FR-MTD-007`: Property MTD guide content shall include HMRC authorisation, property setup, digital records, transaction categorisation, quarterly review, submission history, and agent/client readiness.

Amend:

- Browser extension should be **Should** for third-party app coverage, not the first Property MTD route.
- AI vision fallback should be **Should/Controlled** rather than default Must.
- Autocapture must explicitly state “structure-only, no field values by default.”

### 4.4 Update `03-solution-architecture.md`

Change default implementation stack to:

- **Control plane:** .NET 9 / ASP.NET Core APIs.
- **Admin Studio:** Blazor Web App or ASP.NET Core + React only if UI velocity demands it.
- **Runtime SDK:** TypeScript, Shadow DOM, Vite/esbuild.
- **Browser extension:** TypeScript Manifest V3.
- **Database:** Azure SQL with Row-Level Security or Azure PostgreSQL with RLS; choose based on existing Property MTD data platform. If starting fresh, PostgreSQL remains strong; if staying Microsoft-native end to end, Azure SQL is cleaner.
- **Analytics:** ClickHouse on Azure or Azure Data Explorer/Fabric behind an analytics abstraction.
- **AI:** Azure OpenAI/OpenAI through a provider abstraction; optional additional providers later.
- **Build:** Visual Studio, Codex, GitHub, GitHub Actions.

Add Property MTD integration architecture:

```text
Property MTD Blazor App
  ├── Adoption SDK initialised in app shell
  ├── data-adopt-id anchors in shared components
  ├── route/context updates sent to runtime
  └── runtime experiences rendered in Shadow DOM

Property MTD Backend
  ├── /api/adoption/context
  ├── adoption-safe domain event adapter
  ├── sensitive data redaction
  └── no direct adoption access to HMRC tokens or payloads

Adoption SaaS Control Plane
  ├── content delivery
  ├── targeting
  ├── authoring
  ├── analytics
  ├── AI help
  ├── governance
  └── tenant/security administration
```

### 4.5 Update `04-wireframes.md`

Add Property MTD-specific wireframes:

- Property MTD adoption dashboard.
- Property MTD guide pack management.
- HMRC authorisation walkthrough.
- Quarterly update walkthrough.
- Validation-error guidance panel.
- Agent/client readiness checklist.
- Compliance review screen for tax-sensitive content.
- Runtime contextual help on `/properties`, `/income`, `/expenses`, `/transactions`, `/quarterly-review`, `/hmrc-settings`, `/submissions`.

### 4.6 Replace `05-claude-build-prompt.md`

Replace with `05-codex-visual-studio-build-prompt.md`.

Required changes:

- Replace Claude Code with Codex.
- Replace Next.js/NestJS default with .NET-first control plane.
- Keep TypeScript runtime SDK.
- Add Visual Studio solution structure.
- Add Property MTD integration rules.
- Add maximum five production sprints.
- Add explicit Codex guardrails:
  - no prototype code;
  - no weakening tenant isolation;
  - no tax data leakage;
  - no HMRC workflow changes unless explicitly requested;
  - every PR must include tests, documentation, and acceptance criteria.

---

## 5. Revised maximum-five-sprint production plan

### Sprint 1 — SaaS Foundation, Security & Platform Skeleton

Deliver production-ready:

- Visual Studio solution and repo structure.
- .NET control plane API.
- Admin shell.
- Entra ID workforce authentication.
- Entra External ID support path.
- Multi-tenant data model.
- Row-level tenant isolation.
- RBAC.
- Audit logging.
- Azure infrastructure via Bicep.
- GitHub Actions CI/CD.
- CodeQL, dependency scanning, secret scanning.
- OpenTelemetry baseline.
- Tenant isolation test harness.

Exit gate:

- A tenant admin can sign in, see a tenant-scoped admin shell, create/read tenant-scoped configuration, and CI proves cross-tenant access fails.

### Sprint 2 — Runtime SDK, Property MTD Integration & Core Guidance

Deliver production-ready:

- TypeScript runtime SDK.
- Blazor integration wrapper for Property MTD.
- `data-adopt-id` support in key Property MTD components.
- Safe `/api/adoption/context` endpoint.
- Content model for walkthroughs, tooltips, banners, checklists, launchers, and resource centre.
- CDN/versioned bundle delivery.
- Client-side rule evaluation.
- Element engine v1 using deterministic DOM strategies.
- Initial Property MTD guide pack.

Exit gate:

- A production-safe guide runs in Property MTD, targets users by safe context, attaches to stable anchors, persists completion/dismissal state, and never accesses HMRC/tax-sensitive data.

### Sprint 3 — Authoring Studio, Governance & Content Lifecycle

Deliver production-ready:

- No-code authoring studio.
- In-app editor/capture mode.
- True runtime preview.
- Draft/review/approval/publish lifecycle.
- Compliance review flag for tax-sensitive content.
- Versioning and rollback.
- Environments: dev, test, production.
- Theming and accessibility standards.
- Anchor health v1.

Exit gate:

- An authorised content author can create, preview, submit, approve, and publish Property MTD guidance through governance with full audit trail and rollback.

### Sprint 4 — Analytics, Segmentation, AI Help & Closed Loop

Deliver production-ready:

- Runtime event collector.
- Event ingestion pipeline.
- Structural autocapture.
- Property MTD workflow events.
- Funnel and guide analytics.
- Segment builder.
- Cohort-to-segment targeting.
- Self-help widget.
- RAG answer agent with approved knowledge sources.
- AI co-author.
- AI insights agent.
- No-tax-advice and no-sensitive-data guardrails.

Exit gate:

- Product team can see where users abandon Property MTD workflows, target a segment with guidance, measure guide-to-outcome conversion, and users can ask the help widget approved knowledge questions with cited answers.

### Sprint 5 — Enterprise Hardening, Scale, Extension & Production Readiness

Deliver production-ready:

- Advanced RBAC/custom roles.
- SCIM provisioning.
- Data residency controls.
- Silo tenant seam.
- Surveys/NPS.
- Data export.
- Webhooks.
- Browser extension for third-party apps.
- Element Library v1.
- Vision fallback behind tenant opt-in.
- Performance/load testing.
- Security hardening.
- DR/backup/restore runbooks.
- Operational dashboards.
- Production deployment checklist.

Exit gate:

- The platform is production-ready SaaS for Property MTD and future owned apps, with a controlled path to third-party app coverage through extension mode.

---

## 6. Codex master prompt replacement — summary

A full `05-codex-visual-studio-build-prompt.md` should instruct Codex to:

- build production-grade code only;
- use Visual Studio and .NET-first architecture;
- keep TypeScript runtime SDK isolated and small;
- keep tenant isolation sacred;
- keep Property MTD business/HMRC logic untouched unless explicitly requested;
- use Entra/External ID, RBAC, audit, Key Vault, managed identities, and GitHub Actions;
- implement the five sprints in order;
- create small PRs;
- reference requirement IDs in every PR;
- include tests and docs in every PR;
- never collect sensitive MTD data by default;
- run build, unit, integration, Playwright, isolation, accessibility, and security checks before claiming completion.

---

## 7. Final recommendation

Use the uploaded pack as the specification baseline, but apply the amendments in this document before starting development.

The corrected direction is:

```text
Production-grade Microsoft-secured DAP SaaS
+ first-party SDK for Property MTD and owned apps
+ browser extension for third-party apps
+ .NET/Blazor/Visual Studio control plane
+ TypeScript runtime SDK
+ Entra ID / Entra External ID
+ strict tenant isolation
+ safe Property MTD context/events
+ no sensitive tax-data capture
+ resilient element engine
+ no-code authoring
+ analytics closed loop
+ governed AI assistance
+ maximum five production sprints
```
