# Incident Response and Rollback

## Purpose

This document defines the Sprint 4 incident-response and rollback foundation. It is operational planning guidance only and does not approve database changes, deployment automation, or production infrastructure mutation.

## Current Implemented Behaviour

- No automatic startup migration is allowed.
- No automatic database creation is allowed.
- No migration execution is implemented.
- No production Azure SQL deployment automation is implemented.
- No real database connectivity checks are implemented in this slice.
- SQL Server persistence remains opt-in.

## Incident Response Checklist

1. Confirm incident scope and affected environment.
2. Preserve logs and audit/security audit records without exposing sensitive values.
3. Identify whether the issue affects configuration, tenant isolation, persistence access, authorization, or application availability.
4. Confirm whether persistence is disabled or explicitly enabled in the affected environment.
5. Validate tenant isolation before and after any mitigation.
6. Avoid ad hoc database changes.
7. Escalate to the approved operational owner before any persistence-impacting action.
8. Communicate status using safe summaries that do not include secrets, credentials, tokens, connection strings, hostnames, raw claims, or tenant secrets.

## Rollback Checklist

- Confirm rollback trigger and approval.
- Confirm the target application version.
- Confirm configuration changes required for rollback.
- Confirm persistence state and whether SQL Server persistence is enabled.
- Confirm no startup database mutation path exists.
- Confirm tenant-isolation validation steps after rollback.
- Confirm audit/security audit review after rollback.
- Confirm production secrets remain in secure configuration only.

## Persistence Incident Rules

- Do not run unapproved schema changes.
- Do not create databases from application startup.
- Do not execute migrations from application startup.
- Do not use repository files for production credentials.
- Do not paste credentials, real hostnames, tokens, tenant secrets, or connection string values into incident notes.

## Future Production Steps

Future production rollback automation, database restore runbooks, and deployment automation require explicit approval. Those future steps must include manual gates, rehearsed rollback, secure secret sourcing, and tenant-isolation validation.
