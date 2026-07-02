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

## Slice 3 - Content type source-of-truth planning

### Requirement IDs covered

- `FR-IDN-001` - Preserved controlled and explicit live Studio read activation posture.
- `FR-IDN-005` - Kept downstream API access read-only and server-side.
- `FR-IDN-012` - Preserved the no-tenant-ID-from-Web request model boundary.
- `FR-IDN-031` - Preserved tenant/test header guardrails and tenant context enforcement expectations.
- `FR-GOV-002` - Planned authoritative governed metadata for Studio content inventory, detail, edit, and publish readiness UX.
- `NFR-SEC-1` - Preserved fail-closed tenant and authorization boundaries.
- `NFR-SEC-2` - Planned content type metadata without exposing content body, tenant values, headers, claims, configured values, or sensitive content.
- `NFR-TEST-1` - Added planning guardrail tests for the source-of-truth design and non-goals.

### Scope delivered

- Added `docs/adopta/studio/STUDIO-CONTENT-TYPE-SOURCE-OF-TRUTH-DESIGN.md`.
- Added planning guardrail tests for the content type source-of-truth decision.
- Updated the documentation index.
- Kept this slice documentation-only plus non-invasive tests.
- Did not add production code, domain changes, API contract changes, EF migrations, schema changes, appsettings values, deployment automation, or live API activation.

### Source-of-truth decision

Content type must be a real source-of-truth field. It must live on the authored content item, not the version.

The authored content item represents the guidance artifact. Versions represent revisions and lifecycle state for that same artifact. A type change from tooltip to walkthrough changes the artifact kind and must not be treated as ordinary version metadata.

The domain should use a domain-owned enum, not depend directly on runtime application-layer contracts.

Proposed domain enum values:

- `Tooltip`;
- `Callout`;
- `Checklist`;
- `Walkthrough`.

Content type must not be inferred from content key, title, route, selector, UI fallback, runtime delivery metadata, or any other weak signal.

The Web mapper must not fake accuracy. It should set `HasKnownContentType=true` only when the API returns a valid known content type. Missing, invalid, or legacy absent content type must keep `HasKnownContentType=false`.

### Immutability decision

Content type should be required for new live authored content.

Content type should be immutable after live authoring creation unless a future separately approved rule allows draft-only mutation.

If draft-only mutation is later approved, it must be explicit, permission-checked, tenant-scoped, audit-safe, and limited to content that has not entered review, approval, publish, archive, or delivery history.

### Legacy and backward compatibility

Existing content without content type must remain unknown or unavailable until separately migrated or backfilled.

Backward-compatible read behaviour should be:

- known valid API content type maps to `HasKnownContentType=true`;
- missing content type maps to `HasKnownContentType=false`;
- invalid content type maps to `HasKnownContentType=false`;
- no inference is performed from content key, title, route, selector, or UI fallback.

`Unknown` should not be a normal valid value for new live authored content.

### Schema and migration planning

A future implementation will likely require `ContentType` on `AuthoredContentItems`.

Likely persistence shape:

- controlled string mapping;
- `nvarchar(32)` or equivalent;
- item-level tenant-owned authored content row;
- no free-form type values;
- no sensitive values.

Migration and backfill must be separately approved. No migration should execute automatically. No database should be created automatically. No startup database mutation should be introduced.

Migration options for later approval:

- nullable or staged migration;
- controlled backfill;
- validation requiring type for new content only;
- later hardening migration to require non-null content type only after backfill is complete.

### Future implementation impact

Future implementation is expected to affect:

- domain model;
- create API contract;
- read API contract;
- validation;
- runtime bundle mapping;
- Web read DTOs;
- Web mapper;
- Studio live create flow;
- tests;
- persistence and migration source, separately approved.

No new permission key is expected. `Authoring.Read` remains the read permission. `Authoring.Manage` remains the likely create/manage permission.

### Explicitly not built

- Production code changes.
- Domain enum implementation.
- Domain model changes.
- API contract implementation.
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

Results: .NET debug test, release build, release test, TypeScript typecheck, TypeScript build, TypeScript tests, guardrail searches, and diff whitespace checks passed for Slice 3.

### Known limitations

- This slice does not implement the content type domain model.
- Content type remains unknown for live authoring API reads until a later approved implementation.
- No schema migration or backfill is approved by this slice.
- Application metadata remains ID-only.
- No live write/workflow/publish integration exists.
- No operational live read activation is enabled by default.

### Next recommended slice

Implement the content type source-of-truth in a separately approved, tightly scoped production slice. The smallest safe implementation should add the domain-owned enum, item-level domain property, create/read DTO support, validation, runtime bundle mapping, and Web mapper support. Keep EF migration source and database backfill separately approved unless the implementation slice explicitly includes review-only schema work.

## Slice 4 - Content type source-of-truth domain/API implementation

### Requirement IDs covered

- `FR-IDN-001` - Preserved explicit live Studio read activation posture.
- `FR-IDN-005` - Kept downstream Studio API access read-only.
- `FR-IDN-012` - Preserved no-tenant-ID Web request model boundary.
- `FR-IDN-031` - Preserved tenant/test header guardrails and existing tenant context enforcement.
- `FR-GOV-002` - Added authoritative content type metadata for governed authoring and Studio read UX.
- `NFR-SEC-1` - Preserved tenant-scoped API/repository behaviour and existing permission model.
- `NFR-SEC-2` - Added content type metadata without exposing content body, headers, claims, configured values, tenant values, or sensitive content.
- `NFR-TEST-1` - Added domain, API, runtime mapper, Web mapper, EF model, and migration-source tests.

### Scope delivered

- Added domain-owned `AuthoredContentType`.
- Added item-level `AuthoredContentItem.ContentType`.
- Added create/read API contract support.
- Added validation requiring content type for new authored content.
- Updated runtime bundle mapping to use the authored content type instead of hardcoded tooltip.
- Updated Web live-read DTO and mapper to mark known content type authoritative.
- Preserved Web fallback for missing or invalid live API content type with `HasKnownContentType=false`.
- Added EF model configuration for `AuthoredContentItems.ContentType`.
- Added review-only EF migration source for `AuthoredContentItems.ContentType`.
- Updated content type source-of-truth documentation.

### Domain and API behaviour

Content type is now an authored content item source-of-truth field. It is not version metadata.

Allowed domain values:

- `Tooltip`;
- `Callout`;
- `Checklist`;
- `Walkthrough`.

New create requests must provide a valid content type. Invalid or missing content type returns a typed safe validation issue without echoing raw invalid input.

Read responses include the content type. Existing `Authoring.Read` and `Authoring.Manage` permission boundaries are preserved. No new permission keys were added.

### Runtime mapper behaviour

Runtime bundle mapping now uses controlled mapping:

- `Tooltip` -> `RuntimeContentType.Tooltip`;
- `Callout` -> `RuntimeContentType.Callout`;
- `Checklist` -> `RuntimeContentType.Checklist`;
- `Walkthrough` -> `RuntimeContentType.Walkthrough`.

The mapper no longer hardcodes tooltip for authored content with a known type.

### Web mapper behaviour

The live Studio read mirror DTO keeps content type nullable for compatibility with older or incomplete API responses.

Mapper behaviour:

- known valid API content type maps to runtime content type and `HasKnownContentType=true`;
- missing API content type maps to fallback display type and `HasKnownContentType=false`;
- invalid API content type maps to fallback display type and `HasKnownContentType=false`;
- no inference is performed from content key, title, route, selector, or UI fallback.

### EF mapping and review-only migration

EF model configuration maps `AuthoredContentItem.ContentType` as a controlled string with max length 32.

Review-only migration source adds `ContentType` to `AuthoredContentItems` with a controlled string mapping. The migration is source only for review. It was not executed.

No automatic migration execution, automatic database creation, startup database mutation, production backfill execution, deployment automation, or real database connectivity check was added.

### Backward compatibility and backfill

New authored content requires content type.

Existing persisted rows without content type require separately approved migration/backfill handling before production SQL Server enablement relies on the new field.

The review-only migration uses a safe default for schema transition review, but production backfill and deployment sequencing remain separately approval-gated.

### Explicitly not built

- Migration execution.
- Automatic database creation.
- Startup database mutation.
- Production backfill execution.
- Live read activation by default.
- Live create/update/review/approve/reject/publish integration from Studio.
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

Results: .NET debug test, release build, release test, TypeScript typecheck, TypeScript build, TypeScript tests, guardrail searches, and diff whitespace checks passed for Slice 4.

### Known limitations

- Production SQL Server environments still require separately approved migration execution and backfill planning.
- Live Studio create/update/review/approve/reject/publish integration remains out of scope.
- Live read activation remains explicit and disabled by default.
- Application metadata remains ID-only.

### Next recommended slice

Plan controlled migration/backfill review for `AuthoredContentItems.ContentType`, or proceed to application display metadata for Studio reads if production SQL Server enablement remains deferred. Keep migration execution, startup mutation, live write/workflow/publish wiring, appsettings values, deployment automation, analytics, AI, browser extension work, and Property MTD integration separately approved.

## Slice 5 - Content type schema/backfill readiness and Sprint 11 closeout

### Requirement IDs covered

- `FR-IDN-001` - Preserved explicit live Studio read activation posture.
- `FR-IDN-005` - Kept downstream Studio API access read-only.
- `FR-IDN-012` - Preserved no-tenant-ID Web request model boundary.
- `FR-IDN-031` - Preserved tenant/test header guardrails and tenant context enforcement expectations.
- `FR-GOV-002` - Closed the content type source-of-truth work with schema/backfill readiness guidance.
- `NFR-SEC-1` - Preserved fail-closed migration, activation, and tenant isolation expectations.
- `NFR-SEC-2` - Documented schema/backfill readiness without exposing content body, configured values, tenant values, or sensitive content.
- `NFR-TEST-1` - Added documentation guardrail tests for migration review, backfill, rollback, fail-closed behaviour, and closeout status.

### Scope delivered

- Added `docs/adopta/studio/STUDIO-CONTENT-TYPE-SCHEMA-BACKFILL-READINESS.md`.
- Added documentation guardrail tests for content type schema/backfill readiness.
- Updated the content type source-of-truth design doc to reference the readiness guide.
- Updated the documentation index with the new readiness guide.
- Closed Sprint 11 at the live read contract hardening and schema/backfill readiness level.

### Schema and migration readiness

The review-only migration source remains:

- `src/Adopta.Infrastructure/Persistence/Migrations/20260702000100_AddAuthoredContentType.cs`.

The migration source adds `ContentType` to `AuthoredContentItems`. It remains review-only. Sprint 11 Slice 5 does not approve execution.

No migration was executed. No database was created or mutated. No automatic startup migration logic was added. No deployment automation was added.

### Backfill strategy

Existing authored content without an authoritative type must be handled through a separately approved operational process.

The documented approach is staged:

- inspect tenant-scoped authored content inventory;
- classify only records with an approved authoritative type source;
- leave records without authoritative type unknown/unavailable;
- apply controlled backfill only after data owner approval;
- validate new content still requires a valid content type;
- validate live reads mark known type only when the API returns a valid value.

Content type must not be inferred from content key, title, route, selector, UI fallback, runtime delivery metadata, or weak naming patterns.

### Rollback and fail-closed posture

Rollback must be approval-gated and rehearsed before production use.

Fail-closed expectations remain:

- invalid persistence configuration fails safely;
- live Studio read activation remains disabled unless explicitly configured;
- live write/workflow/publish integration remains unavailable;
- cross-tenant existence remains hidden or safely denied;
- missing or invalid content type must not be treated as authoritative.

### Explicitly not built

- Migration execution.
- Database creation.
- Database mutation.
- Startup migration logic.
- Deployment automation.
- Live read activation by default.
- Live create/update/review/approve/reject/publish integration.
- Backend/API behaviour changes.
- EF model configuration changes.
- Migration source changes.
- Appsettings changes.
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
Run guardrail searches for migration execution, DB mutation, DB connectivity/health checks, appsettings drift, tenant/test headers, secret markers, live write routes/calls, and live activation boundaries.
git diff --check
```

Results: .NET debug test, release build, release test, TypeScript typecheck, TypeScript build, TypeScript tests, guardrail searches, and diff whitespace checks passed for Slice 5.

### Known limitations

- Migration execution remains separately approval-gated.
- Production backfill remains separately approval-gated.
- Live Studio read activation remains explicit and disabled by default.
- Live Studio create/update/review/approve/reject/publish integration remains unavailable.
- Application metadata remains ID-only.

### Sprint 11 closeout checklist

- Slice 1 authoring read contract gap review complete.
- Slice 2 read-only authoring summary metadata complete.
- Slice 3 content type source-of-truth design complete.
- Slice 4 content type domain/API implementation complete.
- Slice 5 content type schema/backfill readiness complete.
- No migration execution performed.
- No database mutation performed.
- No live read activation by default.
- No live write/workflow/publish wiring added.
- No real appsettings values or deployment automation added.
- Guardrail tests and verification completed.

### Sprint 11 closeout status

Sprint 11 is ready to close after Slice 5 verification passes.

### Next recommended sprint

Start ADOPTA-SPRINT-12 with controlled operational planning for one of two paths:

- production migration/backfill execution preparation for `AuthoredContentItems.ContentType`, still separately approval-gated and with no automatic startup mutation; or
- tenant-scoped application display metadata for Studio live reads if database enablement remains deferred.

Keep migration execution, database mutation, live write/workflow/publish wiring, appsettings values, deployment automation, analytics, AI, browser extension work, and Property MTD integration separately approved.
