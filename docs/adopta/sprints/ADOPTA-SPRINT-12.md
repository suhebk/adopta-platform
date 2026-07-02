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
