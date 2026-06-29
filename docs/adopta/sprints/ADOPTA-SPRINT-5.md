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
