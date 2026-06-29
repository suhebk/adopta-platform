# API Security And Route Authorization Checklist

## Purpose

This checklist records the route authorization and safe-response expectations for the current API foundation.

## Current Implemented State

- Tenant-scoped authoring routes require tenant context.
- Authoring routes use explicit permission keys from `AdoptaPermissionKeys`.
- Publishing route uses `Authoring.Publish`.
- Permission checks fail closed.
- Cross-tenant access is hidden or safely denied.
- API responses avoid sensitive values.

## Route Authorization Checklist

- Every tenant-scoped authoring endpoint requires tenant context.
- Every authoring command/query endpoint has an explicit permission requirement.
- No duplicate permission catalog is introduced.
- Missing permission returns a safe denial.
- Wrong permission returns a safe denial.
- Missing tenant context returns a safe denial.
- Cross-tenant entity access does not reveal whether the entity exists.

## Safe Response Checklist

API responses and errors must not include:

- content body;
- raw authored content;
- tokens;
- headers;
- raw claims;
- form values;
- input values;
- tax data;
- HMRC data;
- property data;
- connection strings;
- secrets;
- tenant secrets;
- credentials.

## Publishing Contract Limitations

The publishing API remains contract-only:

- no delivery API;
- no CDN publishing;
- no Blob Storage publishing;
- no runtime bundle external storage;
- no runtime renderer behaviour;
- no external publishing transport.

Successful publish commands may return safe bundle metadata and structural audit metadata only.

## Future Hardening Checklist

- Add route authorization metadata tests for all future endpoint groups.
- Add production Entra token validation tests with approved test tokens or mocks.
- Add operational monitoring for safe authorization denial counts.
- Add database-level tenant isolation validation after database enablement approval.
