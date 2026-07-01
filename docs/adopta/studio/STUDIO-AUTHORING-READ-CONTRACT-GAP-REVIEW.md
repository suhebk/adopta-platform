# Studio Authoring Read Contract Gap Review

This document records the current contract gaps between the authoring read API and the Studio content read model before live read-only Studio API activation is used operationally.

This review does not activate live reads. It does not approve backend API changes, domain model changes, schema changes, migrations, or live write/workflow/publish integration.

## Current Read Boundary

The live Studio read path is intentionally narrow:

- `StudioAuthoringReadApiClient` performs read-only calls.
- `StudioApiRequestBoundaryHandler` remains the only Authorization attachment point.
- `LocalStudioContentClient` remains the default and fallback unless explicit valid configuration activates the read API client.
- Studio request models do not accept tenant IDs.
- The authoring API continues to enforce tenant context and `Authoring.Read`.

## Contract Gap Summary

| Area | Current status | Operational decision |
|---|---|---|
| Content type | Missing from authoring read API and authoring domain source of truth. | Keep marked unknown in Web mapping. Full fix requires separate domain/API/create-update/schema approval. |
| Lifecycle state | Available through version lifecycle state. | No backend change needed for Slice 1. |
| Version metadata | Basic version ID, version string, lifecycle state, and created timestamp are available. | Usable for current UI; richer timestamps require future read-summary work. |
| Audit/history summary | Exposed as a safe read-only summary from existing lifecycle and publishing history repositories. | Use structural counts, controlled labels, and timestamps only. Keep fallback behaviour for older/absent responses. |
| Application metadata | `ApplicationId` only. No application name in authoring read response. | Keep ID-only until a tenant-scoped application metadata read is approved. |
| Delivery/publish metadata | Latest publish summary is exposed when publishing history exists. | Expose status, safe environment, channel, and timestamp only. Do not expose delivery bundle contents. |

## Content Type Gap

The authoring read API currently returns:

- content ID;
- tenant ID in the API DTO;
- application ID;
- content key;
- title;
- version collection.

It does not return a content type. The authoring domain model also does not store content type as a source-of-truth property.

The Web mapper must therefore continue to:

- set `HasKnownContentType=false`;
- avoid presenting the fallback content type as authoritative;
- avoid inferring content type from content key, title, or other weak signals.

A full content type fix requires separate approval because it affects authoring domain contracts, create/update boundaries, API DTOs, persistence, and likely schema.

## Lifecycle State Status

Lifecycle state is available through each version in the authoring read response. The Studio mapper can use the latest version lifecycle state to render:

- Draft;
- InReview;
- Approved;
- Published;
- Archived.

No backend change is required for lifecycle state in Slice 1.

## Version Metadata Status

The current API provides basic version metadata:

- version ID;
- version label;
- lifecycle state;
- created timestamp.

The Studio can safely display this metadata. It cannot yet show richer operational timestamps such as modified, review requested, approved, published, or archived timestamps unless those are derived from future read-summary support.

## Audit And History Summary Status

The Studio read model expects:

- lifecycle event count;
- publishing event count;
- latest safe activity;
- latest activity timestamp.

The authoring read API now exposes this summary as `AuthoredContentReadSummaryResponse` where the metadata can be derived from existing lifecycle and publishing history repositories.

The summary is structural only:

- lifecycle history count;
- publishing history count;
- controlled latest activity label;
- latest activity timestamp;
- latest publish status/environment/channel/timestamp when publishing history exists.

The Web mapper keeps the existing safe fallback when the API summary is absent.

The response does not expose raw lifecycle records, raw publishing records, actor IDs, claims, headers, configured values, tenant details inside the summary, content body, or sensitive content.

## Application Metadata Gap

The current authoring read response exposes `ApplicationId` only. It does not expose application display name or other tenant application metadata.

Tenant-scoped application repositories exist, but the authoring read endpoint does not currently join to them.

For now, the Studio should continue showing application ID only. A future enhancement can add optional application display metadata if it stays tenant-scoped and non-sensitive.

## Delivery And Publish Metadata Status

The authoring read response now includes a latest publish summary when publishing history exists:

- latest published environment;
- latest delivery channel;
- latest publish timestamp;
- latest publish result;

The response does not expose delivery bundle contents, runtime bundle payloads, external storage details, or publishing history records directly.

## Security And Privacy Guardrails

The read contract must continue to preserve these boundaries:

- No tenant IDs from browser/page/request models.
- No `X-Adopta-Tenant-Id` from Web production code.
- No `X-Adopta-Test-*` production shortcut.
- No tokens, headers, claims, secrets, connection strings, tenant values, raw exceptions, or sensitive content in UI, errors, logs, or docs.
- No new permission keys.
- `Authoring.Read` remains the read permission.
- Cross-tenant existence must remain hidden or safely denied by the API and repository boundary.

## Recommended Next Slice

Sprint 11 Slice 2 added the minimal read-only authoring summary contract without schema changes.

The next safest step is:

- keep content type unknown until the authoring domain has a source of truth;
- decide whether application display metadata should be added from the existing tenant-scoped application repository;
- separately plan content type source-of-truth work if the Studio needs live edit/create parity;
- preserve tenant isolation and `Authoring.Read`.

Content type source-of-truth work should be planned separately because it is likely domain/API/create-update/schema work.
