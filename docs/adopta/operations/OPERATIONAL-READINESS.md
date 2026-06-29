# Operational Readiness

## Purpose

This document defines the Sprint 4 operational-readiness foundation for Adopta. It separates current implemented behaviour from future production operations that still require explicit approval.

## Current Implemented Behaviour

- Persistence is disabled by default.
- SQL Server persistence is explicitly opt-in through configuration.
- In-memory repositories remain the default path.
- Persistence configuration validation is implemented.
- Persistence readiness contracts are model-only and do not perform live database connectivity checks.
- No automatic startup migration is allowed.
- No automatic database creation is allowed.
- No production Azure SQL deployment automation exists.
- No real database health checks are implemented in this slice.

## Deployment Readiness Checklist

- Confirm the target build and source revision.
- Confirm `dotnet test Adopta.slnx` passes.
- Confirm release build and release tests pass.
- Confirm TypeScript typecheck, build, and tests pass.
- Confirm persistence remains disabled unless an approved change enables SQL Server persistence.
- Confirm no repository file contains production secrets, credentials, tokens, tenant secrets, real hostnames, or real connection strings.
- Confirm migration execution is not part of application startup.
- Confirm rollback owner and communication path are known before deployment.
- Confirm tenant-isolation validation steps are scheduled for any persistence-affecting change.

## Persistence Operations

Persistence operations must be approval-gated. SQL Server persistence may be enabled only through secure configuration and only after the relevant environment has been reviewed.

Production secrets must come from a secure configuration provider such as Key Vault or an equivalent secret store. Repository files must not contain production connection strings, passwords, tokens, tenant secrets, hostnames, or credentials.

## Audit Operational Conventions

- Audit and security audit records must remain structural and non-sensitive.
- Audit operations must not record tokens, headers, raw claims, credentials, form values, tax data, HMRC data, property data, or sensitive user-entered values.
- Future durable audit storage must preserve tenant ownership and tenant-scoped access controls.
- Audit retention, export, and deletion policies require a future approved operations slice.

## Approval Gates

The following activities require explicit approval before implementation or execution:

- EF migration creation.
- Migration execution.
- Production database creation.
- Production Azure SQL deployment.
- Enabling SQL Server persistence in a shared or production environment.
- Adding live database connectivity health checks.
- Adding deployment automation that can mutate infrastructure or data.

## Out Of Scope For Sprint 4 Slice 5

- EF migrations.
- Migration execution.
- Automatic database creation.
- Automatic migration on startup.
- Production Azure SQL deployment.
- Live database health checks.
- Real SQL Server calls.
- Repository replacement.
- Large-scale infrastructure automation.
