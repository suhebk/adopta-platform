# ADOPTA-SPRINT-11 - Studio Live Read Contract Hardening

## Sprint objective

Harden live read-only Studio API integration so the Studio can safely use real API read data with complete, accurate, production-quality metadata and UX behaviour.

Sprint 11 starts with an authoring read contract gap review before operational live read activation depends on the current read response.

## Slice 1 - Authoring read contract gap review

### Requirement IDs covered

- `FR-IDN-001` - Reconfirmed live Studio reads remain controlled and explicit.
- `FR-IDN-005` - Reconfirmed downstream API access remains server-side and read-only.
- `FR-IDN-012` - Reconfirmed Studio request models do not accept tenant IDs.
- `FR-IDN-031` - Reconfirmed Web production code must not use tenant/test header shortcuts.
- `FR-GOV-002` - Reviewed authoring metadata needed for governed Studio read UX.
- `NFR-SEC-1` - Preserved fail-closed and disabled-by-default activation posture.
- `NFR-SEC-2` - Documented safe metadata gaps without exposing sensitive values.
- `NFR-TEST-1` - Added guardrail tests for current contract gaps and safe mapper behaviour.

### Scope delivered

- Added `docs/adopta/studio/STUDIO-AUTHORING-READ-CONTRACT-GAP-REVIEW.md`.
- Added guardrail tests documenting current live read contract gaps.
- Updated the documentation index for Sprint 11 and the gap review.
- Kept the live read API client read-only.
- Kept `LocalStudioContentClient` as the default/fallback.
- Did not add production code or backend API contract changes.

### Contract gaps reviewed

#### Content type

Content type is missing from the authoring read API response and from the authoring domain source of truth.

The Web mapper must continue to mark content type as unknown/unavailable instead of claiming accuracy. A complete fix requires separately approved domain, API, create/update, persistence, and schema work.

#### Lifecycle state

Lifecycle state is available through API version metadata. No backend change is needed for this field in Slice 1.

#### Version metadata

Basic version metadata is available:

- version ID;
- version label;
- lifecycle state;
- created timestamp.

Richer modified, review, approval, publish, and archive timestamps are not fully exposed.

#### Audit/history summary

The Studio currently uses a safe fallback history summary when reading from the authoring API. The authoring read response does not expose lifecycle or publishing history summary fields.

Existing history repositories may support a future read-only summary without schema changes, but that expansion is not implemented in Slice 1.

#### Application metadata

The authoring read response exposes `ApplicationId` only. It does not join tenant-scoped application display metadata such as application name.

#### Delivery/publish metadata

Delivery and publish metadata are not exposed in the authoring read response. A future read-only structural publish summary may be derived from existing publishing history if approved.

### Security and tenant guardrails

- No tenant IDs from browser/page/request models.
- No `X-Adopta-Tenant-Id`.
- No `X-Adopta-Test-*` production shortcut.
- No tokens, headers, claims, secrets, connection strings, tenant values, raw exceptions, or sensitive content in UI, errors, logs, or docs.
- Existing `Authoring.Read` remains the read permission.
- No new permission keys were added.

### Explicitly not built

- Production code changes.
- Backend API changes.
- Domain model changes.
- EF migrations.
- Database schema changes.
- Live read activation by default.
- Live create/update/review/approve/reject/publish.
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
Run guardrail searches for tenant/test headers, token/secret markers, live write routes/calls, migration/database mutation, DB connectivity/health checks, appsettings drift, and live activation boundaries.
git diff --check
```

### Known limitations

- Content type remains unknown for live authoring API reads.
- Live read history summary remains fallback-only in Web mapping.
- Application metadata remains ID-only.
- Publish/delivery metadata is not exposed by authoring read responses.
- No live write/workflow/publish integration exists.
- No operational live read activation is enabled by default.

### Next recommended slice

Add a separately approved minimal read-only authoring summary contract if feasible without schema change. The likely Slice 2 should focus on safe DTO/API expansion for history and publish summary while keeping content type source-of-truth work separate unless domain/schema changes are explicitly approved.

## Slice 2 - Read-only authoring summary DTO/API expansion

### Requirement IDs covered

- `FR-IDN-001` - Preserved controlled, explicit live Studio read activation posture while improving read contract metadata.
- `FR-IDN-005` - Kept downstream API access read-only and server-side.
- `FR-IDN-012` - Preserved the no-tenant-ID-from-Web request model boundary.
- `FR-IDN-031` - Preserved tenant/test header guardrails and existing tenant context enforcement.
- `FR-GOV-002` - Added safe governed read summary metadata for Studio content inventory and detail views.
- `NFR-SEC-1` - Preserved fail-closed tenant-scoped repository behaviour.
- `NFR-SEC-2` - Added structural summary metadata without exposing actor IDs, raw history records, claims, headers, configured values, content body, or sensitive content.
- `NFR-TEST-1` - Added API and mapper tests for summary mapping, safe fallback, tenant isolation, and content type gap preservation.

### Scope delivered

- Added optional `AuthoredContentReadSummaryResponse` to authoring read responses.
- Added optional latest publish summary metadata.
- Built read summaries from existing tenant-scoped lifecycle and publishing history repositories.
- Loaded lifecycle and publishing history once for list reads.
- Loaded lifecycle and publishing history for get-by-id only after safe content lookup succeeds.
- Updated Web-local read API mirror contracts.
- Updated the Studio authoring read mapper to use API summary metadata when present.
- Preserved the existing safe fallback when summary metadata is absent.
- Updated the authoring read contract gap review.

### Summary fields added

- `LifecycleEventCount` from lifecycle history records matching current tenant and content ID.
- `PublishingEventCount` from publishing history records matching current tenant and content ID.
- `LatestSafeActivity` from controlled labels only:
  - `Review requested`;
  - `Approved for publishing`;
  - `Returned to draft`;
  - `Lifecycle decision recorded`;
  - `Published to runtime delivery`;
  - `No lifecycle or publishing history available.`
- `LatestActivityAtUtc` from the latest lifecycle or publishing history timestamp.
- `LatestPublish.Status` from controlled publishing result mapping.
- `LatestPublish.Environment` from a structural-value guard.
- `LatestPublish.Channel` from publishing history.
- `LatestPublish.OccurredAtUtc` from publishing history.

### Content type status

Content type remains unknown for live authoring read API responses. The Web mapper continues to set `HasKnownContentType=false` and does not treat the fallback `Tooltip` value as authoritative.

Content type source-of-truth work remains out of scope because it requires separately approved domain/API/create-update/persistence/schema decisions.

### Application metadata status

Application metadata remains ID-only. `ApplicationName` and other tenant application display metadata were not added to the authoring read response in this slice.

### Security and tenant guardrails

- Existing `Authoring.Read` endpoint permission is preserved.
- No new permission keys were added.
- Summary composition filters by both tenant ID and content ID.
- The summary does not expose tenant IDs.
- The summary does not expose actor IDs.
- The summary does not expose raw lifecycle or publishing records.
- The summary does not expose content body, claims, headers, tokens, secrets, connection strings, tenant values, raw exceptions, or sensitive content.
- Cross-tenant content remains hidden by existing read endpoint behaviour.

### Explicitly not built

- Content type source-of-truth fix.
- Domain model content type changes.
- Create/update contract changes.
- EF migrations.
- Database schema changes.
- Live read activation by default.
- Live create/update/review/approve/reject/publish integration.
- Real appsettings values.
- Deployment automation.
- Analytics.
- AI.
- Event Hubs.
- ClickHouse.
- Browser extension work.
- Property MTD integration.

### Commands run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
git diff -- src/Adopta.Web/appsettings.json src/Adopta.Web/appsettings.Development.json src/Adopta.Api/appsettings.json src/Adopta.Api/appsettings.Development.json
Run guardrail searches for tenant/test headers, token/secret markers, live write routes/calls, migration/database mutation, DB connectivity/health checks, appsettings drift, and live activation boundaries.
git diff --check
```

Results: .NET debug test, release build, release test, TypeScript typecheck, TypeScript build, TypeScript tests, guardrail searches, and diff whitespace checks passed for Slice 2.

### Known limitations

- Content type remains unknown for live authoring API reads.
- Application metadata remains ID-only.
- Latest publish summary is structural and does not include delivery bundle contents.
- No live write/workflow/publish integration exists.
- No operational live read activation is enabled by default.

### Next recommended slice

Plan the content type source-of-truth decision separately, or add tenant-scoped application display metadata to authoring reads if the Studio needs a friendlier application selector/display. Keep schema changes, live write/workflow/publish integration, appsettings values, deployment automation, analytics, AI, browser extension work, and Property MTD integration separately approved.
