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

## Slice 3 - Migration strategy and operational persistence validation planning

### Requirement IDs covered

- `FR-IDN-030` - Documented migration planning requirements for tenant-owned persisted records.
- `FR-IDN-031` - Documented database-boundary tenant isolation design and future hardening options.
- `FR-IDN-040` - Documented audit/security audit persistence validation expectations.
- `FR-GOV-002` - Documented environment promotion and rollback strategy for future schema changes.
- `NFR-SEC-1` - Documented safe secret handling, no startup migration, and approval-gated operational controls.
- `NFR-TEST-1` - Added migration-readiness guardrail tests.

### Scope delivered

- Added persistence migration strategy documentation.
- Added database-boundary tenant isolation design documentation.
- Added persistence operations runbook.
- Updated the Adopta documentation index.
- Added guardrail tests for:
  - no migration execution calls;
  - no automatic database creation calls;
  - no EF Design package reference;
  - disabled-by-default persistence config;
  - no real connection strings in appsettings;
  - required persistence documentation sections.

### Migration and operational assumptions

This slice is planning and validation only. It does not generate migrations, add EF Design tooling, execute migrations, connect to a database, create databases, or deploy production infrastructure.

Future migrations must be explicit, manual, reviewed, approval-gated, and executed outside normal application startup. Production secrets must come from secure configuration such as Key Vault or equivalent, not repository files.

Database-level tenant isolation hardening is documented as a future defense-in-depth layer. It does not replace current application-level tenant filtering or EF repository boundary checks.

### Explicitly not built

- EF migrations.
- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Production Azure SQL deployment.
- Health/readiness checks.
- Repository changes.
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
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests
```

### Known limitations

- No EF migration files exist yet.
- No EF Design package is referenced.
- No database-level tenant isolation policy or RLS implementation exists.
- No operational automation or deployment integration exists.
- Migration commands remain placeholders pending a future approved migration slice.

### Next recommended slice

Add persistence configuration validation and operational readiness contracts, still without implementing health checks, migration execution, production database deployment, or automatic startup database mutation.

## Slice 4 - Persistence configuration validation and operational readiness contracts

### Requirement IDs covered

- `FR-IDN-030` - Added explicit persistence configuration validation contracts for tenant-owned persistence settings.
- `FR-IDN-031` - Preserved tenant-isolated repository opt-in boundaries while centralising SQL Server persistence validation.
- `FR-GOV-002` - Added safe operational readiness contract states for disabled, invalid, and configured-but-not-checked persistence.
- `NFR-SEC-1` - Ensured validation failures and startup errors do not expose connection string values, credentials, tokens, headers, claims, tenant secrets, hostnames, or sensitive values.
- `NFR-TEST-1` - Added validation/readiness tests and kept migration guardrail tests in place.

### Scope delivered

- Added typed persistence validation result and issue contracts.
- Added safe persistence validation issue codes and non-sensitive messages.
- Added a centralised persistence configuration validator.
- Updated infrastructure DI to use centralised validation while preserving existing behaviour:
  - disabled/default persistence continues to use in-memory repositories;
  - EF repositories remain opt-in only;
  - `AdoptaDbContext` is registered only when SQL Server persistence is explicitly enabled and valid;
  - invalid enabled persistence fails with a generic non-sensitive configuration error.
- Added operational readiness contract states:
  - disabled;
  - invalid configuration;
  - configured but connectivity not checked.
- Added tests for disabled/default validation, valid/invalid SQL Server settings, safe error messages, and readiness result shape.

### Configuration and readiness assumptions

Persistence remains disabled by default. SQL Server persistence remains explicitly opt-in through configuration and still requires a configured provider, connection string name, and configured connection string value.

This slice validates configuration shape only. It does not perform database connectivity checks, SQL Server calls, health checks, migrations, database creation, automatic migration on startup, or any startup database mutation.

Production secrets must not be stored in repository files. Production connection string values must come from secure configuration such as Key Vault or an equivalent secret store.

### Explicitly not built

- EF migrations.
- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Production Azure SQL deployment.
- Real database health checks.
- Real SQL Server connectivity checks.
- Repository replacement.
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
rg "Microsoft.EntityFrameworkCore.Design" src tests
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests
```

### Known limitations

- Readiness contracts are model-only and do not perform live database checks.
- SQL Server persistence remains opt-in and disabled by default.
- No migration files or EF Design package exist yet.
- Database-level tenant isolation policy and RLS-style hardening remain future work.
- Operational deployment automation remains future work.

### Next recommended slice

Add operational persistence readiness integration at the API/readiness boundary only after an approved design decides whether checks remain configuration-only or include a controlled, non-leaky database connectivity probe. Keep migrations, database creation, and production deployment automation separate from startup.

## Slice 5 - Operational readiness foundation and Sprint 4 closeout

### Requirement IDs covered

- `FR-IDN-031` - Added tenant-isolation operational validation guidance for persistence and repository boundaries.
- `FR-IDN-040` - Added audit and security audit operational conventions for safe structural metadata handling.
- `FR-GOV-002` - Added deployment readiness, rollback, incident-response, and approval-gate documentation.
- `NFR-SEC-1` - Added safe logging, observability, and secret-handling guidance.
- `NFR-TEST-1` - Added documentation guardrail tests for operational readiness completeness.

### Scope delivered

- Added operational readiness documentation.
- Added observability and safe logging guidance.
- Added incident-response and rollback guidance.
- Added tenant-isolation validation checklist.
- Updated the Adopta documentation index.
- Added documentation guardrail tests for:
  - required operations docs;
  - deployment, rollback, incident response, tenant isolation, persistence, audit, logging, secrets, and approval gates;
  - current implemented behaviour versus future production steps;
  - Sprint 4 closeout checklist presence;
  - absence of obvious secret-marker examples in operations docs.

### Operational assumptions

Persistence remains disabled by default. SQL Server persistence remains opt-in through explicit configuration and secure secret sourcing.

Operational documentation clearly separates current implemented behaviour from future production steps. Future EF migrations, database creation, live database health checks, production Azure SQL deployment, and deployment automation require explicit approval.

### Explicitly not built

- EF migrations.
- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Production Azure SQL deployment.
- Live database health checks.
- Real SQL Server calls.
- Repository replacement.
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
rg "Microsoft.EntityFrameworkCore.Design" src tests
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests
```

### Known limitations

- Sprint 4 remains an operational-readiness foundation, not production database deployment.
- No EF migration files or EF Design package exist yet.
- No live database health checks or SQL Server connectivity checks exist.
- No production Azure SQL deployment automation exists.
- Database-level tenant isolation and RLS-style hardening remain future work.

### Sprint 4 closeout checklist

- Persistence disabled by default: complete.
- SQL Server persistence remains opt-in: complete.
- EF model foundation added without migrations: complete.
- Selected EF repositories are tenant-isolated and opt-in: complete.
- Migration strategy and persistence guardrails documented: complete.
- Persistence configuration validation and readiness contracts added: complete.
- Operational readiness, observability, incident response, rollback, and tenant-isolation validation docs added: complete.
- No production Azure SQL deployment: complete.
- No live database health checks: complete.
- No automatic database creation or startup migration: complete.
- No real secrets or connection strings in repository files: complete.

### Sprint 4 closeout status

Sprint 4 is ready to close at the persistence, tenant-isolation hardening, and operational-readiness foundation level once verification passes. Remaining production-readiness work belongs in the next approved sprint or slice.
