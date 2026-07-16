# Feature Plan — 00004 SDK / BFF de integração de produtos

> Arquivo: `.ai/WORK/2026-07-16-00004-sdk-integracao-produtos.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Implementado** (2026-07-16)  
> Data: 2026-07-16  
> Depende de: contrato atual (`product-integration.md`, ADR-003/005)  
> Relaciona: 00005 (sinais de revogação no cliente)

## Objetivo

Publicar um **pacote de integração** (.NET + JS/TS) que padronize validação de JWT, leitura de claims SSO, autorização por permission code, helpers de `switch_context` / `perm_ver`, com **sample BFF** de referência — reduzindo atrito entre produtos consumidores.

## Contexto

- Products autorizam localmente a partir do access token (ADR-005).
- Contrato documentado; cada time ainda monta middleware do zero.
- Clients seed: `dev-product-spa`, `dev-product-service`.

## Decisões aceitas

### D-00004-1 — Pacote / local — **Aceito: A**

Monorepo: projeto `src/SSO.Client/` → NuGet **`SSO.Client`**. Claims compartilhadas com `SSO.Shared` (ou reexportadas) para não divergir de `SsoClaimTypes`.

### D-00004-2 — JS/TS — **Aceito: B**

Mesmo épico entrega:

| Pacote | Conteúdo mínimo |
|--------|-----------------|
| `SSO.Client` (.NET) | JwtBearer setup, `RequirePermission`, claim helpers, `SsoTokenClient` (refresh / switch_context) |
| `@sso/client` ou `sso-client` (JS/TS) | Parse claims do JWT, `requirePermission`, helper `perm_ver` / switch_context via BFF |

JS **não** embute client secret; usa BFF ou PKCE public client.

### D-00004-3 — Sample BFF — **Aceito: B**

Entregar **`samples/sso-bff`**: Minimal API que:

- autentica SPA via cookie same-site;
- guarda refresh token server-side;
- anexa Bearer às APIs / faz refresh e `switch_context`;
- usa `SSO.Client` internamente.

Também: `samples/product-api` (Minimal API + `RequirePermission`) como aceite do SDK .NET.

## Escopo

### Inclui

- `SSO.Client` + testes + pack local.
- Pacote JS/TS (pasta `clients/js` ou `src/SSO.Client.Js`) com build/test mínimos.
- `samples/product-api` + `samples/sso-bff`.
- Quickstart em `product-integration.md` + `integrations.md`.
- Semver: breaking de claims = major.

### Fora de escopo

- Portal admin / AuthZ admin (00002/00003).
- SDK gerenciar users/roles.
- Publicação em feed NuGet/npm público (pode ser local/`dotnet pack` até 00010).
- Revogação quente (00005) — só hooks/`perm_ver` agora.

## Entregue

| Item | Caminho |
|------|---------|
| Lib .NET 0.1.0 | `src/SSO.Client/` — `AddSsoAuthentication`, `RequiresPermissionAttribute`, claim extensions, `SsoTokenClient` |
| Tests .NET | `src/SSO.Client.Tests/` (3 tests) |
| Sample API | `samples/product-api/` |
| Sample BFF | `samples/sso-bff/` — `/bff/session`, `/bff/refresh`, `/bff/switch-context`, `/bff/me` |
| JS/TS 0.1.0 | `clients/js/` — `parseJwtPayload`, `requirePermission`, `getPermissionVersion` |
| Docs | `product-integration.md` (quickstart), `integrations.md`, README, `samples/README.md` |

## Critérios de aceite

- [x] Product API sample valida JWT e bloqueia rota sem permission com setup curto.
- [x] `RequirePermission` cobre multi-claim `permissions`.
- [x] `perm_ver` exposto no .NET e no JS.
- [x] `switch_context` helper (.NET) + BFF sample exercita o fluxo.
- [x] Pacote JS: `requirePermission` + parse de claims documentado.
- [x] Docs quickstart copy-paste em Dev.

## Estratégia de testes

- [x] Unit .NET: claims, permission handler
- [x] Unit JS: requirePermission / claim parse
- [x] Sample product-api / sso-bff compilam
- [x] Contrato: claim names = `SsoClaimTypes`

## Checklist

- [x] D-00004-1..3 aceitas
- [x] Alinhado a ADR-003/005
- [x] Versionamento (0.1.0 inicial)
- [x] CONTEXT/integrations na implementação
- [x] Implementado
