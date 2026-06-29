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

Add a controlled publishing-to-delivery-store seam that stores successful publish bundle contracts in the existing in-memory delivery repository by default, without adding external storage, CDN, Blob Storage, renderer behaviour, or migration execution.
