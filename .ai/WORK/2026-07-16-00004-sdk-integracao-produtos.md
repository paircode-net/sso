# Feature Plan — 00004 SDK / BFF de integração de produtos

> Arquivo: `.ai/WORK/2026-07-16-00004-sdk-integracao-produtos.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Refinamento**  
> Data: 2026-07-16  
> Depende de: contrato atual (`product-integration.md`, ADR-003/005)  
> Relaciona: 00005 (sinais de revogação no cliente)

## Objetivo

Publicar um **pacote de integração** (.NET primeiro) que padronize validação de JWT, leitura de claims SSO, autorização por permission code, helpers de `switch_context` e (opcional) invalidação local por `perm_ver` — reduzindo atrito e inconsistências entre produtos consumidores.

## Contexto

- Products autorizam localmente a partir do access token (ADR-005).
- Contrato documentado, mas cada time ainda precisa montar middleware/handlers do zero.
- Client credentials (`dev-product-service`) e SPA PKCE (`dev-product-spa`) já exemplificam padrões.

## Escopo

### Inclui

**Pacote `SSO.Client` / `Baysoft.Sso.AspNetCore` (nome a definir)**

- Extensões `AddSsoAuthentication(IConfiguration)` — Authority, Audience/ClientId, validação iss/aud/sign.
- Helpers de claims: `organization_id`, `branch_id`, `permissions`, `perm_ver`, `sub`.
- `IAuthorizationHandler` / policy factory: `RequirePermission("hq.reports")`.
- Opção de cache de menu/feature flags keyed por `perm_ver`.
- Cliente HTTP tipado para:
  - token endpoint (`refresh`, `switch_context`)
  - opcional: `menus/effective` (diagnóstico; products preferem JWT)
- Documentação + sample app mínimo (SPA ou Minimal API) no repo ou pasta `samples/`.
- Versionamento semver alinhado ao contrato de claims (breaking = major).

**Opcional na mesma feature (fase B)**

- Pacote JS/TS espelho (claims + `requirePermission`) — pode ser feature filha se timing apertar.

### Fora de escopo

- Portal admin (00003).
- Servir como Authorization Server.
- SDK gerenciar usuários/roles (isso é API admin).
- BFF genérico completo com agregação de APIs de negócio — apenas **auth BFF helpers** se necessário (refresh no server, cookie session).

## Abordagem

### Fase A — .NET AuthZ library

1. Novo projeto class library + pacote NuGet (local/feed).
2. Mapear options: `SsoClient:Authority`, `Audience`, `ClientId`, `ClientSecret` (confidential).
3. Policies baseadas em claim type `permissions` (multi-value).
4. Testes unitários com tokens JWT sintéticos (signing key de teste).

### Fase B — Token / switch_context client

1. `SsoTokenClient` encapsulando form posts OAuth.
2. Documentar PKCE no sample SPA (pode ser só README + referência OpenIddict).

### Fase C — Sample + docs

1. Atualizar `product-integration.md` com “Quickstart SDK”.
2. `integrations.md` aponta para o pacote.

### BFF (quando necessário)

- Pattern documentado: BFF guarda refresh token; browser só cookie same-site; BFF anexa Bearer às APIs.
- Implementação de referência opcional em `samples/sso-bff` — não obrigatória para fechar a feature se a lib cobrir APIs server-side.

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Novo projeto | `src/SSO.Client/` ou `src/SSO.AspNetCore.Client/` |
| Samples | `samples/product-api/` |
| Tests | `SSO.Client.Tests` |
| Docs | `product-integration.md`, `integrations.md`, README trecho “Consumindo o SSO” |
| CI | Pack/push NuGet (pode depender de 00010) |

## Critérios de aceite

- [ ] Product API de sample valida JWT e bloqueia rota sem permission em &lt; 30 linhas de setup.
- [ ] `RequirePermission` cobre multi-claim `permissions`.
- [ ] `perm_ver` exposto para invalidar cache local.
- [ ] `switch_context` helper emite novo access token e documenta claims resultantes.
- [ ] Docs de quickstart passam “copy-paste” em ambiente Dev.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Nome/claims mudarem e quebrarem products | Semver + changelog; ADR para mudanças de claim |
| Duplicar OpenIddict validation bugs | Reusar `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Secret em SPA | SDK documenta: secret só confidential/BFF |
| Pacote interno vs público | Começar feed interno; API estável antes de publicar |

## Estratégia de testes

- [ ] Unit: parse claims, policy handler
- [ ] Integração: TestServer product + TestServer SSO (ou JWT assinado local)
- [ ] Contrato: snapshot dos claim names vs `SsoClaimTypes`

## Decisões abertas

- [ ] **D-00004-1:** Nome do pacote e se vive neste monorepo ou repo `sso-client`.
- [ ] **D-00004-2:** Incluir JS/TS no mesmo épico ou feature separada?
- [ ] **D-00004-3:** Sample BFF é entregável ou só documentação do pattern?

## Checklist

- [ ] Alinhado a ADR-003/005 e product-integration
- [ ] Sem vazar secrets em samples
- [ ] Versionamento definido
- [ ] CONTEXT/integrations atualizado
- [ ] Pronto para implementação (após D-00004-1)
