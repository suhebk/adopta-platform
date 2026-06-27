# 04 — Wireframes & UX Specification

> **Product:** Adopta (DAP) · **Status:** Baseline v1.0
> **Fidelity:** low-fi, annotated. These define **layout, regions, and behaviour** — not final visual design. High-fidelity design uses the tenant **design tokens** (see `frontend-design` skill) and **WCAG 2.2 AA** (`NFR-A11Y-1`).
> **Two surfaces:** **(A) Admin Studio** (Next.js web app for Tara/Aria/Noah) and **(B) End-User Runtime** (the in-browser overlay for Eli). Each wireframe cites the requirements it serves.

**Legend:** `[ Button ]` · `( ) radio` · `[x] checkbox` · `▢` element/card · `▼` dropdown · `⌕` search · `◔` chart · `★` AI-powered · `↹` tab.

---

## A. ADMIN STUDIO

### A0 — Global shell (frame for all admin screens)
```
┌───────────────────────────────────────────────────────────────────────────────┐
│ Adopta   [ Tenant: Contoso ▼ ]   [ Env: Production ▼ ]        ⌕ Search   ◔ ?  ◐ │  ← env switch = dev/QA/prod (FR-GOV-002)
├───────────┬───────────────────────────────────────────────────────────────────┤
│ ▸ Home    │                                                                     │
│ ▸ Content │                      ( main content region )                        │
│ ▸ Segments│                                                                     │
│ ▸ Analytics                                                                     │
│ ▸ Self-Help                                                                     │
│ ▸ Surveys │                                                                     │
│ ▸ Governance                                                                    │
│ ▸ Settings│                                                                     │
│           │                                                                     │
│  ★ Ask    │  ← persistent AI launcher (co-author / insights agent)              │
└───────────┴───────────────────────────────────────────────────────────────────┘
```
*Roles gate the left nav (FR-IDN-010). Env switch scopes everything to dev/QA/prod.*

---

### A1 — Tenant onboarding & identity setup  · *serves US-IAM-1..9, FR-IDN-001..033*
```
┌── Get Adopta running ───────────────────────────── Step 2 of 5 ───────────────┐
│  ① Verify domain  ②>Connect identity  ③ Provisioning  ④ Scope apps  ⑤ Invite  │
├───────────────────────────────────────────────────────────────────────────────┤
│  Connect your identity provider                                                │
│                                                                                 │
│  ▢ Microsoft Entra ID (workforce)            [ Connect & grant admin consent ] │  ← multi-tenant app consent (FR-IDN-001)
│     Your employees sign in with corporate credentials. SSO + Conditional       │
│     Access + MFA honoured automatically.                            ✓ Connected │
│                                                                                 │
│  ▢ Microsoft Entra External ID (customers)   [ Configure CIAM ]                │  ← current CIAM platform, NOT B2C (FR-IDN-002)
│     For external/customer users. Email + social sign-up.                        │
│                                                                                 │
│  ▢ Other (OIDC / SAML 2.0)                    [ Configure ]                     │  ← Okta/Ping/Google (FR-IDN-004)
│                                                                                 │
│  Security posture (read from Entra):                                            │
│   • MFA: phishing-resistant ✓   • Conditional Access: 3 policies apply ✓        │  ← (FR-IDN-003)
│   • Trust home-tenant MFA/device claims: [x]                                    │
│                                                                                 │
│                                            [ Back ]   [ Continue → ]            │
└───────────────────────────────────────────────────────────────────────────────┘
```

### A1b — Provisioning, residency & data policy  · *FR-IDN-020/021/032/042*
```
┌── Provisioning & data ────────────────────────────────────────────────────────┐
│  SCIM provisioning   [ Enabled ●○ ]   Endpoint: https://…/scim   [ Copy token ]│  ← lifecycle automation (FR-IDN-020)
│  Group → role mapping:  Engineering▼ → Author▼   Finance▼ → Viewer▼  [ + Add ] │
│                                                                                 │
│  Data residency        ( ) EU   (•) UK   ( ) US        ⓘ set once per tenant   │  ← (FR-IDN-032)
│  PII capture default   [x] Structure only (no content/PII)                      │  ← privacy by construction (FR-IDN-042)
│  Masking rules         password, payment, ssn  (default-on)   [ + Add rule ]    │
│  Tenant-managed storage[ ] Store media/content in our own storage account       │  ← (FR-IDN-033)
│                                                  [ Back ]   [ Continue → ]       │
└───────────────────────────────────────────────────────────────────────────────┘
```

---

### A2 — Home / overview dashboard  · *US-AN-3, FR-ANL-012, US-GOV-3*
```
┌── Home ───────────────────────────────────────────────────────────────────────┐
│  MAU 12,480 ▲6%   Guides live 84   Completion 71% ▲   Tickets deflected 1,902  │  ← KPI strip
│                                                                                 │
│  ⚠ Anchor health (3 need attention)                          [ Review → ]      │  ← (FR-ELM-005, US-GOV-3)
│    ▢ "Submit invoice" tooltip — broken on Dynamics release   [ Re-capture ]    │
│    ▢ Onboarding step 4 — low confidence match                [ Re-capture ]    │
│                                                                                 │
│  ★ Friction detected this week                               [ See all → ]     │  ← (FR-ANL-014)
│    • Drop-off 38% at "Approve" step  → suggested: add tooltip [ Create ★ ]      │  ← closed loop: friction → guidance
│    • Rage-clicks on "Export" button (212 users)              [ Investigate ]    │
│                                                                                 │
│  Top guides ◔ ──────────────  Pending approvals (2) ───────────────────────    │
│  1 Welcome tour     94% ░░░░░  ▢ "Q3 policy banner"  by Aria   [ Review ]       │  ← (FR-GOV-001)
│  2 Invoice flow     71% ░░░    ▢ "New CRM tips"      by Sam    [ Review ]       │
└───────────────────────────────────────────────────────────────────────────────┘
```

---

### A3 — Content library  · *US-AUTH-10, FR-AUT-032, FR-GOV-002*
```
┌── Content ──────────────────────────────────────────────  [ + New ▼ ]─────────┐
│  ⌕ Search   Type:[All▼] Status:[All▼] App:[All▼] Segment:[All▼] Lang:[All▼]   │
│                                              [ + New: Walkthrough / Tip /       │
│                                                Checklist / Banner / Survey ★ ]  │
├──────┬──────────────────────┬────────┬────────┬──────────┬─────────┬───────────┤
│ Type │ Name                 │ App    │ Status │ Segment  │ Compl.  │ Updated   │
├──────┼──────────────────────┼────────┼────────┼──────────┼─────────┼───────────┤
│ ⛓ WT │ Invoice approval     │ D365   │ ●Live  │ Finance  │ 71% ◔   │ 2d ago    │  ← status: Draft/Review/Live/Archived
│ ◔ Tip│ Search field help    │ M365   │ ◐Review│ All      │  –      │ 1h ago    │
│ ☑ CL │ New-hire onboarding   │ All    │ ●Live  │ New users│ 64% ◔   │ 5d ago    │
│ ▭ Ban│ Q3 policy notice     │ All    │ ○Draft │ EU staff │  –      │ 3d ago    │
├──────┴──────────────────────┴────────┴────────┴──────────┴─────────┴───────────┤
│  Selected: 2   [ Move to QA ]  [ Submit for review ]  [ Clone ]  [ Archive ]   │  ← promote across envs (FR-GOV-002)
└───────────────────────────────────────────────────────────────────────────────┘
```

---

### A4 — In-App Editor (THE core authoring experience)  · *US-AUTH-1/2/5/6, FR-AUT-001..021, FR-ELM-001*
> Opens the **live target app** in a framed/extension session; author selects real elements; a true-to-runtime preview renders.

```
┌── Editing: "Invoice approval"  ·  on app.dynamics.com  ·  ◐ Preview: [Finance▼]┐
│┌─ Steps ──────────┐┌──────────── LIVE TARGET APP (capture canvas) ───────────┐ │
││ 1 ▢ Open invoices││   ┌─────────────────────────────────────────────────┐  │ │
││ 2 ▢ Select record││   │  Dynamics 365 — Invoices                        │  │ │
││ 3 ▢ Click Approve││   │  [ New ] [ Edit ]            ⌕ ▢▢▢              │  │ │
││ 4 ▢ Confirm  ◀───┤│   │  ┌─────────────────────────────────────────┐    │  │ │
││ + Add step       ││   │  │ INV-1042  Acme   £4,210   ● Pending      │    │  │ │
│└──────────────────┘│   │  └─────────────────────────────────────────┘    │  │ │
│                    │   │                    ╔════════════╗  ← selected     │  │ │
│┌─ Step 4 settings ┐│   │            [ Approve ]║ Approve  ║   element      │  │ │
││ Anchor: "Approve"││   │                       ╚════════════╝  highlighted │  │ │
││  ✓ resilient (4) ││   │     ┌──────────────────────────────┐             │  │ │
││  [ Re-capture ]  ││   │     │ ▣ Click Approve to continue ▸ │ ← runtime   │  │ │
││ Trigger: on click││   │     │   Step 3 of 4         [ Next ]│   balloon   │  │ │
││ Content:         ││   │     └──────────────────────────────┘   preview   │  │ │
││  [ rich text … ] ││   └─────────────────────────────────────────────────┘  │ │
││  ★ Draft with AI ││                                                          │ │
││ Placement: auto ▼││  Anchor strength ●●●● strong   ⓘ uses 4 signals         │ │  ← (FR-ELM-001/002)
││ Branch: if £>10k ││                                                          │ │
││   → step 4b      ││  [ ◀ Prev step ]              [ Save draft ] [ Submit ▸]│ │  ← submit → review (FR-GOV-001)
│└──────────────────┘└──────────────────────────────────────────────────────┘ │ │
│ ★ AI: "Drafted 4 steps for invoice approval. Review copy & placement."  [↩]   │  ← co-author (FR-AUT-020)
└───────────────────────────────────────────────────────────────────────────────┘
```
*Key behaviours:* element selection highlights the **real** element; **anchor strength** shows multi-signal confidence (FR-ELM-002); **★ Draft with AI** generates editable steps/copy (FR-AUT-021 — never auto-publishes); **branching** and **per-step triggers** per FR-AUT-010/011; **Re-capture** is one action (FR-ELM-006).

---

### A5 — Segment builder  · *US-SEG-1, FR-DEL-001/004*
```
┌── New segment: "Stuck on approvals" ──────────────────────────────────────────┐
│  Match  (•) All  ( ) Any   of the following:                                   │
│  ┌─────────────────────────────────────────────────────────────────────────┐  │
│  │ Identity ▼  Department    is        Finance              [ – ]           │  │  ← traits from PG (FR-DEL-001)
│  │ Behaviour▼  Completed     "Invoice flow"   is  false     [ – ]           │  │  ← behaviour from ClickHouse
│  │ Behaviour▼  Rage-clicked  "Approve"        in last 7d    [ – ]           │  │
│  └─────────────────────────────────────────────────────────────────────────┘  │
│  [ + Add condition ]                                                            │
│                                                                                 │
│  Live audience preview:  ▓▓▓▓▓▓░░░  1,284 users                                │  ← (FR-DEL-001)
│  ⓘ Created from analytics cohort "Approval drop-off"   [ unlink ]               │  ← cohort→segment loop (FR-DEL-004)
│                                                  [ Cancel ]   [ Save segment ]  │
└───────────────────────────────────────────────────────────────────────────────┘
```

---

### A6 — Analytics: funnel + paths  · *US-AN-2, FR-ANL-010/011*
```
┌── Analytics ▸ Funnel: "Invoice onboarding" ───────────  Segment:[All▼] 30d▼───┐
│  Step 1 View invoices   ████████████████████  10,000  100%                     │
│  Step 2 Select record   ███████████████░░░░░   7,400   74%   ▼ 26%             │
│  Step 3 Click Approve    ████████░░░░░░░░░░░░   4,560   62%   ▼ 38% ⚠          │  ← biggest drop (friction)
│  Step 4 Confirm          ███████░░░░░░░░░░░░░   4,100   90%                     │
│                                                                                 │
│  ★ Insight: 38% drop at "Approve" correlates with no prior tooltip exposure.    │  ← (FR-ANL-014)
│            [ Create targeted tip ★ ]   [ Save cohort as segment ]               │  ← closed loop
│                                                                                 │
│  [ ↹ Funnel ] [ Paths ] [ Retention ] [ Cohorts ] [ Guidance perf ] [ Replay ] │  ← (FR-ANL-010..015)
│  [ Export to warehouse ⤓ ]                                                      │  ← (FR-ANL-021)
└───────────────────────────────────────────────────────────────────────────────┘
```

### A6b — Insights agent (natural language)  · *US-AN-6, FR-ANL-020*
```
┌── ★ Ask Adopta ───────────────────────────────────────────────────────────────┐
│  You: where are Finance users dropping off in onboarding this week?            │
│  ─────────────────────────────────────────────────────────────────────────    │
│  ★ The largest drop is at the "Approve" step (38%, ▲9% vs last week).          │
│     ◔ [ funnel chart rendered ]                                                 │
│     Generated query:  events ▸ funnel(view→select→approve→confirm)              │  ← transparent/editable (FR-ANL-020)
│     filter dept=Finance, range=7d        [ Edit query ] [ Open in Analytics ]   │
│     Suggested action: target a tooltip at "Approve" for this cohort. [ Create ] │
│  ─────────────────────────────────────────────────────────────────────────    │
│  [ Type a question…                                                      ↩ ]   │
└───────────────────────────────────────────────────────────────────────────────┘
```

---

### A7 — Governance / approval review  · *US-GOV-1, FR-GOV-001/002*
```
┌── Review: "Q3 policy banner"  · requested by Aria · env: QA → Production ──────┐
│  Preview ▢ [ rendered banner ]      Targeting: EU staff · 1,840 users           │
│  Changes vs live:  + new banner, schedule Jul 1–14, frequency 1/session         │
│  Anchor health: n/a (banner)        Accessibility: ✓ AA                          │
│  Reviewers:  Tara (Admin) ◷ pending    Legal ✓ approved                          │  ← multi-step approval (FR-GOV-001)
│  Comments: ▢ "Add link to policy doc" — Tara                                    │
│            [ Request changes ]   [ Approve & publish to Production ▸ ]           │  ← gated publish; versioned (FR-GOV-002)
│  Version history:  v3 (now) ◂ v2 ◂ v1     [ Rollback to v2 ]                     │  ← one-click rollback
└───────────────────────────────────────────────────────────────────────────────┘
```

---

### A8 — Settings: roles, scope, integrations  · *US-IAM-4/8, US-INT-3/4/5*
```
┌── Settings ───────────────────────────────────────────────────────────────────┐
│ [ ↹ Roles ] [ App scope ] [ Theme ] [ Integrations ] [ API & MCP ] [ Audit ]   │
├───────────────────────────────────────────────────────────────────────────────┤
│ APP SCOPE — where Adopta is allowed to run (client-side matched)                │  ← (FR-IDN-041)
│   Allowed:  *.dynamics.com  ·  contoso.sharepoint.com  ·  app.contoso.com       │
│   Blocked:  *.bank.example.com                              [ + Add pattern ]   │
│                                                                                 │
│ API & MCP                                                                       │
│   REST API   ● Enabled   OpenAPI: /openapi.json   Rate limit: 600/min  [ Keys ]│  ← (FR-INT-001)
│   Webhooks   publish, anchor-broken, survey-response       [ Manage ]           │  ← (FR-INT-002)
│   ★ MCP server  ● Enabled — query analytics & manage content from Claude/Cursor │  ← (FR-INT-003)
│       Endpoint: mcp://adopta/contoso     Scopes: analytics:read content:write   │
│                                                                                 │
│ INTEGRATIONS  [Microsoft 365 ✓] [Salesforce +] [ServiceNow +] [Slack ✓] [Zendesk +]│ ← (FR-INT-004)
└───────────────────────────────────────────────────────────────────────────────┘
```

---

## B. END-USER RUNTIME (the in-browser overlay)

> Injected via CDN snippet or extension; **Shadow-DOM isolated**; never blocks/breaks the host app (FR-DEL-014/015); **WCAG 2.2 AA**.

### B1 — Self-help widget (collapsed → expanded)  · *US-HELP-1/2/3, FR-HLP-001..003*
```
   collapsed (corner of host app):                expanded panel:
   ┌─────────────────────────────┐               ┌───────────────────────────────┐
   │  … host application …       │               │  Help — Invoices       [ × ]  │
   │                             │               │  ⌕ Ask or search…             │  ← NL ask + search (FR-HLP-002)
   │                             │               ├───────────────────────────────┤
   │                             │               │  For this screen ▸             │  ← contextual (FR-HLP-003)
   │                        ┌──┐ │               │   ⛓ How to approve an invoice  │
   │                        │ ? │ │  ← launcher   │   ◔ Add a new vendor           │
   │                        └──┘ │     (FR-AUT-013)│   ▶ Video: Invoice basics     │
   └─────────────────────────────┘               │  Popular ▸                     │
                                                  │   ⛓ Bulk approvals             │
                                                  │  ────────────────────────────  │
                                                  │  Can't find it? [ Contact ▸ ] │  ← deflection fallback (FR-HLP-005)
                                                  └───────────────────────────────┘
```

### B2 — Answer agent (grounded, with citations)  · *US-HELP-2, FR-HLP-003*
```
┌── Help ───────────────────────────────────────────────── [ × ] ──┐
│  You: how do I approve an invoice over £10,000?                   │
│  ──────────────────────────────────────────────────────────────  │
│  ★ Over £10,000 needs second approval:                            │
│     1. Open the invoice → 2. Click Approve →                      │
│     3. A "Senior approval" panel appears → 4. Select an approver. │
│     Sources: ▸ "Invoice approval policy"  ▸ "Approval limits"     │  ← citations (FR-HLP-003)
│     [ ▶ Show me on screen ]   👍  👎                              │  ← launches walkthrough; logged (FR-HLP-006)
│  ──────────────────────────────────────────────────────────────  │
│  [ Ask a follow-up…                                          ↩ ]  │
└────────────────────────────────────────────────────────────────--┘
```
*If unsupported, the agent says "I couldn't find that in your help content" rather than inventing an answer (FR-HLP-003).*

### B3 — Walkthrough step (balloon anchored to a real element)  · *US-AUTH-2, FR-DEL-013/014*
```
        … host application UI …
        ┌───────────────────────────┐
        │  [ Approve ]  ◀───────────╮│
        └───────────────────────────┘│
            ╭────────────────────────┴───────╮
            │ ▣ Click "Approve" to continue   │   ← balloon anchored to element
            │   ░░░░░░░░░  Step 3 of 4         │   ← progress (FR-DEL-013)
            │             [ Skip ]   [ Next ▸]│
            ╰─────────────────────────────────╯
   • Element resolved by the engine (s7); re-anchors on scroll/route change.
   • If the element can't be found → step is suppressed silently + anchor-health event (FR-ELM-003/005).
```

### B4 — Onboarding checklist (launcher-driven)  · *US-AUTH-4, FR-AUT-013*
```
   ┌──────────────────────────────┐
   │ Get started 〔 2 / 5 done 〕  │
   │  ✓ Complete your profile      │
   │  ✓ Connect your calendar      │
   │  ▢ Create your first invoice  ▸│ ← launches a walkthrough
   │  ▢ Invite a teammate          ▸│
   │  ▢ Explore reports            ▸│
   │  ░░░░░░░░░░ 40%                │ ← persisted progress per user
   └──────────────────────────────┘
```

### B5 — In-app survey / NPS  · *US-FB-1, FR-ENG-001*
```
   ┌──────────────────────────────────────────┐
   │  How likely are you to recommend us?  [×] │
   │   0  1  2  3  4  5  6  7  8  9  10         │  ← NPS scale (FR-ENG-001)
   │  ◦  ◦  ◦  ◦  ◦  ◦  ◦  ◦  ●  ◦  ◦          │
   │  What's the main reason?                   │
   │  [ free text … ]                           │  ← sentiment/theme analysis (FR-ENG-003)
   │                       [ Submit ]           │
   └──────────────────────────────────────────┘
   • Targeted + throttled so users aren't repeatedly surveyed (FR-DEL-003).
```

---

## C. Cross-cutting UX rules

1. **Non-intrusive by default** — the runtime defers to the host app; modals only when warranted; respect snooze/dismiss memory (FR-DEL-003).
2. **Always isolated & removable** — Shadow-DOM root, scoped styles, clean teardown (FR-DEL-014).
3. **Accessible** — full keyboard nav, focus management, ARIA, reduced-motion, AA contrast (NFR-A11Y-1).
4. **Localised** — runtime resolves user locale; RTL supported (FR-AUT-030, NFR-I18N-1).
5. **Themable** — tenant tokens drive all runtime + admin UI (FR-AUT-031).
6. **Honest AI** — every AI surface is editable (authoring) or cited + abstaining (answers) (FR-AUT-021, FR-HLP-003).
7. **Preview parity** — what authors preview is exactly what users get (FR-AUT-002).

---

### Next step for design
Convert these into high-fidelity comps using the **`frontend-design`** skill and the tenant **design-token** system, then build with the shared **`packages/ui`** component library (`03 §12`). The in-app editor (A4) and the answer agent (B2) are the two experiences to prototype first — they are the product's signature moments.
