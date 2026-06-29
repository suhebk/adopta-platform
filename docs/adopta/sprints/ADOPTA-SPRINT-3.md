# ADOPTA-SPRINT-3 - Adoption Studio and Authoring Foundations

## Sprint intent

Sprint 3 establishes the authoring and governance foundation for Adoption Studio without building full UI screens, runtime rendering, analytics, AI, browser extensions, Property MTD integration, or production database infrastructure.

## Slice 1 - Authoring domain and lifecycle model

### Requirement IDs covered

- `FR-AUT-001` - Added tenant-scoped authored content domain entities.
- `FR-AUT-002` - Added explicit authored content version metadata.
- `FR-AUT-003` - Added explicit lifecycle states: `Draft`, `InReview`, `Approved`, `Published`, and `Archived`.
- `FR-AUT-004` - Added explicit lifecycle transition validation.
- `FR-AUT-005` - Added application-layer authoring contracts and typed validation issues.
- `FR-IDN-031` - Authoring repository access is tenant-scoped and fails closed on missing or mismatched tenant context.
- `NFR-SEC-1` - Authoring foundation does not store tokens, headers, raw claims, form values, input values, tax data, HMRC data, property data, or user-entered sensitive values.
- `NFR-TEST-1` - Added tests for lifecycle validation, version metadata, validation failure shape, and tenant isolation.

### Scope delivered

- Added `AuthoredContentItem` and `AuthoredContentVersion` domain entities.
- Added `ContentLifecycleState` and `ContentLifecycleTransition`.
- Added typed authoring application contracts and validator.
- Added `IAuthoredContentRepository` as a future EF-ready repository abstraction.
- Added `InMemoryAuthoredContentRepository` as a non-durable foundation implementation.
- Registered the in-memory repository in infrastructure dependency injection.
- Added unit and integration tests for lifecycle rules and tenant isolation.

### Assumptions

Authored content is tenant-scoped from the first authoring slice. The repository boundary enforces tenant context using the same tenant guard pattern as the Sprint 1 persistence seams.

Lifecycle validation is explicit and contract-level only. Approval workflow orchestration, publishing implementation, authoring API endpoints, and UI screens are intentionally deferred to later Sprint 3 slices.

### Explicitly not built

- Full Adoption Studio UI.
- Authoring screens.
- API endpoints.
- Publishing implementation.
- Approval workflow implementation beyond lifecycle model and validation seams.
- AI assistant.
- Analytics pipeline.
- Event Hubs or ClickHouse.
- Browser extension.
- Property MTD integration.
- EF Core, `DbContext`, EF migrations, or production database infrastructure.
- Runtime renderer changes.

### Commands to run

```powershell
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
pnpm typecheck
pnpm build
pnpm test
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md
```

### Known limitations

- Authoring persistence is in-memory and non-durable.
- No authoring API endpoints exist yet.
- No Approval Studio UI or authoring screens exist yet.
- No approval workflow orchestration or publishing implementation exists yet.
- No production database infrastructure is enabled.

### Next recommended slice

Add the approval workflow foundation and authoring permission catalog entries, then introduce API boundary contracts once RBAC and audit requirements for authoring actions are explicit.
