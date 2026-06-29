# ADOPTA-SPRINT-5 - Controlled Production Enablement Foundations

## Sprint intent

Sprint 5 starts controlled production enablement after the Sprint 4 persistence and operational-readiness foundation. The sprint remains approval-gated and must not mutate production infrastructure or data without explicit approval.

## Slice 1 - Controlled EF schema baseline generation

### Requirement IDs covered

- `FR-IDN-030` - Added reviewable EF schema baseline source for tenant-owned durable persistence.
- `FR-IDN-031` - Verified tenant-scoped baseline tables include tenant ownership columns and tenant-scoped indexes.
- `FR-IDN-040` - Included audit and security audit tables in the baseline as structural, tenant-scoped persistence records.
- `FR-GOV-001` - Included authored content item/version tables for future governed authoring persistence.
- `FR-GOV-002` - Added schema baseline review documentation with approval and rollback review gates.
- `NFR-SEC-1` - Kept migration execution, startup mutation, real secrets, real database calls, and deployment automation out of scope.
- `NFR-TEST-1` - Updated migration guardrails and added schema baseline tests.

### Scope delivered

- Added EF Design tooling dependency to Infrastructure only with `PrivateAssets="all"`.
- Added design-time `AdoptaDbContext` factory using placeholder-only configuration.
- Generated initial schema baseline migration source files.
- Added schema baseline review documentation.
- Updated migration-readiness guardrails to allow approved migration source and tooling-only EF Design while still blocking execution/startup mutation.
- Added tests for migration file presence, expected tables, tenant ownership columns, tenant-scoped indexes, placeholder-only design-time factory, and baseline review documentation.

### Controlled baseline assumptions

The generated migration is a source review artifact only. It is not approval to execute migrations, create databases, connect to SQL Server, deploy Azure SQL, or mutate production infrastructure.

Persistence remains disabled by default. In-memory repositories remain the default runtime path. SQL Server persistence remains explicitly opt-in.

### Explicitly not built

- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Production Azure SQL deployment.
- Infrastructure automation.
- Real SQL Server connectivity checks.
- Live health checks.
- Durable audit implementation.
- Lifecycle history persistence.
- Publishing history persistence.
- Studio UI.
- Runtime renderer.
- Delivery API.
- Analytics pipeline.
- AI assistant.
- Event Hubs or ClickHouse.
- Browser extension.
- Property MTD integration.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests
```

### Known limitations

- Migration source exists, but migration execution is not approved.
- No real database has been created or modified.
- No live SQL Server connectivity checks exist.
- No production deployment automation exists.
- Database-level tenant isolation and RLS-style hardening remain future work.

### Next recommended slice

Add durable audit, security audit, lifecycle, and publishing history persistence behind explicit SQL Server opt-in configuration, preserving safe tenant isolation and leaving migration execution approval separate.

## Slice 2 - Durable audit, lifecycle, and publishing history persistence

### Requirement IDs covered

- `FR-IDN-030` - Added EF-backed durable repository seams for approved tenant-owned history records.
- `FR-IDN-031` - Enforced tenant isolation at audit, lifecycle history, and publishing history repository boundaries.
- `FR-IDN-040` - Added durable audit/security audit support and structural lifecycle/publishing history persistence.
- `FR-GOV-001` - Added lifecycle history persistence for authored content decisions.
- `FR-GOV-002` - Added publishing history persistence for environment/channel-scoped publishing decisions.
- `NFR-SEC-1` - Kept history records structural and excluded sensitive values, secrets, claims, tokens, headers, form values, tax data, HMRC data, and property data.
- `NFR-TEST-1` - Added DI, tenant-isolation, and migration-source tests for durable history persistence.

### Scope delivered

- Added `IAuthoredContentLifecycleHistoryRepository`.
- Added `IAuthoredContentPublishingHistoryRepository`.
- Added in-memory lifecycle and publishing history repositories for the default path.
- Added EF-backed repositories for:
  - general audit events;
  - lifecycle history;
  - publishing history.
- Kept the existing EF security audit repository and added durable-history coverage around the approved persistence path.
- Registered EF history repositories only when SQL Server persistence is explicitly enabled and valid.
- Added review-only migration source for:
  - `AuthoredContentLifecycleHistory`;
  - `AuthoredContentPublishingHistory`.
- Added tenant-scoped indexes and tenant ownership columns for the new history tables.
- Added tests for default in-memory registration, EF opt-in registration, tenant isolation, missing tenant context, cross-tenant writes, and migration-source shape.

### Durable history assumptions

History records are structural metadata only. They do not store content bodies, tokens, headers, raw claims, form values, input values, tax data, HMRC data, property data, secrets, credentials, or sensitive values.

The `AddAuthoringHistoryPersistence` migration is a source review artifact only. It is not approval to execute migrations, create databases, connect to SQL Server, deploy Azure SQL, or mutate production infrastructure.

Persistence remains disabled by default. In-memory repositories remain the default runtime path. SQL Server persistence remains explicitly opt-in.

### Explicitly not built

- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Production Azure SQL deployment.
- Deployment automation.
- Real SQL Server connectivity checks.
- Live health checks.
- Appsettings changes.
- Studio UI.
- Runtime renderer.
- Delivery API.
- Analytics pipeline.
- AI assistant.
- Event Hubs or ClickHouse.
- Browser extension.
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
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
```

### Known limitations

- Migration source exists, but migration execution is not approved.
- No real database has been created or modified.
- No live SQL Server connectivity checks exist.
- No production deployment automation exists.
- Database-level tenant isolation and RLS-style hardening remain future work.

### Next recommended slice

Harden API authorization and operational readiness around the durable history path, including route permission verification and non-leaky operational diagnostics, while keeping migration execution separately approval-gated.

## Slice 3 - Production API hardening, route authorization review, and command audit persistence usage

### Requirement IDs covered

- `FR-IDN-031` - Verified tenant context requirements remain applied to every existing authoring content endpoint.
- `FR-RBAC-001` - Verified explicit authoring permission requirements on create, read/list, review, approve, and reject routes.
- `FR-GOV-001` - Persisted successful lifecycle command audit records for request review, approve, and reject decisions.
- `FR-GOV-002` - Covered publishing audit persistence at the existing application command/repository boundary without adding a publishing endpoint.
- `FR-IDN-040` - Used the approved lifecycle/publishing history repository seams while preserving in-memory defaults and SQL Server opt-in behaviour.
- `NFR-SEC-1` - Kept API errors, command responses, and audit records structural and non-sensitive.
- `NFR-TEST-1` - Added route authorization, lifecycle audit persistence, and publishing audit boundary tests.

### Scope delivered

- Reviewed the existing `/authoring/content` route authorization mapping.
- Preserved the existing permission mapping:
  - create: `Authoring.Manage`;
  - get/list: `Authoring.Read`;
  - request review: `Authoring.Review`;
  - approve: `Authoring.Approve`;
  - reject/return to draft: `Authoring.Review`.
- Persisted lifecycle history records through `IAuthoredContentLifecycleHistoryRepository` only when workflow decisions succeed.
- Kept failed, denied, and invalid lifecycle commands non-persistent in this slice.
- Added test coverage that missing tenant context is denied across all existing authoring content routes.
- Added test coverage for wrong-permission denial across create, get, and list routes.
- Added test coverage that successful request-review, approve, and reject commands create exactly one lifecycle history record.
- Added test coverage that invalid lifecycle commands do not create lifecycle history records.
- Added publishing audit persistence coverage at the existing application command/repository boundary only.

### Assumptions

Publishing remains an application-layer workflow and repository boundary in this slice. No publishing API endpoint is added.

Lifecycle and publishing history records remain structural metadata only. They do not store content body, tokens, headers, raw claims, form values, input values, tax data, HMRC data, property data, connection strings, secrets, credentials, or sensitive values.

Default behaviour remains in-memory. EF/durable persistence remains explicitly opt-in through the existing SQL Server persistence configuration.

### Explicitly not built

- EF migrations.
- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Production Azure SQL deployment.
- Deployment automation.
- Real SQL Server connectivity checks.
- Live health checks.
- Appsettings changes.
- Publishing API endpoint.
- Studio UI.
- Runtime renderer.
- Delivery API.
- Analytics pipeline.
- AI assistant.
- Event Hubs or ClickHouse.
- Browser extension.
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
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
```

### Known limitations

- Publishing audit persistence is covered only at the application command/repository boundary because no publishing endpoint exists yet.
- Lifecycle command audit persistence is written after successful workflow decisions only.
- No real database has been created or modified.
- No live SQL Server connectivity checks exist.
- No production deployment automation exists.
- Database-level tenant isolation and RLS-style hardening remain future work.

### Next recommended slice

Add controlled publishing API contract hardening or operational diagnostics around persistence readiness, while keeping migration execution and production database mutation separately approval-gated.

## Slice 4 - Controlled publishing API contract hardening

### Requirement IDs covered

- `FR-IDN-031` - Added tenant-required publishing API boundary for authored content versions.
- `FR-RBAC-001` - Enforced the existing `Authoring.Publish` permission key on the publishing route.
- `FR-GOV-002` - Added a controlled publish command endpoint that uses the existing publishing workflow and persists successful publishing history records.
- `FR-IDN-040` - Used the approved publishing history repository seam while preserving in-memory defaults and SQL Server opt-in behaviour.
- `NFR-SEC-1` - Kept publish responses, errors, and persisted audit records minimal, structural, and non-sensitive.
- `NFR-TEST-1` - Added integration coverage for publish authorization, invalid command failure, cross-tenant hiding, safe response shape, and publishing history persistence.

### Scope delivered

- Added `POST /authoring/content/{contentId}/versions/{versionId}/publish`.
- Required tenant context through the existing tenant endpoint filter.
- Required `AdoptaPermissionKeys.AuthoringPublish`.
- Added safe publishing request and response DTOs.
- Used `AuthoredContentPublishingWorkflow` for validation and contract-only bundle mapping.
- Persisted `AuthoredContentPublishingAuditRecord` through `IAuthoredContentPublishingHistoryRepository` only after successful publish validation.
- Kept failed, denied, invalid, and cross-tenant publish commands non-persistent.
- Returned safe publish metadata only:
  - success/status;
  - bundle ID;
  - tenant/application IDs for the successful caller's tenant;
  - environment/channel;
  - version;
  - generated timestamp;
  - item count;
  - structural audit metadata;
  - typed issues.

### Assumptions

Publishing remains contract-only in this slice. The endpoint validates and maps to the existing runtime bundle contract, but it does not deliver or externally publish the bundle.

Publishing history records remain structural metadata only. They do not store content body, raw authored content, tokens, headers, raw claims, form values, input values, tax data, HMRC data, property data, connection strings, secrets, credentials, or sensitive values.

Default behaviour remains in-memory. EF/durable persistence remains explicitly opt-in through the existing SQL Server persistence configuration.

### Explicitly not built

- EF migrations.
- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Production Azure SQL deployment.
- Deployment automation.
- Real SQL Server connectivity checks.
- Live health checks.
- Appsettings changes.
- Delivery API.
- CDN publishing.
- Blob Storage publishing.
- Runtime bundle external storage.
- Runtime renderer behaviour.
- External publishing transport.
- Studio UI.
- Analytics pipeline.
- AI assistant.
- Event Hubs or ClickHouse.
- Browser extension.
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
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
```

### Known limitations

- Publishing remains contract-only and does not write bundles to a delivery store.
- No delivery API, CDN, Blob Storage, renderer, or external publishing side effect exists.
- Migration execution and production database mutation remain separately approval-gated.
- Database-level tenant isolation and RLS-style hardening remain future work.

### Next recommended slice

Complete Sprint 5 with a final production-readiness closeout slice covering route/security guardrails, persistence enablement checklist status, and release-readiness documentation without executing migrations or adding deployment automation.

## Slice 5 - Final production-readiness closeout

### Requirement IDs covered

- `FR-IDN-030` - Documented controlled persistence enablement status and outstanding database enablement gates.
- `FR-IDN-031` - Documented tenant isolation status and future database-level hardening needs.
- `FR-IDN-040` - Documented durable audit/history foundation status and operational constraints.
- `FR-RBAC-001` - Documented API route authorization checklist and existing authoring/publishing permission coverage.
- `FR-GOV-001` - Documented lifecycle history persistence status.
- `FR-GOV-002` - Documented publishing API contract limitations and publishing history status.
- `NFR-SEC-1` - Documented safe response, secret handling, migration approval, and no-sensitive-data constraints.
- `NFR-TEST-1` - Added non-invasive documentation guardrail tests for production-readiness closeout documents.

### Scope delivered

- Added production-readiness closeout documentation.
- Added controlled persistence enablement checklist.
- Added API security and route authorization checklist.
- Added migration execution approval checklist.
- Updated the documentation index.
- Added documentation guardrail tests for closeout coverage and secret-marker checks.

### Sprint 5 closeout status

Sprint 5 is complete at the controlled production-enablement foundation level.

Production-ready foundation exists for:

- reviewable EF schema baseline source;
- disabled-by-default persistence;
- SQL Server opt-in validation;
- tenant-scoped repository boundaries;
- durable audit/history repository seams;
- authoring API route authorization;
- controlled publishing API contract;
- safe structural audit/history records;
- migration and operational readiness documentation;
- guardrail tests for migration/database mutation boundaries.

The following are not production-enabled:

- migration execution;
- database creation;
- automatic startup migration;
- production Azure SQL deployment;
- deployment automation;
- live SQL Server connectivity checks;
- database-level tenant isolation controls;
- delivery API;
- CDN or Blob Storage publishing;
- runtime renderer;
- analytics pipeline;
- AI assistant;
- Event Hubs or ClickHouse;
- browser extension;
- Property MTD integration.

### Assumptions

Migration execution is not approved by this slice. Production database enablement remains a future approval-gated activity.

Repository files must not contain real connection strings, hostnames, passwords, tokens, tenant secrets, credentials, or environment-specific secret values.

All production operations must continue to distinguish implemented foundation behaviour from future production enablement steps.

### Explicitly not built

- New product features.
- New API endpoints.
- Route behaviour changes.
- Appsettings changes.
- EF model changes.
- EF migrations.
- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Startup database mutation.
- Production Azure SQL deployment.
- Deployment automation.
- Real SQL Server calls.
- Live health checks.
- Runtime SDK changes.
- CI changes.
- Studio UI.
- Runtime renderer.
- Delivery API.
- Analytics pipeline.
- AI assistant.
- Event Hubs or ClickHouse.
- Browser extension.
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
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
```

### Known limitations

- Production database migration execution remains unapproved.
- SQL Server persistence is opt-in and not production-enabled by repository files.
- Publishing remains contract-only and does not write bundles to a delivery store.
- Database-level tenant isolation and row-level security remain future work.
- Production deployment automation remains future work.

### Next recommended sprint direction

After Sprint 5 closeout, the next work should be explicitly planned as a new approved phase. Recommended options are:

- approval-gated non-production migration execution and validation;
- delivery API and runtime bundle retrieval foundation;
- runtime renderer foundation;
- database-level tenant isolation hardening;
- production observability implementation after privacy and logging review.
