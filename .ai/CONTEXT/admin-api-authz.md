# Admin API authorization

> Feature 00002 — F00002-D1/D2/D3  
> Related: ADR-003, ADR-005, `product-integration.md`

## Overview

Management APIs under `api/identity/*` require a Bearer access token (OpenIddict validation) and at least one admin permission claim (`permissions`).

Products continue to authorize **their own** APIs from JWT permission codes. Admin codes use the `sso.admin.*` prefix and should be ignored by product apps.

## Permission codes

| Code | Scope | Typical use |
|------|-------|-------------|
| `sso.admin.platform` | Platform | Organizations, products, permission/role catalog, client bindings |
| `sso.admin.org` | Tenant | Branches, memberships, user-role assignments (token org) |
| `sso.admin.audit.read` | Platform | `GET /api/identity/auth-audit-events` |
| `sso.admin.sessions.revoke` | Tenant (+ platform) | `POST .../account/sessions/{userId}/revoke` |
| `sso.admin.menus` | Platform | MenuItem CRUD + `GET /api/identity/menus/effective` |

Mechanism: `[RequiresPermission(...)]` → `PermissionAuthorizationHandler` reading claim `permissions` (OR of listed codes).

## Platform vs org context (F00002-D2)

- **Platform-scoped** `UserRoleAssignment` has `OrganizationId = null`. Those permissions are resolved even when the token has no `organization_id`.
- **Org admin** must have `organization_id` in the token (via `switch_context`). Writes to Branches/Memberships/Assignments call `ICurrentAdminContext.EnsureCanAccessOrganization`.
- **Platform admin** may act cross-org on tenant resources.

## Seed (dev)

- Product `sso-platform` + client `sso-admin-api`
- Roles `platform-admin` / `org-admin` on `admin@sso.local`
- Platform assignment: `OrganizationId` null on product `sso-platform`

## Public exceptions

| Route | Auth |
|-------|------|
| `POST /api/identity/account/request-email-confirmation` | Anonymous |
| `GET /api/identity/external-identity-providers` | Anonymous (login UI) |

## `menus/effective` (F00002-D3)

Admin/diagnostic only (`sso.admin.menus`). Product UIs should map features from JWT `permissions` / `perm_ver`.
