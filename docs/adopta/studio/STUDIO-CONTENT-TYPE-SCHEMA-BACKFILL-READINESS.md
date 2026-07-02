# Studio Content Type Schema And Backfill Readiness

This guide defines the operational readiness boundary for applying the authored content type schema change introduced as review-only migration source in Sprint 11.

The migration source is present for review:

- `src/Adopta.Infrastructure/Persistence/Migrations/20260702000100_AddAuthoredContentType.cs`

Migration execution is not approved by Sprint 11 Slice 5. This slice does not create a database, mutate a database, run a migration, add startup migration logic, add deployment automation, or activate live Studio reads by default.

## Current Implemented State

Implemented:

- domain-owned `AuthoredContentType`;
- item-level `AuthoredContentItem.ContentType`;
- create/read API contract support;
- validation requiring content type for new authored content;
- runtime bundle mapping from authored content type;
- Web live-read mapper support for valid API content type;
- EF model configuration for `AuthoredContentItems.ContentType`;
- review-only migration source for the database column.

Not implemented by this slice:

- migration execution;
- database creation;
- database mutation;
- startup database mutation;
- production backfill execution;
- deployment automation;
- live read activation by default;
- live create/update/review/approve/reject/publish integration;
- Property MTD integration.

## Migration Review Boundary

The migration source must be reviewed before any environment execution is approved.

Reviewers should verify:

- the migration only changes `AuthoredContentItems`;
- the added column is `ContentType`;
- the mapped database type is controlled and bounded;
- the default value used for transition is intentional and documented;
- no sensitive values are stored;
- no tenant IDs are supplied by browser, page, or request models;
- no automatic migration execution is introduced;
- rollback is understood before execution.

Execution must happen outside normal application startup and only after explicit operational approval.

## Operational Approval Gates

Before applying the migration in any environment, operators must confirm:

- the target environment is identified through approved operational process, not repository files;
- secure configuration remains outside the repository;
- the deployment package has been reviewed;
- database backup or restore point strategy is approved;
- rollback owner and decision authority are assigned;
- backfill approach is approved;
- validation queries are prepared with safe structural checks only;
- application owners accept the fallback behaviour for legacy records;
- production execution has explicit change approval.

The approval record should state that automatic startup migration remains prohibited.

## Backfill Strategy

Existing authored content may predate the content type source-of-truth field. Backfill must be controlled and approval-gated.

Recommended staged approach:

1. Review current tenant-scoped authored content inventory using approved operational queries.
2. Classify records that already have an authoritative type from approved metadata.
3. Leave records without an authoritative source as unknown/unavailable until an approved owner assigns a type.
4. Apply a controlled backfill only after data owners approve the mapping.
5. Validate that new content creation still requires a valid content type.
6. Validate that Web live reads report known content type only when the API returns a valid value.

Content type must not be inferred from content key, title, route, selector, UI fallback, runtime delivery metadata, or weak naming patterns.

Content type remains required for new content.

The safe fallback for legacy reads remains:

- known valid API content type maps with `HasKnownContentType=true`;
- missing or invalid API content type maps with `HasKnownContentType=false`;
- no inference is performed.

## Rollback And Fail-Closed Expectations

Rollback planning must be completed before execution.

Rollback expectations:

- rollback requires manual approval;
- rollback must happen outside normal application startup;
- rollback must not expose content body or sensitive values;
- rollback must preserve tenant isolation;
- rollback must be rehearsed in a non-production environment before production use.

Fail-closed expectations:

- invalid persistence configuration fails safely;
- live Studio read activation remains disabled unless explicitly configured;
- live write/workflow/publish integration remains unavailable;
- cross-tenant existence must remain hidden or safely denied;
- missing or invalid content type must not be treated as authoritative.

## Pre-Execution Checks

Before execution is approved, verify:

- `dotnet test Adopta.slnx` passes;
- release build and release tests pass;
- TypeScript typecheck, build, and tests pass;
- migration/database mutation guardrail searches are clean for startup execution paths;
- appsettings files contain no real environment values;
- no deployment automation has been added for this change;
- no live write/workflow/publish API wiring has been added;
- no `X-Adopta-Tenant-Id` or `X-Adopta-Test-*` production shortcut has been introduced by Web production code;
- no production secrets are committed.

## Post-Execution Validation Checks

After any separately approved future execution, operators should validate:

- the `ContentType` column exists on `AuthoredContentItems`;
- new authored content requires a valid content type;
- read responses include valid content type for newly created content;
- legacy records without authoritative type are reported safely as unknown/unavailable where applicable;
- tenant-scoped reads remain tenant isolated;
- no content body or sensitive values are exposed in validation output;
- rollback criteria remain available until the change window closes.

## Explicit Non-Goals

Sprint 11 Slice 5 does not approve:

- automatic migration execution;
- deployment automation;
- schema mutation in this slice;
- database creation;
- database mutation;
- startup migration logic;
- live read activation by default;
- live create/update/review/approve/reject/publish integration;
- analytics;
- AI;
- Event Hubs;
- ClickHouse;
- browser extension work;
- Property MTD integration.

## Sprint 11 Closeout Position

Sprint 11 is ready to close at the read-contract hardening and schema/backfill readiness level.

Remaining production enablement requires a separately approved operational slice for migration execution planning, environment-specific change approval, and controlled backfill execution.

Sprint 12 adds the live draft readiness gate in `docs/adopta/studio/STUDIO-CONTENT-TYPE-MIGRATION-LIVE-DRAFT-READINESS-GATE.md`. That gate blocks live draft create/update integration until migration/backfill readiness is separately approved.
