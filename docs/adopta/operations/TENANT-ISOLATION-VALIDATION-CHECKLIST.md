# Tenant Isolation Validation Checklist

## Purpose

This checklist defines operational validation expectations for tenant isolation across persistence, authoring, security, and audit boundaries.

## Current Implemented Behaviour

- Tenant context is required for tenant-scoped flows.
- In-memory repositories enforce tenant boundaries.
- Selected EF repositories enforce tenant boundaries when SQL Server persistence is explicitly enabled.
- SQL Server persistence remains opt-in and disabled by default.
- Database-level tenant isolation hardening remains future work.

## Validation Checklist

- Confirm tenant context is required for tenant-scoped APIs.
- Confirm missing tenant context fails closed.
- Confirm tenant A cannot read tenant B authored content.
- Confirm tenant A cannot write tenant B authored content.
- Confirm tenant mapping lookups are tenant-filtered.
- Confirm authenticated user mapping lookups are tenant-filtered.
- Confirm security audit writes and reads preserve tenant boundaries.
- Confirm cross-tenant denials do not reveal whether another tenant's record exists.
- Confirm validation errors do not expose tenant secrets, claims, tokens, headers, credentials, or connection string values.
- Confirm audit/security audit records contain structural metadata only.

## Persistence Boundary Checks

For any future persistence change:

- Verify tenant-owned records include tenant ownership.
- Verify repository reads filter by current tenant.
- Verify writes reject missing or mismatched tenant context.
- Verify cross-tenant reads are hidden or denied safely.
- Verify cross-tenant writes fail closed.
- Verify database-level hardening options are reviewed before production use.

## Future Database-Level Hardening

Future SQL Server hardening may include additional constraints, indexes, policies, row-level security considerations, and operational validation queries. These remain future production-readiness work and require explicit approval before implementation.
