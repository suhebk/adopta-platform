# Studio Content Type Source-of-Truth Design

This document defines the source-of-truth model for authored content type.

Sprint 11 Slice 3 introduced this design as planning-only. Sprint 11 Slice 4 implemented the domain/API/runtime/Web mapping source-of-truth and added review-only EF migration source. The migration is not executed automatically, no database is created automatically, no startup database mutation is introduced, and live read activation remains explicit.

## Slice 4 Implementation Status

Implemented:

- domain-owned authored content type enum;
- item-level `AuthoredContentItem.ContentType`;
- create/read API contract support;
- validation requiring content type for new authored content;
- runtime bundle mapper support;
- Web live-read DTO and mapper support;
- EF model configuration;
- review-only EF migration source for `AuthoredContentItems.ContentType`.

Not implemented:

- migration execution;
- automatic database creation;
- startup database mutation;
- production backfill execution;
- live read activation by default;
- live create/update/review/approve/reject/publish API wiring from Studio.

## Design Decision

Content type must be a real source-of-truth field for authored content.

Content type must live on the authored content item, not on the authored content version. The authored content item represents the guidance artifact. Versions represent revisions and lifecycle state for that same artifact.

Content type must not be inferred from:

- content key;
- title;
- route;
- selector;
- UI fallback;
- runtime delivery metadata;
- any other weak signal.

The Web mapper must not fake content type accuracy. It must set `HasKnownContentType=true` only when the live authoring API returns a valid known content type. Missing, invalid, or legacy absent content type must keep `HasKnownContentType=false`.

## Domain Model

The domain should use a domain-owned enum rather than depending directly on runtime application-layer contracts.

Proposed domain enum values:

- `Tooltip`;
- `Callout`;
- `Checklist`;
- `Walkthrough`.

The domain enum should map explicitly to the existing runtime content type contract during runtime bundle mapping. The mapping must be controlled and tested.

`Unknown` should not be a normal valid value for new live authored content. Unknown or unavailable content type is a compatibility state for legacy reads only.

## Mutability Rule

Content type should be required for new live authored content.

Content type should be immutable after live authoring creation unless a future separately approved rule allows draft-only mutation. A content type change from tooltip to walkthrough changes the authored artifact kind and can affect runtime rendering, validation, and published bundle expectations.

If a future draft-only mutation rule is approved, it must be explicit, permission-checked, tenant-scoped, audit-safe, and limited to content that has not entered review, approval, publish, archive, or delivery history.

## Backward Compatibility

Existing content without content type must remain unknown or unavailable until separately migrated or backfilled.

Backward-compatible read behaviour should be:

- known valid API content type maps to `HasKnownContentType=true`;
- missing content type maps to `HasKnownContentType=false`;
- invalid content type maps to `HasKnownContentType=false`;
- no inference is performed from content key, title, route, selector, or UI fallback.

New live authored content should require content type at validation time. Legacy records may require a staged persistence strategy before a required database column is enforced.

## Schema And Migration Planning

A future implementation will likely require a `ContentType` column on `AuthoredContentItems`.

Likely persistence shape:

- controlled string mapping;
- `nvarchar(32)` or equivalent;
- tenant-owned authored content item row remains the owner;
- no sensitive values;
- no free-form type values.

Migration and backfill must be separately approved. No migration should execute automatically, no database should be created automatically, and no startup database mutation should be introduced.

Migration options to evaluate in a later implementation plan:

- nullable or staged migration that allows legacy rows to remain readable;
- controlled backfill for known records if an authoritative source exists;
- validation requiring type for new content only;
- later hardening migration to require non-null content type only after backfill is complete.

## API Contract Impact

Future implementation likely affects:

- create authoring API contract, adding required content type for new content;
- read authoring API contract, returning content type when known;
- authoring validation, rejecting missing or invalid content type for new content;
- authoring command responses through the authored content response shape.

The read API must continue to use `Authoring.Read`. Create/update boundaries must continue to use `Authoring.Manage` or the existing approved permission mapping. No new permission key is expected for content type.

The Web/page/request model must not supply tenant IDs. Tenant identity remains server-side through existing tenant context and authorization boundaries.

## Runtime Bundle Mapping Impact

Runtime bundle mapping must stop hardcoding tooltip once the domain source of truth exists.

The mapper should translate domain content type to runtime content type using a controlled switch. Unsupported or legacy unknown content type must fail safely and return typed validation issues without echoing raw input values.

Runtime delivery contracts should receive only the approved runtime type value and safe structural metadata.

## Web And Studio Impact

Future Web contract changes likely include:

- add content type to the Web mirror of the authoring read DTO;
- update `StudioAuthoringReadApiMapper` to map valid known content type;
- keep `HasKnownContentType=false` for missing or invalid API values;
- preserve the existing safe UI state for unavailable type;
- later align live Studio create flow with the approved create API contract.

The current local Studio editor already models content type as metadata. That local/foundation value is not a production source of truth until the live authoring API and domain model support it.

## Validation Rules

Future validation should require:

- content type is present for new live authored content;
- content type is one of the controlled values;
- content type is not inferred;
- content type validation messages are typed and safe;
- validation messages do not echo raw input values;
- content type does not carry tenant IDs, claims, headers, configured values, content body, or sensitive content.

## Security And Tenant Guardrails

The future implementation must preserve these boundaries:

- no tenant IDs from browser, page, or request models;
- no `X-Adopta-Tenant-Id` from Web production code;
- no `X-Adopta-Test-*` production shortcut;
- no tokens, headers, claims, secrets, connection strings, tenant values, raw exceptions, or sensitive content in UI, errors, logs, docs, DTOs, or validation messages;
- existing `Authoring.Read` and `Authoring.Manage` permissions remain the relevant read/create permissions;
- cross-tenant existence remains hidden or safely denied.

## Future Implementation Impact

Likely files in a later approved implementation slice:

- `src/Adopta.Domain/Authoring/AuthoredContentType.cs`;
- `src/Adopta.Domain/Authoring/AuthoredContentItem.cs`;
- `src/Adopta.Application/Authoring/AuthoredContentContracts.cs`;
- `src/Adopta.Application/Authoring/AuthoredContentValidator.cs`;
- `src/Adopta.Application/Authoring/AuthoredContentRuntimeBundleMapper.cs`;
- `src/Adopta.Api/Authoring/AuthoringApiContracts.cs`;
- `src/Adopta.Api/Authoring/AuthoringEndpoints.cs`;
- `src/Adopta.Web/Studio/StudioAuthoringReadApiContracts.cs`;
- `src/Adopta.Web/Studio/StudioAuthoringReadApiMapper.cs`;
- `src/Adopta.Infrastructure/Persistence/Configurations/AuthoredContentItemConfiguration.cs`;
- EF migration and model snapshot files only if separately approved.

Likely tests:

- domain construction and validation tests;
- create/read API DTO tests;
- authoring API integration tests;
- runtime bundle mapper tests;
- Web mapper tests for known and unknown content type;
- persistence model and migration guardrail tests if schema work is approved;
- documentation guardrail tests.

## Explicit Non-Goals

This design and Slice 4 implementation do not approve:

- migration execution;
- automatic database creation;
- startup database mutation;
- live read activation by default;
- live create/update/review/approve/reject/publish integration;
- real appsettings values;
- deployment automation;
- analytics;
- AI;
- Event Hubs;
- ClickHouse;
- browser extension work;
- Property MTD integration.

## Smallest Safe Next Implementation Slice

The smallest safe implementation slice after this planning work is:

1. Add the domain-owned content type enum.
2. Add item-level content type to the authoring domain model.
3. Add create/read DTO support.
4. Add validation for new content.
5. Update runtime bundle mapping to use the domain value.
6. Update Web read DTO and mapper to mark known types accurately.
7. Keep persistence migration source separately approved unless that slice explicitly includes review-only schema work.

This keeps semantic correctness separate from migration execution, production activation, and live write/workflow/publish integration.
