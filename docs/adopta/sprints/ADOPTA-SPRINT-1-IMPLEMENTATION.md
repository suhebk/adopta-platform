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
