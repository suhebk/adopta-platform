# Adoption Studio Information Architecture

## Purpose

This document captures the Sprint 3 foundation direction for the Adoption Studio/Admin shell. It is not a full UI specification and does not implement authoring screens, editors, publishing actions, analytics, AI, runtime rendering, or production infrastructure.

## Shell Areas

| Area | Route | Permission | Foundation purpose |
| --- | --- | --- | --- |
| Studio Overview | `/studio` | `Authoring.Read` | Entry point for the authoring and governance shell. |
| Authored Content | `/studio/content` | `Authoring.Read` | Future inventory for authored guidance and content versions. |
| Review Queue | `/studio/review` | `Authoring.Review` | Future review and approval work queue. |
| Publishing | `/studio/publishing` | `Authoring.Publish` | Future publishing readiness and delivery bundle mapping area. |
| Governance & Audit | `/studio/governance` | `Audit.Read` | Future governance and audit visibility area. |

## Assumptions

- Navigation metadata is centralized in code so routes, labels, descriptions, and permission requirements remain consistent.
- Permission metadata references the existing `AdoptaPermissionKeys` catalog only.
- Pages are placeholder shell routes only.
- Full authorization UI, route protection UX, and tenant-aware Studio interaction flows are future work.

## Out Of Scope

- Full Adoption Studio UI.
- Authoring screens and content editor.
- Drag-and-drop builder.
- Approval workflow UI.
- Publishing UI or production publishing.
- Runtime renderer.
- Analytics, AI, browser extension, Property MTD integration, EF Core, migrations, or production database infrastructure.
