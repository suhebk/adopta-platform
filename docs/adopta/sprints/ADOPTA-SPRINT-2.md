# ADOPTA-SPRINT-2 - Runtime SDK, Element Anchoring, Content Model, and Delivery Contract

## Sprint intent

Sprint 2 establishes the browser-delivered runtime foundation for Adopta without building Adoption Studio, analytics, AI, browser extensions, production database infrastructure, or Property MTD integration. The runtime SDK is framework-agnostic TypeScript and is designed to be integrated later through a controlled SDK/API contract.

## Slice 1 - Runtime SDK contract foundation

### Requirement IDs covered

- `FR-RUN-001` - Framework-agnostic runtime SDK package foundation.
- `FR-RUN-002` - Strongly typed runtime initialization contract.
- `FR-RUN-003` - Runtime options, context, result, logger, and error-boundary contracts.
- `FR-RUN-004` - Invalid runtime configuration fails safely without throwing into host applications.
- `FR-RUN-005` - Runtime can initialize in no-op mode without rendering or mutating host application UI.
- `NFR-SEC-1` - Runtime foundation does not capture form values, tokens, claims, headers, or sensitive host data.
- `NFR-TEST-1` - TypeScript unit tests cover the runtime contract.

### Scope delivered

- Added a pnpm TypeScript workspace at the repository root.
- Added `@adopta/runtime-sdk` as the first framework-agnostic SDK package.
- Added typed runtime options, context, result, logger, and error-boundary primitives.
- Added safe validation for required runtime configuration.
- Added a no-op initialization path for host-safe integration testing.
- Added unit tests for valid initialization, invalid configuration, safe error handling, and no-op behavior.
- Extended CI to run .NET validation and TypeScript install/build/test checks.

### Explicitly not built

- Element anchoring implementation beyond type placeholders.
- Tooltip, walkthrough, or content rendering.
- Delivery bundle API.
- Demo host.
- Runtime event pipeline or analytics.
- AI features.
- Browser extension.
- Property MTD integration.
- Adoption Studio screens.
- Production database infrastructure.

### Security and privacy assumptions

The Slice 1 runtime SDK is contract-only and does not inspect DOM fields, forms, headers, tokens, claims, cookies, browser storage, or host application data. Logger metadata is intentionally structural and safe. Future slices must preserve the default posture that sensitive host values are not captured unless a later requirement explicitly approves a governed, documented, tenant-controlled behavior.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm install --frozen-lockfile=false
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md
```

### Known limitations

- Runtime SDK persistence, delivery, anchoring, rendering, telemetry, and analytics are not implemented.
- The package is private and foundation-only until publishing and versioning rules are approved.
- No local demo host exists yet.
- No Property MTD integration contract has been implemented yet.

### Next recommended slice

Add the element anchoring foundation and content/experience model types, keeping rendering, delivery APIs, analytics, and Property MTD integration out of scope until their dedicated slices.

## Slice 2 - Element anchoring foundation

### Requirement IDs covered

- `FR-ELM-001` - Added the first explicit anchor descriptor contract for stable first-party element markers.
- `FR-ELM-002` - Added deterministic runtime resolution for the approved first-party anchor strategy.
- `FR-ELM-003` - Missing, duplicate, invalid, unsupported, unavailable-DOM, and resolver-error cases fail safely without uncaught host errors.
- `FR-MTD-003` - Established `data-adopt-id` as the future first-party stable anchor convention, without modifying the Property MTD repository.
- `NFR-SEC-1` - Resolver does not capture input values, form values, raw DOM text, tokens, headers, or sensitive host data.
- `NFR-TEST-1` - Added TypeScript unit tests for anchor resolution behavior.

### Scope delivered

- Added privacy-safe anchor descriptor and resolution result types.
- Added `DataAdoptIdResolver` for exact `data-adopt-id` matching.
- Added `AnchorResolver` orchestration that validates descriptors and routes only the supported strategy.
- Replaced the Slice 1 placeholder anchor type in runtime options with the real `AnchorDescriptor`.
- Exported anchor APIs from the runtime SDK package.
- Added ADR-002 for the runtime anchor contract.

### Assumptions

`data-adopt-id` is the only real anchor strategy in this slice. The contract is intentionally explicit so first-party applications can choose stable semantic identifiers without exposing page content or sensitive values.

The resolver receives a DOM root from the host runtime or uses `globalThis.document` when available. It reads only `data-adopt-id` attributes and never mutates the DOM.

### Explicitly not built

- CSS selector, XPath, text, aria, positional, AI, or vision fallback strategies.
- Tooltip or walkthrough rendering.
- Delivery bundle API.
- Demo host.
- Runtime event pipeline or analytics.
- Browser extension.
- Property MTD integration.
- Adoption Studio.
- Production database infrastructure.

### Commands to run

```powershell
pnpm test
pnpm typecheck
pnpm build
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
rg "net9\.0" src tests docs .github package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md
```

### Known limitations

- Anchor resolution is deterministic but intentionally narrow.
- No authoring capture flow exists yet.
- No anchor-health event pipeline exists yet.
- No content model, delivery bundle contract, renderer, or demo host exists yet.

### Next recommended slice

Add the experience/content model and delivery bundle contract types while continuing to avoid rendering, analytics, AI, browser extension, Adoption Studio, and Property MTD integration.

## Slice 3 - Experience and content model

### Requirement IDs covered

- `FR-AUT-010` - Added contract-only content items for future walkthrough/checklist-style experiences.
- `FR-DEL-012` - Added versioned, tenant-aware, application-aware, environment-aware, and channel-aware content bundle contracts.
- `FR-DEL-013` - Added a targeting placeholder contract without implementing targeting evaluation.
- `FR-ELM-001` - Content items reuse the Slice 2 `AnchorDescriptor` contract instead of duplicating anchor semantics.
- `NFR-SEC-1` - Content contracts and validation avoid form values, input values, user profiles, tax/HMRC data, tokens, and raw DOM content.
- `NFR-TEST-1` - Added TypeScript and .NET tests for content item, bundle, validation, duplicate IDs, anchors, and targeting placeholder shape.

### Scope delivered

- Added TypeScript runtime SDK contracts for content type, content item, content bundle, targeting placeholder, and validation.
- Added explicit initial content types: `tooltip`, `callout`, `checklist`, and `walkthrough`.
- Added safe typed validation issues for normal invalid content.
- Added .NET Application-layer mirror records and validator for runtime content contracts.
- Added tests for valid and invalid item/bundle contracts, duplicate item IDs, invalid anchors, and targeting placeholder shape.

### Assumptions

Content bundles are contract-only in this slice. They can be validated locally without network calls and are prepared for future delivery design, but no delivery endpoint, persistence model, CDN behavior, or runtime loading is implemented.

Targeting is intentionally represented as a placeholder shape only. No segment, route, user, event, or context evaluation is implemented.

### Explicitly not built

- Tooltip, walkthrough, checklist, banner, or callout renderer.
- Delivery API or delivery service.
- Demo host.
- Runtime event pipeline or analytics.
- AI.
- Browser extension.
- Property MTD integration.
- Adoption Studio.
- EF migrations, database schema, or production database infrastructure.

### Commands to run

```powershell
pnpm test
pnpm typecheck
pnpm build
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
rg "net9\.0" src tests docs .github package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md
```

### Known limitations

- Content contracts are not rendered.
- Content bundles are not fetched from or served by an API.
- Targeting is not evaluated.
- No authoring, publishing, analytics, or event pipeline behavior exists yet.

### Next recommended slice

Add runtime API/delivery contract boundaries and local validation fixtures, still without implementing a delivery service, renderer, analytics pipeline, browser extension, Adoption Studio, or Property MTD integration.

## Slice 4 - Runtime delivery bundle contract

### Requirement IDs covered

- `FR-DEL-012` - Added delivery bundle contract boundaries for versioned runtime content bundles.
- `FR-DEL-013` - Added tenant/application/environment/channel-scoped lookup contracts without implementing runtime targeting evaluation.
- `FR-IDN-031` - Delivery bundle lookup enforces tenant scope and denies cross-tenant access safely.
- `NFR-SEC-1` - Delivery contract seams avoid form values, input values, user profile data, tax/HMRC data, tokens, raw DOM content, and network calls.
- `NFR-TEST-1` - Added contract, fixture, and isolation tests for delivery bundle lookup behavior.

### Scope delivered

- Added .NET Application-layer delivery contracts:
  - `DeliveryBundle`;
  - `DeliveryChannel`;
  - `DeliveryBundleLookupRequest`;
  - `DeliveryBundleLookupResult`;
  - `IDeliveryBundleRepository`.
- Added `InMemoryDeliveryBundleRepository` as a non-durable Sprint 2 foundation seam.
- Registered the in-memory repository in `AddAdoptaInfrastructure()`.
- Added local JSON fixtures for delivery bundle validation tests.
- Added tests for valid/invalid lookup requests, matching bundle lookup, missing bundles, cross-tenant denial, scope-safe not-found behavior, and fixture validation.

### Assumptions

The in-memory delivery repository exists only to validate contract boundaries and tenant/application/environment/channel scoping. It is not durable storage and is not production persistence.

Missing bundles return a typed not-found result. Cross-tenant requests return a typed access-denied result and do not reveal whether another tenant, application, environment, or channel has a bundle.

### Explicitly not built

- Real API endpoint.
- CDN integration.
- Blob Storage.
- Renderer.
- Demo host.
- Runtime event pipeline or analytics.
- AI.
- Browser extension.
- Property MTD integration.
- Adoption Studio.
- EF migrations, `DbContext`, connection strings, or production database infrastructure.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm test
pnpm typecheck
pnpm build
rg "net9\.0" src tests docs .github package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md
```

### Known limitations

- Delivery bundle repository is in-memory and non-durable.
- No delivery HTTP API exists yet.
- No CDN, Blob Storage, cache invalidation, or runtime fetch behavior exists yet.
- Content remains contract-only and is not rendered.

### Next recommended slice

Add local runtime SDK integration fixtures or a small contract-only runtime loading boundary, while still avoiding a real delivery API, renderer, analytics, browser extension, Adoption Studio, and Property MTD integration.

## Slice 5 - Local runtime demo host

### Requirement IDs covered

- `FR-DEL-010` - Added a local-only host that initialises the runtime SDK without blocking or depending on external services.
- `FR-ELM-001` - Demo content uses stable first-party `data-adopt-id` anchors.
- `FR-ELM-003` - Smoke tests prove missing anchors fail safely through the existing resolver.
- `FR-DEL-012` - Demo content uses local content bundle fixtures based on the existing contract.
- `NFR-SEC-1` - Demo host avoids external calls and does not collect form values, tokens, raw user-entered data, or sensitive host data.
- `NFR-TEST-1` - Added smoke tests for static anchors, content validation, anchor resolution, missing anchor behavior, and capture-safety.

### Scope delivered

- Added `apps/runtime-demo` as a local-only pnpm workspace app.
- Added a static demo page with `data-adopt-id` anchors.
- Added local demo content fixtures using existing runtime SDK content contracts.
- Initialised the SDK in no-op mode against the demo page.
- Validated local content and resolved anchors using existing SDK helpers.
- Added smoke tests for the demo host.

### Assumptions

The demo host is a local development aid only. It does not fetch delivery bundles, call APIs, load CDN assets, send events, or integrate with Property MTD. Diagnostics are static local status messages and are not production guidance UI.

### Explicitly not built

- Real renderer, tooltip renderer, walkthrough renderer, checklist renderer, banner renderer, or overlays.
- Runtime event pipeline.
- Real delivery API endpoint.
- CDN or Blob Storage.
- Analytics.
- AI.
- Browser extension.
- Property MTD integration.
- Adoption Studio.
- EF migrations or production database infrastructure.

### Commands to run

```powershell
pnpm test
pnpm typecheck
pnpm build
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md
```

### Known limitations

- Demo host is local-only and static.
- No real runtime UI is rendered.
- No delivery bundle fetch, cache, or API behavior exists.
- No analytics, event pipeline, browser extension, Adoption Studio, or Property MTD integration exists.

### Next recommended slice

Close Sprint 2 with a final review and acceptance summary, then plan Sprint 3 for Adoption Studio and authoring foundations without starting implementation until approved.
