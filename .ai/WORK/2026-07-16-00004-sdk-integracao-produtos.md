# Feature Plan — 00004 SDK / BFF de integração de produtos

> Arquivo: `.ai/WORK/2026-07-16-00004-sdk-integracao-produtos.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Pronto para implementação** (D-00004-1..3 aceitas)  
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

## Abordagem

### Fases

1. **SSO.Client** — options, JwtBearer, `RequirePermission`, claim extensions, `perm_ver`.
2. **SsoTokenClient** — refresh + switch_context.
3. **samples/product-api** — quickstart &lt; 30 linhas de setup.
4. **JS/TS client** — claims + requirePermission + tipos alinhados a `SsoClaimTypes`.
5. **samples/sso-bff** — cookie + refresh + proxy pattern.
6. **Docs** — product-integration quickstart.

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Lib .NET | `src/SSO.Client/` |
| Lib JS | `clients/js/` (ou equivalente) |
| Samples | `samples/product-api/`, `samples/sso-bff/` |
| Tests | `src/SSO.Client.Tests/` (+ testes JS) |
| Solution | incluir projetos no `.sln` |
| Docs | `product-integration.md`, `integrations.md`, README |

## Critérios de aceite

- [ ] Product API sample valida JWT e bloqueia rota sem permission com setup curto.
- [ ] `RequirePermission` cobre multi-claim `permissions`.
- [ ] `perm_ver` exposto no .NET e no JS.
- [ ] `switch_context` helper (.NET) + BFF sample exercita o fluxo.
- [ ] Pacote JS: `requirePermission` + parse de claims documentado.
- [ ] Docs quickstart copy-paste em Dev.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Drift claims Shared vs Client | Referenciar `SSO.Shared` ou gerar constantes compartilhadas |
| Secret em SPA | JS só public/PKCE ou via BFF |
| Escopo JS + BFF estoura | MVP JS = claims + requirePermission; BFF mínimo cookie/refresh |
| Dois packages sem CI pack | `dotnet pack` + npm script local até 00010 |

## Estratégia de testes

- [ ] Unit .NET: claims, permission handler
- [ ] Unit JS: requirePermission / claim parse
- [ ] Sample product-api sobe e retorna 401/403
- [ ] Contrato: claim names = `SsoClaimTypes`

## Checklist

- [x] D-00004-1..3 aceitas
- [x] Alinhado a ADR-003/005
- [ ] Versionamento (0.1.0 inicial)
- [ ] CONTEXT/integrations na implementação
- [x] Pronto para implementação
