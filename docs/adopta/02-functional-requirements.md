# 02 — Functional Requirements Document (FRD)

> **Product:** Adopta (DAP) · **Status:** Baseline v1.0
> **Traceability:** Each requirement cites the user story it satisfies (`→ US-*`). Architecture realises these in `03-solution-architecture.md`.
> **Conventions:** **FR** = functional requirement. **MoSCoW** priority (M/S/C). "The system" = the Adopta platform (control plane + runtime + APIs).

---

## 0. System Decomposition (functional view)

Adopta comprises seven functional subsystems. FRs are grouped accordingly.

1. **Identity & Tenancy** (auth, RBAC, provisioning, isolation, audit) — `FR-IDN-*`
2. **Authoring Studio** (admin app + in-app editor + AI co-author) — `FR-AUT-*`
3. **Element Engine** (capture, anchoring, resilience, library) — `FR-ELM-*`
4. **Targeting & Delivery** (segments, rules, runtime SDK) — `FR-DEL-*`
5. **Self-Service & AI Assist** (widget, search, answer agent) — `FR-HLP-*`
6. **Analytics & Data** (ingestion, autocapture, reporting, friction, export, insights agent) — `FR-ANL-*`
7. **Engagement, Governance & Integration** (surveys, lifecycle, APIs, MCP, connectors) — `FR-GOV-*`, `FR-INT-*`

---

## 1. Identity & Tenancy — `FR-IDN-*`

### 1.1 Authentication & federation
- **FR-IDN-001 (M)** The system SHALL authenticate via **OpenID Connect / OAuth 2.0** against **Microsoft Entra ID**, registered as a **multi-tenant application**, supporting admin-consent onboarding for customer workforce tenants. → US-IAM-1
- **FR-IDN-002 (M)** The system SHALL support **Microsoft Entra External ID** (external-tenant configuration) for customer/consumer identities (CIAM), including self-service sign-up, email, and social IdP options, **without legacy Azure AD B2C custom XML policies**. The system SHALL NOT depend on Azure AD B2C for new tenants (end-of-sale 1 May 2025). → US-IAM-2
- **FR-IDN-003 (M)** The system SHALL honour the customer tenant's **Conditional Access** outcomes and support **MFA**, including **phishing-resistant methods**; where configured, it SHALL accept **MFA/device-compliance claims** from the user's home tenant. → US-IAM-3
- **FR-IDN-004 (S)** The system SHALL support **generic OIDC and SAML 2.0** federation for non-Microsoft IdPs (Okta, Ping, Google Workspace) so non-Entra tenants can adopt it. → US-IAM-1
- **FR-IDN-005 (M)** The system SHALL issue **short-lived access tokens** and **rotating refresh tokens**, validate audience/issuer/signature on every request, and support token revocation on user deactivation. → US-IAM-3, US-IAM-5

### 1.2 Authorisation (RBAC)
- **FR-IDN-010 (M)** The system SHALL provide built-in roles: **Owner, Administrator, Author, Reviewer/Approver, Analyst, Viewer**, with least-privilege defaults; permissions SHALL be enforced server-side on every API. → US-IAM-4
- **FR-IDN-011 (S)** The system SHALL support **custom roles** composed from a documented permission set (resource × action). → US-IAM-4
- **FR-IDN-012 (M)** The system SHALL scope all authorisation checks to the **acting user's tenant**; a token issued for tenant A SHALL never authorise access to tenant B data. → US-IAM-6

### 1.3 Provisioning & lifecycle
- **FR-IDN-020 (M)** The system SHALL expose a **SCIM 2.0** endpoint for create/update/deactivate of users and **group→role mapping**. → US-IAM-5
- **FR-IDN-021 (M)** Deactivation via SCIM or IdP SHALL revoke active sessions/tokens within a defined SLA (≤5 min target). → US-IAM-5

### 1.4 Tenancy, isolation & residency
- **FR-IDN-030 (M)** Every persisted record (config, content, events, files) SHALL carry an immutable **tenant_id**; all queries SHALL be tenant-filtered by default at the data-access layer. → US-IAM-6
- **FR-IDN-031 (M)** The system SHALL enforce isolation such that no UI/API path can return cross-tenant data; this SHALL be covered by **automated isolation tests** in CI. → US-IAM-6
- **FR-IDN-032 (S)** The system SHALL support **per-tenant data residency** selection at tenant creation, keeping data at rest in the chosen region. → US-IAM-9
- **FR-IDN-033 (S)** The system SHALL support **tenant-managed storage** for content/media (data remains in the customer's boundary) as a configurable option. → US-IAM-9

### 1.5 Audit & app scoping
- **FR-IDN-040 (M)** The system SHALL record an **immutable audit event** (actor, tenant, action, target, before/after, timestamp, IP/agent) for all admin, authoring, security, and publish actions; audit logs SHALL be queryable and exportable and retained for a configurable window. → US-IAM-7
- **FR-IDN-041 (M)** The system SHALL allow admins to define **allowed/blocked domains and URL patterns**; the runtime SHALL refuse to initialise outside allowed scope, performing matching **client-side** where possible to avoid leaking browsing data. → US-IAM-8
- **FR-IDN-042 (S)** The system SHALL provide **PII redaction/masking** policies (field types, selectors, regex) applied to captured text, analytics, and replay; masking SHALL be **default-on** for known sensitive inputs (password, payment, etc.). → US-IAM-10

---

## 2. Authoring Studio — `FR-AUT-*`

### 2.1 In-app editor
- **FR-AUT-001 (M)** The system SHALL provide an **in-browser editor** that activates on an allowed target app and lets authors **select elements visually** with a highlight overlay and inspector. → US-AUTH-1
- **FR-AUT-002 (M)** The editor SHALL render a **true-to-runtime preview** so authors see exactly what end users will see (same anchoring, theming, and rules). → US-AUTH-1
- **FR-AUT-003 (M)** The editor SHALL support **SPA navigation, Shadow DOM, and same-origin iframes** during capture where technically feasible, and clearly indicate when an element is in an unsupported context. → US-NFR-5

### 2.2 Content types
- **FR-AUT-010 (M)** The system SHALL support **multi-step walkthroughs/flows** with per-step: anchor, rich content (text/image/video/links), placement, and **advance trigger** (click, hover, input, navigation, custom event). → US-AUTH-2
- **FR-AUT-011 (M)** Flows SHALL support **conditional branching** based on element state, URL, segment, or user response. → US-AUTH-2
- **FR-AUT-012 (M)** The system SHALL support **smart tips (field-level), tooltips, and hotspots/beacons** with configurable triggers and grouping per page. → US-AUTH-3
- **FR-AUT-013 (M)** The system SHALL support **launchers** (visible buttons/menus and **invisible launchers** bound to elements), **onboarding checklists/task lists** with persisted progress, **banners**, and **modals/lightboxes**. → US-AUTH-4
- **FR-AUT-014 (S)** The system SHALL support a **Resource Center** container that aggregates flows, articles, videos, links, and announcements, themable and segment-aware. → US-HELP-1, US-HELP-3

### 2.3 AI co-author
- **FR-AUT-020 (M)** The system SHALL provide an **AI co-author** that, given an author's natural-language intent and a captured screen, **drafts a flow and copy** as fully editable content. → US-AUTH-5
- **FR-AUT-021 (M)** All AI-generated content SHALL be **editable, attributable, and non-binding** (author can accept/reject/modify any element); the system SHALL not auto-publish AI output. → US-AUTH-5, Principle 6
- **FR-AUT-022 (S)** The AI co-author SHALL support **tone, length, reading-level, and brand-voice** controls and **rewrite/expand/shorten/translate** operations on selected content. → US-AUTH-5, US-AUTH-8

### 2.4 Localisation, theming, reuse
- **FR-AUT-030 (M)** The system SHALL support **multi-language** content: a base locale plus variants, **machine-translation assist**, per-locale review status, and **runtime locale resolution** by user/browser locale or explicit setting. → US-AUTH-8
- **FR-AUT-031 (S)** The system SHALL support **tenant theming** (colours, typography, logo, button/element styles, motion) applied consistently to all runtime UI, with per-campaign overrides. → US-AUTH-9
- **FR-AUT-032 (S)** The system SHALL support **templates, cloning, folders, tags, and search** in an organisation content library. → US-AUTH-10

---

## 3. Element Engine — `FR-ELM-*`

> The durability core. See `03 §7` for the algorithmic design.

- **FR-ELM-001 (M)** On capture, the system SHALL record a **multi-signal element descriptor**, including: stable attributes (id/name/data-* /aria), tag, role, accessible name/text, class tokens (weighted), structural path (parent/sibling context), bounding/relative position, and nearby anchor text. → US-AUTH-6
- **FR-ELM-002 (M)** At runtime, the system SHALL locate the target by **scoring candidate elements** against the descriptor and selecting the best match above a confidence threshold; it SHALL NOT rely on a single brittle selector. → US-AUTH-6, NFR resilience
- **FR-ELM-003 (M)** When the primary strategy fails, the system SHALL apply **ordered fallbacks** (alternative selectors, text/role match, positional/visual heuristics) and, if still unresolved, **fail gracefully** (suppress the item, log an anchor-health event) without errors visible to the end user. → US-AUTH-6, US-NFR-1
- **FR-ELM-004 (S)** The system SHALL provide an **optional vision/computer-use fallback**: when DOM strategies fail, a multimodal model MAY identify the element from a screenshot + DOM context to re-establish or suggest a new anchor. This SHALL be privacy-gated (tenant opt-in; no PII egress). → US-AUTH-6 (Pillar 1)
- **FR-ELM-005 (M)** The system SHALL **continuously/periodically validate anchors** against target apps and emit **anchor-health events**; broken anchors SHALL surface as alerts listing affected items. → US-GOV-3
- **FR-ELM-006 (M)** The system SHALL provide **one-action re-capture** and **bulk find/replace** of element descriptors/selectors across content. → US-GOV-3
- **FR-ELM-007 (S)** The system SHALL provide a **maintained Element Library** for designated apps (e.g., Microsoft 365, Dynamics 365, Salesforce, Workday, ServiceNow); library updates SHALL **propagate automatically** to content bound to library elements. → US-AUTH-7
- **FR-ELM-008 (M)** The engine SHALL handle **Shadow DOM** (pierce/scope queries) and **same-origin iframes**; cross-origin iframes SHALL be detected and reported as a limitation. → US-NFR-5
- **FR-ELM-009 (M)** Captured descriptors SHALL exclude end-user content/PII by default (structure only), consistent with the privacy principle and `FR-IDN-042`. → US-IAM-10

---

## 4. Targeting & Delivery — `FR-DEL-*`

### 4.1 Segmentation & rules
- **FR-DEL-001 (M)** The system SHALL build **segments** from identity metadata (role, department, plan, locale, custom) and behaviour (events, page visits, item completion), with **AND/OR/NOT** logic and a **live audience-size preview**. → US-SEG-1
- **FR-DEL-002 (M)** Each content item SHALL support **display conditions**: URL/route patterns, element-presence/state, app/device/browser, and segment membership, combinable with boolean logic. → US-SEG-2
- **FR-DEL-003 (M)** The system SHALL support **frequency caps, snooze/dismiss memory (persisted per user), date/time windows, and cross-item priority** to prevent conflicting/overlapping prompts. → US-SEG-3
- **FR-DEL-004 (S)** Any **analytics cohort** SHALL be saveable as a reusable **segment** for targeting with no redefinition. → US-SEG-4
- **FR-DEL-005 (C)** The system SHALL support **A/B/n testing**: split a segment across content variants and report completion/goal **lift with statistical significance**. → US-SEG-5

### 4.2 Runtime SDK & delivery
- **FR-DEL-010 (M)** The system SHALL provide a **single asynchronous JavaScript snippet** that loads the runtime agent from a **CDN**, with **no render-blocking** of the host app. → US-INT-1, US-NFR-1
- **FR-DEL-011 (M)** The system SHALL provide a **browser-extension** deployment path that injects the runtime by **client-side URL matching** for apps where the snippet cannot be installed. → US-IAM-8 (deployment flexibility)
- **FR-DEL-012 (M)** Published content SHALL be delivered as **cacheable, versioned bundles** to the browser so the runtime does not require a server round-trip per page; updates SHALL invalidate caches deterministically. → US-NFR-3
- **FR-DEL-013 (M)** The runtime SHALL evaluate targeting/rules **client-side** against the loaded config and the current page, requesting only the content the user is eligible for. → US-SEG-2, US-NFR-1
- **FR-DEL-014 (M)** The runtime SHALL **isolate its UI** (e.g., Shadow DOM / scoped styles) to avoid CSS/JS collisions with the host app, and SHALL be removable without side effects. → US-NFR-1
- **FR-DEL-015 (M)** The runtime SHALL **degrade safely**: any internal error SHALL be caught, logged, and SHALL NOT surface to or break the host app. → US-NFR-1
- **FR-DEL-016 (S)** The runtime SHALL support **published vs. preview/QA channels** so authors/reviewers can see unpublished content without affecting end users. → US-GOV-2
- **FR-DEL-017 (S)** The system SHALL provide **mobile SDKs** (phase 2) sharing the content/targeting model. → roadmap

---

## 5. Self-Service & AI Assist — `FR-HLP-*`

- **FR-HLP-001 (M)** The system SHALL render a **self-help widget** (persistent, unobtrusive, themable, **WCAG 2.2 AA**, keyboard-accessible) that opens a panel with **search**, **contextual content**, and **launchable guides**. → US-HELP-1, US-NFR-2
- **FR-HLP-002 (M)** The widget SHALL surface **contextually relevant content** for the current URL/segment/element context, ranked by relevance. → US-HELP-3
- **FR-HLP-003 (M)** The system SHALL provide an **AI answer agent** that responds to **natural-language questions** grounded (RAG) in the tenant's content, returning **citations**, offering to **launch the relevant walkthrough**, and **declining ("I don't know")** when unsupported rather than fabricating. → US-HELP-2
- **FR-HLP-004 (S)** The system SHALL **federate external knowledge sources** (e.g., SharePoint, Zendesk, web KB) into search/answers with attribution and per-source toggles. → US-HELP-4
- **FR-HLP-005 (S)** When unresolved, the widget SHALL offer a **support fallback** (ticket/chat) with **context handoff** (app, page, question, attempted guides) to the configured channel. → US-HELP-5
- **FR-HLP-006 (M)** Answer-agent interactions SHALL be **logged** (query, sources used, resolution, rating) for analytics and quality, honouring PII policy. → US-ANL (quality), US-IAM-10

---

## 6. Analytics & Data — `FR-ANL-*`

### 6.1 Ingestion & autocapture
- **FR-ANL-001 (M)** The runtime SHALL **autocapture** page views, navigations, clicks, and form interactions **structurally** (no content/PII by default) from install, batching and sending events asynchronously with retry/offline buffering. → US-AN-1, US-NFR-1
- **FR-ANL-002 (M)** The system SHALL support **retroactive tagging**: defining a page/feature later SHALL apply to **historical** captured data back to install. → US-AN-1
- **FR-ANL-003 (M)** The system SHALL accept **custom events** and **identity/account traits** via SDK/API with **signed identity** to prevent spoofing. → US-INT-2
- **FR-ANL-004 (M)** Ingestion SHALL be **idempotent and ordered-enough** for analytics, scale **horizontally**, and apply **per-tenant rate limiting** and schema validation. → US-NFR-3, US-IAM-6

### 6.2 Reporting
- **FR-ANL-010 (M)** The system SHALL provide **funnels** (multi-step, segmentable, with conversion/drop-off per step). → US-AN-2
- **FR-ANL-011 (M)** The system SHALL provide **path/journey analysis, retention curves, and cohort comparisons**, filterable by segment and date range. → US-AN-2
- **FR-ANL-012 (M)** The system SHALL provide **guidance performance analytics** per item: views, starts, step-level drop-off, completions, dismissals, **goal attainment**, and **lift vs. control**. → US-AN-3
- **FR-ANL-013 (S)** The system SHALL provide **process/cross-app analytics**: unified journeys and tool-usage/portfolio views across multiple apps captured via SDK/extension. → US-AN-4
- **FR-ANL-014 (S)** The system SHALL provide **friction detection**: automatic identification of **rage-clicks, dead-ends, error loops, repeated back-tracking, and high drop-off**, surfaced as **alerts with suggested actions** (e.g., "add a tooltip here"). → US-AN-5
- **FR-ANL-015 (C)** The system SHALL provide **privacy-masked session replay** linked to analytics, with sensitive content redacted by default per `FR-IDN-042`. → US-AN-8

### 6.3 Insights agent & export
- **FR-ANL-020 (M)** The system SHALL provide an **insights agent** answering **natural-language analytics questions** over the tenant's data, rendering charts/tables and exposing the **generated query** for transparency/editing. → US-AN-6
- **FR-ANL-021 (M)** The system SHALL support **data export** of raw events and aggregates to the customer's **warehouse/object storage** (scheduled and/or streaming) with a **documented, versioned schema**. → US-AN-7

---

## 7. Engagement & Feedback — `FR-GOV-*` (engagement) 

- **FR-ENG-001 (M)** The system SHALL support **in-app surveys, NPS, CSAT, and polls** with multiple question types, targeting/scheduling/frequency, a results dashboard, and export. → US-FB-1
- **FR-ENG-002 (S)** The system SHALL support **announcements/targeted messaging** (modals/banners) to segments with **delivery/acknowledgement tracking**. → US-FB-2
- **FR-ENG-003 (C)** The system SHALL provide **sentiment/theme analysis** over free-text feedback with drill-down to verbatims. → US-FB-3

## 8. Governance & Lifecycle — `FR-GOV-*`

- **FR-GOV-001 (M)** The system SHALL enforce a configurable **draft → review → approve → publish** workflow; publishing SHALL be **gated on approval**, with reviewer notifications and full history. → US-GOV-1
- **FR-GOV-002 (M)** The system SHALL provide **environments (dev/QA/prod)** with **content promotion**, **immutable version history**, and **one-click rollback**. → US-GOV-2
- **FR-GOV-003 (M)** The system SHALL provide **anchor-health monitoring**, alerts, and remediation tooling (re-capture, bulk replace) per `FR-ELM-005/006`. → US-GOV-3
- **FR-GOV-004 (S)** The system SHALL provide **operator dashboards** for per-tenant health, error rates, MAU, content stats, and **SLO/alerting** (operator persona). → US-GOV-4

## 9. Integrations, APIs & Extensibility — `FR-INT-*`

- **FR-INT-001 (M)** The system SHALL expose **versioned, authenticated REST APIs** (OpenAPI 3.x) for content, segments, analytics queries, and admin, with **per-tenant rate limiting** and pagination. → US-INT-3
- **FR-INT-002 (M)** The system SHALL emit **webhooks** for key events (publish, anchor-broken, survey-response, threshold alerts) with retry and signature verification. → US-INT-3
- **FR-INT-003 (S)** The system SHALL provide an **MCP server** exposing analytics queries and content/segment objects to MCP-compatible clients (Claude, Cursor), respecting **tenant auth + RBAC**. → US-INT-4
- **FR-INT-004 (S)** The system SHALL provide **prebuilt connectors** (Microsoft 365/Teams/SharePoint, Salesforce, ServiceNow, Slack, Zendesk) configurable via UI with least-privilege scopes and health status. → US-INT-5
- **FR-INT-005 (M)** All public interfaces SHALL be documented with examples, typed schemas, and a sandbox/test mode. → US-INT-1, US-INT-3

---

## 10. Non-Functional Requirements (system) — `NFR-*`

> User-visible NFRs are in `01 §3 Epic I`; these are the engineering targets. Final numeric SLOs to be ratified during architecture sign-off; values below are **recommended baselines**.

| ID | Category | Requirement (baseline) |
|---|---|---|
| **NFR-PERF-1** | Runtime weight | Core runtime initial payload ≤ ~50–100 KB gzip for the agent loader; content bundles cached and lazy-loaded. |
| **NFR-PERF-2** | Runtime impact | No render-blocking; main-thread work chunked; target < 50 ms added to host interactions; zero layout shift from the overlay container. |
| **NFR-PERF-3** | Anchor resolution | Median element resolution < 50 ms on a typical page; bounded worst case with timeout + graceful suppression. |
| **NFR-PERF-4** | API latency | p95 control-plane API < 300 ms; p95 analytics query < 2 s on standard ranges. |
| **NFR-PERF-5** | Ingestion scale | Sustain high event throughput per tenant with horizontal scaling; backpressure without data loss (buffer + retry). |
| **NFR-AVAIL-1** | Availability | Control plane 99.9% target; **runtime content delivery** resilient to backend outage via CDN/cache (higher effective availability). |
| **NFR-SEC-1** | Crypto | TLS 1.2+ in transit; encryption at rest (keys managed in a KMS/Key Vault); secrets never in source. |
| **NFR-SEC-2** | AppSec | OWASP ASVS-aligned; dependency and container scanning in CI; SAST/DAST; signed artefacts. |
| **NFR-SEC-3** | Isolation | Automated cross-tenant isolation tests gate every release (`FR-IDN-031`). |
| **NFR-COMP-1** | Compliance | Architecture and processes aligned to **SOC 2 Type II / ISO 27001 / GDPR**; DPA + sub-processor list; data-subject request tooling. |
| **NFR-A11Y-1** | Accessibility | All runtime + admin UI meet **WCAG 2.2 AA**; respects reduced-motion/contrast. |
| **NFR-OBS-1** | Observability | Structured logs, distributed tracing, metrics, and per-tenant dashboards; alerting on SLO burn. |
| **NFR-PRIV-1** | Privacy | Default no-PII capture; tenant-controlled masking/redaction; data residency; retention controls; right-to-erasure support. |
| **NFR-I18N-1** | Internationalisation | Full Unicode; RTL support in runtime UI; locale-aware formatting. |
| **NFR-COMPAT-1** | Compatibility | Latest 2 versions of Chrome/Edge/Firefox/Safari; SPA + Shadow DOM + same-origin iframe support. |
| **NFR-SCALE-1** | Multi-tenant scale | Pooled multi-tenant by default; design seam for **dedicated/siloed** deployment for high-tier customers without code change. |

---

## 11. Acceptance & Definition of Done (functional)

A feature is **Done** when: (1) it satisfies its FRs and traces to a `US-*`; (2) it has automated tests including **tenant-isolation** and **graceful-degradation** cases where applicable; (3) it meets the relevant NFR baselines; (4) it is documented (user + API where relevant); (5) it passes security and accessibility checks in CI; and (6) it is observable (emits the metrics/logs needed to operate it).

---

### Traceability index (summary)
- Identity/Tenancy `FR-IDN-*` ← US-IAM-1..10
- Authoring `FR-AUT-*` ← US-AUTH-1..10
- Element engine `FR-ELM-*` ← US-AUTH-6/7, US-GOV-3, US-NFR-5
- Targeting/Delivery `FR-DEL-*` ← US-SEG-1..5, US-INT-1, US-NFR-1/3
- Self-service/AI `FR-HLP-*` ← US-HELP-1..5
- Analytics `FR-ANL-*` ← US-AN-1..8
- Engagement `FR-ENG-*` ← US-FB-1..3
- Governance `FR-GOV-*` ← US-GOV-1..4
- Integration `FR-INT-*` ← US-INT-1..5
