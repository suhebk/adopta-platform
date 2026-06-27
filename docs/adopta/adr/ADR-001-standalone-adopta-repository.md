# ADR-001 — Standalone Adopta Repository

## Status

Accepted

## Decision

Adopta will be built in a new standalone GitHub repository:

```text
suhebk/adopta-platform
```

The existing Property Income MTD repository remains separate:

```text
suhebk/property-income-mtd-platform
```

## Rationale

The Property Income MTD product has active enhancement sprints still in progress. Building Adopta in the same repository would increase risk, create branch noise, complicate GitHub Actions usage, and distract Codex from the new SaaS platform foundation.

A standalone repository allows:

- independent sprint cadence;
- cleaner PRs;
- focused Codex instructions;
- independent CI/CD;
- production-grade SaaS architecture without disturbing Property MTD;
- later controlled integration through SDK/API contracts.

## Consequences

- Adopta is built independently first.
- Property MTD is not modified during Adopta Sprint 1.
- Property MTD becomes the first reference integration later.
- Integration will happen through a small, controlled branch in `property-income-mtd-platform` only when the SDK contract is ready.
