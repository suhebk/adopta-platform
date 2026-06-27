# 00 — Product Vision & Competitive Analysis

> **Product working name:** **Adopta** (codename — rename freely).
> **Document status:** Baseline v1.0
> **Audience:** Founders, product, engineering, design, GTM.
> **Companion documents:** `01-user-requirements`, `02-functional-requirements`, `03-solution-architecture`, `04-wireframes`, `05-claude-build-prompt`.

---

## 1. Executive Summary

Adopta is a **production-grade, multi-tenant SaaS Digital Adoption Platform (DAP)**. It overlays contextual, in-the-flow guidance, self-service support, and behavioural analytics onto any web application — internal line-of-business apps, commercial SaaS (Salesforce, Workday, SAP, ServiceNow, Dynamics 365, Microsoft 365), and a customer's own products — **without requiring changes to the underlying application's source code**.

The platform is engineered from day one to be **enterprise-ready**: multi-tenant, secure-by-design on **Microsoft Entra** identity, observable, horizontally scalable, and governed. It is positioned at the **AI-native / agentic** frontier of the category, where the market leaders are now competing.

### Why now

- The DAP market reached **~$1.04B in 2024, growing ~27.7% YoY**, with projected 15–20% growth through 2025 (Gartner, via Whatfix). It is large, fast-growing, and consolidating around a handful of enterprise-grade vendors.
- Gartner's strategic guidance is explicit: **"DAPs will increasingly function as orchestrators within an application ecosystem by coordinating multiple first- and third-party AI agents and AI assistants across applications."** This is the defensible frontier — and where a new, AI-native entrant can leapfrog incumbents carrying legacy architecture.
- Every incumbent has bolted AI onto an architecture designed in the 2013–2018 era. A greenfield build can make AI/agentic a **first-class architectural primitive** rather than a feature.

---

## 2. What a DAP Actually Does (Capability Model)

A DAP is composed of four cooperating capability layers. Every serious competitor implements all four; the differentiation is in **how well** and **how intelligently**.

| Layer | Purpose | Representative artefacts |
|---|---|---|
| **1. Guidance** | Teach and steer users *in the flow of work* | Walkthroughs/flows, smart tips (field-level), tooltips, hotspots, launchers, onboarding checklists, banners, modals, task lists |
| **2. Self-service support** | Let users resolve their own questions without leaving the app | Self-help widget, federated search over KB/videos/flows, AI answer assistant, resource center |
| **3. Analytics** | Understand behaviour and prove ROI | Product analytics (events, funnels, paths, retention, cohorts), process analytics (cross-app), session replay, guidance performance, friction detection |
| **4. Engagement & feedback** | Capture sentiment and communicate | In-app surveys, NPS/CSAT, polls, announcements, targeted messaging |

Underpinning all four is the **hard technical core**:

- **An element-detection / anchoring engine** that reliably attaches guidance to UI elements in applications you don't control, and survives DOM/UI changes.
- **A runtime delivery mechanism** (JS SDK snippet and/or browser extension and/or mobile SDK) that injects the overlay safely and performantly.
- **A no-code authoring studio** that lets non-developers build content *on top of* live target applications.

> The element engine and the runtime SDK are the two highest-risk, highest-moat components. They are specified in depth in `03-solution-architecture`.

---

## 3. Competitive Landscape — Deep Dive

Research basis: vendor documentation, help centers, engineering blogs, analyst commentary, and press material (2024–2026). Each profile focuses on **architecture and capability** so the design decisions in this repo are grounded in how the category actually works.

### 3.1 WalkMe (SAP) — the enterprise guidance & automation incumbent

**Architecture & deployment**
- **Desktop Editor** (authoring client) + **Editor browser extension** for building/previewing content.
- Two deployment paths: (a) a **JavaScript snippet** inserted into the app's master page `<head>`, or (b) a **browser extension** that injects the snippet by matching URLs locally (no server round-trip for URL matching — a privacy/perf design choice).
- Content is delivered from a **CDN** and cached in the **browser** (~1–2 MB), so runtime does not constantly hit the server.
- **Regional data centers** (US/EU, plus FedRAMP) for residency/compliance.

**Element detection — "DeepUI"**
- WalkMe's patented, AI-driven element recognition. On authoring, it **generates a rich description of the target element and its surrounding context**; at runtime it scans the DOM and **re-locates the element even after UI updates**, automatically updating the mapping. It explicitly avoids uploading personal data — only structural descriptions.
- Falls back to **jQuery selectors** (with a "Selector Optimizer" that rewrites old selectors into faster modern syntax) for edge cases and dynamic IDs.
- **Element Library**: pre-captured, vendor-maintained elements for major platforms (Salesforce Lightning, Workday, Oracle HCM, SuccessFactors, MS Dynamics, MS Teams). When a platform's HTML changes, WalkMe updates the library centrally and **all connected customer content inherits the fix automatically**.
- Handles **Shadow DOM** explicitly (special query path for encapsulated components).
- **Advanced Search** to find/replace selectors and element data across an entire implementation.

**Takeaways for Adopta**
- A dual strategy of **resilient AI/heuristic anchoring + selector fallback** is table stakes.
- A **centrally-maintained element library** for the top 10–15 enterprise apps is a major maintenance differentiator and should be on the roadmap.
- **Shadow DOM and SPA support** must be designed in, not retrofitted.

---

### 3.2 Whatfix — the AI-native, three-product "userization" platform

**Product structure**
- **DAP** (in-app guidance + self-help), **Mirror** (interactive *sandbox replicas* of web apps for risk-free, hands-on training and AI roleplay), and **Product Analytics** (no-code behavioural insights). Marketed as a unified "readiness → execution → optimization" loop.

**AI — "ScreenSense"**
- Whatfix's flagship AI engine, **built on a Computer-Use model trained to understand and interact with GUIs like a human**. Capabilities: **advanced element & context detection, segmentation, behavioural intelligence, and inference generation.**
- **AI Agents** across the suite for **content authoring, in-app user guidance, and usage analysis**; an **Insights Agent** that replaces dashboards with natural-language queries and proactively surfaces friction before tickets are filed.
- **Quick Capture Mode** to lower content-creation effort.

**Analytics**
- Funnels, journeys, **real-time cohorts, session replay, autocapture**, and a natural-language analyst — positioned as an "insights-to-action" platform that links behavioural data directly to in-app guidance (closed loop).

**Reach**
- Web, desktop, mobile (Mobile SDK 2.0), and **VDI/Citrix** — important for enterprises with mixed estates.

**Takeaways for Adopta**
- **GUI-vision / computer-use models for element + context detection** is the bleeding edge. Adopta should treat **multimodal element understanding** as a core capability, not a gimmick.
- The **closed loop** (analytics → friction detection → auto-suggested/auto-targeted guidance) is the most compelling enterprise story. Adopta's data model should make this loop native.
- A **simulation/sandbox training mode** (Mirror-equivalent) is a credible future module.

---

### 3.3 Pendo — the analytics-led product-experience platform

**Product structure**
- Three pillars: **Analyze** (product/process analytics), **Assess** (feedback/sentiment), **Act** (in-app guidance). Strong heritage in **product analytics** specifically.

**Architecture & deployment**
- **Single JS snippet** loads the `pendo.js` agent for web; **native mobile SDKs** for iOS, Android, **React Native, Flutter, Jetpack Compose** (codeless, offline-capable, retroactive).
- **Pendo Launcher** browser extension for **process analytics** across a portfolio of web apps (built or bought).
- **Retroactive autocapture**: data collection begins on install; you **tag pages/features after the fact** and still see historical behaviour — a major differentiator vs. event-instrumentation tools (Amplitude/Mixpanel) that only see what you instrumented going forward. One data model, one snippet, one segmentation engine.

**AI**
- **AI guide generation** (historically using Google's Vertex LLM) — generate multi-step guides from a prompt + start URL + tone.
- **Pendo Predict** (churn/upsell prediction without a data-science team), **Pendo Listen** (sentiment/localization).
- **MCP server** to query visitor/account metadata, analytics, and objects (pages, features, guides, agents) in **natural language from any MCP-compatible tool**.

**Takeaways for Adopta**
- **Retroactive autocapture + post-hoc tagging is the analytics gold standard.** Adopta's ingestion pipeline must capture rich interaction data by default and allow tagging later.
- **One unified data model** across analytics + guidance + segmentation is the architectural principle that makes the closed loop cheap. Build it that way from the start.
- **Exposing the platform over MCP** (so admins/analysts can query and even author from Claude, Cursor, etc.) is a 2025-era differentiator that aligns perfectly with this project's Claude-centric workflow.

---

### 3.4 VisualSP — the Microsoft-native, fast-to-deploy contextual-help platform

**Positioning**
- A **certified Microsoft ISV partner** and **Microsoft-native DAP**, deeply optimised for **Microsoft 365, Copilot, Dynamics 365, Power Platform, SharePoint, Teams** — while still supporting any web app (Salesforce, Workday, SAP, ServiceNow, etc.).

**Architecture & deployment**
- Two deployment modes that *don't conflict*: a **Dynamics/Power Apps managed solution** (deploy once, no per-machine extension) **or** a **browser extension**.
- On load it **auto-detects the signed-in user and matches them to a subscription** on the backend — friction-light onboarding.
- **"Just-in-Time Learning"** philosophy: inline icons, links, buttons, **invisible launchers**, walkthroughs, how-to videos, in-app notifications, banners, popup alerts.
- **Analytics via Microsoft Clarity integration** (heatmaps, session recordings, clickstreams) — toggled on with zero setup, plus its own learning-activity analytics.
- **Role / permission / workflow-based content targeting**; content can live in the customer's **own M365 tenant** or any repository behind their firewall.
- **AI Assistant** for contextual support; **PDF export** of any help item/walkthrough into training material. **Adopt365** is a free lightweight M365 web extension as a funnel.

**Takeaways for Adopta**
- **Deep Microsoft integration is a viable wedge** — especially given this project's mandate to use **Microsoft Entra** security. Adopta can lead with a best-in-class Microsoft 365 / Dynamics / Power Platform experience and expand outward.
- **Effortless deployment & auto-provisioning** (sign in → it just works) is a real differentiator vs. heavyweight rollouts.
- **Tenant-owned content storage** (data stays in the customer's boundary) is attractive to security-conscious buyers and aligns with a strong multi-tenant data-isolation design.

---

### 3.5 Side-by-side summary

| Dimension | WalkMe | Whatfix | Pendo | VisualSP | **Adopta (target)** |
|---|---|---|---|---|---|
| **Core strength** | Enterprise guidance + automation | AI-native unified suite | Product/process analytics | Microsoft-native contextual help | **AI-native + Microsoft-secured, unified, MCP-exposed** |
| **Element engine** | DeepUI (AI) + jQuery + Element Library | ScreenSense (computer-use GUI model) | DOM tagging + autocapture | DOM + MS-app awareness | **Hybrid: resilient selector graph + multimodal vision fallback + maintained library** |
| **Authoring** | Desktop editor + extension | No-code + AI + Quick Capture | Visual Design Studio + AI gen | No-code + AI assistant | **In-browser no-code studio + AI/agent co-author** |
| **Delivery** | Snippet / extension / CDN cache | Snippet / extension / mobile / VDI | Snippet / mobile SDKs / Launcher ext | Managed solution / extension | **SDK snippet + extension + mobile SDK (phased)** |
| **Analytics** | Adoption + goals | Funnels, journeys, replay, autocapture, NL | Retroactive autocapture, paths, Predict | Clarity integration | **Retroactive autocapture, unified model, funnels/paths/retention, friction AI, replay (phased)** |
| **AI / agentic** | DeepUI, AI features | ScreenSense, AI Agents, Insights Agent | Guide gen, Predict, Listen, MCP | AI Assistant | **Agentic authoring + answer agent + insights agent + MCP server + agent orchestration (frontier)** |
| **Identity/security** | Enterprise SSO, regional DCs | Enterprise SSO | Enterprise SSO | Microsoft-native | **Microsoft Entra (workforce + External ID), Conditional Access, per-tenant isolation** |
| **Simulation/training** | — | Mirror (sandbox + roleplay) | — | — | **Sandbox module (roadmap)** |

---

## 4. Adopta's Strategic Differentiation

Four pillars define where Adopta wins. These thread through every requirement and design decision in this repo.

### Pillar 1 — AI-native & agentic by architecture (not bolted on)
- **Multimodal element understanding**: combine a resilient DOM **selector graph** with a **vision/computer-use fallback** so anchors survive aggressive UI change. (See `03 §7`.)
- **Agentic authoring**: an author describes intent ("create an onboarding flow for the invoice-approval screen") and an **agent drafts the flow** by inspecting the captured screen.
- **Answer agent**: the self-help widget answers natural-language questions grounded in the tenant's content (RAG), with citations.
- **Insights agent**: natural-language analytics ("where are users dropping off in onboarding this week?") + **proactive friction alerts**.
- **MCP server**: the entire platform is queryable and partially controllable from Claude/Cursor/other MCP clients — a first-class interface, perfectly aligned with this project's build workflow.
- **Agent orchestration (frontier)**: Adopta coordinates first- and third-party AI agents *across* applications, fulfilling Gartner's stated direction for the category.

### Pillar 2 — Microsoft-secured, enterprise-grade by default
- Identity on **Microsoft Entra** — **workforce tenants** (federate the customer's own Entra ID as IdP via a multi-tenant app) and **Microsoft Entra External ID** (the current CIAM platform; **Azure AD B2C is end-of-sale to new customers as of 1 May 2025** and must not be used for greenfield).
- **Conditional Access, phishing-resistant MFA, SCIM provisioning, RBAC, per-tenant data isolation, audit logging, data residency.**

### Pillar 3 — Unified data model → native closed loop
- One event/identity/segmentation model shared by analytics, guidance, and engagement — so "detect friction → target guidance → measure impact" is a built-in loop, not an integration project. (Pendo and Whatfix both prove this is the winning architecture.)
- **Retroactive autocapture** so customers get value on day one and tag later.

### Pillar 4 — Frictionless deployment & governance
- Sign in → auto-provision → it works (VisualSP-style), with a **maintained element library** for top enterprise apps (WalkMe-style) to minimise breakage and maintenance.
- Enterprise **content lifecycle governance**: draft → review → approve → publish → version → rollback, with environments (dev/QA/prod) and audit trails.

---

## 5. Target Market & Personas (summary)

Detailed personas and journeys are in `01-user-requirements`. In brief, Adopta serves two buying motions and five core personas:

- **Buying motions:** (a) **Enterprise IT / L&D / Change** driving adoption of internal + bought software; (b) **Product / CS teams** at software vendors driving onboarding, activation, and retention of their own product.
- **Core personas:** Tenant Administrator (IT/security), Content Author (L&D/PM/enablement), End User (employee or customer), Analyst (product/ops), and Developer (integrations/SDK).

---

## 6. Guiding Product Principles

1. **Zero-code on the target app.** Never require source changes to the application being guided.
2. **Resilience over cleverness.** A guide that silently breaks is worse than no guide — invest disproportionately in anchor durability and graceful degradation.
3. **Performance is a feature.** The runtime must be lightweight, async, cached, and never block or visibly slow the host app.
4. **Privacy by construction.** Capture structural/behavioural data, not user content/PII, by default; allow tenant-controlled redaction and data residency.
5. **One data model.** Analytics, guidance, segmentation, and feedback share identities and events.
6. **AI augments authors and users; humans stay in control.** Every AI output is editable, reviewable, and governed.
7. **Multi-tenant and secure from commit #1.** Tenant isolation, RBAC, and auditability are foundational, not phase 2.
8. **Open & interoperable.** Public APIs, webhooks, SCIM, SSO, data export to the warehouse, and an MCP server.

---

## 7. Out of Scope (initial release) / Roadmap candidates

- **Native mobile SDK** (iOS/Android/RN/Flutter) — phase 2.
- **Desktop/thick-client and Citrix/VDI guidance** — phase 3.
- **Sandbox/simulation training module** (Mirror-equivalent) — phase 3.
- **Full cross-app agent orchestration** — frontier track, parallel R&D.

These are documented so the architecture in `03` reserves the right seams (SDK abstraction, event schema, agent framework) to add them without re-platforming.

---

### Sources informing this analysis
Vendor documentation and help centers (WalkMe DeepUI / jQuery selectors / Element Library; Whatfix ScreenSense / Product Analytics / Mirror; Pendo Analytics / Guides / Mobile SDK / MCP; VisualSP M365 / Dynamics / Clarity), analyst commentary (Gartner Market Guide for DAPs 2025 via vendor reporting), and Microsoft Learn (Entra External ID, SaaS multitenant architecture). Market sizing figures are Gartner estimates as reported by Whatfix. Treat all competitor specifics as point-in-time (2024–2026) and re-verify before external use.
