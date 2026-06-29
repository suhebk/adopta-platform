# ADOPTA-SPRINT-4 - Production Persistence, Tenant Isolation Hardening, and Operational Readiness Foundations

## Sprint intent

Sprint 4 hardens Adopta's production persistence and operational foundations while preserving the Sprint 1-3 tenant isolation, RBAC, audit, runtime, and authoring seams.

## Slice 1 - EF Core persistence model foundation

### Requirement IDs covered

- `FR-IDN-030` - Added EF Core model configuration for tenant-owned records so future durable storage can carry tenant ownership.
- `FR-IDN-031` - Added tenant-scoped indexes and model tests that support future repository/database isolation enforcement.
- `FR-IDN-040` - Added EF model configuration for audit and security audit records.
- `FR-GOV-001` - Added authored content lifecycle/version persistence model foundation.
- `FR-GOV-002` - Added authored content version model configuration for future environment/version governance.
- `NFR-SEC-1` - Persistence configuration is disabled by default and does not include real secrets or connection strings.
- `NFR-TEST-1` - Added EF model and persistence configuration tests.

### Scope delivered

- Added Infrastructure-only EF Core package references for EF Core and the SQL Server provider.
- Added `AdoptaDbContext`.
- Added EF configuration classes for:
  - tenants;
  - tenant applications;
  - adoption users;
  - roles;
  - permissions;
  - tenant mappings;
  - authenticated user mappings;
  - authored content items;
  - authored content versions;
  - audit events;
  - security audit events.
- Added `AdoptaPersistenceOptions`.
- Added disabled-by-default API persistence configuration.
- Added SQL Server DbContext registration only when persistence is explicitly enabled and configured.
- Preserved all existing in-memory repository registrations and behavior.

### EF and persistence assumptions

The EF model is a foundation for future durable repositories. It is not the active persistence path in this slice.

Persistence remains disabled by default. If SQL Server persistence is enabled without a provider, connection string name, or connection string, startup configuration fails with a generic non-sensitive error.

`AuthoredContentVersion` remains part of the authored content aggregate and is configured as an owned collection. Its tenant ownership is represented in the EF model to support future tenant-isolated storage rules without changing the domain contract in this slice.

### Explicitly not built

- EF migrations.
- Migration execution.
- Repository replacement.
- Production Azure SQL deployment.
- Automatic database creation.
- Automatic migration on startup.
- Production database infrastructure.
- Health/readiness checks.
- Durable audit implementation beyond model configuration.
- Full Adoption Studio UI or content editor.
- Runtime renderer.
- AI assistant.
- Analytics pipeline.
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
```

### Known limitations

- EF model configuration exists, but durable repositories are not implemented yet.
- SQL Server persistence is disabled by default.
- No migrations, DbContext factory, connection strings, database creation, automatic migration, or production deployment configuration exists.
- Existing in-memory repositories remain the active persistence implementation.
- Database-level tenant enforcement and RLS-style hardening are deferred to later Sprint 4 slices.

### Next recommended slice

Add durable repository implementations behind explicit configuration, preserving the in-memory default until migration and operational readiness decisions are approved.

## Slice 2 - Tenant-isolated EF repositories behind explicit configuration

### Requirement IDs covered

- `FR-IDN-030` - Added EF-backed repository implementations for selected tenant-owned persistence seams.
- `FR-IDN-031` - Enforced tenant isolation at EF repository query/write boundaries and added repository-level tenant isolation tests.
- `FR-IDN-040` - Added EF-backed security audit repository seam while preserving the in-memory default.
- `FR-GOV-001` - Added EF-backed authored content repository seam for governed authored content.
- `FR-GOV-002` - Added EF-backed authored content version persistence through the authored content aggregate.
- `NFR-SEC-1` - EF repositories remain opt-in, avoid real secrets, and return safe tenant access denial errors.
- `NFR-TEST-1` - Added EF repository registration and tenant-isolation integration tests.

### Scope delivered

- Added EF-backed implementations for approved repository seams only:
  - `IAuthoredContentRepository`;
  - `ITenantMappingRepository`;
  - `IAuthenticatedUserMappingRepository`;
  - `ISecurityAuditEventRepository`.
- Preserved existing in-memory repositories and default disabled persistence behavior.
- Registered EF repositories only when SQL Server persistence is explicitly enabled and fully configured.
- Kept non-target repositories on in-memory implementations in this slice.
- Added test-only EF InMemory provider to integration tests.
- Added EF repository tenant-isolation tests for authored content, tenant mappings, authenticated user mappings, and security audit records.

### EF repository assumptions

The EF repositories are an opt-in durable persistence path for selected seams only. They do not run unless `Persistence:Enabled=true`, `Persistence:Provider=SqlServer`, and the configured SQL Server connection string name resolves to a non-empty connection string.

Service registration configures `AdoptaDbContext` but does not connect to a database, create a database, run migrations, or perform readiness checks.

Repository methods enforce tenant context before reads and writes. Cross-tenant reads are hidden or denied safely, and cross-tenant writes fail closed using the existing tenant access denial pattern.

The authenticated user mapping EF repository stores a scalar user ID through the EF persistence entity and translates back to the existing application-layer mapping record.

### Explicitly not built

- EF migrations.
- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Production Azure SQL deployment.
- Health/readiness checks.
- Full Adoption Studio UI or content editor.
- Runtime renderer.
- AI assistant.
- Analytics pipeline.
- Event Hubs or ClickHouse.
- Browser extension.
- Property MTD integration.
- Large-scale infrastructure automation.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md
```

### Known limitations

- EF repositories are implemented only for selected seams.
- SQL Server persistence remains disabled by default.
- No migrations, database creation, automatic migration, health checks, or production deployment configuration exists.
- Tenant/application/user/role/general audit repositories remain in-memory unless explicitly targeted by a later slice.
- Database-level tenant isolation hardening remains future work.

### Next recommended slice

Add migration strategy and operational persistence validation planning, including how tenant isolation will be enforced at the database boundary without enabling automatic migrations or production database deployment prematurely.
