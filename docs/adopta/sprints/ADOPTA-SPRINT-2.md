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
