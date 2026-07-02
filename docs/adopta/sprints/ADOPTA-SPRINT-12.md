# ADOPTA-SPRINT-12 - Live Draft Authoring Readiness

## Sprint objective

Enable live draft authoring safely and durably, starting with the content type schema/backfill readiness gate required before production-grade create/update draft flows are wired to live APIs.

Sprint 12 starts with a readiness gate. This sprint does not begin by executing migrations, mutating databases, activating live reads by default, or wiring live write/workflow/publish APIs.

## Slice 1 - Content type migration and live draft readiness gate

### Requirement IDs covered

- `FR-IDN-001` - Preserved explicit live Studio activation posture.
- `FR-IDN-005` - Kept downstream Studio API access controlled and server-side.
- `FR-IDN-012` - Preserved no-tenant-ID Web/page/request model boundary.
- `FR-IDN-031` - Preserved tenant/test header guardrails and tenant context enforcement expectations.
- `FR-GOV-002` - Added the readiness gate required before live draft authoring depends on durable content type schema state.
- `NFR-SEC-1` - Preserved fail-closed migration, activation, and tenant isolation expectations.
- `NFR-SEC-2` - Documented migration/backfill readiness without exposing content body, configured values, tenant values, or sensitive content.
- `NFR-TEST-1` - Added guardrail tests for readiness-gate documentation and read-only Studio client boundaries.

### Scope delivered

- Added `docs/adopta/studio/STUDIO-CONTENT-TYPE-MIGRATION-LIVE-DRAFT-READINESS-GATE.md`.
- Added non-invasive guardrail tests for the migration/live draft readiness gate.
- Updated the Adopta documentation index and current sprint wording.
- Added a cross-reference from the existing schema/backfill readiness guide.
- Kept this slice documentation and tests only.

### Live draft blocking decision

Live draft create/update integration must not proceed until content type migration/backfill readiness is approved.

This gate exists because live draft authoring depends on durable storage and accurate content type source-of-truth behaviour. Proceeding before the migration/backfill plan is approved risks persistence failures, legacy metadata ambiguity, and accidental inference of content type from weak signals.

### Migration and backfill approval boundary

The existing migration source remains review-only:

- `src/Adopta.Infrastructure/Persistence/Migrations/20260702000100_AddAuthoredContentType.cs`.

Migration execution requires separate operational approval. Backfill requires a separately approved plan. Rollback must be approved before execution.

No migration was executed in this slice. No database was created or mutated. No startup migration logic, automatic database creation, deployment automation, or real appsettings values were added.

### Guardrails

- Content type remains required for new authored content.
- Legacy content without authoritative content type remains unknown/unavailable.
- Content type must not be inferred from content key, title, route, selector, UI fallback, runtime delivery metadata, or weak naming patterns.
- Live draft create/update remains unwired.
- Live review/approve/reject/publish remains unwired.
- Live read activation remains explicit and disabled by default.
- Web production code must not use tenant/test header shortcuts.
- No new permission keys were added.

### Explicitly not built

- Migration execution.
- Database creation.
- Database mutation.
- Startup migration logic.
- Live draft create/update implementation.
- Live review/approve/reject/publish integration.
- Backend/API behaviour changes.
- EF/schema changes beyond the existing reviewed migration source.
- Real appsettings values.
- Deployment automation.
- Analytics.
- AI.
- Event Hubs.
- ClickHouse.
- Browser extension work.
- Property MTD integration.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
git diff -- src/Adopta.Web/appsettings.json src/Adopta.Web/appsettings.Development.json src/Adopta.Api/appsettings.json src/Adopta.Api/appsettings.Development.json
Run guardrail searches for migration execution, DB mutation/connectivity, appsettings drift, tenant/test headers, secret markers, live write calls/routes, and live activation boundaries.
git diff --check
```

Results: .NET debug test, release build, release test, TypeScript typecheck, TypeScript build, TypeScript tests, guardrail searches, and diff whitespace checks passed for Slice 1.

### Known limitations

- Migration execution remains separately approval-gated.
- Production backfill remains separately approval-gated.
- Live draft create/update integration remains unavailable.
- Live review/approve/reject/publish integration remains unavailable.
- Live read activation remains explicit and disabled by default.

### Next recommended slice

Add a controlled live draft create/update API integration plan only after the readiness gate is satisfied, or continue with operational migration/backfill execution preparation if database enablement remains the highest blocker. Keep migration execution, database mutation, startup migration logic, appsettings values, deployment automation, analytics, AI, browser extension work, and Property MTD integration separately approved.

## Slice 2 - Controlled live draft create/update integration planning

### Requirement IDs covered

- `FR-IDN-001` - Preserved explicit live Studio activation posture.
- `FR-IDN-005` - Kept downstream Studio API access controlled and server-side.
- `FR-IDN-012` - Preserved no-tenant-ID Web/page/request model boundary.
- `FR-IDN-031` - Preserved tenant/test header guardrails and tenant context enforcement expectations.
- `FR-GOV-002` - Planned gated live draft authoring integration over existing authoring boundaries.
- `NFR-SEC-1` - Preserved fail-closed migration, activation, write, and tenant isolation expectations.
- `NFR-SEC-2` - Planned safe write-result and save-state behaviour without exposing content body, configured values, tenant values, or sensitive content.
- `NFR-TEST-1` - Added guardrail tests for create/update planning and read-only client boundaries.

### Scope delivered

- Added `docs/adopta/studio/STUDIO-LIVE-DRAFT-CREATE-UPDATE-INTEGRATION-PLAN.md`.
- Added non-invasive planning guardrail tests.
- Updated the Adopta documentation index.
- Kept this slice documentation and tests only.
- Did not implement live create.
- Did not implement live update.
- Did not add a backend update endpoint.

### Current create/update state

`POST /authoring/content` exists and requires tenant context plus `Authoring.Manage`.

No live authoring update endpoint exists today.

`StudioAuthoringReadApiClient` remains read-only and returns safe unavailable results for create/update/workflow/publish methods.

`LocalStudioContentClient` remains the fallback.

### Future create/update recommendation

Live create and live update should be separate future implementation slices.

The future live create slice can use the existing API create endpoint after the readiness gate is satisfied.

The future live update slice must first add an approved backend update endpoint and contract.

Future write integration should use a separately gated draft-write boundary, such as `StudioAuthoringDraftApiClient`, rather than silently expanding `StudioAuthoringReadApiClient`.

### Migration and backfill blocking summary

Live draft create remains blocked until content type migration/backfill readiness is approved.

Live draft update is blocked by both migration/backfill readiness and the missing backend update endpoint/contract.

### Tenant, auth, and result guardrails

- `StudioApiRequestBoundaryHandler` remains the only Authorization attachment boundary.
- Web/page/request models must not send tenant IDs.
- Web production code must not use `X-Adopta-Tenant-Id`.
- Web production code must not use `X-Adopta-Test-*`.
- Web `RuntimeContentType` must map explicitly to API/domain `AuthoredContentType`.
- Client results must remain safe typed `StudioContentClientResult<T>` values.
- No new permission keys were added.
- Future create/update should preserve `Authoring.Manage`.
- Existing reads preserve `Authoring.Read`.

### Explicitly not built

- Live create implementation.
- Live update implementation.
- Backend update endpoint.
- Migration execution.
- Database creation.
- Database mutation.
- Startup migration logic.
- Live review/approve/reject/publish integration.
- Backend/API behaviour changes.
- EF/schema changes.
- Real appsettings values.
- Deployment automation.
- Analytics.
- AI.
- Event Hubs.
- ClickHouse.
- Browser extension work.
- Property MTD integration.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
git diff -- src/Adopta.Web/appsettings.json src/Adopta.Web/appsettings.Development.json src/Adopta.Api/appsettings.json src/Adopta.Api/appsettings.Development.json
Run guardrail searches for migration execution, DB mutation/connectivity, appsettings drift, tenant/test headers, secret markers, live write calls/routes, and live activation boundaries.
git diff --check
```

Results: .NET debug test, release build, release test, TypeScript typecheck, TypeScript build, TypeScript tests, guardrail searches, and diff whitespace checks passed for Slice 2.

### Known limitations

- Live draft create remains unavailable.
- Live draft update remains unavailable.
- Backend update endpoint and contract remain unavailable.
- Migration execution remains separately approval-gated.
- Production backfill remains separately approval-gated.
- Live workflow/publish integration remains unavailable.

### Next recommended slice

Implement live draft create behind the readiness gate, or design the backend draft update endpoint if create remains blocked by database enablement. Keep update implementation, migration execution, database mutation, startup migration logic, workflow/publish integration, appsettings values, deployment automation, analytics, AI, browser extension work, and Property MTD integration separately approved.
