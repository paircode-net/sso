# Feature Plan — 00003 Portal admin por papel

> Arquivo: `.ai/WORK/2026-07-16-00003-portal-admin-por-papel.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Refinamento**  
> Data: 2026-07-16  
> Depende de: **00002** (AuthZ APIs admin)  
> Relaciona: 00007 (gestão AuthClients na UI)

## Objetivo

Entregar um **portal administrativo** (UI) com experiências distintas para **Administrador de plataforma** e **Administrador de organização**, substituindo o uso exclusivo de API/Swagger para operações do dia a dia.

## Contexto

- D11: Admin MVP = API-only; login/consent Razor já existem em `SSO.Web.Api`.
- Atores em `business.md`: Platform Admin vs Org Admin.
- Sem UI, onboarding de tenants e operação de roles/memberships não escala.

## Escopo

### Inclui

**MVP do portal (v1)**

- Shell autenticado (OIDC Authorization Code + PKCE contra o próprio AS, client `sso-admin-spa` ou Razor Area).
- Navegação por papel (esconder menus sem permission `sso.admin.*`).
- **Org Admin**
  - Listar/criar/editar Branches da org ativa
  - Memberships (convidar/vincular usuário ↔ org/branch)
  - Atribuir roles (UserRoleAssignment) no contexto org/branch/product habilitado
  - Ver sessões do usuário e revogar (via API já existente)
  - Troca de contexto (org/branch) na UI
- **Platform Admin**
  - Organizations / Products / ProductEnablement
  - Permissions / Roles / RolePermissions (catálogo)
  - AuthClients e bindings client→product (CRUD mínimo; detalhe fino em 00007)
  - External IdPs (enable/config flags; secrets via config/KV, não na UI em texto claro)
  - Consulta de audit events (filtro por user/data)
- UX pt-BR; responsivo básico (desktop-first aceitável no v1).
- Integração apenas via APIs `api/identity/*` (sem bypass de Domain).

### Fora de escopo (v1)

- Design system completo / white-label por org.
- Self-service de usuário final (perfil além do que Razor Account já faz).
- Workflows de aprovação (joiner/mover/leaver).
- Relatórios analíticos avançados.
- App mobile nativo.

## Abordagem

### Opção de stack (decisão aberta)

| Opção | Prós | Contras |
|-------|------|---------|
| **A. Razor Pages/Areas** no `SSO.Web.Api` | Zero novo host; alinhado ao login atual | Menos “SPA”; acopla UI ao host AS |
| **B. SPA (Blazor WASM ou React) + client OIDC** | UX moderna; separação | Novo projeto, CORS, deploy |
| **C. Blazor Server** no mesmo host | Interatividade; um deploy | Sticky sessions / scaling |

**Recomendação de refinamento:** Opção A para v1 (rápido, consistente com D6), com Area `/Admin`; reavaliar SPA na v2 se a UX exigir.

### Fases

1. **Shell + auth** — Area Admin, cookie/OIDC session, menu por permission.
2. **Org Admin flows** — Branches, Memberships, Assignments.
3. **Platform Admin flows** — Orgs, Products, catálogo Permission/Role, audit read-only.
4. **Polish** — empty states, erros FluentValidation, confirmações de revoke.

### Modelo de autorização na UI

- Não confiar só em esconder botões: toda ação chama API já protegida (00002).
- Menu efetivo pode usar `GET menus/effective` **ou** mapa estático Admin → `sso.admin.*`.

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Web | `SSO.Web.Api/Areas/Admin/**` (ou novo `SSO.Web.Admin`) |
| Middleware | CORS/client seed `sso-admin-*`; cookies antiforgery |
| Data / Seed | AuthClient admin; MenuItems admin opcionais |
| Tests | Smoke Selenium/Playwright **ou** integração de páginas mínimas; priorizar API (já em 00002) |
| Docs | `modules.md`, `business.md`, README (como acessar `/Admin`) |

## Critérios de aceite

- [ ] OrgAdmin autentica, seleciona org, gerencia branch/membership/role sem Swagger.
- [ ] PlatformAdmin gerencia org/product/permission sem Swagger.
- [ ] Usuário sem `sso.admin.*` não acessa Area Admin (redirect/403).
- [ ] Nenhuma operação de escrita sem CSRF protection adequada (Razor) ou Bearer+PKCE (SPA).
- [ ] Fluxos cobertos por checklist manual + testes de API.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Escopo de UI explode | Strict v1 checklist; sem “nice to have” |
| Duplicar lógica de negócio na UI | UI só orquestra commands/queries existentes |
| Confundir Product Admin com Platform Admin | Permissions e menus separados; copy clara |
| Deploy do AS com UI pesada | Area mínima; assets leves |

## Estratégia de testes

- [ ] Integração API (já 00002) como garantia principal
- [ ] Testes de autorização de Area (filtro) se Razor
- [ ] Checklist manual E2E por papel (release checklist)
- [ ] Opcional: 1–2 Playwright smoke (login admin → list orgs)

## Decisões abertas

- [ ] **D-00003-1:** Razor Area vs SPA vs Blazor Server?
- [ ] **D-00003-2:** Convite de usuário = create User + e-mail ou só membership de user existente?
- [ ] **D-00003-3:** Multi-org switch no portal reusa grant `switch_context` ou cookie de sessão admin separado?

## Checklist

- [ ] 00002 concluído ou em paralelo com contrato de permissions fechado
- [ ] Alinhado a architecture / security
- [ ] Naming / localization pt-BR
- [ ] CONTEXT + README atualizados
- [ ] Pronto para implementação (após D-00003-1)
