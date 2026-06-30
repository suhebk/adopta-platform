# ADOPTA-SPRINT-7 - Runtime Experience Layer Foundation

## Sprint intent

Sprint 7 expands the runtime renderer from tooltip/callout foundation into a production-grade runtime guidance experience layer. The sprint must preserve the privacy-safe runtime delivery model and must not add analytics, event transport, external storage, CDN, Blob Storage, Property MTD integration, migration execution, database creation, production deployment automation, or production database mutation unless explicitly approved in a later slice.

## Slice 1 - Runtime experience content contracts

### Requirement IDs covered

- `FR-AUT-016` - Extended runtime content contracts for checklist and walkthrough experience structures while preserving existing tooltip and callout compatibility.
- `FR-IDN-031` - Kept anchor references tied to the existing first-party `data-adopt-id` descriptor contract.
- `NFR-SEC-1` - Kept contracts privacy-safe and avoided field/form values, raw DOM text, tokens, headers, claims, secrets, connection strings, tax/HMRC/property data, and sensitive values.
- `NFR-A11Y-1` - Added contract-only placement, dismiss behaviour, and theme/style-token metadata for future accessible renderer behavior.
- `NFR-TEST-1` - Added TypeScript and .NET mirror validation coverage for valid and invalid runtime experience content.

### Scope delivered

- Added optional rich checklist content structures with ordered steps.
- Added optional rich walkthrough content structures with ordered steps.
- Added controlled renderer placement tokens.
- Added controlled dismiss behaviour tokens.
- Added controlled theme/style tokens.
- Added runtime experience metadata for item-level and step-level contract use.
- Preserved existing tooltip and callout contracts.
- Preserved existing placeholder-safe checklist and walkthrough items without rich structures.
- Updated TypeScript content validation.
- Updated .NET Application runtime mirror contracts and validator for delivery/API parity.

### Contract assumptions

Checklist and walkthrough structures are optional in this slice. Existing `checklist` and `walkthrough` content items without step structures remain valid and continue to be placeholder-safe.

Placement is represented only by controlled tokens. It does not permit raw CSS selectors, XPath, text matching, screen coordinates, free-form CSS, or AI/vision fallback.

Theme/style metadata is represented only by safe tokens. It does not permit raw CSS, CSS custom properties, script, markup, or host style mutation.

Dismiss behaviour is contract-only. No renderer behavior, persistence, event emission, analytics, or cross-session dismissal state is implemented in this slice.

### Validation rules

- Checklist and walkthrough steps require non-empty IDs and titles when rich structures are present.
- Duplicate checklist and walkthrough step IDs fail with typed validation issues.
- Optional step bodies must be strings in the TypeScript contract.
- Optional anchor references must use the existing `data-adopt-id` anchor descriptor.
- Placement, dismiss behaviour, and theme metadata must use approved tokens.
- Validation messages must be generic and must not echo sensitive input values.

### Explicitly not built

- Checklist renderer.
- Walkthrough renderer.
- Tooltip/callout renderer changes.
- DOM mutation.
- Analytics pipeline.
- Event transport.
- Event Hubs.
- ClickHouse.
- AI assistant.
- Browser extension.
- Property MTD integration.
- Backend delivery endpoint changes.
- Appsettings changes.
- EF migrations.
- Database schema changes.
- DB calls.
- External storage.
- CDN publishing.
- Blob Storage publishing.
- Deployment automation.

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
rg "innerHTML|XPath|screen-coordinate|text matching|querySelector\(|\.value|FormData|localStorage|sessionStorage|Authorization|Bearer|Password|ConnectionString|HMRC|tax|property|secret|token|claim" packages/runtime-sdk/src/content packages/runtime-sdk/tests/runtimeExperienceContent.test.ts src/Adopta.Application/Runtime tests/Adopta.UnitTests/RuntimeContentContractTests.cs
```

### Known limitations

- Checklist and walkthrough are contract-only and are not rendered.
- Placement, dismiss behaviour, and theme/style tokens are validated but not interpreted by the renderer.
- Targeting remains placeholder-only.
- No analytics, event transport, external storage, CDN, Blob Storage, Property MTD integration, or production deployment automation exists.
- No database schema or persistence changes were added for runtime experience metadata.

### Next recommended slice

Add renderer support for checklist content using the new contracts, keeping walkthrough rendering, analytics, event transport, external storage, Property MTD integration, and production infrastructure out of scope until separately approved.

## Slice 2 - Checklist renderer foundation

### Requirement IDs covered

- `FR-AUT-013` - Added runtime checklist rendering foundation for onboarding/task-list style guidance.
- `FR-DEL-014` - Kept runtime output isolated to SDK-owned nodes with explicit teardown.
- `FR-DEL-015` - Preserved safe renderer failure behaviour and host-app safety boundaries.
- `NFR-A11Y-1` - Added accessible checklist region/list semantics and accessible dismiss behaviour.
- `NFR-PRIV-1` - Avoided field values, form values, host DOM text, tokens, headers, claims, secrets, connection strings, tax/HMRC/property data, and user-entered sensitive values.
- `NFR-TEST-1` - Added TypeScript renderer coverage for checklist rendering, teardown, raw-markup safety, unsupported walkthrough handling, and privacy guardrails.

### Scope delivered

- Added `ChecklistRenderer` as a runtime SDK renderer peer to tooltip and banner/callout renderers.
- Rendered checklist title, optional body, ordered steps, and display-only incomplete state from the approved checklist contract.
- Preserved placeholder-safe checklist items by keeping them valid and skipped until rich checklist steps are present.
- Wired checklist support into the existing `Renderer`.
- Preserved tooltip and callout/banner behaviour.
- Kept walkthrough content unsupported/placeholder-safe.
- Kept checklist state local and in-memory only.
- Added TypeScript tests for checklist rendering, dismiss/unmount, Escape teardown, raw-markup handling, host DOM privacy, and existing renderer compatibility.

### Renderer assumptions

Checklist rendering is display-only in this slice. The current checklist contract does not contain durable completion state, so rendered rich checklist steps are shown as incomplete and no controls are provided to toggle progress.

Placeholder checklist items without rich checklist steps remain valid but are skipped safely. This preserves existing placeholder compatibility while enabling rendering for the approved rich checklist contract.

The checklist surface appends an SDK-owned node to the document body or document element, matching the existing banner/callout renderer append-target pattern. The renderer does not mutate host application nodes beyond appending/removing SDK-owned nodes.

Walkthrough rendering remains out of scope and continues to be skipped safely as an unsupported content type.

### Accessibility behaviour

- Checklist container uses an accessible labelled region.
- Checklist steps render inside an ordered list with list/listitem semantics.
- Dismiss control has an accessible label.
- Escape dismissal follows the existing renderer pattern.
- No autofocus, focus trap, or animation is introduced.

### Commands to run

```powershell
pnpm typecheck
pnpm build
pnpm test
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
rg "innerHTML|XPath|screen-coordinate|text matching|querySelector\(|\.value|FormData|localStorage|sessionStorage|Authorization|Bearer|Password|ConnectionString|HMRC|tax|property|secret|token|claim" packages/runtime-sdk/src/rendering packages/runtime-sdk/tests/renderer.test.ts
```

### Known limitations

- Checklist progress is display-only and not persisted.
- Checklist items do not launch walkthroughs yet.
- Walkthrough rendering remains unsupported.
- No targeting evaluation, analytics, event transport, backend/API change, persistence change, external storage, CDN/Blob Storage publishing, Property MTD integration, migration execution, database creation, or deployment automation was added.

### Next recommended slice

Add walkthrough renderer foundation using the approved walkthrough contracts, keeping analytics, event transport, targeting evaluation, backend/API changes, persistence changes, external storage, Property MTD integration, and production infrastructure out of scope until separately approved.

## Slice 3 - Walkthrough renderer foundation

### Requirement IDs covered

- `FR-AUT-010` - Added runtime walkthrough rendering foundation for ordered, multi-step guidance.
- `FR-DEL-014` - Kept walkthrough output isolated to SDK-owned nodes with explicit teardown.
- `FR-DEL-015` - Preserved fail-safe renderer behaviour for invalid anchors and renderer errors.
- `NFR-A11Y-1` - Added accessible walkthrough region semantics and keyboard-reachable native controls.
- `NFR-PRIV-1` - Avoided field values, form values, host DOM text, tokens, headers, claims, secrets, connection strings, tax/HMRC/property data, and user-entered sensitive values.
- `NFR-TEST-1` - Added TypeScript renderer coverage for walkthrough rendering, local navigation, anchor failures, teardown, raw-markup safety, and privacy guardrails.

### Scope delivered

- Added `WalkthroughRenderer` as a runtime SDK renderer peer to tooltip, banner/callout, and checklist renderers.
- Rendered rich walkthrough steps from the approved walkthrough contract.
- Added local in-memory previous/next navigation between steps.
- Added progress text in the form `Step n of m`.
- Added native previous, next, and dismiss button controls with accessible labels.
- Added Escape dismissal using the existing renderer pattern.
- Kept placeholder walkthrough items valid and skipped until rich walkthrough steps are present.
- Preflighted optional step anchors through the existing `data-adopt-id` anchor resolver only.
- Preserved tooltip, callout/banner, and checklist renderer behaviour.

### Renderer assumptions

Walkthrough state is local to the rendered SDK-owned surface. The current step index is stored only in closure state and is lost on unmount or re-render.

Walkthrough rendering uses a single SDK-owned surface and updates the current step text, progress text, and control state in place. It does not mutate host application nodes beyond appending/removing the SDK-owned surface.

Optional step anchors are validated with the existing `data-adopt-id` anchor resolver before the walkthrough surface is rendered. Missing, duplicate, or unresolvable step anchors fail safely and do not reveal host page details.

Placeholder walkthrough items without rich steps remain valid but are skipped safely for compatibility.

### Accessibility behaviour

- Walkthrough container uses an accessible labelled region.
- Previous, next, and dismiss controls are native buttons.
- Controls expose accessible labels.
- Previous/next availability is exposed through `aria-disabled` and an SDK-owned state attribute.
- Escape dismissal follows the existing renderer pattern.
- No autofocus, focus trap, or animation is introduced.

### Commands to run

```powershell
pnpm typecheck
pnpm build
pnpm test
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
rg "innerHTML|XPath|screen-coordinate|text matching|querySelector\(|\.value|FormData|localStorage|sessionStorage|Authorization|Bearer|Password|ConnectionString|HMRC|tax|property|secret|token|claim" packages/runtime-sdk/src/rendering packages/runtime-sdk/tests/renderer.test.ts
```

### Known limitations

- Walkthrough state is local only and not persisted.
- Walkthrough navigation does not evaluate targeting, branching, completion rules, or advance triggers.
- Walkthrough placement/theme tokens are not interpreted yet.
- No analytics, event transport, backend/API change, persistence change, external storage, CDN/Blob Storage publishing, Property MTD integration, migration execution, database creation, or deployment automation was added.

### Next recommended slice

Add renderer placement and theme token interpretation for the existing tooltip, callout/banner, checklist, and walkthrough foundations, keeping targeting evaluation, analytics, event transport, backend/API changes, persistence changes, external storage, Property MTD integration, and production infrastructure out of scope until separately approved.

## Slice 4 - Renderer placement and theme token interpretation

### Requirement IDs covered

- `FR-DEL-014` - Applied controlled placement and theme metadata to SDK-owned runtime surfaces.
- `FR-DEL-015` - Preserved fail-safe renderer behaviour for invalid content, missing anchors, duplicate anchors, and renderer errors.
- `FR-IDN-031` - Kept anchoring limited to the existing first-party `data-adopt-id` strategy.
- `NFR-A11Y-1` - Preserved accessible roles, labels, keyboard behaviour, dismiss behaviour, Escape dismissal, and unmount behaviour while adding visual metadata interpretation.
- `NFR-PRIV-1` - Avoided host DOM text, field values, form values, tokens, headers, claims, secrets, connection strings, tax/HMRC/property data, and sensitive values.
- `NFR-TEST-1` - Added TypeScript renderer coverage for placement/theme interpretation, invalid token failures, and raw style/CSS injection guardrails.

### Scope delivered

- Added `RendererPlacementResolver` to map approved placement tokens to SDK-owned `data-adopta-placement` attributes.
- Added `RendererThemeResolver` to map approved theme tokens to SDK-owned `data-adopta-theme-*` attributes.
- Applied placement and theme metadata to tooltip, callout/banner, checklist, and walkthrough surfaces.
- Preserved existing tooltip, callout/banner, checklist, and walkthrough rendering behaviour.
- Preserved existing dismiss, Escape, and unmount behaviour.
- Kept placement/theme interpretation token-based only.
- Added TypeScript tests for supported surfaces, invalid token validation, no inline style/class injection, and existing renderer compatibility.

### Renderer assumptions

Placement interpretation is metadata-only in this slice. It records approved placement tokens on SDK-owned nodes for stylesheet and future layout use, but it does not add screen-coordinate positioning, selector targeting, XPath, text matching, AI/vision fallback, or host DOM measurement.

Theme interpretation is token-only. The renderer records approved tone, density, and emphasis tokens on SDK-owned nodes through safe attributes. It does not accept raw CSS, CSS custom properties, inline style strings, script, markup, or content-provided class names.

Invalid placement or theme metadata fails through the existing content validation path before rendering. Validation messages remain typed and generic and do not echo rejected values.

### Accessibility behaviour

- Tooltip, callout/banner, checklist, and walkthrough roles and labels are preserved.
- Existing native button controls remain keyboard reachable.
- Existing Escape dismissal and explicit unmount behaviour are preserved.
- No autofocus, focus trap, or animation is introduced.
- Placement/theme attributes do not alter semantic roles or accessible labels.

### Commands to run

```powershell
pnpm typecheck
pnpm build
pnpm test
dotnet test Adopta.slnx
dotnet build Adopta.slnx --configuration Release --no-restore
dotnet test Adopta.slnx --configuration Release --no-build
rg "net9\.0" src tests docs .github packages apps package.json pnpm-workspace.yaml tsconfig.base.json Adopta.slnx global.json NuGet.config README.md AGENTS.md -g "!**/bin/**" -g "!**/obj/**"
rg "innerHTML|XPath|screen-coordinate|text matching|querySelector\(|\.value|FormData|localStorage|sessionStorage|Authorization|Bearer|Password|ConnectionString|HMRC|tax|property|secret|token|claim|style=" packages/runtime-sdk/src/rendering packages/runtime-sdk/tests/renderer.test.ts
rg "Migrate\(|EnsureCreated\(|EnsureDeleted\(|Database\.Ensure" src tests -g "!**/bin/**" -g "!**/obj/**" -g "!tests/Adopta.UnitTests/PersistenceMigrationReadinessTests.cs"
```

### Known limitations

- Placement tokens are interpreted as safe metadata only; precise overlay positioning and collision handling are not implemented.
- Theme tokens are exposed as safe attributes only; a full visual design system stylesheet remains future work.
- Targeting evaluation, analytics, event transport, backend/API changes, persistence changes, external storage, CDN/Blob Storage publishing, Property MTD integration, migration execution, database creation, and deployment automation remain out of scope.

### Next recommended slice

Add runtime targeting evaluation contracts or renderer polish only after confirming the next slice boundary, keeping analytics, event transport, backend persistence changes, external storage, Property MTD integration, and production infrastructure separately approved.
