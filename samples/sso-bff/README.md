# SSO BFF sample

Browser keeps a **session cookie**; refresh token stays on the BFF. Uses `SSO.Client` (`SsoTokenClient`) for refresh / `switch_context`.

```bash
dotnet run --project samples/sso-bff
```

| Route | Purpose |
|-------|---------|
| `POST /bff/session` | Body `{ accessToken, refreshToken? }` — store after SPA code exchange |
| `POST /bff/logout` | Clear session |
| `GET /bff/me` | Proxy to product API with Bearer from session |
| `POST /bff/refresh` | Refresh access token |
| `POST /bff/switch-context` | Body `{ organizationId, branchId? }` |

Configure `SsoClient:*` and `ProductApi:MeUrl`.
