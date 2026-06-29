# Tenant Isolation at the Database Boundary

## Purpose

This document defines the tenant-isolation design direction for Adopta's future database boundary. It complements the current application-level tenant isolation and EF repository enforcement.

## Current Application-Level Enforcement

Adopta currently enforces tenant isolation through:

- tenant context resolution before tenant-scoped operations;
- `TenantRepositoryGuard` checks for missing or mismatched tenant context;
- repository queries that filter by the current tenant;
- safe denial with `TenantAccessDeniedException`;
- integration tests for cross-tenant access denial.

Cross-tenant access must not reveal whether another tenant's record exists. Reads should return null or empty results where appropriate, or fail with a generic tenant access denial when the requested tenant is mismatched.

## EF Repository Boundary Enforcement

EF repositories must:

- require a current tenant context before tenant-scoped reads and writes;
- reject writes where the entity tenant differs from the current tenant;
- filter reads by the current tenant;
- avoid exposing other tenant identifiers in exception messages;
- avoid logging tokens, headers, raw claims, form values, HMRC data, tax data, property data, or sensitive values.

The EF repository boundary is necessary but not sufficient for production. Database-level hardening remains required before production use.

## Future SQL Server Hardening Options

Future SQL Server hardening should consider:

- required `TenantId` columns on tenant-owned tables;
- composite indexes beginning with `TenantId` for tenant-scoped queries;
- foreign keys that preserve tenant ownership across related data;
- restricted database permissions for application identities;
- separate operational identities for migration execution;
- data classification and auditing for sensitive columns if later introduced.

## Future Row-Level Security Considerations

SQL Server row-level security can provide an additional enforcement layer. A future RLS design should define:

- how the application sets tenant context for each database session;
- predicate functions for tenant-owned tables;
- block predicates for writes where tenant context does not match;
- migration and operations behavior under RLS;
- tests proving cross-tenant reads and writes are denied at the database layer.

RLS should be treated as defense in depth, not a replacement for application-level tenant checks.

## Safe Denial and Hiding Behavior

The platform should use safe denial semantics:

- missing tenant context fails closed;
- mismatched requested tenant fails closed;
- cross-tenant reads do not disclose existence;
- cross-tenant writes fail closed;
- errors remain generic and non-sensitive.

No error or audit output should include connection strings, tokens, headers, raw claims, tenant secrets, form values, HMRC data, tax data, property data, or sensitive values.

## Audit and Operational Validation

Future operational validation should confirm:

- tenant-isolation tests run before release;
- database scripts preserve tenant ownership;
- audit records are structural and tenant-scoped;
- migration review includes tenant-isolation impact;
- production database-boundary changes require explicit manual approval;
- database-boundary changes must not run automatically during application startup;
- rollback plans preserve tenant boundaries and are rehearsed before production;
- production failures are safe and non-sensitive.

## Explicit Non-Goals

- No RLS implementation in this slice.
- No database policy creation.
- No production database deployment.
- No migration generation or execution.
- No connection string or secret storage in repository files.
