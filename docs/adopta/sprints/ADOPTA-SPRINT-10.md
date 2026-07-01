# ADOPTA-SPRINT-10 - Controlled Studio Read API Activation Validation

## Sprint objective

Add controlled, secure environment-level validation for live read-only Studio API integration without compromising tenant isolation, authentication boundaries, Studio UX, or production safety.

Sprint 10 starts with a preflight-only slice so environments can inspect read-only activation readiness before live Studio reads are enabled.

## Slice 1 - Studio Read API Activation Preflight

### Requirement IDs covered

- `FR-IDN-001` - Added a server-side preflight layer for future Microsoft Entra backed Studio read activation readiness.
- `FR-IDN-005` - Reconfirmed that downstream API access remains server-side and disabled unless explicitly configured.
- `FR-IDN-012` - Preserved API-side tenant resolution and avoided tenant IDs in Web page/request models.
- `FR-IDN-031` - Added preflight and test coverage for tenant/test header guardrails.
- `FR-GOV-002` - Preserved read-only authoring API activation boundaries and kept workflow/publish operations unavailable.
- `NFR-SEC-1` - Kept activation disabled by default and fail-closed.
- `NFR-SEC-2` - Added safe, non-sensitive preflight output that does not echo configured values.
- `NFR-TEST-1` - Added unit and documentation guardrails for preflight readiness.

### Scope delivered

- Added `IStudioReadApiPreflightService`.
- Added typed preflight result and check records.
- Added `StudioReadApiPreflightService` as a read-only operational readiness reporter.
- Registered the preflight service in Web DI.
- Reused `StudioReadApiActivationValidator` as the source of truth for activation validity.
- Added tests for disabled/default behaviour, valid explicit configuration, invalid configuration, safe output, tenant/test header guardrails, read-only client posture, local fallback, and documentation readiness.
- Updated the Studio read API activation readiness guide with the preflight layer.

### Preflight versus activation validator

`StudioReadApiActivationValidator` remains the activation source of truth. It decides whether explicit configuration can activate live read-only Studio API reads.

`StudioReadApiPreflightService` does not make activation decisions. It reports operational readiness by aggregating:

- configuration readiness from the activation validator;
- disabled-by-default and fail-closed posture;
- request boundary availability;
- tenant/test header guardrails;
- read-only client posture;
- local fallback posture.

The preflight service does not enable live reads, alter DI activation decisions, call the network, change appsettings, or mutate external state.

### Safe output and redaction behaviour

Preflight output contains only:

- check code;
- check status;
- generic safe message.

Preflight output does not echo:

- configured API endpoint values;
- authority values;
- client identifiers;
- configured access values;
- tenant identifiers;
- raw headers;
- raw claims;
- secrets;
- raw exceptions.

### Default and fail-closed behaviour

- Missing configuration keeps Studio API activation disabled.
- Disabled activation continues to resolve `IStudioContentClient` to `LocalStudioContentClient`.
- Invalid activation configuration continues to resolve `IStudioContentClient` to `LocalStudioContentClient`.
- Valid explicit activation remains required before `StudioAuthoringReadApiClient` can be used.
- No live API call is made by default.

### Tenant, header, and access safety

- Tenant identity remains server-side.
- Web page and request models do not accept tenant IDs.
- Web production code must not add `X-Adopta-Tenant-Id`.
- Web production code must not add or forward `X-Adopta-Test-*`.
- `StudioApiRequestBoundaryHandler` remains the only Web-to-API boundary that can attach authorization.
- Access values are not exposed in UI models, preflight output, validation messages, docs, logs, or exceptions.

### Write, workflow, and publish boundaries

Live read activation remains read-only. The live read client keeps these operations unavailable:

- live create draft;
- live update draft;
- live request review;
- live approve;
- live reject;
- live publish.

### Explicitly not built

- Live Studio read activation by default.
- Live create draft.
- Live update draft.
- Live request review.
- Live approve.
- Live reject.
- Live publish.
- Backend/API changes.
- EF migrations.
- Database schema changes.
- Real appsettings values.
- Deployment automation.
- Analytics.
- AI assistant.
- Event Hubs.
- ClickHouse.
- Browser extension work.
- Property MTD integration.

### Commands run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
rg "X-Adopta-Tenant-Id|X-Adopta-Test-" src/Adopta.Web -g "!src/Adopta.Web/Studio/StudioApiRequestBoundaryHandler.cs" -g "!**/bin/**" -g "!**/obj/**"
rg "PostAsync|PutAsync|PatchAsync|DeleteAsync|/request-review|/approve|/reject|/publish" src/Adopta.Web/Studio/StudioAuthoringReadApiClient.cs
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
git diff --check
```

### Known limitations

- No live Studio read values are committed.
- No live Studio reads are activated by default.
- The preflight service is currently an internal server-side model/service, not an exposed admin UI.
- Web sign-in UI remains outside this slice.
- Live write/workflow/publish API integration remains separately approval-gated.

### Next recommended slice

Add a controlled environment-level validation path for read-only Studio API activation using secure external configuration. Keep live write/workflow/publish activation, backend changes, appsettings values, migrations, analytics, AI, deployment automation, and Property MTD integration separately approved.

## Slice 2 - Controlled Read-Only Studio API Environment Validation

### Requirement IDs covered

- `FR-IDN-001` - Documented and tested controlled environment validation for future Microsoft Entra backed Studio read activation.
- `FR-IDN-005` - Reconfirmed that API access values must come from secure external configuration and are not committed.
- `FR-IDN-012` - Reconfirmed that Web does not supply tenant IDs and tenant resolution remains API-side.
- `FR-IDN-031` - Added guardrails for tenant/test header safety during environment validation.
- `FR-GOV-002` - Preserved read-only authoring API activation boundaries and kept workflow/publish operations unavailable.
- `NFR-SEC-1` - Reconfirmed local/default and invalid configuration fail closed.
- `NFR-SEC-2` - Added documentation and tests proving environment validation output avoids configured value exposure.
- `NFR-TEST-1` - Added non-invasive guardrail coverage for external-style in-memory configuration and documentation readiness.

### Scope delivered

- Added `docs/adopta/studio/STUDIO-READ-API-ENVIRONMENT-VALIDATION.md`.
- Added non-invasive guardrail tests for controlled external-style configuration.
- Updated the Studio read API activation readiness guide with environment validation guidance.
- Updated this Sprint 10 document with Slice 2 scope and guardrails.
- Updated the documentation index.

### Environment validation design

Environment validation is a documentation and test-backed operating pattern. It uses secure external configuration in real environments and in-memory configuration in automated tests.

The slice does not add a second activation model. It continues to rely on:

- `StudioReadApiActivationValidator` for activation validity;
- `StudioReadApiPreflightService` for readiness reporting;
- `AddStudioReadApiActivationGate` for disabled-by-default and fail-closed DI selection;
- `StudioApiRequestBoundaryHandler` as the only authorization attachment boundary.

### Placeholder-only configuration guidance

The environment validation guide documents placeholders only:

- `<https-studio-api-base-address-from-secure-configuration>`;
- `<web-auth-authority-from-secure-configuration>`;
- `<web-client-id-from-secure-configuration>`;
- `<downstream-api-scope-from-secure-configuration>`.

These placeholders are not real values. Real values must come from secure external environment configuration and must not be committed.

### Default and fail-closed behaviour

- Missing configuration resolves `IStudioContentClient` to `LocalStudioContentClient`.
- Invalid external-style configuration resolves `IStudioContentClient` to `LocalStudioContentClient`.
- Valid external-style in-memory test configuration can resolve `IStudioContentClient` to `StudioAuthoringReadApiClient`.
- Preflight reports ready only when the existing activation validator can activate.

### Write, workflow, and publish boundaries

The live read API client remains read-only. These operations stay unavailable:

- live create draft;
- live update draft;
- live request review;
- live approve;
- live reject;
- live publish.

### Tenant, header, and access safety

- Web request/page models do not accept tenant IDs.
- Web production code must not add `X-Adopta-Tenant-Id`.
- Web production code must not add or forward `X-Adopta-Test-*`.
- Test-only integration headers must not become production shortcuts.
- Access values and configured values must not be exposed by docs, preflight output, validation messages, UI models, logs, or exceptions.

### Explicitly not built

- Live Studio read activation by default.
- Live create/update/review/approve/reject/publish.
- Production code changes.
- Backend/API changes.
- EF migrations.
- Database schema changes.
- Real appsettings values.
- Deployment automation.
- Analytics.
- AI.
- Event Hubs.
- ClickHouse.
- Browser extension work.
- Property MTD integration.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
rg "StudioApi|Authentication:StudioWeb|TokenAcquisition|BaseAddress|Authority|ClientId|Scopes" src/Adopta.Web/appsettings*.json
Run the configured secret-marker guardrail search across source, docs, and config files.
rg "X-Adopta-Tenant-Id|X-Adopta-Test-" src/Adopta.Web tests/Adopta.UnitTests docs/adopta/studio docs/adopta/sprints/ADOPTA-SPRINT-10.md -g "!**/bin/**" -g "!**/obj/**"
rg "PostAsync|PutAsync|PatchAsync|DeleteAsync|/request-review|/approve|/reject|/publish" src/Adopta.Web/Studio/StudioAuthoringReadApiClient.cs
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
git diff --check
```

### Known limitations

- No real environment values are committed.
- No live Studio reads are activated by default.
- Environment validation remains an operational/docs and test-backed pattern, not an exposed admin UI.
- Live write/workflow/publish API integration remains separately approval-gated.

### Next recommended slice

Add a controlled operator-facing read API activation status surface or diagnostics endpoint only if explicitly approved. Keep live write/workflow/publish activation, backend changes, appsettings values, migrations, analytics, AI, deployment automation, and Property MTD integration separately approved.

## Slice 3 - Controlled Read-Only Studio API Activation Rehearsal

### Requirement IDs covered

- `FR-IDN-001` - Added a controlled rehearsal for future Microsoft Entra backed Studio read activation without live environment values.
- `FR-IDN-005` - Reconfirmed that access values remain server-side and are supplied only by a test fake provider in rehearsal.
- `FR-IDN-012` - Reconfirmed that Web does not supply tenant IDs and tenant resolution remains API-side.
- `FR-IDN-031` - Added rehearsal coverage for tenant/test header stripping at the Web-to-API request boundary.
- `FR-GOV-002` - Proved the activated read client remains read-only and keeps workflow/publish operations unavailable.
- `NFR-SEC-1` - Reconfirmed disabled and invalid activation fall back to local/fail-closed behaviour.
- `NFR-SEC-2` - Added tests and documentation proving rehearsal output avoids access value exposure.
- `NFR-TEST-1` - Added test-only activation rehearsal coverage with fake transport and fake server-side access provider.

### Scope delivered

- Added `docs/adopta/studio/STUDIO-READ-API-ACTIVATION-REHEARSAL.md`.
- Added unit tests for the controlled activation rehearsal.
- Exercised the real `AddStudioReadApiActivationGate` activation path.
- Exercised the real `StudioAuthoringReadApiClient` typed client pipeline.
- Exercised the real `StudioApiRequestBoundaryHandler`.
- Used valid in-memory configuration only.
- Used a fake server-side access provider only in tests.
- Used a fake capturing HTTP transport only in tests.
- Updated the environment validation guide with rehearsal and rollback validation.
- Updated this Sprint 10 document with Slice 3 scope and guardrails.
- Updated the documentation index.

### Rehearsal design

The activation rehearsal is a test-only path. It builds the Web Studio service collection with valid in-memory activation configuration, then injects:

- a fake server-side access provider;
- a fake capturing HTTP transport.

This proves the activation gate and typed client pipeline without a network call. It does not alter production registration or runtime behaviour.

### Read-only behaviour proven

The rehearsal verifies that activated reads use only:

- `GET /authoring/content`;
- `GET /authoring/content/{contentId}`.

The fake transport captures the outbound method and path. No write, workflow, or publish route is called.

### Request boundary and access safety

The rehearsal verifies:

- access is attached only through `StudioApiRequestBoundaryHandler`;
- tenant and test headers are stripped or absent before the fake transport receives a request;
- safe client messages do not expose the synthetic access value;
- no browser/page/request model supplies tenant IDs.

### Rollback and fail-closed behaviour

The rehearsal verifies:

- disabled activation resolves `IStudioContentClient` to `LocalStudioContentClient`;
- invalid activation resolves `IStudioContentClient` to `LocalStudioContentClient`;
- unavailable write/workflow/publish methods do not call the fake transport.

### Write, workflow, and publish boundaries

The activated read client keeps these operations unavailable:

- live create draft;
- live update draft;
- live request review;
- live approve;
- live reject;
- live publish.

### Explicitly not built

- Live Studio read activation by default.
- Real network calls.
- Live create/update/review/approve/reject/publish.
- Production code changes.
- Backend/API changes.
- EF migrations.
- Database schema changes.
- Real appsettings values.
- Deployment automation.
- Analytics.
- AI.
- Event Hubs.
- ClickHouse.
- Browser extension work.
- Property MTD integration.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
git diff -- src/Adopta.Web/appsettings.json src/Adopta.Web/appsettings.Development.json src/Adopta.Api/appsettings.json src/Adopta.Api/appsettings.Development.json
Run the configured secret-marker guardrail search across source, docs, and config files.
rg "X-Adopta-Tenant-Id|X-Adopta-Test-" src/Adopta.Web -g "!src/Adopta.Web/Studio/StudioApiRequestBoundaryHandler.cs" -g "!**/bin/**" -g "!**/obj/**"
rg "PostAsync|PutAsync|PatchAsync|DeleteAsync|/request-review|/approve|/reject|/publish" src/Adopta.Web/Studio/StudioAuthoringReadApiClient.cs
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
git diff --check
```

### Known limitations

- No environment values are committed.
- No live Studio reads are activated by default.
- The rehearsal uses fake test-only transport and access seams.
- The rehearsal does not validate a deployed identity provider or real API environment.
- Live write/workflow/publish API integration remains separately approval-gated.

### Next recommended slice

Add an operator-facing read API activation status surface only if explicitly approved, using the existing preflight and rehearsal outputs. Keep live write/workflow/publish activation, backend changes, appsettings values, migrations, analytics, AI, deployment automation, and Property MTD integration separately approved.
