# Persistence Enablement Checklist

## Purpose

This checklist defines the required approval gates before enabling SQL Server persistence outside the current in-memory default path.

Persistence remains disabled by default. SQL Server persistence remains opt-in only.

## Current Status

- In-memory repositories remain the default.
- SQL Server persistence registration is enabled only through explicit valid configuration.
- Migration execution is not approved by Sprint 5 Slice 5.
- No automatic migration, database creation, or startup database mutation is approved.

## Pre-Enablement Checklist

- Confirm the target environment is approved for persistence testing.
- Confirm persistence enablement is approved by the product and engineering owner.
- Confirm production secrets are stored in secure configuration, not repository files.
- Confirm no real connection strings, hostnames, passwords, tokens, tenant secrets, or credentials are committed.
- Confirm rollback ownership and incident-response contacts.
- Confirm tenant isolation validation has a named owner.
- Confirm audit/history data retention expectations.
- Confirm the migration execution approval checklist is complete.
- Confirm release-readiness tests pass.

## Configuration Checklist

- `Persistence:Enabled` must remain `false` by default.
- SQL Server persistence must be explicitly opt-in.
- Provider must be `SqlServer` only when approved.
- Connection string name must be configured without exposing the value.
- Connection string value must be supplied by secure configuration.
- App startup must not create, migrate, or mutate the database.

## Operational Checklist

- No automatic migrations.
- No automatic database creation.
- No startup database mutation.
- No live database health checks until separately approved.
- No deployment automation until separately approved.
- No production Azure SQL deployment until separately approved.
- Rollback plan must be rehearsed before production execution.
- Incident response must include safe tenant-isolation triage.

## Tenant Isolation Checklist

- Missing tenant context fails closed.
- Cross-tenant reads are hidden or denied safely.
- Cross-tenant writes fail closed.
- Tenant-scoped indexes support repository access patterns.
- Future database-level tenant isolation controls are tracked separately.

## Secret Handling

Secrets must not be stored in repository files. This includes connection strings, passwords, tokens, tenant secrets, credentials, and environment-specific values.

Use safe placeholders in documentation and tests.
