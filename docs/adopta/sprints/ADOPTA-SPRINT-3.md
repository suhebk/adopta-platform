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

## Slice 2 - Approval workflow foundation and authoring permissions

### Requirement IDs covered

- `FR-AUT-006` - Added review request and approval decision contracts for authored content lifecycle decisions.
- `FR-AUT-007` - Added deterministic approval workflow validation for requesting review, approving, and rejecting content.
- `FR-AUT-008` - Added structural lifecycle decision audit record shape.
- `FR-AUT-009` - Added authoring permission keys to the existing permission catalog.
- `FR-IDN-031` - Repository-involved workflow validation remains tenant-scoped and denies or hides cross-tenant access safely.
- `NFR-SEC-1` - Workflow results and audit records avoid content body, raw DOM text, user-entered values, tokens, headers, raw claims, form/input values, tax data, HMRC data, property data, and sensitive values.
- `NFR-TEST-1` - Added workflow, permission catalog, audit shape, and tenant-isolation tests.

### Scope delivered

- Added `AuthoredContentReviewRequest`.
- Added `AuthoredContentApprovalDecision`.
- Added lifecycle decision result/status contracts and typed validation issues.
- Added `AuthoredContentLifecycleAuditRecord` for safe structural audit metadata.
- Added `AuthoredContentApprovalWorkflow` as an application-layer validator/service seam.
- Added authoring permission keys to `AdoptaPermissionKeys`:
  - `Authoring.Read`;
  - `Authoring.Manage`;
  - `Authoring.Review`;
  - `Authoring.Approve`;
  - `Authoring.Publish`.
- Added tests for allowed and denied workflow decisions, permission catalog representation, audit shape, and tenant isolation.

### Assumptions

The approval workflow seam validates lifecycle decisions and produces safe result/audit shapes. It does not mutate authored content, publish content, call APIs, render UI, or persist workflow history.

`Authoring.Publish` is a permission placeholder only in this slice. Publishing behavior is intentionally deferred.

### Explicitly not built

- Full Adoption Studio UI.
- Authoring screens.
- API endpoints.
- Publishing implementation.
- Runtime renderer.
- AI assistant.
- Analytics pipeline.
- Event Hubs or ClickHouse.
- Browser extension.
- Property MTD integration.
- EF Core, `DbContext`, EF migrations, or production database infrastructure.

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

- Workflow validation does not mutate authored content.
- Workflow history is not persisted.
- Lifecycle audit records are structural contracts only and are not written to durable storage by this seam.
- No authoring API endpoints or UI screens exist yet.
- No publishing implementation exists yet.

### Next recommended slice

Add authoring API contract boundaries for tenant-scoped lifecycle commands and queries, enforcing the authoring permission keys and audit requirements introduced in this slice.

## Slice 3 - Authoring API contract boundaries

### Requirement IDs covered

- `FR-AUT-010` - Added Minimal API contract boundaries for tenant-scoped authored content commands and queries.
- `FR-AUT-011` - Added safe request and response DTOs for create, get, list, review request, approval, and rejection commands.
- `FR-AUT-012` - Authoring API boundaries enforce tenant context and authoring permission keys.
- `FR-AUT-013` - Lifecycle command endpoints use the existing approval workflow seam and safe audit record shape.
- `FR-IDN-031` - Cross-tenant authored content access is denied or hidden safely.
- `NFR-SEC-1` - API responses avoid tokens, headers, raw claims, form values, input values, tax data, HMRC data, property data, raw content body, and sensitive values in errors.
- `NFR-TEST-1` - Added integration tests for tenant context, permissions, tenant scope, lifecycle commands, and safe invalid-command behavior.

### Scope delivered

- Added `/authoring/content` Minimal API endpoint shells:
  - `POST /authoring/content`;
  - `GET /authoring/content/{contentId}`;
  - `GET /authoring/content`;
  - `POST /authoring/content/{contentId}/versions/{versionId}/request-review`;
  - `POST /authoring/content/{contentId}/versions/{versionId}/approve`;
  - `POST /authoring/content/{contentId}/versions/{versionId}/reject`.
- Added safe API DTOs for authored content command and query boundaries.
- Enforced tenant context on every authoring endpoint.
- Enforced existing authoring permissions:
  - create requires `Authoring.Manage`;
  - get/list require `Authoring.Read`;
  - request review requires `Authoring.Review`;
  - approve requires `Authoring.Approve`;
  - reject requires `Authoring.Review`.
- Used existing repository, validator, and approval workflow seams.

### Assumptions

This slice provides API boundary foundations only. The endpoints validate and route commands through existing in-memory seams but do not introduce durable persistence, production publishing, UI screens, or renderer behavior.

Lifecycle command responses may include the safe structural lifecycle audit shape from the workflow seam. They do not persist audit history in this slice.

### Explicitly not built

- Full Adoption Studio UI.
- Authoring screens.
- Publishing implementation.
- Runtime renderer.
- AI assistant.
- Analytics pipeline.
- Event Hubs or ClickHouse.
- Browser extension.
- Property MTD integration.
- EF Core, `DbContext`, EF migrations, or production database infrastructure.

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

- Endpoint handlers use in-memory/foundation repositories only.
- Lifecycle endpoints validate workflow decisions but do not mutate authored content state.
- Lifecycle audit records are returned as safe command metadata only and are not durably persisted.
- No full authoring UI or publishing behavior exists yet.

### Next recommended slice

Add publishing contract design and version-to-runtime bundle mapping boundaries, still without implementing production publishing infrastructure or a full authoring UI.
