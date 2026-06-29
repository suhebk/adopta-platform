# Schema Baseline Review

## Purpose

This document records the review scope for the initial Adopta EF schema baseline. It is a review artifact only. Migration execution is not approved in this slice.

## Tables Included

The initial schema baseline targets the `adopta` schema and includes:

- `Tenants`
- `TenantApplications`
- `AdoptionUsers`
- `Roles`
- `Permissions`
- `AdoptionUserRoles`
- `RolePermissions`
- `TenantMappings`
- `AuthenticatedUserMappings`
- `AuthoredContentItems`
- `AuthoredContentVersions`
- `AuditEvents`
- `SecurityAuditEvents`

## Tenant-Owned Tables And Tenant Columns

Tenant-owned tables include non-null tenant ownership where appropriate:

- `TenantApplications.TenantId`
- `AdoptionUsers.TenantId`
- `Roles.TenantId`
- `AdoptionUserRoles.TenantId`
- `RolePermissions.TenantId`
- `TenantMappings.TenantId`
- `AuthenticatedUserMappings.TenantId`
- `AuthoredContentItems.TenantId`
- `AuthoredContentVersions.TenantId`
- `AuditEvents.TenantId`
- `SecurityAuditEvents.TenantId`

`Tenants` is the tenant root table and does not include a separate tenant-owner column.

## Important Indexes And Constraints

The schema baseline includes tenant-scoped indexes and keys for:

- authored content lookup by tenant, application, and content key;
- user lookup by tenant and external user ID;
- role lookup by tenant and role name;
- tenant mapping lookup by tenant, external tenant ID, and application ID;
- authenticated user mapping lookup by tenant and external subject ID;
- audit/security audit ordering by tenant and occurrence time;
- role/permission and user/role joins with tenant-scoped indexes.

Foreign keys use restrictive behaviour where cross-aggregate deletion should not cascade, and aggregate-owned version records cascade from authored content items.

## Migration Review Checklist

- Confirm tenant-owned tables include non-null tenant ownership.
- Confirm tenant-scoped indexes support repository filters.
- Confirm audit/security audit records remain structural and non-sensitive.
- Confirm no table stores tokens, headers, raw claims, form values, HMRC data, tax data, property data, or sensitive user-entered values.
- Confirm no real connection strings, passwords, tenant secrets, hostnames, or credentials are present in source.
- Confirm the migration source is reviewed before any future execution approval.

## Rollback Review Checklist

- Confirm rollback strategy before any future execution.
- Confirm whether rollback should use backup/restore or a reviewed reverse script.
- Confirm tenant-isolation impact of rollback.
- Confirm application version compatibility.
- Confirm operational owner approval.
- Rehearse rollback outside production before production execution.

## Approval Gates

The schema baseline source may be reviewed in this slice. The following remain separate approval-gated activities:

- migration execution;
- database creation;
- production Azure SQL deployment;
- automatic startup migration;
- live database health checks;
- production connection string or secret configuration.

## Explicit Non-Approval

Migration execution is not approved in this slice. The generated migration files are source artifacts for review only and must not be applied to any real database until a future approved slice authorizes execution.

## Slice 2 History Persistence Review

The `AddAuthoringHistoryPersistence` migration source adds review-only tables for durable history:

- `AuthoredContentLifecycleHistory`
- `AuthoredContentPublishingHistory`

Both tables are tenant-owned and include non-null `TenantId` columns. They store structural metadata only:

- content ID;
- version ID where applicable;
- actor user ID;
- lifecycle or publishing action metadata;
- environment/channel metadata for publishing;
- result;
- UTC occurrence timestamp.

They must not store content body, tokens, headers, raw claims, form values, input values, tax data, HMRC data, property data, secrets, credentials, or sensitive values.

The migration source includes tenant-scoped indexes for occurrence time and content/version lookup. Publishing history also includes a tenant/environment/channel index.

Migration execution remains not approved. The history migration is a source review artifact only and must not be applied to a real database until a future approved slice authorizes execution.
