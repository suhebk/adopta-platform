# ADOPTA-SPRINT-7 - Runtime Experience Layer Foundation

## Sprint intent

Sprint 7 expands the runtime renderer from tooltip/callout foundation into a production-grade runtime guidance experience layer. The sprint must preserve the privacy-safe runtime delivery model and must not add analytics, event transport, external storage, CDN, Blob Storage, Property MTD integration, migration execution, database creation, production deployment automation, or production database mutation unless explicitly approved in a later slice.

## Slice 1 - Runtime experience content contracts

### Requirement IDs covered

- `FR-AUT-016` - Extended runtime content contracts for checklist and walkthrough experience structures while preserving existing tooltip and callout compatibility.
- `FR-IDN-031` - Kept anchor references tied to the existing first-party `data-adopt-id` descriptor contract.
- `NFR-SEC-1` - Kept contracts privacy-safe and avoided field/form values, raw DOM text, tokens, headers, claims, secrets, connection strings, tax/HMRC/property data, and sensitive values.
- `NFR-A11Y-1` - Added contract-only placement, dismiss behaviour, and theme/style-token metadata for future accessible renderer behavior.
- `NFR-TEST-1` - Added TypeScript and .NET mirror validation coverage for valid and invalid runtime experience content.

### Scope delivered

- Added optional rich checklist content structures with ordered steps.
- Added optional rich walkthrough content structures with ordered steps.
- Added controlled renderer placement tokens.
- Added controlled dismiss behaviour tokens.
- Added controlled theme/style tokens.
- Added runtime experience metadata for item-level and step-level contract use.
- Preserved existing tooltip and callout contracts.
- Preserved existing placeholder-safe checklist and walkthrough items without rich structures.
- Updated TypeScript content validation.
- Updated .NET Application runtime mirror contracts and validator for delivery/API parity.

### Contract assumptions

Checklist and walkthrough structures are optional in this slice. Existing `checklist` and `walkthrough` content items without step structures remain valid and continue to be placeholder-safe.

Placement is represented only by controlled tokens. It does not permit raw CSS selectors, XPath, text matching, screen coordinates, free-form CSS, or AI/vision fallback.

Theme/style metadata is represented only by safe tokens. It does not permit raw CSS, CSS custom properties, script, markup, or host style mutation.

Dismiss behaviour is contract-only. No renderer behavior, persistence, event emission, analytics, or cross-session dismissal state is implemented in this slice.

### Validation rules

- Checklist and walkthrough steps require non-empty IDs and titles when rich structures are present.
- Duplicate checklist and walkthrough step IDs fail with typed validation issues.
- Optional step bodies must be strings in the TypeScript contract.
- Optional anchor references must use the existing `data-adopt-id` anchor descriptor.
- Placement, dismiss behaviour, and theme metadata must use approved tokens.
- Validation messages must be generic and must not echo sensitive input values.

### Explicitly not built

- Checklist renderer.
- Walkthrough renderer.
- Tooltip/callout renderer changes.
- DOM mutation.
- Analytics pipeline.
- Event transport.
- Event Hubs.
- ClickHouse.
- AI assistant.
- Browser extension.
- Property MTD integration.
- Backend delivery endpoint changes.
- Appsettings changes.
- EF migrations.
- Database schema changes.
- DB calls.
- External storage.
- CDN publishing.
- Blob Storage publishing.
- Deployment automation.

### Commands to run

```powershell
pnpm typecheck
pnpm build
pnpm test
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
rg "innerHTML|XPath|screen-coordinate|text matching|querySelector\(|\.value|FormData|localStorage|sessionStorage|Authorization|Bearer|Password|ConnectionString|HMRC|tax|property|secret|token|claim" packages/runtime-sdk/src/content packages/runtime-sdk/tests/runtimeExperienceContent.test.ts src/Adopta.Application/Runtime tests/Adopta.UnitTests/RuntimeContentContractTests.cs
```

### Known limitations

- Checklist and walkthrough are contract-only and are not rendered.
- Placement, dismiss behaviour, and theme/style tokens are validated but not interpreted by the renderer.
- Targeting remains placeholder-only.
- No analytics, event transport, external storage, CDN, Blob Storage, Property MTD integration, or production deployment automation exists.
- No database schema or persistence changes were added for runtime experience metadata.

### Next recommended slice

Add renderer support for checklist content using the new contracts, keeping walkthrough rendering, analytics, event transport, external storage, Property MTD integration, and production infrastructure out of scope until separately approved.
