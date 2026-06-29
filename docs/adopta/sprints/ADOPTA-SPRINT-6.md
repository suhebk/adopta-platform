# ADOPTA-SPRINT-6 - Runtime Delivery API And Bundle Retrieval Foundation

## Sprint intent

Sprint 6 creates a safe, tenant-scoped runtime delivery boundary for retrieving runtime delivery bundle contracts. The sprint must not add runtime rendering, external publishing, CDN, Blob Storage, analytics, AI, browser extension, Property MTD integration, migration execution, database creation, production Azure deployment automation, or production database mutation.

## Slice 1 - Delivery API contract endpoint foundation

### Requirement IDs covered

- `FR-IDN-031` - Added tenant-scoped runtime delivery API lookup boundary.
- `FR-RBAC-001` - Added explicit `RuntimeDelivery.Read` permission through the existing `AdoptaPermissionKeys` catalog.
- `FR-AUT-016` - Reused existing runtime delivery bundle contracts for bundle retrieval.
- `NFR-SEC-1` - Kept delivery responses and errors non-sensitive and avoided tokens, headers, raw claims, form/input values, tax/HMRC/property data, connection strings, secrets, and sensitive values.
- `NFR-TEST-1` - Added integration tests for authorization, validation, safe not-found behaviour, cross-tenant hiding, and safe response shape.

### Scope delivered

- Added `GET /runtime/delivery/bundles/{applicationId}?environment={environment}&channel={channel}`.
- Required tenant context with the existing tenant endpoint filter.
- Required `AdoptaPermissionKeys.RuntimeDeliveryRead`.
- Used the existing `IDeliveryBundleRepository` seam.
- Used the existing `DeliveryBundleLookupRequest` and runtime content bundle contracts.
- Returned safe delivery metadata and existing runtime content contract data.
- Returned safe typed validation issues for invalid lookup requests.
- Returned safe not-found responses for missing or access-denied lookup results.

### Assumptions

The endpoint is a runtime delivery contract boundary only. It does not render content, publish bundles externally, call a CDN, write to Blob Storage, call analytics, or mutate production infrastructure.

Default behaviour remains in-memory through the existing delivery bundle repository. SQL Server/EF persistence remains opt-in and unchanged.

Runtime content contracts must remain privacy-safe. Delivery responses must not include tokens, headers, raw claims, form values, input values, tax data, HMRC data, property data, connection strings, secrets, tenant secrets, credentials, or sensitive values.

### Explicitly not built

- Runtime renderer.
- CDN publishing.
- Blob Storage publishing.
- Delivery external storage.
- External publishing transport.
- Analytics pipeline.
- Event Hubs.
- ClickHouse.
- AI assistant.
- Browser extension.
- Property MTD integration.
- Studio UI.
- Production Azure deployment automation.
- Large-scale infrastructure.
- EF migrations.
- Migration execution.
- Database creation.
- Automatic startup migration.
- Live database health checks.
- Real SQL Server connectivity checks.
- Appsettings changes.
- Deployment files.

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
```

### Known limitations

- Delivery bundle repository remains in-memory only in this slice.
- No delivery bundle external storage exists.
- No runtime renderer exists.
- No CDN, Blob Storage, analytics, or external publishing transport exists.
- Production database migration execution remains unapproved.

### Next recommended slice

Add a controlled runtime delivery follow-up that hardens delivery bundle persistence and operational validation without adding external storage, CDN, Blob Storage, renderer behaviour, or migration execution.

## Slice 2 - Publishing-to-delivery store seam

### Requirement IDs covered

- `FR-AUT-016` - Reused the existing publishing workflow output and runtime delivery bundle contracts.
- `FR-IDN-031` - Preserved tenant-scoped delivery storage and lookup through the existing delivery repository seam.
- `FR-RBAC-001` - Kept publishing and runtime delivery route authorization on the existing explicit permission filters.
- `NFR-SEC-1` - Stored and returned only safe runtime delivery bundle contract metadata/content and did not add tokens, headers, raw claims, form/input values, tax/HMRC/property data, connection strings, secrets, or sensitive values.
- `NFR-TEST-1` - Added integration coverage proving successful publish stores a bundle, delivery API retrieval succeeds, invalid/failed/cross-tenant publish commands store nothing, and wrong delivery scopes return safe not-found responses.

### Scope delivered

- Updated the existing publish endpoint to store the generated `DeliveryBundle` through `IDeliveryBundleRepository` only after `AuthoredContentPublishingWorkflow` returns `Succeeded`.
- Preserved success-only publishing history persistence.
- Kept failed, denied, invalid, and cross-tenant publish commands non-persistent.
- Kept the default delivery store in-memory.
- Left SQL Server/EF persistence opt-in behaviour unchanged.
- Added publish-to-delivery integration tests covering runtime delivery retrieval by tenant/application/environment/channel.

### Assumptions

The publishing workflow remains the source of the generated delivery bundle. The API layer commits that workflow output to the delivery repository only after successful publish validation.

Delivery retrieval remains tenant-scoped through `IAdoptionTenantContext` and `IDeliveryBundleRepository.LookupAsync`. Cross-tenant and wrong-scope lookups return safe not-found responses and must not reveal whether another tenant, application, environment, or channel has a bundle.

### Explicitly not built

- Runtime renderer.
- CDN publishing.
- Blob Storage publishing.
- Delivery external storage.
- External publishing transport.
- Analytics pipeline.
- Event Hubs.
- ClickHouse.
- AI assistant.
- Browser extension.
- Property MTD integration.
- Studio UI.
- Production Azure deployment automation.
- EF migrations.
- Migration execution.
- Database creation.
- Automatic startup migration.
- Live database health checks.
- Real SQL Server connectivity checks.
- Appsettings changes.
- Deployment files.
- Database schema changes.

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
```

### Known limitations

- Delivery bundle storage remains in-memory by default.
- No external delivery store exists.
- No CDN, Blob Storage, renderer, analytics, or external publishing transport exists.
- The publishing endpoint stores the bundle and history through separate repository calls; a future durable implementation should consider transactional consistency when SQL Server persistence is enabled.
- Production database migration execution remains unapproved.

### Next recommended slice

Add controlled delivery persistence hardening and operational validation for runtime bundle storage, while keeping external storage, CDN, Blob Storage, renderer behaviour, migration execution, and production deployment automation out of scope until explicitly approved.

## Slice 3 - Runtime SDK delivery client

### Requirement IDs covered

- `FR-AUT-016` - Added a framework-agnostic runtime SDK delivery client for retrieving existing delivery bundle contracts.
- `FR-IDN-031` - Mapped SDK bundle retrieval to the tenant-scoped runtime delivery API boundary without sending tenant identity from the SDK.
- `NFR-SEC-1` - Kept delivery errors and logger metadata safe and avoided tokens, headers, claims, secrets, connection strings, raw DOM text, field/form values, tax/HMRC/property data, and user-entered values.
- `NFR-PERF-1` - Added timeout/cancellation boundaries to the SDK delivery request contract.
- `NFR-TEST-1` - Added TypeScript tests for URL construction, success normalization, validation, status mapping, network failure, timeout/cancellation, malformed responses, and safe logging/result messages.

### Scope delivered

- Added `DeliveryClient` to `@adopta/runtime-sdk`.
- Added delivery client options, request, result, and transport contracts.
- Added a minimal fetch-backed transport abstraction for framework-agnostic use and testability.
- Mapped SDK requests to `GET /runtime/delivery/bundles/{applicationId}?environment={environment}&channel={channel}`.
- Normalized successful API responses into the existing `ContentBundle` contract.
- Validated returned content using `validateContentBundle`.
- Returned typed safe failures for invalid request, unauthorized, forbidden, not found, network failure, timeout, unexpected response, and bundle validation failure.

### Assumptions

Tenant identity remains server-side. The SDK does not send tenant ID in route, query, or body. The optional `expectedTenantId` client option is validation-only and is never added to the delivery API request.

Authentication remains host-owned. The SDK does not accept, store, log, or expose tokens or headers. Hosts that need custom authorization can provide a custom delivery transport without making token handling part of the SDK contract.

The delivery client is contract-only. It retrieves and validates bundle data but does not render guidance, mutate DOM, mount components, inspect page contents, capture form values, or evaluate targeting.

### Explicitly not built

- Runtime renderer.
- Tooltip renderer.
- Banner renderer.
- Walkthrough renderer.
- Checklist renderer.
- DOM mounting.
- DOM mutation.
- Analytics pipeline.
- CDN publishing.
- Blob Storage publishing.
- Delivery external storage.
- Event Hubs.
- ClickHouse.
- AI assistant.
- Browser extension.
- Property MTD integration.
- Studio UI.
- Production Azure deployment automation.
- EF migrations.
- Migration execution.
- Database creation.
- Automatic startup migration.
- Live database health checks.
- Real SQL Server connectivity checks.
- Appsettings changes.
- Deployment files.
- Database schema changes.

### Commands to run

```powershell
pnpm typecheck
pnpm build
pnpm test
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
rg "AddHealthChecks|IHealthCheck|CanConnect|OpenConnection|SqlConnection|AddSqlServer" src tests -g "!**/bin/**" -g "!**/obj/**"
```

### Known limitations

- The delivery client retrieves and validates bundles only; it does not render content.
- Authentication remains host-owned through browser/session behavior or a caller-provided transport.
- The SDK has no external delivery storage, CDN, Blob Storage, analytics, browser extension, or Property MTD integration.
- The runtime demo remains local/static and was not changed in this slice.

### Next recommended slice

Add runtime renderer foundation for safe, accessible, non-invasive display of retrieved bundle content, while keeping analytics, external delivery storage, CDN, Blob Storage, browser extension, Property MTD integration, and production deployment automation out of scope until explicitly approved.
