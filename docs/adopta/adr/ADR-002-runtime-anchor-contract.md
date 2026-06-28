# ADR-002 - Runtime Anchor Contract

## Status

Accepted for ADOPTA-SPRINT-2 Slice 2.

## Context

Adopta needs a deterministic first-party anchoring strategy for applications that can include stable runtime markers. The first runtime SDK foundation must be privacy-safe, framework-agnostic, and safe for host applications even when anchors are missing, duplicated, or invalid.

The broader product architecture includes future multi-signal anchoring, resilience scoring, authoring support, telemetry, and optional advanced fallbacks. Those capabilities are intentionally out of scope for this slice.

## Decision

The first real runtime anchor strategy is `data-adopt-id`.

Runtime descriptors use a small explicit contract:

```ts
{
  strategy: "data-adopt-id",
  value: "stable.anchor.key"
}
```

The resolver exact-matches the `data-adopt-id` attribute value. Exactly one match resolves successfully. Zero matches fail safely as `missing_anchor`; more than one match fails safely as `duplicate_anchor`.

## Why CSS Selectors Are Not Primary

CSS selectors are easy to break through layout, class, framework, and component-library changes. They also encourage authors and integrators to couple guidance to implementation detail rather than a stable first-party contract. Adopta will support richer multi-signal anchoring later, but Slice 2 deliberately keeps the production-ready first-party contract narrow and deterministic.

## Privacy And Safety Constraints

- The resolver must not mutate the host DOM.
- The resolver must not capture input values, form values, raw DOM text, tokens, headers, claims, cookies, or sensitive host data.
- Failure results must be typed and safe for callers to inspect.
- Resolver failures must not throw uncaught errors into host applications.
- Missing, duplicate, unsupported, invalid, and unexpected resolver failures must fail closed.

## Out Of Scope

- Tooltip renderer.
- Walkthrough renderer.
- Delivery bundle API.
- Demo host.
- Runtime event pipeline.
- Analytics.
- AI or vision fallback.
- Browser extension.
- Property MTD integration.
- Adoption Studio.
- Production database infrastructure.

