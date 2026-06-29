# Sprint 5 Production Readiness Closeout

## Purpose

This document closes Sprint 5 at the controlled production-enablement foundation level. It records what is implemented, what is ready as a foundation, and what remains not production-enabled.

Sprint 5 does not approve migration execution, database creation, production Azure SQL deployment, startup database mutation, live database health checks, deployment automation, or production data mutation.

## Current Implemented State

- EF Core schema baseline source exists for review.
- EF Core design-time tooling is present as a private tooling dependency.
- SQL Server persistence remains explicitly opt-in.
- Persistence remains disabled by default.
- In-memory repositories remain the default local/test path.
- Tenant-isolated EF repository implementations exist for approved repository seams.
- Durable audit, security audit, lifecycle history, and publishing history repository seams exist.
- Authoring API route authorization has been hardened and tested.
- Lifecycle command audit records are persisted on successful lifecycle workflow decisions.
- A controlled publishing API contract endpoint exists and remains contract-only.
- Publishing history records are persisted on successful publish command validation.

## Production-Ready Foundation

The platform has foundation-level readiness for:

- explicit tenant context requirements on tenant-scoped authoring routes;
- explicit authoring permission requirements on authoring and publishing routes;
- safe non-sensitive API error responses;
- tenant-scoped repository boundaries;
- reviewable EF schema baseline source;
- controlled SQL Server persistence configuration validation;
- durable-history repository seams;
- migration planning and approval documentation;
- operational readiness and incident-response documentation;
- automated guardrail tests for migration/database mutation boundaries.

## Not Production-Enabled Yet

The following are not production-enabled:

- migration execution;
- production database creation;
- automatic startup migration;
- automatic database creation;
- live SQL Server connectivity checks;
- production Azure SQL deployment;
- deployment automation;
- database-level tenant isolation hardening such as row-level security;
- runtime bundle delivery storage;
- CDN or Blob Storage publishing;
- delivery API implementation;
- full Adoption Studio UI;
- analytics pipeline;
- AI assistant;
- Event Hubs;
- ClickHouse;
- browser extension;
- Property MTD integration.

## Schema Baseline Status

The schema baseline and history migration source are review artifacts only. They are not approval to execute migrations or create a database.

Schema source currently covers tenant-owned tables, authored content, audit/security audit, lifecycle history, and publishing history. Tenant ownership columns and tenant-scoped indexes are present where required by the current foundation model.

## Migration Execution Status

Migration execution is not approved by Sprint 5 Slice 5.

No migration has been executed by this slice. No `Migrate`, `EnsureCreated`, `EnsureDeleted`, or `Database.Ensure*` startup mutation is approved or implemented.

Future migration execution requires a separate approval gate, reviewed target environment, rollback plan, tested backup/restore approach, and production secret handling outside repository files.

## Durable Audit And History Status

Durable repository seams exist for:

- general audit events;
- security audit events;
- authored content lifecycle history;
- authored content publishing history.

History records are structural metadata only. They must not store content body, raw authored content, tokens, headers, raw claims, form values, input values, tax data, HMRC data, property data, secrets, credentials, connection strings, or sensitive values.

## API Hardening Status

The authoring API foundation includes tenant and permission enforcement for authored content, lifecycle, and publishing routes. Cross-tenant access is hidden or safely denied without revealing whether another tenant's content exists.

API responses and errors are expected to remain minimal, typed where practical, and non-sensitive.

## Publishing API Contract Status

The publishing endpoint is contract-only. It validates an approved authored content version, maps it to a runtime bundle contract, and records structural publishing history on success.

It does not publish to a delivery API, CDN, Blob Storage, runtime renderer, external transport, or production delivery store.

## Persistence Opt-In Status

Persistence remains disabled by default. SQL Server persistence is opt-in only and must be enabled through explicit validated configuration.

Production secrets must come from secure configuration such as Key Vault or an equivalent approved secret store. Repository files must not contain real connection strings, passwords, tokens, tenant secrets, hostnames, credentials, or environment-specific secret values.

## Tenant Isolation Status

Tenant isolation is enforced at the current application and repository boundaries:

- missing tenant context fails closed;
- mismatched tenant context fails closed;
- cross-tenant reads are hidden or denied safely;
- cross-tenant writes fail closed;
- API responses must not reveal cross-tenant entity existence.

Future production hardening should add database-level isolation controls and operational validation over production-like environments.

## Known Limitations

- The EF migrations are reviewable source only and remain unexecuted.
- No production database exists from this slice.
- No live database readiness checks exist.
- No delivery bundle store exists.
- Publishing remains contract-only.
- Database-level tenant isolation remains future work.
- Production deployment automation remains future work.

## Next Recommended Sprint Direction

The next recommended sprint should focus on approval-gated production enablement and runtime delivery foundations:

- controlled migration execution planning in a non-production environment;
- database-level tenant isolation hardening design and validation;
- delivery API and bundle retrieval foundation;
- runtime renderer foundation;
- production observability implementation after logging and privacy review.
