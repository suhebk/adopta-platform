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
