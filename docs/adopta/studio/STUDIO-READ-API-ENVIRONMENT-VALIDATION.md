# Studio Read API Environment Validation

This guide defines how a controlled environment should validate read-only Studio API activation using secure external configuration.

It does not approve committing real configuration values, enabling live Studio reads by default, enabling live write/workflow/publish commands, changing backend APIs, or adding deployment automation.

## Current Implemented Behaviour

- `LocalStudioContentClient` remains the default and fallback Studio content client.
- `StudioAuthoringReadApiClient` is the read-only API client implementation.
- `StudioReadApiActivationValidator` remains the activation source of truth.
- `StudioReadApiPreflightService` reports readiness only.
- `StudioApiRequestBoundaryHandler` remains the only Web-to-API boundary that can attach authorization.
- Invalid or incomplete configuration fails closed to the local client.
- Live create, update, review, approve, reject, and publish remain unavailable on the read API client.

## External Configuration Pattern

Activation values must be supplied by secure external configuration owned by the target environment. Repository files must not contain real values.

Placeholder-only example:

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

These placeholders are not real values. They must be replaced only by approved secure external configuration at environment level.

## Environment Validation Steps

Before enabling read-only activation in an environment:

1. Confirm repository appsettings do not contain Studio API activation values.
2. Confirm the target environment supplies `StudioApi:Enabled=true` externally.
3. Confirm the target environment supplies an HTTPS Studio API base address externally.
4. Confirm Studio Web authentication is enabled externally.
5. Confirm the Web authority and client identifier are supplied externally.
6. Confirm the callback path is a safe application path.
7. Confirm API access acquisition is enabled externally.
8. Confirm downstream API access scopes are supplied externally.
9. Run the Studio read API preflight and confirm it reports ready with safe output only.
10. Confirm `IStudioContentClient` resolves to `StudioAuthoringReadApiClient` only for valid explicit configuration.
11. Confirm invalid configuration resolves to `LocalStudioContentClient`.
12. Confirm live write/workflow/publish methods remain unavailable.
13. Confirm no Web page or request model supplies tenant IDs.
14. Confirm Web production code does not add `X-Adopta-Tenant-Id`.
15. Confirm Web production code does not add or forward `X-Adopta-Test-*`.

## Default And Fail-Closed Behaviour

Default behaviour:

- With no external activation values, `IStudioContentClient` resolves to `LocalStudioContentClient`.
- No live API call is made by default.
- Preflight reports disabled rather than ready.

Invalid configuration behaviour:

- Missing API endpoint configuration fails closed.
- Missing Web authentication configuration fails closed.
- Missing API access configuration fails closed.
- Invalid values fail closed.
- Validation and preflight messages remain generic and do not echo configured values.

Valid external configuration behaviour:

- `StudioReadApiActivationValidator` can report active.
- `StudioReadApiPreflightService` can report ready.
- `IStudioContentClient` can resolve to `StudioAuthoringReadApiClient`.
- Only read-only list/get API operations are available.

## Rollback

Rollback means disabling the activation flag or removing invalid external configuration from the environment.

After rollback:

- `IStudioContentClient` must resolve to `LocalStudioContentClient`.
- Preflight must no longer report ready.
- No live read calls should be made by default.
- Write/workflow/publish commands remain unavailable on the read API client.

## Tenant, Header, And Access Safety

- Tenant identity remains server-side.
- Web page and request models must not accept tenant IDs.
- `X-Adopta-Tenant-Id` must not be added by Web production code.
- `X-Adopta-Test-*` must not be added or forwarded by Web production code.
- Test-only integration headers must not become production shortcuts.
- Access values must not be logged, displayed, returned to page models, or echoed in validation/preflight output.
- Raw headers, raw claims, secrets, connection strings, tenant values, authority values, endpoint values, scope values, tax data, HMRC data, property data, user-entered values, sensitive content, and raw exceptions must not be exposed.

## Explicit Non-Goals

This validation step does not build or approve:

- live Studio read activation by default;
- live create draft;
- live update draft;
- live request review;
- live approve;
- live reject;
- live publish;
- production appsettings values;
- real secrets;
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

