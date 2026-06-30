# ADOPTA-SPRINT-8 - Adoption Studio UI Foundation

## Sprint intent

Sprint 8 builds the enterprise-grade Adoption Studio UI on top of the existing authoring, lifecycle, approval, publishing, delivery, and runtime foundations. The sprint must preserve tenant isolation, permission boundaries, safe output, and production-quality UI conventions while avoiding unapproved backend, persistence, analytics, AI, deployment, or Property MTD integration work.

## Slice 1 - Studio content list and view

### Requirement IDs covered

- `FR-AUT-001` - Added a read-only authored content inventory foundation for Adoption Studio.
- `FR-AUT-004` - Displayed authored content lifecycle state and version metadata.
- `FR-AUT-007` - Included a safe structural audit/history summary.
- `FR-GOV-002` - Preserved Studio navigation permission metadata for `Authoring.Read`.
- `NFR-A11Y-1` - Used semantic headings, table markup, labels, status text, and safe state messaging.
- `NFR-SEC-1` - Kept the local read model structural and excluded sensitive values, content body, tokens, headers, claims, secrets, connection strings, tax/HMRC/property data, and user-entered sensitive values.
- `NFR-TEST-1` - Added unit coverage for Studio content metadata, lifecycle states, safe UI states, route permission mapping, and unsafe output guardrails.

### Scope delivered

- Replaced the `/studio/content` placeholder with a read-only content list and selected-content detail foundation.
- Added a compact enterprise page header and local-data notice.
- Added lifecycle summary counts for Draft, InReview, Approved, Published, and Archived.
- Added a responsive authored content inventory table.
- Added lifecycle status chips.
- Added a selected content detail panel.
- Added version metadata and safe audit/history summary sections.
- Added explicit UI state branches for loading, empty, error, not authorized, and loaded states.
- Added Web-local safe foundation read models that can be replaced by authenticated tenant-aware API integration in a later slice.
- Added scoped component CSS for the Studio content view.

### Data assumptions

The Slice 1 Studio content page uses local foundation data only. This is not live production data and is not presented as live production data. The shape mirrors safe authored content metadata already available through existing authoring API boundaries, but no API client, token propagation, or tenant-aware web integration is added in this slice.

The local read model stores structural metadata only:

- content ID;
- application ID;
- content key;
- title;
- lifecycle state;
- version metadata;
- safe lifecycle/publishing event counts;
- safe latest activity text.

It does not include content body, raw authored content, tokens, headers, claims, secrets, connection strings, tax data, HMRC data, property data, or user-entered sensitive values.

### Tenant and permission assumptions

The `/studio/content` route remains mapped through `StudioNavigation` to the existing `AdoptaPermissionKeys.AuthoringRead` permission. No new permission keys are added.

Full Web UI authentication, tenant-aware API calls, and token propagation are deferred to a later approved slice. This slice includes safe not-authorized UI state support but does not implement full Web auth enforcement.

### Explicitly not built

- Create forms.
- Edit forms.
- Review, approve, or reject actions.
- Publish actions.
- Live API client.
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
rg "token|header|claim|ConnectionString|Password|Secret|HMRC|tax|property|MarkupString|innerHTML|<form|<input" src/Adopta.Web/Components/Pages/Studio/StudioContent.razor src/Adopta.Web/Studio/StudioContentReadModel.cs src/Adopta.Web/Studio/StudioContentFoundationData.cs
```

### Known limitations

- The Studio content page uses local foundation data only.
- No live authoring API integration is wired into the Web app.
- No create, edit, review, approval, rejection, or publish action UI exists.
- No Web auth/token propagation implementation is added.
- Audit/history is summary-only and structural.

### Next recommended slice

Add authenticated tenant-aware Web API client boundaries for read-only Studio content retrieval, keeping create/edit/review/publish actions, backend behavior changes, migrations, appsettings changes, analytics, AI, deployment automation, and Property MTD integration out of scope unless separately approved.

## Slice 2 - Authenticated Studio content API client boundary

### Requirement IDs covered

- `FR-AUT-001` - Added a read-only Studio content client boundary for authored content retrieval.
- `FR-GOV-002` - Preserved `/studio/content` mapping to the existing `Authoring.Read` permission through Studio navigation.
- `NFR-SEC-1` - Kept tenant/auth boundaries explicit and avoided client-supplied tenant IDs.
- `NFR-SEC-2` - Added safe client result statuses for unauthorized, forbidden, not found, invalid response, unavailable, and unexpected error conditions.
- `NFR-TEST-1` - Added unit coverage for local client behaviour, safe errors, non-sensitive messages, no tenant request input, and page/client boundary usage.

### Scope delivered

- Added `IStudioContentClient` as the read-only Studio content retrieval boundary.
- Added list and get-by-ID request models.
- Added typed safe client result/status models.
- Added `LocalStudioContentClient` as the Slice 2 foundation implementation.
- Registered the local client in `Adopta.Web` dependency injection.
- Updated `/studio/content` to load through `IStudioContentClient`.
- Kept the Studio content page read-only.
- Kept local foundation data as the backing source behind the local client.
- Added tests for result handling, safe messages, local list/get behaviour, no tenant request input, and page/client boundary usage.

### Client boundary assumptions

The client boundary is ready for a future authenticated tenant-aware implementation, but this slice does not call the live API. The Web project still does not own token propagation, authenticated server context mapping, or an API base-address configuration seam.

Studio content requests do not accept tenant IDs from the browser/page. Future tenant resolution must come from authenticated server context and API token propagation rather than client-supplied tenant values.

The local implementation is a foundation/development seam only. It is not a production trust boundary and must be replaced by a live authenticated client in a later approved slice.

### Safe error handling

Client results use these statuses:

- `Success`;
- `Unauthorized`;
- `Forbidden`;
- `NotFound`;
- `InvalidResponse`;
- `Unavailable`;
- `UnexpectedError`.

Messages are generic and non-sensitive. They do not include raw exceptions, tokens, headers, claims, connection strings, secrets, tenant secrets, tax data, HMRC data, property data, user-entered sensitive values, or sensitive content.

### Explicitly not built

- Live API calls.
- API token propagation.
- HTTP client configuration.
- Client-supplied tenant IDs.
- Create or edit forms.
- Review, approve, or reject actions.
- Publish actions.
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
rg "HttpClient|Authorization|Bearer|token|header|claim|ConnectionString|Password|Secret|HMRC|tax|property|MarkupString|innerHTML|<form|<input" src/Adopta.Web/Components/Pages/Studio/StudioContent.razor src/Adopta.Web/Studio tests/Adopta.UnitTests/StudioContentClientTests.cs tests/Adopta.UnitTests/StudioContentPageTests.cs
```

### Known limitations

- The Studio content client uses local foundation data only.
- No live authenticated API integration is implemented.
- No Web token propagation or API client configuration exists.
- No create, edit, review, approval, rejection, or publish UI exists.

### Next recommended slice

Add authenticated tenant-aware HTTP/API client implementation planning for read-only Studio content retrieval, including token propagation and safe configuration seams, while keeping action UI, backend behavior changes, migrations, appsettings changes, analytics, AI, deployment automation, and Property MTD integration separately approved.
