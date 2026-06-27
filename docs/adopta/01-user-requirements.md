# 01 — User Requirements Document (URD)

> **Product:** Adopta (DAP) · **Status:** Baseline v1.0 · **Companion:** `02-functional-requirements` (the FRD translates these into testable functional specs).

This document captures **what users need and why**, in their language. It is intentionally solution-agnostic; the *how* lives in the FRD and architecture. Every user story carries an ID (`US-<area>-<n>`) so functional requirements and tests can trace back to it.

---

## 1. Personas

### P1 — Tara, the **Tenant Administrator** (IT / Security / Platform owner)
- **Context:** Owns the Adopta tenant for her organisation. Cares about SSO, security posture, provisioning, data residency, governance, and cost.
- **Goals:** Stand up the tenant on corporate identity; control who can do what; ensure no PII leaks; prove compliance; manage which apps/domains Adopta runs on.
- **Frustrations with incumbents:** Heavy rollouts; per-machine extension management; unclear data handling; weak RBAC; SSO that only supports SAML and not modern Entra federation.
- **Success:** "I connected our Microsoft Entra tenant, set Conditional Access, scoped Adopta to three domains, and delegated authoring to L&D in under a day — with a full audit trail."

### P2 — Aria, the **Content Author** (L&D / Enablement / Product Manager)
- **Context:** Non-developer. Builds walkthroughs, tooltips, checklists, surveys, and announcements on top of live apps (e.g., the company's Dynamics 365 or its own SaaS product).
- **Goals:** Create durable guidance fast, without engineering; target the right users; know if it's working; fix it quickly when the app changes.
- **Frustrations:** Brittle element selection that breaks on every release; slow authoring; no idea which guides actually help; clunky targeting.
- **Success:** "I described the flow, the AI drafted it on the captured screen, I tweaked copy and targeting, sent it for approval, and shipped it — and I get an alert when an anchor breaks."

### P3 — Eli, the **End User** (employee or the customer's customer)
- **Context:** Trying to get a job done in an app. May be new, occasional, or expert. Does **not** want to be trained — wants to be *helped, in the moment*.
- **Goals:** Finish the task; get unstuck without leaving the app or filing a ticket; not be nagged.
- **Frustrations:** Pop-ups that block work; guidance pointing at the wrong place; help that doesn't match what's on screen; repetitive prompts.
- **Success:** "I clicked the help widget, asked a question in plain language, got the exact steps highlighted on my screen, and finished — no ticket, no tab-switching."

### P4 — Noah, the **Analyst** (Product Ops / Business Analyst / CS Ops)
- **Context:** Needs to understand behaviour and prove value across one or many apps.
- **Goals:** See adoption, funnels, drop-off, friction, and guide performance; segment; export to the warehouse; answer ad-hoc questions fast.
- **Frustrations:** Only seeing events someone remembered to instrument; dashboards that can't answer the new question; no link between "where users struggle" and "what guidance to ship."
- **Success:** "I asked, in plain English, where onboarding drops off this week, saw the friction point, and shipped a targeted tooltip — then watched completion rise."

### P5 — Devin, the **Developer / Integrator** (customer-side or partner)
- **Context:** Embeds the SDK, passes user/account metadata for segmentation, wires SSO/SCIM, consumes APIs/webhooks, and may build on the MCP server.
- **Goals:** Fast, well-documented integration; secure identity passing; events and data accessible programmatically; CI-friendly.
- **Frustrations:** Poorly documented SDKs; heavy bundles; no typed APIs; no event export; no programmatic authoring.
- **Success:** "I dropped in the snippet, identified users securely, and now I query analytics and manage content via API and MCP from our own tooling."

### P0 — Sam, the **Adopta Operator** (internal — us)
- **Context:** Operates the SaaS: onboards tenants, monitors health, manages the maintained element library, ships releases, supports customers.
- **Goals:** Reliable multi-tenant operations, observability, safe deploys, per-tenant support without breaching isolation.

---

## 2. Primary User Journeys (narrative)

### J1 — Tenant onboarding & hardening (Tara)
Sign up → verify domain → **connect Microsoft Entra** (workforce SSO via multi-tenant app, or External ID for external users) → enforce **Conditional Access / MFA** → configure **SCIM provisioning** → set **data residency** and **PII redaction** policy → scope Adopta to allowed **domains/apps** → invite & **role-assign** authors and analysts → review **audit log**. *Outcome: a secure, governed tenant.*

### J2 — Build & ship guidance (Aria)
Open the **authoring studio** → launch the **in-app editor** on the target app → **capture** the screen/elements → **AI drafts** a flow/tooltip → edit content, branching, styling, and **localisation** → define **targeting** (segment + page rules + frequency) → **preview** as different segments → submit to **review/approval** → **publish** to an environment → monitor **performance** and **anchor-health alerts**. *Outcome: durable, measured guidance.*

### J3 — Get unstuck in the moment (Eli)
Hit friction → open **self-help widget** (or an inline launcher fires) → **search or ask** in natural language → get an **AI answer with citations** and/or a **launchable walkthrough** that **highlights on-screen elements** → complete the task → optionally rate it. *Outcome: self-service resolution, fewer tickets.*

### J4 — Understand & act on behaviour (Noah)
Open **analytics** → explore **funnels/paths/retention/cohorts** (over **retroactively captured** data) → spot **drop-off/friction** → ask the **insights agent** a follow-up → create a **segment** of stuck users → hand off to Aria (or self-serve) to **target guidance** → measure **lift**. *Outcome: closed-loop optimisation.*

### J5 — Integrate & automate (Devin)
Install **SDK** → **identify** users/accounts with signed metadata → verify events in a debug view → call **REST APIs**/consume **webhooks** → wire **SCIM/SSO** → optionally drive analytics/content via the **MCP server**. *Outcome: programmatic, secure integration.*

---

## 3. User Requirements by Epic

> Format: **US-ID** — *As a [persona], I want [capability], so that [benefit].* Each story lists **Acceptance criteria (AC)** as user-observable outcomes. Priority: **M**=Must, **S**=Should, **C**=Could (MoSCoW). These map to FRs in `02`.

### Epic A — Identity, Access & Tenant Administration

- **US-IAM-1 (M)** — As Tara, I want to connect our **Microsoft Entra** directory so users sign in with corporate credentials and SSO.
  - AC: Can register Adopta as a **multi-tenant Entra app** and complete admin consent; users from our Entra tenant can sign in via OIDC; no separate passwords exist.
- **US-IAM-2 (M)** — As Tara, I want **Microsoft Entra External ID** support so we can also serve external/customer users (CIAM) on the current Microsoft platform (not the deprecated B2C).
  - AC: External users can self-sign-up/sign-in via an External ID tenant; social/email options configurable; flows work without legacy custom XML policies.
- **US-IAM-3 (M)** — As Tara, I want **Conditional Access & MFA** honoured so access meets our security policy.
  - AC: Sign-ins respect tenant CA policies; phishing-resistant MFA supported; MFA/device claims from the user's home tenant can be trusted.
- **US-IAM-4 (M)** — As Tara, I want **role-based access control** so people only do what their role permits.
  - AC: Built-in roles (Owner, Admin, Author, Reviewer/Approver, Analyst, Viewer); least-privilege defaults; role changes are audited.
- **US-IAM-5 (M)** — As Tara, I want **SCIM user/group provisioning** so lifecycle is automated.
  - AC: Create/update/deactivate users and map groups → Adopta roles via SCIM 2.0; deactivation revokes access promptly.
- **US-IAM-6 (M)** — As Tara, I want **strict tenant data isolation** so no other customer can ever see our data.
  - AC: All data is tenant-scoped; cross-tenant access is impossible via UI/API; verified by automated isolation tests.
- **US-IAM-7 (M)** — As Tara, I want a complete **audit log** so I can satisfy security and compliance.
  - AC: Every admin/author/security action is logged with actor, time, before/after; exportable; immutable retention window.
- **US-IAM-8 (M)** — As Tara, I want to **scope where Adopta runs** (allowed domains/apps/URLs) so it never executes where it shouldn't.
  - AC: Allow/deny lists by domain and URL pattern; runtime refuses to inject outside allowed scope; matching happens client-side where possible.
- **US-IAM-9 (S)** — As Tara, I want **data residency** choices so data stays in an approved region.
  - AC: Select region at tenant creation; data at rest stays in-region; documented sub-processors.
- **US-IAM-10 (S)** — As Tara, I want **PII redaction & masking** policies so we don't capture sensitive content.
  - AC: Configurable masking of inputs/text/selectors; default-on for known sensitive field types; replay/analytics honour masks.

### Epic B — Content Authoring (No-Code Studio)

- **US-AUTH-1 (M)** — As Aria, I want to author **on top of my live app in the browser** so guidance matches reality.
  - AC: Launch an in-app editor on an allowed app; select elements visually; see exactly what users will see.
- **US-AUTH-2 (M)** — As Aria, I want to build **multi-step walkthroughs/flows** with branching so I can guide complex processes.
  - AC: Add/reorder steps; per-step element anchor, copy, media, and advance trigger (click/hover/input/navigation); conditional branching; save as draft.
- **US-AUTH-3 (M)** — As Aria, I want **field-level smart tips and tooltips/hotspots** so I can give just-in-time, inline help.
  - AC: Attach tips/hotspots to elements; configure trigger (hover/click/auto) and content; group tips per page.
- **US-AUTH-4 (M)** — As Aria, I want **launchers, checklists, banners, and modals** so I can drive onboarding and announcements.
  - AC: Create visible or invisible launchers; multi-task onboarding checklists with progress; banners/modals with scheduling.
- **US-AUTH-5 (M)** — As Aria, I want an **AI co-author** that drafts content from my intent so I build faster.
  - AC: Describe a goal + (optionally) a captured screen → receive an editable draft flow/copy; every AI output is fully editable; tone/length controllable.
- **US-AUTH-6 (M)** — As Aria, I want **durable element anchoring** so guidance survives app updates.
  - AC: Anchors use a resilient strategy (not a single brittle selector); when an app changes, the system re-locates elements where possible and **flags** the rest; I can re-capture in one action.
- **US-AUTH-7 (S)** — As Aria, I want a **maintained element library** for common apps (M365, Dynamics, Salesforce, Workday, ServiceNow) so I rely on vendor-tested anchors.
  - AC: Pick library elements for supported apps; library updates propagate to my content automatically.
- **US-AUTH-8 (M)** — As Aria, I want **localisation/multi-language** so global users get guidance in their language.
  - AC: Author in a base language; add/auto-translate variants; runtime serves by user locale; per-language review.
- **US-AUTH-9 (S)** — As Aria, I want **theming/branding** so guidance matches our look.
  - AC: Tenant theme (colours, fonts, logos, button styles) applied consistently; per-campaign overrides.
- **US-AUTH-10 (S)** — As Aria, I want **reusable templates & a content library** so I don't start from scratch.
  - AC: Save/clone templates; organisation-wide library with folders/tags/search.

### Epic C — Targeting, Segmentation & Delivery Rules

- **US-SEG-1 (M)** — As Aria, I want to **target content by user/account attributes and behaviour** so the right people see the right guidance.
  - AC: Build segments from identity metadata (role, dept, plan, locale) and behaviour (events, page visits, completion); preview audience size.
- **US-SEG-2 (M)** — As Aria, I want **page/URL and element conditions** so content only shows in the right context.
  - AC: Show/hide rules by URL pattern, element presence, and app state; combine with AND/OR logic.
- **US-SEG-3 (M)** — As Aria, I want **frequency/throttling and scheduling** so users aren't nagged.
  - AC: Per-item frequency caps, snooze/dismiss memory, date windows, and priority ordering across competing items.
- **US-SEG-4 (S)** — As Noah, I want to **create segments from analytics** and reuse them for targeting so the loop is seamless.
  - AC: Save any analytics cohort as a segment usable in targeting without re-definition.
- **US-SEG-5 (C)** — As Aria, I want **A/B testing** of guidance so I can optimise.
  - AC: Split audience across variants; measure completion/goal lift with significance.

### Epic D — Self-Service Support & Search

- **US-HELP-1 (M)** — As Eli, I want a **self-help widget** in the app so I can find answers without leaving.
  - AC: Persistent, unobtrusive widget; opens a panel with search, contextual content, and launchable guides; keyboard accessible.
- **US-HELP-2 (M)** — As Eli, I want to **ask questions in natural language** and get grounded answers so I get unstuck fast.
  - AC: NL query returns an AI answer grounded in the tenant's content with **citations**; offers to launch the relevant walkthrough; says "I don't know" rather than hallucinating.
- **US-HELP-3 (M)** — As Aria, I want the widget to surface **contextual content for the current screen** so help is relevant.
  - AC: Items filtered by current URL/segment/element context; most-relevant first.
- **US-HELP-4 (S)** — As Aria, I want to **federate external knowledge** (KB, videos, docs) into search so users get one place to look.
  - AC: Connect sources (e.g., SharePoint, Zendesk, web KB); results blended and attributed.
- **US-HELP-5 (S)** — As Tara, I want a **deflection/ticket fallback** so unresolved questions route correctly.
  - AC: If unresolved, offer "contact support" with context handoff to the configured channel.

### Epic E — Analytics (Product & Process)

- **US-AN-1 (M)** — As Noah, I want **automatic, retroactive capture** of usage so I'm not limited to pre-instrumented events.
  - AC: Capture pages/clicks/inputs (structurally) on install; **tag features/pages after the fact** and see historical data back to install.
- **US-AN-2 (M)** — As Noah, I want **funnels, paths, retention, and cohorts** so I can find drop-off and stickiness.
  - AC: Build multi-step funnels; visualise common paths; retention curves; cohort comparisons; filter by segment/date.
- **US-AN-3 (M)** — As Noah, I want **guidance performance analytics** so I know what works.
  - AC: Per-item views, starts, completions, drop-off step, dismissals, goal attainment, and lift vs. control.
- **US-AN-4 (S)** — As Noah, I want **process/cross-app analytics** so I can see journeys across multiple apps.
  - AC: Aggregate behaviour across a portfolio of apps (via extension/SDK) into unified journeys and tool-usage views.
- **US-AN-5 (S)** — As Noah, I want **friction detection** so problems surface proactively.
  - AC: Automatic detection of rage-clicks, dead-ends, error loops, and high drop-off; surfaced as alerts with suggested actions.
- **US-AN-6 (M)** — As Noah, I want an **insights agent** so I can ask questions in plain language.
  - AC: NL query returns charts/answers over the tenant's data with the query made transparent and editable.
- **US-AN-7 (M)** — As Noah, I want **data export to our warehouse** so we can join with other data.
  - AC: Scheduled/streamed export (e.g., to cloud storage / warehouse) of events and aggregates; documented schema.
- **US-AN-8 (C)** — As Noah, I want **session replay** so I can see the "why."
  - AC: Privacy-masked replays of sessions tied to analytics, with sensitive content redacted by default.

### Epic F — Engagement & Feedback

- **US-FB-1 (M)** — As Aria, I want **in-app surveys, NPS, CSAT, and polls** so I can capture sentiment in context.
  - AC: Multiple question types; targeting/scheduling/frequency; results dashboard; export.
- **US-FB-2 (S)** — As Aria, I want **announcements/targeted messaging** so I can communicate changes.
  - AC: Schedule modals/banners to segments; track delivery/acknowledgement.
- **US-FB-3 (C)** — As Noah, I want **sentiment analysis** of free-text feedback so themes surface automatically.
  - AC: Auto-theme/sentiment over responses with drill-down to verbatims.

### Epic G — Governance, Lifecycle & Operations

- **US-GOV-1 (M)** — As Tara, I want **draft → review → approve → publish** workflow so nothing ships unreviewed.
  - AC: Configurable approval steps; reviewers notified; publish gated on approval; full history.
- **US-GOV-2 (M)** — As Aria, I want **environments (dev/QA/prod)** and **versioning/rollback** so releases are safe.
  - AC: Promote content between environments; every publish is versioned; one-click rollback.
- **US-GOV-3 (M)** — As Aria/Sam, I want **anchor-health monitoring & alerts** so breakage is caught early.
  - AC: Continuous/periodic checks of anchors against target apps; alerts on breakage with the affected items listed; bulk find/replace of selectors.
- **US-GOV-4 (S)** — As Sam, I want **tenant health & usage dashboards** so I can operate the SaaS reliably.
  - AC: Per-tenant health, error rates, MAU, and content stats; SLO dashboards; alerting.

### Epic H — Integrations, APIs & Extensibility

- **US-INT-1 (M)** — As Devin, I want a **lightweight, well-documented SDK** so install is trivial and fast.
  - AC: Single async snippet; framework-agnostic + React/Vue/Angular notes; small bundle; no host-app blocking; debug mode.
- **US-INT-2 (M)** — As Devin, I want to **identify users/accounts securely** so segmentation and analytics are accurate.
  - AC: Pass user/account IDs + metadata; support signed identity to prevent spoofing; PII handling documented.
- **US-INT-3 (M)** — As Devin, I want **REST APIs + webhooks** so I can automate and react to events.
  - AC: Typed, versioned, authenticated APIs for content/segments/analytics; webhooks for key events; rate-limited; OpenAPI spec.
- **US-INT-4 (S)** — As Devin/Noah, I want an **MCP server** so I can query and manage Adopta from AI tools.
  - AC: MCP server exposes analytics queries and content/segment objects; respects tenant auth and RBAC.
- **US-INT-5 (S)** — As Tara, I want **prebuilt connectors** (Microsoft 365/Teams/SharePoint, Salesforce, ServiceNow, Slack, Zendesk) so integration is fast.
  - AC: Configure connectors via UI; least-privilege scopes; status/health visible.

### Epic I — Non-Functional Expectations (user-visible)

- **US-NFR-1 (M)** — As Eli, I want the overlay to **never noticeably slow or break my app** so I trust it.
  - AC: Runtime loads async; no render-blocking; guidance appears within perceptual budget; failure is silent and safe.
- **US-NFR-2 (M)** — As Eli, I want guidance to be **accessible** so everyone can use it.
  - AC: WCAG 2.2 AA; keyboard navigable; screen-reader friendly; respects reduced-motion and contrast.
- **US-NFR-3 (M)** — As Tara, I want **high availability** so the platform is dependable.
  - AC: Published runtime content served even during partial backend outages (CDN/cache); documented uptime SLO.
- **US-NFR-4 (M)** — As Tara, I want **security & compliance** so we can buy with confidence.
  - AC: Encryption in transit/at rest; least privilege; audit; a clear path to SOC 2 / ISO 27001 / GDPR alignment.
- **US-NFR-5 (S)** — As Eli, I want guidance to work across **modern browsers and SPAs** so it's reliable everywhere we work.
  - AC: Latest Chrome/Edge/Firefox/Safari; SPA route changes handled; Shadow DOM and iframes handled where feasible.

---

## 4. Assumptions & Constraints (user-level)

- **Initial surface:** web applications (desktop browsers). Native mobile and thick-client are roadmap (`00 §7`).
- **Identity:** Microsoft Entra is the primary IdP strategy (workforce + External ID). Generic OIDC/SAML federation is supported for non-Microsoft tenants but Microsoft is the lead experience.
- **No source access required** to any target app; where the customer *does* control the app, a direct snippet is preferred over the extension.
- **Privacy default:** capture structure and behaviour, not content/PII, unless the tenant explicitly opts in with masking controls.

## 5. Out of Scope (this release)

Native mobile SDK; desktop/Citrix/VDI guidance; sandbox/simulation training; full cross-app agent orchestration. (Seams reserved in architecture; see `03`.)

---

### Traceability
Each `US-*` ID is referenced by one or more `FR-*` requirements in `02-functional-requirements.md`, which are in turn realised by components in `03-solution-architecture.md`. Tests should cite the `US-*`/`FR-*` they cover.
