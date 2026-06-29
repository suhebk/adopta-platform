# Adopta Persistence Migration Strategy

## Purpose

This document defines the migration strategy for Adopta's future SQL Server persistence path. It is planning guidance only. It does not create migrations, execute migrations, create databases, deploy infrastructure, or enable production persistence.

## Current Baseline

- Persistence is disabled by default.
- In-memory repositories remain the default application behavior.
- SQL Server persistence is opt-in through explicit configuration.
- EF repository implementations exist for selected seams only.
- A reviewable initial EF schema baseline migration exists, but execution is not approved.
- No startup migration, database creation, or schema mutation is allowed.

## Baseline Schema Approach

The first generated migration establishes a controlled schema baseline from the current `AdoptaDbContext` model. The baseline is documented in `SCHEMA-BASELINE-REVIEW.md` and must be reviewed as a schema contract before it is applied to any shared environment.

The baseline review should confirm:

- tenant-owned tables include non-null tenant ownership where appropriate;
- tenant-scoped indexes support repository-level tenant filters;
- audit and security audit tables store structural metadata only;
- authored content and version tables preserve lifecycle/version metadata;
- mapping tables preserve tenant boundaries;
- no tables include raw tokens, headers, raw claims, form values, HMRC data, tax data, property data, or sensitive values.

## Migration Generation Strategy

Migration generation is now approval-gated and limited to the initial schema baseline source. Migration execution remains a future, manually approved activity.

Future placeholder command shape:

```powershell
# Future/manual/approval-gated example only.
# Do not run until EF migrations are explicitly approved.
dotnet ef migrations add <ApprovedMigrationName> --project src/Adopta.Infrastructure --startup-project src/Adopta.Api --context AdoptaDbContext
```

This repository must not add migration execution or `dotnet ef` execution against a real database until that future slice is approved.

## Environment Promotion Flow

Migrations should move through environments in this order:

1. Local developer validation against an isolated disposable database.
2. Dev environment with manual approval.
3. QA environment with release review and rollback rehearsal.
4. Production with explicit change approval.

Each promotion must include:

- migration script review;
- tenant-isolation review;
- backup or restore-point confirmation;
- rollback plan confirmation;
- operational owner approval;
- verification steps before and after execution.

## Manual Approval Gates

Every schema migration requires manual approval before execution. Approval must cover:

- schema changes;
- data migration impact;
- expected downtime or online migration posture;
- tenant isolation impact;
- rollback strategy;
- operational monitoring plan.

No migration should execute automatically from normal application startup.

## Rollback Strategy

Rollback must be planned before a migration is approved. The rollback strategy should include:

- restore point or backup validation;
- downgrade script only where safe and reviewed;
- data preservation notes;
- application version compatibility;
- criteria for rollback versus forward fix;
- named approval owner.

Rollback must be rehearsed in a non-production environment before production use.

## Operational Persistence Validation

Before any future production migration, operators should validate:

- persistence remains explicitly configured for the target environment;
- secrets come from secure configuration such as Key Vault or equivalent;
- no connection strings or secrets are present in repository files;
- no application startup path runs migrations;
- tenant isolation tests pass;
- release build and tests pass;
- migration script has been reviewed and approved.

## Explicit Non-Goals

- No migration execution in this slice.
- No migration execution.
- No automatic database creation.
- No automatic migration on startup.
- No production Azure SQL deployment.
- No health/readiness checks.
- No repository replacement.
- No real connection strings or secrets in source.
