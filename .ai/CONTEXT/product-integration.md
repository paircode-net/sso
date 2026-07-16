# Product integration — JWT authz contract

> Audience: times de produto que consomem o SSO  
> Fase 5 — feature 00001  
> Related: ADR-003, ADR-005

## Overview

Products authorize **locally** from the access token. Effective permissions are resolved at issuance (login, refresh, `switch_context`) and placed in JWT claims. Clients **do not** need redeploy when new permission codes are added to the catalog — they appear on the **next** token issuance.

## Claim names

| Claim | Multiplicity | Description |
|-------|--------------|-------------|
| `sub` | 1 | User id (Guid) |
| `email` / `name` / `preferred_username` | 0..1 | OIDC profile |
| `organization_id` | 0..1 | Active org after switch-context |
| `branch_id` | 0..1 | Active branch (exact match; no parent inheritance — ADR-004) |
| `permissions` | 0..N | One claim value per permission **code** (e.g. `hq.reports`) |
| `perm_ver` | 1 | Opaque policy etag (UTC ticks max of Permission / RolePermission / UserRoleAssignment stamps). Changes when catalog or assignments change. |
| `sid` | 1 | Stable session id (Guid). Used for hot revocation (feature 00005 / ADR-007). |

Constants: `SSO.Shared.Identity.SsoClaimTypes`.

## Token lifetimes

| Token | TTL |
|-------|-----|
| Access / ID | **15 minutes** |
| Refresh | **14 days** |

Stale window: permission changes are visible only after refresh, switch-context, or access expiry + re-login. Use short TTL + `perm_ver` for local cache invalidation.

## Hot revocation (feature 00005)

SLA: after a session is revoked, products using `SSO.Client` reject the access token within **≤ 60 seconds** (default cache 30s).

| Mechanism | Detail |
|-----------|--------|
| Claim `sid` | Emitted on every user access token |
| Deny-list | SQL `RevokedSessions` on the Authorization Server |
| Status API | `GET /api/identity/sessions/{sid}/status` → `{ session_id, revoked }` |
| SDK | `AddSsoAuthentication` enables check **by default**; opt-out: `SsoClient:RevocationCheck:Enabled=false` |
| Cache | `SsoClient:RevocationCheck:CacheSeconds` (1–60, default 30) |
| Fail mode | `FailClosed=false` (default: allow if AS unreachable); set `true` for high-security |
| Webhook | `session.revoked` via outbox + HMAC header `X-SSO-Signature: sha256=…` (AuthClient URL in `ClientWebhookEndpoints`) |

Without the SDK check (opt-out), access tokens remain valid until expiry (~15 min) even after revoke. Refresh / switch_context with a revoked `sid` returns `invalid_grant`.

Session APIs: `GET /api/identity/sessions/me`, `POST .../me/{id}/revoke`, admin list/revoke under `sso.admin.sessions.revoke`. UI: `/Account/Sessions`.

## Flows that re-resolve permissions

1. Authorization code (+ PKCE) / interactive login  
2. Refresh token grant  
3. Custom grant `urn:sso:params:oauth:grant-type:switch_context` with `organization_id` (+ optional `branch_id`)

Client → Product mapping: OpenIddict `client_id` → `ClientProductBindings` → `ProductId` on assignments.

## Authz pattern in product apps

```text
1. Validate JWT (issuer, audience/client, signature, expiry)
2. Read permissions claims (string codes)
3. Gate routes/handlers: requires("hq.reports")
4. Optional: if cached menus keyed by perm_ver and claim differs → rebuild menu from permissions
```

Prefer JWT claims for runtime authz. Do **not** call SSO on every request for permissions.

## Quickstart — SSO.Client (.NET)

Package: `src/SSO.Client` (`SSO.Client` 0.1.0). Sample: `samples/product-api`.

```csharp
// Program.cs
using SSO.Client;
using SSO.Client.Authorization;

builder.Services.AddSsoAuthentication(builder.Configuration);
// appsettings: SsoClient:Authority, Audience, ClientId

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/reports", () => Results.Ok())
    .RequireAuthorization(new RequiresPermissionAttribute("hq.reports"));

// Claims: User.GetPermissions(), GetPermissionVersion(), GetOrganizationId(), …
// Token ops (BFF/service): inject SsoTokenClient → RefreshAsync / SwitchContextAsync
```

```bash
dotnet add reference ../../src/SSO.Client/SSO.Client.csproj
# or: dotnet pack src/SSO.Client && nuget add
dotnet run --project samples/product-api
```

## Quickstart — @sso/client (JS/TS) + BFF

Package: `clients/js` (`@sso/client` 0.1.0). Does **not** verify JWT — use API/BFF. Sample BFF: `samples/sso-bff` (session cookie, refresh, `switch_context`).

```ts
import { parseJwtPayload, requirePermission, getPermissionVersion } from "@sso/client";

const payload = parseJwtPayload(accessToken);
if (!requirePermission(payload, "hq.reports")) throw new Error("forbidden");
const cacheKey = getPermissionVersion(payload);
```

```bash
cd clients/js && npm install && npm test
dotnet run --project samples/sso-bff
# POST /bff/session { access_token, refresh_token? }
# POST /bff/refresh | /bff/switch-context | GET /bff/me
```

## Menus / UI features

**Primary:** map `permissions` codes → features in the product app.

**Catalog (admin):** `MenuItem` rows (`api/identity/menuitems`) bind `PermissionCode` → title/route per Product.

**Diagnostic:** `GET /api/identity/menus/effective?userId=&organizationId=&branchId=&clientId=`  
Returns `{ perm_ver, permissions, menus }` filtered by effective permissions. Useful for admin/debug; products should still prefer the token.

Seed (dev product):

| Menu code | Route | Permission |
|-----------|-------|------------|
| `home` | `/app` | `sso.access` |
| `hq-reports` | `/app/hq/reports` | `hq.reports` |
| `filial-ops` | `/app/filial/ops` | `filial.ops` |

## Adding a permission without client redeploy

1. `POST /api/identity/permissions` with new `code`  
2. `POST /api/identity/rolepermissions` linking Role → Permission  
3. Ensure `UserRoleAssignment` covers the user/org/branch/product  
4. User obtains a **new** access token → claim `permissions` includes the new code; `perm_ver` changes  

## Dev clients

| client_id | Type | Notes |
|-----------|------|-------|
| `dev-product-spa` | public + PKCE | Auth code, refresh, switch_context |
| `dev-product-service` | confidential | client_credentials (no user permissions) |

Dev user: `admin@sso.local` / `ChangeMe!123` (see `IdentitySeed`).
