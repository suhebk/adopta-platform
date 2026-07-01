# Studio Read API Activation Rehearsal

This guide defines the controlled rehearsal for read-only Studio API activation.

The rehearsal proves the activation path with in-memory configuration, a fake server-side access provider, and a fake capturing HTTP transport. It does not enable live Studio reads by default, commit environment values, call a real API, or approve live write/workflow/publish integration.

## Purpose

The rehearsal validates the production activation shape without using production infrastructure:

- the existing activation gate can switch `IStudioContentClient` to `StudioAuthoringReadApiClient` only for complete explicit configuration;
- the typed `HttpClient` pipeline includes `StudioApiRequestBoundaryHandler`;
- access is attached only by the server-side boundary handler;
- tenant and test headers are removed before the fake transport receives a request;
- read operations use only authoring list/get routes;
- disabled or invalid configuration falls back to `LocalStudioContentClient`;
- create, update, review, approve, reject, and publish remain unavailable.

## Relationship To Earlier Sprint 10 Slices

Slice 1 added preflight readiness reporting. It answers whether configuration and guardrails are ready, but it does not run an activated client request path.

Slice 2 added environment validation guidance and non-invasive guardrails. It proves default, invalid, and valid configuration behaviour using safe in-memory values.

Slice 3 adds an activation rehearsal. It exercises the real activation gate and typed client pipeline with fake test-only infrastructure so the read-only path can be verified without a network call.

## Rehearsal Inputs

The automated rehearsal uses only:

- valid in-memory configuration;
- a synthetic fake access value generated during test execution;
- a fake server-side access provider registered only in the test service collection;
- a fake capturing HTTP transport registered only in the test service collection.

The fake access value is not documented as an environment value and is not suitable for any deployed environment.

## Rehearsed Read Operations

The activated read client may call only:

- `GET /authoring/content`;
- `GET /authoring/content/{contentId}`.

The rehearsal verifies the method and path captured by the fake transport.

## Request Boundary Validation

`StudioApiRequestBoundaryHandler` remains the only component that can attach access to outbound Studio API requests.

The rehearsal verifies:

- a fake server-side provider supplies a synthetic access value;
- the boundary handler attaches access before forwarding;
- prohibited tenant and test headers are stripped before the fake transport receives the request;
- safe client result messages do not include the fake access value.

The rehearsal must not add `X-Adopta-Tenant-Id` or `X-Adopta-Test-*` to production Web code.

## Rollback And Fail-Closed Validation

Rollback is rehearsed by disabling or invalidating activation configuration.

Expected rollback behaviour:

- disabled configuration resolves `IStudioContentClient` to `LocalStudioContentClient`;
- invalid configuration resolves `IStudioContentClient` to `LocalStudioContentClient`;
- no fake transport call is made by unavailable write/workflow/publish methods;
- no live API call is made by default.

## Explicit Non-Goals

This rehearsal does not build or approve:

- live Studio read activation by default;
- live create draft;
- live update draft;
- live request review;
- live approve;
- live reject;
- live publish;
- production appsettings values;
- committed environment values;
- backend/API changes;
- EF migrations;
- database schema changes;
- deployment automation;
- analytics;
- AI;
- Event Hubs;
- ClickHouse;
- browser extension work;
- Property MTD integration.

## Operator Checklist

Before any future environment activation:

- review the Slice 1 preflight result;
- review the Slice 2 environment validation guide;
- confirm this rehearsal test path passes;
- confirm environment values are supplied only through approved secure configuration;
- confirm the API enforces tenant context and `Authoring.Read`;
- confirm write/workflow/publish integration remains separately approval-gated;
- confirm rollback returns the Web app to local/fail-closed behaviour.
