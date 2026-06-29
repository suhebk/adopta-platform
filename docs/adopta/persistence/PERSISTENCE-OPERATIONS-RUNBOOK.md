# Persistence Operations Runbook

## Purpose

This runbook defines safe operational guidance for future Adopta persistence work. It is planning guidance only and must not be interpreted as approval to run migrations or modify databases.

## Safety Rules

- No automatic migration on application startup.
- Migration execution requires manual approval.
- Migration execution must happen outside normal app startup.
- Rollback must be rehearsed and approval-gated.
- Production secrets must come from secure configuration such as Key Vault or equivalent, not repository files.
- Do not store real connection strings, passwords, tokens, tenant secrets, or environment-specific hostnames in source.
- Do not run database creation or migration commands without an approved change.

## Future Manual Migration Flow

1. Review the migration proposal and schema diff.
2. Confirm tenant-isolation impact.
3. Confirm backup or restore-point readiness.
4. Confirm rollback approach.
5. Obtain manual approval.
6. Execute in dev.
7. Validate application and tenant-isolation tests.
8. Promote to QA after approval.
9. Rehearse rollback in QA.
10. Promote to production after production change approval.

## Placeholder Command Examples

These commands are examples only. They must not be run until a future migration slice explicitly approves EF migrations and operational execution.

```powershell
# Future/manual/approval-gated example only.
dotnet ef migrations script <FromMigration> <ToMigration> --idempotent --project src/Adopta.Infrastructure --startup-project src/Adopta.Api --context AdoptaDbContext --output <reviewed-script-path>
```

```powershell
# Future/manual/approval-gated example only.
# Run only through an approved deployment/change process with secure configuration.
sqlcmd -S <approved-server> -d <approved-database> -i <reviewed-script-path>
```

The placeholders above must be replaced only by approved environment-specific values from secure operational configuration.

## Pre-Execution Checklist

- Approved change record exists.
- Migration script is reviewed.
- Tenant-isolation impact is documented.
- Backup or restore point is validated.
- Rollback plan is rehearsed.
- Application version compatibility is confirmed.
- Secrets are sourced from secure configuration.
- No startup migration path exists.

## Post-Execution Validation

- Verify migration completion.
- Verify application startup.
- Verify tenant-isolation tests.
- Verify representative tenant-scoped reads and writes.
- Verify audit/security audit behavior.
- Verify no sensitive values appear in logs or errors.

## Configuration Validation and Readiness Contracts

Persistence configuration validation is a startup/configuration guardrail only. It verifies whether persistence is disabled or whether explicit SQL Server opt-in configuration is present.

The current readiness contract can report:

- disabled;
- invalid configuration;
- configured but connectivity not checked.

No database connectivity is performed by the Slice 4 readiness contracts. No SQL Server calls, database health checks, migrations, database creation, or startup database mutation are allowed in this slice.

SQL Server persistence remains opt-in. Production secrets, connection string values, credentials, tenant secrets, tokens, and host details must come from secure configuration such as Key Vault or equivalent, not repository files.

## Rollback

Rollback must be approval-gated and rehearsed before production. The rollback plan should specify:

- whether to restore from backup or execute a reviewed rollback script;
- data preservation expectations;
- application version compatibility;
- operational owner;
- validation steps after rollback.

## Explicit Non-Goals

- No migration execution in this slice.
- No automatic database creation.
- No automatic migration on startup.
- No production Azure SQL deployment.
- No health/readiness check implementation.
- No real connection strings or secrets in repository files.
