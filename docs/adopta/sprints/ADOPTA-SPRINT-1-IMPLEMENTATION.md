# ADOPTA-SPRINT-1 Implementation Notes

## Slice 2 - API health, DI, tenant context placeholder, audit skeleton

### Requirement IDs covered

- `FR-IDN-010` - RBAC service registration and allow/deny tests for permission checks.
- `FR-IDN-012` - Tenant context convention added for server-side tenant-scoped flows.
- `FR-IDN-030` - Tenant-scoped service patterns continue to require a tenant id on scoped records.
- `FR-IDN-031` - Tenant isolation and API health tests extended.
- `FR-IDN-040` - Temporary audit service skeleton added for Sprint 1 foundation work.
- `NFR-OBS-1` - Health and readiness endpoints added as an observability baseline.

### Scope delivered

- `GET /health/live` returns liveness without tenant context.
- `GET /health/ready` validates that required Slice 2 services resolve from dependency injection.
- `AddAdoptaApplication()` registers application-layer services.
- `AddAdoptaInfrastructure()` registers infrastructure-layer Sprint 1 services.
- `X-Adopta-Tenant-Id` is supported only as a Sprint 1 development/test placeholder.
- `InMemoryAdoptionAuditService` provides a temporary audit implementation for tests and early wiring.
- GitHub Actions CI restores, builds, and tests `Adopta.slnx` using the .NET SDK from `global.json`.

### Security and tenant-resolution assumptions

The `X-Adopta-Tenant-Id` header is not a production trust boundary. It exists only to exercise tenant-context plumbing before Microsoft Entra authentication is wired in. Production tenant resolution must come from validated Microsoft Entra token claims plus server-side tenant mappings.

Malformed tenant headers return a safe `400` response without exception details. Tenant-required endpoint flows must deny missing tenant context; the reusable endpoint filter pattern has been added, and concrete tenant-required API endpoints should use it as they are introduced.

### Audit limitations

`InMemoryAdoptionAuditService` is temporary and non-durable. Durable audit persistence, immutability controls, retention, export, and database-backed tenant isolation remain later Sprint 1 work.

### Commands to run

```powershell
dotnet test Adopta.slnx
rg "net9\.0" .
```

### Known limitations

- No database, Event Hubs, ClickHouse, Azure readiness, or external dependency checks are included.
- No production authentication or Entra-derived tenant resolver is implemented yet.
- Tenant-required endpoint tests are deferred until the first tenant-required API endpoint is introduced.
- CI is build/test only; it does not deploy or provision Azure infrastructure.

### Next recommended slice

Implement the Entra authentication shell and production tenant-resolution design seam: validated token claims, server-side tenant mapping, and first tenant-required API endpoint protected by the tenant context filter.

## Slice 3 - Entra auth seam, production tenant-resolution seam, tenant diagnostic endpoint

### Requirement IDs covered

- `FR-IDN-001` - Microsoft Entra authentication configuration seam added without requiring live tenant secrets for local build/test.
- `FR-IDN-005` - Tenant resolution is designed to run after authentication and read only validated principal claims.
- `FR-IDN-010` - Controlled permission-key catalog and fail-closed permission evaluator/filter pattern added.
- `FR-IDN-012` - Tenant-required endpoint pattern now protects the first diagnostic tenant-context endpoint.
- `FR-IDN-031` - Tests cover missing, malformed, and valid tenant context behavior.
- `FR-IDN-040` - Audit remains temporary/in-memory; no durable audit persistence added in this slice.

### Scope delivered

- Added non-secret Microsoft Entra authentication configuration placeholders under `Authentication:MicrosoftEntra`.
- Added production tenant-resolution seam through `IProductionTenantResolver`.
- Added `ClaimsPrincipalProductionTenantResolver`, which reads configured claim types from an authenticated `ClaimsPrincipal`.
- Added explicit separation between authenticated production claim resolution and the Sprint 1 development/test `X-Adopta-Tenant-Id` header.
- Added `GET /diagnostics/tenant-context`, protected by the tenant-required endpoint filter.
- Added a small permission catalog:
  - `Diagnostics.Read`
  - `Tenants.Read`
  - `Tenants.Manage`
  - `Applications.Read`
  - `Applications.Manage`
  - `Audit.Read`
- Added fail-closed permission evaluator and endpoint filter pattern.

### Security and tenant-resolution assumptions

Production tenant resolution does not trust request headers, query strings, request bodies, or arbitrary client-supplied tenant values. It reads only from an authenticated `ClaimsPrincipal` after authentication has run. Normal missing-claim cases return an unresolved result instead of throwing, so protected endpoints fail closed.

The `X-Adopta-Tenant-Id` header remains development/test-only. The middleware uses the production claim resolver for authenticated principals and only considers the header for unauthenticated local/test traffic. This keeps the temporary header path separate from the production trust boundary.

The diagnostic endpoint returns only minimal tenant context information: tenant id and whether an external tenant id exists. It does not return raw tokens, full claims, bearer tokens, request headers, email addresses, or sensitive identity details.

### Audit limitations

No new durable audit persistence was added. In-memory audit remains temporary. Tenant/security audit events should be wired once authenticated actors and durable audit storage are introduced.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
rg "net9\.0" .
```

### Known limitations

- The API does not yet validate JWT bearer tokens.
- Entra authority/audience configuration values are placeholders and contain no secrets.
- Server-side tenant mapping is not implemented yet; the resolver currently parses the configured tenant claim as the internal tenant id for the Sprint 1 seam.
- Permission filters are present but no authenticated production user/role mapper exists yet.
- Audit remains in-memory and non-durable.

### Next recommended slice

Add the first production authentication shell using JWT bearer validation configuration, then introduce server-side tenant mapping and authenticated user-to-role mapping while keeping local tests independent from live Azure resources.
