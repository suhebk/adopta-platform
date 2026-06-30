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
