# ADOPTA-SPRINT-9 - Authenticated Studio API Integration

## Sprint objective

Replace local-only Adoption Studio seams with authenticated, tenant-aware, permission-aware API integration in controlled increments, without damaging the existing Studio UX or introducing unsafe infrastructure drift.

## Slice 1 - Studio read API client seam

### Requirement IDs covered

- `FR-AUT-001` - Added a read-only Studio authoring API client seam for authored content list and detail retrieval.
- `FR-AUT-002` - Preserved the existing Studio content inventory/view boundary through `IStudioContentClient`.
- `FR-GOV-002` - Aligned the seam with the existing `Authoring.Read` protected backend endpoints.
- `NFR-SEC-1` - Kept tenant identity server-side and did not add tenant IDs to Web request models or page input.
- `NFR-SEC-2` - Added safe result/error handling for unauthorized, forbidden, not found, invalid response, unavailable transport, and unexpected errors.
- `NFR-TEST-1` - Added unit coverage for API client mapping, safe failures, mapper behaviour, tenant identifier dropping, and page boundary usage.

### Scope delivered

- Added Web-local authoring read API response DTOs.
- Added `StudioAuthoringReadApiMapper` for safe API-to-Studio read model mapping.
- Added inactive `StudioAuthoringReadApiClient` implementation of `IStudioContentClient`.
- Targeted only existing backend read endpoints:
  - `GET /authoring/content`;
  - `GET /authoring/content/{contentId:guid}`.
- Kept `/studio/content` on the existing local client path.
- Kept `LocalStudioContentClient` as the default registered UI client.
- Kept create, update, review, approve, reject, and publish operations disabled in the API read client seam.

### Slice 1 findings

Live read activation is not yet safe in this slice because:

- `Adopta.Web` has no approved token acquisition or API token propagation seam.
- `Adopta.Web` has no approved authenticated tenant context propagation boundary.
- There is no approved Web API base URL configuration.
- The backend authoring read contract does not yet return all current Studio UI fields, including content type and full audit/history summary.

The slice therefore adds the read client seam and tests it, but deliberately does not activate it in dependency injection or route the Studio page to live API reads.

### Backend endpoint mapping

The read client maps:

- `200 OK` list responses to `StudioContentPageModel`.
- `200 OK` get responses to `StudioContentListItem`.
- `401 Unauthorized` to `StudioContentClientStatus.Unauthorized`.
- `403 Forbidden` to `StudioContentClientStatus.Forbidden`.
- `404 NotFound` to `StudioContentClientStatus.NotFound`.
- malformed or unsupported JSON to `StudioContentClientStatus.InvalidResponse`.
- network or transport failures to `StudioContentClientStatus.Unavailable`.
- unexpected exceptions to `StudioContentClientStatus.UnexpectedError`.

All failure messages are generic and safe.

### Mapper assumptions

The backend authoring read response includes tenant IDs because the API contract is tenant-scoped. The Web mapper drops tenant identifiers from Studio UI models.

The backend authoring read response does not include content type. The mapper preserves compatibility with the current Studio read model and marks API content type as unavailable via `HasKnownContentType = false`.

The backend authoring read response does not include full audit/history records. The mapper uses limited safe metadata from version summaries and sets a generic history message.

### Explicitly not built

- Live `/studio/content` API activation.
- Live create draft.
- Live update draft.
- Live request review.
- Live approve.
- Live reject.
- Live publish.
- API token propagation.
- Backend API changes.
- EF migrations.
- Database schema changes.
- Appsettings changes.
- Deployment automation.
- Analytics.
- AI assistant.
- Event Hubs.
- ClickHouse.
- Browser extension.
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
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
rg "AddScoped<IStudioContentClient, StudioAuthoringReadApiClient>|AddHttpClient|BaseAddress|AuthenticationStateProvider|GetTokenAsync|Authorization|Bearer" src/Adopta.Web -g "!**/bin/**" -g "!**/obj/**"
rg "CreateDraftAsync|UpdateDraftAsync|RequestReviewAsync|ApproveAsync|RejectAsync|PublishAsync" src/Adopta.Web/Studio/StudioAuthoringReadApiClient.cs
```

### Known limitations

- The API read client seam is not active in DI.
- `/studio/content` still uses local foundation data.
- No Web token propagation exists.
- No Web API base URL configuration exists.
- Content type remains unavailable from the backend read contract.
- Full audit/history details remain unavailable from the backend read contract.
- Write/workflow/publish operations remain local or disabled until separately approved.

### Next recommended slice

Add Web auth/token propagation and explicit API client activation planning, including safe API base URL configuration, authenticated server-side tenant handling, and `Authoring.Read` enforcement validation. Keep live writes, workflow commands, publishing, backend drift, migrations, appsettings changes, analytics, AI, deployment automation, and Property MTD integration separately approved.

## Slice 2 - Web-to-API auth and tenant propagation boundary

### Requirement IDs covered

- `FR-IDN-001` - Added a Web-side boundary for future server-side authenticated API calls.
- `FR-IDN-005` - Added an access-token provider abstraction without exposing or logging token values.
- `FR-IDN-012` - Preserved API-side tenant resolution from validated identity context; Web does not supply tenant identifiers.
- `FR-IDN-031` - Added tests proving tenant and test-auth headers are stripped before future API forwarding.
- `FR-GOV-002` - Preserved the existing `Authoring.Read` read-client seam without activating live reads.
- `NFR-SEC-1` - Kept the boundary disabled/unavailable by default and avoided insecure development shortcuts.
- `NFR-SEC-2` - Added generic, non-sensitive failure behaviour for unavailable or invalid API access.
- `NFR-TEST-1` - Added unit coverage for boundary service registration, token handling, header stripping, and no live activation.

### Scope delivered

- Added `StudioApiClientOptions` for future explicit Studio API configuration.
- Added `IStudioApiAccessTokenProvider` and `StudioApiAccessTokenResult`.
- Added `UnavailableStudioApiAccessTokenProvider` as the default safe server-side provider.
- Added `StudioApiRequestBoundaryHandler` to attach `Authorization: Bearer` only when a future server-side provider supplies a valid access value.
- Added header guardrails that strip:
  - `X-Adopta-Tenant-Id`;
  - all `X-Adopta-Test-*` headers.
- Added `StudioApiServiceCollectionExtensions` and registered the disabled boundary in `Adopta.Web`.
- Preserved `IStudioContentClient -> LocalStudioContentClient` as the default Studio UI client.
- Kept `StudioAuthoringReadApiClient` inactive.

### Slice 2 findings

Live Studio read activation remains intentionally deferred because:

- `Adopta.Web` still has no approved interactive sign-in/token acquisition implementation.
- No approved production API access token provider exists yet.
- No approved Web API base address configuration exists in repository configuration.
- The API correctly derives tenant context from authenticated claims plus server-side tenant mappings, so Web must not send tenant IDs as a shortcut.
- API test-auth headers are integration-test-only and must not become a Web production integration strategy.

This slice therefore creates the safe boundary needed for future activation but does not switch `/studio/content` to live API data.

### Auth boundary design

- `IStudioApiAccessTokenProvider` is a server-side seam only.
- The default provider returns `Unavailable` with a generic safe message.
- `StudioApiRequestBoundaryHandler` returns a generic service-unavailable response when API access is unavailable or invalid.
- Token values are not included in result messages, handler error responses, logs, page models, or request models.
- The handler only sets an `Authorization` header after the server-side provider returns a non-empty value that is safe for an HTTP header.

### Tenant boundary design

- Tenant resolution remains API-side.
- The API tenant context must continue to come from validated token claims and server-side tenant/user mappings.
- Web request/page models do not include tenant IDs.
- Web production code does not add `X-Adopta-Tenant-Id`.
- Web production code strips `X-Adopta-Test-*` headers before forwarding future API requests.

### Explicitly not built

- Live `/studio/content` API activation.
- Live create draft.
- Live update draft.
- Live request review.
- Live approve.
- Live reject.
- Live publish.
- Real token acquisition.
- Browser-context token forwarding.
- Backend API changes.
- EF migrations.
- Database schema changes.
- Appsettings changes.
- Deployment automation.
- Analytics.
- AI assistant.
- Event Hubs.
- ClickHouse.
- Browser extension.
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
rg "StudioAuthoringReadApiClient|HttpClient|GetAsync|/authoring/content" src/Adopta.Web/Components/Pages/Studio/StudioContent.razor
rg "AddScoped<IStudioContentClient, LocalStudioContentClient>" src/Adopta.Web/Program.cs
rg "AddScoped<IStudioContentClient, StudioAuthoringReadApiClient>|AddHttpClient<IStudioContentClient|AddHttpClient<StudioAuthoringReadApiClient" src/Adopta.Web/Program.cs
rg "X-Adopta-Tenant-Id|X-Adopta-Test-" src/Adopta.Web -g "!src/Adopta.Web/Studio/StudioApiRequestBoundaryHandler.cs" -g "!**/bin/**" -g "!**/obj/**"
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
```

### Known limitations

- The Web auth/token provider remains unavailable by default.
- The Studio API base address is modeled but not configured.
- `/studio/content` still uses the local foundation client.
- `StudioAuthoringReadApiClient` remains inactive.
- No live API integration is enabled until a production-grade sign-in/token acquisition design is approved.
- Write/workflow/publish operations remain local or disabled until separately approved.

### Next recommended slice

Implement the authenticated Web sign-in and server-side API token acquisition seam, still without live write/workflow/publish activation. The next slice should decide how Web obtains an API access token securely, how production configuration supplies the API base address without repository secrets, and whether read-only Studio API activation can be enabled behind explicit configuration.

## Slice 3 - Authenticated Web sign-in and server-side API token acquisition seam

### Requirement IDs covered

- `FR-IDN-001` - Added typed Web authentication options for the future Microsoft Entra sign-in seam.
- `FR-IDN-005` - Added a server-side Studio API access provider seam that can use a saved server-side access value when future Web sign-in is enabled.
- `FR-IDN-012` - Preserved API-side tenant resolution from validated identity claims and server-side tenant mapping.
- `FR-IDN-031` - Added tests proving Web page/request models do not accept tenant IDs and Web does not use test headers as an integration strategy.
- `FR-GOV-002` - Preserved the existing authoring read API seam without activating live Studio reads.
- `NFR-SEC-1` - Kept authentication and API access disabled by default; no secrets or real configuration values were added.
- `NFR-SEC-2` - Added safe validation and provider failures that do not expose raw exceptions or sensitive values.
- `NFR-TEST-1` - Added tests for configuration safety, provider behaviour, non-disclosure, and no live activation.

### Scope delivered

- Added disabled-by-default `StudioWebAuthenticationOptions`.
- Added disabled-by-default `StudioApiTokenAcquisitionOptions`.
- Added `StudioWebAuthenticationConfigurationValidator` and typed validation result/issue records.
- Added `MicrosoftIdentityStudioApiAccessTokenProvider` as a server-side provider seam.
- Added `StudioWebAuthenticationServiceCollectionExtensions`.
- Registered the Web authentication seam in `Adopta.Web` without changing the default Studio client.
- Kept `IStudioContentClient -> LocalStudioContentClient` as the default.
- Kept `StudioAuthoringReadApiClient` inactive.
- Kept `/studio/content` on local foundation data.

### Slice 3 findings

Live read activation remains deferred because:

- `Adopta.Web` still has no approved production sign-in UI or sign-out UI.
- No real Microsoft Entra authority, client ID, API base address, scopes, or secret material is committed.
- Production secrets must come from secure external configuration.
- The API already owns tenant resolution through validated identity claims and server-side tenant/user mappings.
- Web must not send tenant IDs, tenant headers, or API test-auth headers.

This slice therefore adds the Web sign-in/token acquisition seam only. It does not enable live reads.

### Future configuration shape

The future secure configuration shape is documented conceptually only. Repository files must not contain real values:

```json
{
  "Authentication": {
    "StudioWeb": {
      "Enabled": false,
      "Authority": "<secure-authority-url-from-environment>",
      "ClientId": "<client-id-from-secure-configuration>",
      "CallbackPath": "/signin-oidc"
    }
  },
  "StudioApi": {
    "TokenAcquisition": {
      "Enabled": false,
      "Scopes": [
        "<api-scope-from-secure-configuration>"
      ]
    }
  }
}
```

### Auth and token design

- Authentication is disabled by default.
- API access acquisition is disabled by default.
- Enabled Web authentication requires complete non-secret settings.
- Enabled API access acquisition requires explicit scopes.
- Invalid configuration fails with a generic non-sensitive startup error.
- The server-side provider reads only server-side authenticated context.
- The provider does not accept browser-supplied access values.
- The provider does not return access values to UI models.
- Provider failures return generic unavailable results.
- The existing `StudioApiRequestBoundaryHandler` remains the only place future Authorization headers are attached.

### Tenant boundary design

- Tenant IDs are not accepted from page models, request models, routes, query strings, forms, browser headers, or UI state.
- `X-Adopta-Tenant-Id` is not added by Web.
- `X-Adopta-Test-*` headers are not used as a Web production path.
- Tenant resolution remains API-side through validated token claims and server-side tenant mapping.

### Explicitly not built

- Live `/studio/content` API activation.
- Live create draft.
- Live update draft.
- Live request review.
- Live approve.
- Live reject.
- Live publish.
- Sign-in/sign-out UI endpoints.
- Browser-supplied token forwarding.
- Backend API changes.
- EF migrations.
- Database schema changes.
- Appsettings changes.
- Deployment automation.
- Analytics.
- AI assistant.
- Event Hubs.
- ClickHouse.
- Browser extension.
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
rg "StudioAuthoringReadApiClient|HttpClient|GetAsync|/authoring/content" src/Adopta.Web/Components/Pages/Studio/StudioContent.razor
rg "AddScoped<IStudioContentClient, LocalStudioContentClient>" src/Adopta.Web/Program.cs
rg "AddScoped<IStudioContentClient, StudioAuthoringReadApiClient>|AddHttpClient<IStudioContentClient|AddHttpClient<StudioAuthoringReadApiClient" src/Adopta.Web/Program.cs
rg "X-Adopta-Tenant-Id|X-Adopta-Test-" src/Adopta.Web -g "!src/Adopta.Web/Studio/StudioApiRequestBoundaryHandler.cs" -g "!**/bin/**" -g "!**/obj/**"
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
```

Results:

- `dotnet test Adopta.slnx` passed after restore completed with package feed access: 299 unit tests and 90 integration tests.
- `dotnet build Adopta.slnx --configuration Release --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test Adopta.slnx --configuration Release --no-build` passed: 299 unit tests and 90 integration tests.
- `pnpm typecheck`, `pnpm build`, and `pnpm test` passed.
- No legacy .NET 9 target references were found.
- `/studio/content` remains local-only and does not activate `StudioAuthoringReadApiClient`.
- `IStudioContentClient` still defaults to `LocalStudioContentClient`.
- No Web tenant/test header forwarding was added outside the existing request boundary handler.
- No migration execution, database creation, startup mutation, live DB connectivity, or health-check registrations were added.

### Known limitations

- Web sign-in UI is not implemented.
- Real token acquisition against Microsoft Entra is not enabled.
- No production authority, client ID, API scope, API base address, or secret source is configured in repository files.
- `/studio/content` still uses local foundation data.
- `StudioAuthoringReadApiClient` remains inactive.
- Write/workflow/publish operations remain local or disabled until separately approved.

### Next recommended slice

Add explicit, secure configuration activation for read-only Studio API integration after production Web sign-in and API access settings are provided through secure configuration. Keep live write/workflow/publish integration separately approved.
