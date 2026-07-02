# Studio Live Draft Create/Update Integration Plan

This document defines the planned integration path for live Studio draft create/update. It is planning only.

This slice does not implement live create, live update, a backend update endpoint, migration execution, database mutation, startup migration logic, review/approve/reject/publish wiring, deployment automation, or Property MTD integration.

## Current API State

The authoring API currently exposes live create:

- `POST /authoring/content`;
- requires tenant context;
- requires `Authoring.Manage`;
- accepts `CreateAuthoredContentRequest`;
- requires content type for new authored content.

The authoring API does not currently expose a live draft update endpoint.

Current read operations remain protected by `Authoring.Read`.

## Planning Decisions

Live create and live update should be separate future implementation slices.

Live draft create remains blocked until content type migration/backfill readiness is approved.

Live draft update is blocked by the missing backend update endpoint and update contract.

`StudioAuthoringReadApiClient` should remain read-only. Future write integration should use a separately gated draft-write boundary, such as `StudioAuthoringDraftApiClient`, rather than silently expanding the read client.

`LocalStudioContentClient` remains the fallback and default when live API activation is not valid.

`StudioApiRequestBoundaryHandler` remains the only Authorization attachment boundary.

Web pages and Web request models must not send tenant IDs.

Web `RuntimeContentType` values must map explicitly to API/domain `AuthoredContentType` values.

Client results must remain safe typed `StudioContentClientResult<T>` values.

## Future Live Create Slice

A future live create slice should:

- keep `LocalStudioContentClient` as fallback;
- add a draft-write client boundary only when the readiness gate is satisfied;
- call `POST /authoring/content`;
- require `Authoring.Manage` through the API;
- attach Authorization only through `StudioApiRequestBoundaryHandler`;
- never send tenant ID from Web/page/request models;
- map content type explicitly from Web runtime type to API/domain type;
- map successful create response back into `StudioContentEditorModel` and selected content read model state;
- map `401`, `403`, `404`, validation failure, invalid response, unavailable, and unexpected errors to safe typed client results;
- avoid raw response bodies and raw exception details in UI state.

## Future Live Update Slice

A future live update slice should start by defining the backend update contract.

That future backend work should decide:

- route shape;
- request DTO;
- response DTO;
- whether content type remains immutable;
- draft-only update rules;
- version update rules;
- tenant context and `Authoring.Manage` permission enforcement;
- audit/history behaviour, if any;
- safe validation issues.

No live update client should be implemented until the backend update endpoint and contract are approved.

## Migration And Backfill Gate

Live draft create/update depends on the content type readiness gate.

Before live draft integration proceeds:

- migration source review must be complete;
- migration execution must have separate operational approval or a documented safe deferral;
- backfill plan must be approved or explicitly deferred with safe unknown/unavailable handling;
- rollback plan must be approved;
- no startup migration path may exist;
- no automatic database creation path may exist;
- no deployment automation may be added by this feature slice.

Legacy content without authoritative content type remains unknown/unavailable. Content type must not be inferred from content key, title, route, selector, UI fallback, runtime delivery metadata, or weak naming patterns.

## Tenant And Authorization Boundary

The Web client must not send tenant IDs in route, query string, headers, or request body.

The Web client must not send `X-Adopta-Tenant-Id`.

The Web client must not use `X-Adopta-Test-*` as a production shortcut.

Authorization must be attached only by `StudioApiRequestBoundaryHandler`.

Future write requests must preserve:

- `Authoring.Read` for read operations;
- `Authoring.Manage` for create/update draft operations.

No new permission keys are planned for draft create/update.

## Safe Result And UX Behaviour

The Studio page should continue to use `IStudioContentClient`.

Future write-client responses should map into safe `StudioContentClientResult<T>` values:

- `Success`;
- `Unauthorized`;
- `Forbidden`;
- `NotFound`;
- `InvalidResponse`;
- `Unavailable`;
- `UnexpectedError`;
- `ValidationError`.

Save-state UX should remain:

- saving;
- saved;
- validation error;
- not authorized;
- generic error.

Messages must remain generic and must not expose tokens, headers, claims, secrets, connection strings, tenant values, raw exceptions, content body, form values, input values, or sensitive content.

## Explicit Non-Goals

This planning slice does not approve:

- live create implementation;
- live update implementation;
- backend update endpoint;
- migration execution;
- database mutation;
- startup migration logic;
- live review/approve/reject/publish integration;
- backend/API behaviour changes;
- EF/schema changes;
- real appsettings values;
- deployment automation;
- analytics;
- AI;
- Event Hubs;
- ClickHouse;
- browser extension work;
- Property MTD integration.

## Recommended Future Slices

Recommended order:

1. Live create draft client integration behind the readiness gate.
2. Backend draft update contract and endpoint design.
3. Live update draft client integration after the backend update contract is approved.

Keep migration execution, database mutation, startup migration logic, workflow/publish integration, appsettings values, deployment automation, analytics, AI, browser extension work, and Property MTD integration separately approved.
