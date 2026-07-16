# Samples — feature 00004

| Projeto | Porta típica | Papel |
|---------|--------------|--------|
| `product-api` | http://localhost:5101 | API de produto com `SSO.Client` + `RequiresPermission` |
| `sso-bff` | http://localhost:5102 | BFF: cookie/session, refresh, `switch_context`, proxy `/bff/me` |

Pré-requisito: Authorization Server em `SsoClient:Authority` (default `https://localhost:5001`).

```bash
dotnet run --project samples/product-api --urls http://localhost:5101
dotnet run --project samples/sso-bff --urls http://localhost:5102
```
