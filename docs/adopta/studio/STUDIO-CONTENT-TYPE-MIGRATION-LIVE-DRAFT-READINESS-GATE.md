# Studio Content Type Migration And Live Draft Readiness Gate

This guide defines the readiness gate that must be satisfied before live Studio draft create/update integration can proceed.

Live draft create/update must not proceed until content type migration/backfill readiness is approved.

This slice does not execute migrations, mutate databases, add startup migration logic, add deployment automation, wire live draft create/update, wire live review/approve/reject/publish, or add Property MTD integration.

## Current Dependency

The authored content type source of truth is implemented in domain, API, validation, runtime mapping, Web live-read mapping, EF model configuration, and review-only migration source.

The migration source is:

- `src/Adopta.Infrastructure/Persistence/Migrations/20260702000100_AddAuthoredContentType.cs`.

This migration source remains review-only. It is not an execution approval.

## Gate Decision

Live draft create/update is blocked until all of these conditions are met:

- migration source review is complete;
- migration execution receives separate operational approval;
- backfill plan is approved;
- rollback plan is approved;
- no startup migration path exists;
- no automatic database creation path exists;
- no deployment automation has been added for this change;
- live workflow and publish wiring remains out of scope;
- production secrets and real environment values remain outside repository files.

## Migration Source Review

Reviewers must confirm:

- the migration only targets `AuthoredContentItems`;
- the migration adds the `ContentType` column;
- the column uses a controlled bounded string mapping;
- the transition default is reviewed as a schema transition mechanism, not as business inference;
- the migration does not include sensitive values;
- the migration does not add tenant IDs from browser, page, or request models;
- the migration does not add automatic execution or startup mutation.

## Separate Operational Approval

Migration execution must require a separate operational approval.

The approval package should include:

- target environment identifier handled outside repository files;
- migration review sign-off;
- backfill plan sign-off;
- rollback plan sign-off;
- change window ownership;
- validation checklist;
- evidence that live draft create/update remains blocked until readiness is confirmed.

No appsettings changes are required for this readiness gate.

## Backfill Plan Requirement

Existing authored content without authoritative content type must remain unknown/unavailable until a controlled backfill plan is approved.

The approved backfill plan must:

- use tenant-scoped inventory review;
- use only authoritative approved metadata;
- avoid content body inspection;
- avoid sensitive value exposure;
- preserve tenant isolation;
- leave records unknown/unavailable when no authoritative type exists;
- validate that new content still requires content type.

Content type must not be inferred from:

- content key;
- title;
- route;
- selector;
- UI fallback;
- runtime delivery metadata;
- weak naming patterns.

## Rollback Plan Requirement

Rollback planning must be approved before execution.

The rollback plan must:

- define the rollback decision owner;
- define the rollback window;
- preserve tenant isolation;
- avoid content body or sensitive value exposure;
- avoid startup database mutation;
- be rehearsed before production use.

## Live Draft Readiness Checks

Before live draft create/update can be implemented, verify:

- `AuthoredContentItem.ContentType` remains item-level source of truth;
- content type remains required for new content;
- live authoring API read responses return valid content type when known;
- Web mapper marks missing or invalid content type with `HasKnownContentType=false`;
- schema/backfill execution has been approved or deferred with a documented safe fallback;
- live workflow and publish integration remains out of scope for this gate.

## Fail-Closed Behaviour

If any gate item is not satisfied:

- live draft create/update must remain unavailable;
- live workflow and publish integration must remain unavailable;
- live read activation must remain explicit and disabled by default;
- missing or invalid content type must not be treated as authoritative;
- cross-tenant existence must remain hidden or safely denied.

## Security Guardrails

The readiness gate preserves these boundaries:

- no tenant IDs from browser, page, or request models;
- no `X-Adopta-Tenant-Id`;
- no `X-Adopta-Test-*` production shortcut;
- no tokens, headers, claims, secrets, connection strings, tenant values, raw exceptions, content body, or sensitive content;
- no new permission keys.

## Explicit Non-Goals

This slice does not approve:

- migration execution;
- database mutation;
- startup migration logic;
- automatic database creation;
- deployment automation;
- live draft create/update implementation;
- live review/approve/reject/publish integration;
- backend/API behaviour changes;
- EF/schema changes beyond the existing reviewed migration source;
- real appsettings values;
- analytics;
- AI;
- Event Hubs;
- ClickHouse;
- browser extension work;
- Property MTD integration.

## Exit Criteria For The Gate

This gate is ready only when the documentation, guardrail tests, and verification commands prove:

- the gate is explicit;
- the migration source remains review-only;
- the backfill and rollback plans are approval-gated;
- no execution or mutation paths were added;
- no live draft/workflow/publish wiring was added.
