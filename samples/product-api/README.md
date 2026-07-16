# Product API sample

Minimal API using **SSO.Client** (feature 00004).

```bash
dotnet run --project samples/product-api
```

Configure `SsoClient:Authority` to your SSO host. Call with `Authorization: Bearer <access_token>` after login + optional `switch_context`.

| Route | Authz |
|-------|--------|
| `GET /health` | anonymous |
| `GET /me` | authenticated |
| `GET /reports` | permission `hq.reports` |
