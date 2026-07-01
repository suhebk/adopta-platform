# Studio Read API Activation Readiness

This guide defines the readiness checklist for enabling read-only Adoption Studio API integration in a secure environment.

It documents the approved configuration shape and activation prerequisites only. It does not approve committing real values, enabling live writes, changing backend APIs, or adding deployment automation.

## Current Implemented Behaviour

- `LocalStudioContentClient` remains the default Studio content client.
- `StudioAuthoringReadApiClient` can be activated only through the explicit read API activation gate.
- The activation gate is disabled by default.
- Invalid or incomplete activation configuration fails closed to `LocalStudioContentClient`.
- `StudioApiRequestBoundaryHandler` remains the only Web-to-API boundary that can attach Authorization.
- The live API client supports read-only list/get operations only.
- Create, update, review, approve, reject, and publish operations remain unavailable on the live API client.

## Studio Read API Preflight Layer

The Studio read API preflight layer is a server-side operational readiness reporter. It is not an activation mechanism.

The preflight layer:

- reuses `StudioReadApiActivationValidator` as the source of truth for activation validity;
- reports whether activation is disabled, ready, or invalid;
- reports readiness checks using generic safe messages only;
- confirms disabled-by-default and fail-closed posture;
- confirms the request boundary and tenant/test header guardrails;
- confirms the live read client remains read-only;
- confirms local fallback remains available for disabled or invalid activation.

The preflight layer does not:

- enable live Studio reads;
- alter dependency injection activation decisions;
- perform HTTP or network calls;
- read or expose configured endpoint values;
- expose authority values, client identifiers, configured access values, tenant identifiers, raw headers, raw claims, secrets, or raw exceptions.

Preflight output is limited to check code, check status, and a generic safe message.

## Required Configuration Keys

The following keys must be supplied by secure environment configuration before read-only Studio API activation is considered ready. Repository files must not contain real values.

```json
{
  "StudioApi": {
    "Enabled": true,
    "BaseAddress": "<https-studio-api-base-address-from-secure-configuration>",
    "TokenAcquisition": {
      "Enabled": true,
      "Scopes": [
        "<downstream-api-scope-from-secure-configuration>"
      ]
    }
  },
  "Authentication": {
    "StudioWeb": {
      "Enabled": true,
      "Authority": "<web-auth-authority-from-secure-configuration>",
      "ClientId": "<web-client-id-from-secure-configuration>",
      "CallbackPath": "/signin-oidc"
    }
  }
}
```

Allowed placeholder-only documentation keys:

- `StudioApi:Enabled`
- `StudioApi:BaseAddress`
- `Authentication:StudioWeb:Enabled`
- `Authentication:StudioWeb:Authority`
- `Authentication:StudioWeb:ClientId`
- `Authentication:StudioWeb:CallbackPath`
- `StudioApi:TokenAcquisition:Enabled`
- `StudioApi:TokenAcquisition:Scopes`

## Activation Prerequisites

Read-only activation is ready only when all prerequisites are true:

- `StudioApi:Enabled=true`.
- `StudioApi:BaseAddress` is an HTTPS absolute base address supplied by secure configuration.
- `Authentication:StudioWeb:Enabled=true`.
- `Authentication:StudioWeb:Authority` is configured through secure environment configuration.
- `Authentication:StudioWeb:ClientId` is configured through secure environment configuration.
- `Authentication:StudioWeb:CallbackPath` is configured as a safe callback path.
- `StudioApi:TokenAcquisition:Enabled=true`.
- `StudioApi:TokenAcquisition:Scopes` contains at least one safe downstream API scope from secure configuration.
- Web page and request models do not accept tenant IDs.
- Web does not send `X-Adopta-Tenant-Id`.
- Web does not use `X-Adopta-Test-*` as a production shortcut.
- The API continues to enforce tenant context and `Authoring.Read` permission.

## Default And Fail-Closed Behaviour

Default behaviour:

- Missing configuration leaves activation disabled.
- Disabled activation resolves `IStudioContentClient` to `LocalStudioContentClient`.
- No live API call is made by default.

Invalid configuration behaviour:

- Missing or non-HTTPS API base address fails closed.
- Missing or incomplete Web authentication configuration fails closed.
- Missing or incomplete API access acquisition configuration fails closed.
- Missing downstream API scopes fail closed.
- Validation messages must remain generic and must not echo configured values.

## Valid Configuration Behaviour

When every activation prerequisite is satisfied:

- `IStudioContentClient` may resolve to `StudioAuthoringReadApiClient`.
- `StudioAuthoringReadApiClient` may call the existing read-only authoring API endpoints.
- `StudioApiRequestBoundaryHandler` remains the only boundary that can attach Authorization.
- The activated client may list authored content and get authored content by ID.
- The activated client must keep create, update, review, approve, reject, and publish unavailable.

## Secret Handling

Production values must come from secure configuration such as a managed secret store or approved environment configuration. Repository files must not contain:

- real secrets;
- real tenant identifiers;
- real client identifiers;
- real authority values;
- real API base addresses;
- real downstream API scopes when environment-specific;
- connection strings;
- credentials;
- tokens;
- raw claims;
- production host values.

Documentation and tests must use placeholders only.

## Tenant And Header Safety

- Tenant identity must remain server-side.
- Web page state, forms, routes, queries, and request models must not carry tenant IDs.
- Web production code must not add `X-Adopta-Tenant-Id`.
- Web production code must not add or forward `X-Adopta-Test-*`.
- Cross-tenant access must continue to be denied or hidden by API-side tenant enforcement.

## Explicit Non-Goals

This readiness step does not build or approve:

- live create draft;
- live update draft;
- live request review;
- live approve;
- live reject;
- live publish;
- real committed configuration values;
- backend API changes;
- EF migrations;
- database schema changes;
- deployment automation;
- analytics;
- AI;
- Event Hubs;
- ClickHouse;
- browser extension work;
- Property MTD integration.

## Operational Readiness Checklist

Before enabling read-only Studio API integration in an environment:

- Confirm production values are supplied outside repository files.
- Confirm the API base address is HTTPS.
- Confirm Web authentication is enabled and validated in the target environment.
- Confirm downstream API access acquisition is enabled and scoped to read-only Studio API use.
- Confirm the API still enforces tenant context and `Authoring.Read`.
- Confirm no Web page or request model accepts tenant IDs.
- Confirm no Web production path sends tenant or test-auth headers.
- Confirm the Studio read API preflight reports ready with safe output only.
- Confirm the environment has rollback instructions to disable `StudioApi:Enabled`.
- Confirm logs and UI errors remain generic and non-sensitive.
- Confirm write/workflow/publish API activation remains separately approved.

## Controlled Environment Validation

Environment validation is documented in `docs/adopta/studio/STUDIO-READ-API-ENVIRONMENT-VALIDATION.md`.

The validation process uses secure external configuration in real environments and in-memory configuration in automated tests. It must not commit real values to repository appsettings files.

Controlled environment validation must prove:

- local/default configuration remains on `LocalStudioContentClient`;
- invalid external-style configuration fails closed to `LocalStudioContentClient`;
- valid external-style configuration can resolve the read-only API client;
- preflight readiness does not expose configured values;
- live write/workflow/publish operations remain unavailable;
- Web request/page models do not supply tenant IDs;
- `X-Adopta-Tenant-Id` is not used by Web production code;
- `X-Adopta-Test-*` is not used as a production shortcut.

## Operator-Facing Status Surface

The operator-facing status surface is hosted on `/studio/governance`.

The surface:

- uses `StudioReadApiPreflightService` as the only status source;
- displays only overall status, check code, check status, and generic safe message;
- does not display configured endpoint values, authority values, client identifiers, scopes, access values, claims, tenant identifiers, option values, or raw failures;
- does not activate live reads;
- does not perform network calls;
- does not enable write/workflow/publish operations;
- keeps the existing `/studio/governance` navigation permission mapping to `Audit.Read`.

The page is an operational readiness view only. Activation still requires explicit secure environment configuration and remains disabled/fail-closed by default.

## Sprint 9 Closeout Position

Sprint 9 closes with a controlled, disabled-by-default read API activation path. The platform is ready for a future environment-level validation of read-only Studio API activation, but no live activation values are committed and no write/workflow/publish API integration is enabled.
