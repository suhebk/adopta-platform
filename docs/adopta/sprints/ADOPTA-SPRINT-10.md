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
