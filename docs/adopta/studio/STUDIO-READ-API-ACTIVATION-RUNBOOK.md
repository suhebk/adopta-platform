# Studio Read API Activation Runbook

This runbook closes Sprint 10 by documenting the controlled handoff for read-only Studio API activation validation.

It does not approve live activation by default. It does not approve write, workflow, or publish integration.

## Sprint 10 Delivery Summary

Sprint 10 delivered the controlled validation foundation for read-only Studio API activation:

- Internal preflight validation through the Studio read API preflight service.
- Controlled environment validation guidance using secure external configuration and placeholders only.
- Test-only activation rehearsal using fake access and fake transport.
- Operator-facing governance status surface for read API activation readiness.

The delivered capability is a production-readiness foundation. It is not an automatic production activation mechanism.

## Current Runtime Posture

- `LocalStudioContentClient` remains the default and fallback Studio content client.
- Live read API activation remains explicit.
- Activation remains fail-closed when configuration is missing, disabled, or invalid.
- `StudioAuthoringReadApiClient` remains read-only.
- Live write, workflow, and publish operations remain unavailable.
- No live API call is made by default.

The current posture is intentionally conservative. Operators can validate readiness without enabling live reads by accident.

## Activation Prerequisites

Before a controlled environment can activate read-only Studio API integration, these prerequisites must be satisfied from secure external configuration. Repository files must contain placeholders only.

Required configuration keys:

- `StudioApi:Enabled=true`
- `StudioApi:BaseAddress=<https-studio-api-base-address-placeholder>`
- `Authentication:StudioWeb:Enabled=true`
- `Authentication:StudioWeb:Authority=<studio-web-authority-placeholder>`
- `Authentication:StudioWeb:ClientId=<studio-web-client-id-placeholder>`
- `Authentication:StudioWeb:CallbackPath=<studio-web-callback-path-placeholder>`
- `StudioApi:TokenAcquisition:Enabled=true`
- `StudioApi:TokenAcquisition:Scopes=<downstream-api-scope-placeholder>`

Operational prerequisites:

- The Web application must authenticate users through the approved server-side authentication boundary.
- The downstream API must enforce tenant context server-side.
- The downstream API must enforce `Authoring.Read`.
- The request boundary remains the only place where outbound Authorization is attached.
- Configuration values must come from secure environment configuration, not repository files.

## Operator Validation Flow

Use this sequence before any environment-level activation decision.

1. Run preflight validation.
   - Confirm the preflight status is ready only when all explicit configuration requirements are met.
   - Confirm disabled, missing, or invalid configuration reports a fail-closed posture.

2. Run controlled environment validation.
   - Confirm the environment uses secure external configuration.
   - Confirm placeholders are not promoted as actual values.
   - Confirm no tenant ID is supplied by Web pages, UI models, or request models.

3. Run activation rehearsal.
   - Use the documented fake access and fake transport pattern only in tests.
   - Confirm the activated read client performs only read operations.
   - Confirm write, workflow, and publish methods remain unavailable.

4. Review the governance status surface.
   - Confirm the operator-facing surface displays readiness state only.
   - Confirm it displays safe check codes, safe statuses, and generic messages only.
   - Confirm it does not display configured values, access values, tenant identifiers, raw failures, or secrets.

Activation must not proceed when any validation step fails.

## Rollback And Fail-Closed Expectations

Rollback means returning the environment to local/fallback read behaviour.

Expected rollback posture:

- Disable Studio API activation through secure external configuration.
- Remove or invalidate the external read API activation settings.
- Confirm `IStudioContentClient` resolves to `LocalStudioContentClient`.
- Confirm the governance status surface reports disabled or not ready.
- Confirm no live read calls occur.

Fail-closed behaviour is required for:

- Missing configuration.
- Disabled activation.
- Invalid base address configuration.
- Missing authentication configuration.
- Missing token acquisition configuration.
- Missing downstream scope configuration.
- Missing server-side access.

No rollback step may require a database migration, schema change, deployment automation change, or committed configuration value.

## Security Boundaries

- Browser pages and Web request models must not provide tenant IDs.
- `X-Adopta-Tenant-Id` must not be used by Web production code.
- `X-Adopta-Test-*` must not be used as a production shortcut.
- No committed secrets are allowed.
- No configured values are displayed by the status surface or validation output.
- The request boundary remains the only Authorization attachment point.
- Raw exceptions, configured values, access values, claims, headers, connection strings, tenant values, tax data, HMRC data, property data, and sensitive content must not be exposed.

## Explicit Non-Goals

This Sprint 10 closeout does not approve:

- Live writes.
- Workflow integration.
- Publish integration.
- Live create draft.
- Live update draft.
- Live request review.
- Live approve.
- Live reject.
- Live publish.
- EF migrations.
- Database schema changes.
- Backend or API changes.
- Appsettings changes.
- Real network calls during automated tests.
- Deployment automation.
- Analytics.
- AI.
- Event Hubs.
- ClickHouse.
- Browser extension work.
- Property MTD integration.

## Troubleshooting Boundaries

When validation fails, operators should inspect only safe readiness outputs:

- preflight check code;
- preflight check status;
- generic safe message;
- documented configuration key presence;
- disabled/fail-closed state.

Do not copy configured values, access values, claims, headers, tenant identifiers, raw failures, or environment-specific values into tickets, docs, logs, or test output.

## Next Recommended Sprint Direction

The next sprint should either:

- proceed with separately approved, environment-specific read-only activation execution using secure external configuration; or
- start controlled live write/workflow API integration planning if read-only activation has been validated and accepted.

Live write, workflow, publish, migrations, backend changes, deployment automation, analytics, AI, and Property MTD integration should remain separately approval-gated.
