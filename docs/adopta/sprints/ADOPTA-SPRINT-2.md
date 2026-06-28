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
