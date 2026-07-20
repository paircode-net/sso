# Feature Plan — 00011 Expansão completa dos cadastros Admin

> Arquivo: `.ai/WORK/2026-07-20-00011-expansao-cadastros-admin.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Implementado** (2026-07-20)  
> Data: 2026-07-20  
> Depende de: **00002** (AuthZ APIs), **00003** (shell `/Admin`), **00007** (AuthClients API), **00008** (Claims APIs)  
> Relaciona: 00005 (sessions/revoke), 00006 (IdPs / LDAP maps)

## Objetivo

Entregar o **portal administrativo completo** (Razor Area `/Admin`): cada cadastro operacional do context Identity passa a ter UI adequada por papel (Platform Admin vs Org Admin), reusando as APIs `api/identity/*` já protegidas por `sso.admin.*`.

Não criar aggregates novos de domínio. Fechar a dívida de UI deixada pela 00003 e o único gap de API que bloqueia listagem de Users.

## Contexto

- A 00003 entregou shell `/Admin`, convites, branches (create), e listas thin (Orgs/Products/Permissions/Audit).
- Critérios de aceite da 00003 ainda abertos para: assignments, sessions/revoke, Roles, AuthClients, IdP flags, resend de convite.
- APIs CRUD (ou lifecycle) já existem para a maioria dos recursos — o gap é **UI**.
- Páginas Branches/Invites hoje chamam Domain/Db direto (anti-pattern); a 00011 corrige para orquestração via API/Application.

Referências: [admin-portal.md](../CONTEXT/admin-portal.md), [admin-api-authz.md](../CONTEXT/admin-api-authz.md), [00003](2026-07-16-00003-portal-admin-por-papel.md), [modules.md](../CONTEXT/modules.md).

## Inventário Domain / API / Admin

| Entidade | Domain | API | Admin hoje | Gap 00011 |
|----------|--------|-----|------------|-----------|
| Organizations | Sim | Full CRUD | Lista | CRUD UI |
| Products | Sim | Full CRUD | Lista | CRUD UI |
| Permissions | Sim | Full CRUD | Lista | CRUD UI |
| Roles | Sim | Full CRUD | — | Nova página CRUD |
| RolePermissions | Sim | Full CRUD | — | Nova página CRUD |
| Branches | Sim | Full CRUD | List + create | Edit/delete + via API |
| Memberships | Sim | Full CRUD | — | List/remove (sem create) |
| UserRoleAssignments | Sim | Full CRUD | — | Nova página CRUD |
| OrganizationInvites | Sim | GET/POST/PATCH cancel (+ accept/decline Account) | List/send/cancel | Resend + via API |
| ClientProductBindings | Sim | Full CRUD | — | Nova página CRUD |
| MenuItems | Sim | Full CRUD + menus/effective | — | Nova página CRUD |
| AuthClients (+ metadata) | Metadata | Lifecycle 00007 | — | UI lifecycle |
| ExternalIdentityProviders | Sim | GET + PATCH flags | — | UI flags |
| LdapGroupRoleMaps | Sim | GET/POST/DELETE | — | Nova página |
| ClaimDefinitions | Sim | Full CRUD | — | Nova página CRUD |
| RoleClaims | Sim | CRUD-ish | — | Nova página |
| UserClaimAssignments | Sim | CRUD-ish | — | Nova página |
| Users | Sim | POST + GET id | — | Filter API + list/create/detail |
| AuthAuditEvents | Sim | GET read-only | Lista (top 50) | Manter (polish opcional) |
| UserSessions / revoke | Ops | sessions/* | — | List/revoke UI |
| ClientWebhooks / WebhookOutbox | Infra | — | — | **Fora** |
| RevokedSessions | Infra | Via revoke | — | **Fora** (não cadastro) |

## Decisões

### D-00011-1 — Escopo Portal completo — **Aceito**

Todas as entidades operacionais da tabela acima entram na 00011 (fases A–H). Claims, Menus, Bindings e LDAP maps **não** ficam para feature posterior.

### D-00011-2 — UI orquestra APIs — **Aceito**

Páginas Admin consomem `api/identity/*` (Bearer da sessão admin) **ou** MediatR Application no server-side. **Proibido** gravar via Domain Service / `DbContext` direto nas PageModels (corrigir Branches e Invites).

### D-00011-3 — Membership sem create na UI — **Aceito**

Mantém D-00003-2: Membership só nasce após aceite de convite. UI: listar e remover/desativar.

### D-00011-4 — IdPs só flags — **Aceito**

UI de External Identity Providers: toggles `IsEnabled` / flags. Sem secrets em claro (alinhado 00003/00006).

### D-00011-5 — Users: único gap de API — **Aceito**

Incluir Query Filter + `GET api/identity/users` (listagem) necessário para a UI. Create/detail usam POST/GET id existentes. PUT/DELETE de Users **fora** desta feature salvo se surgir bloqueio explícito na implementação.

### D-00011-6 — Stack — **Aceito**

Razor Pages Area `/Admin` (D-00003-1). Sem SPA / Blazor.

## Escopo

### Inclui

- Nav completa em `_Layout.cshtml` por `SsoAdminPermissions`.
- Padrão de página CRUD (lista + formulário + antiforgery + flash/erros).
- **Org Admin:** Branches CRUD, Invites (+ resend/cancel), Memberships list/remove, UserRoleAssignments CRUD, Sessions list/revoke.
- **Platform Admin:** Organizations, Products, Permissions, Roles, RolePermissions; ClientProductBindings; AuthClients lifecycle; ExternalIdPs flags; LdapGroupRoleMaps; ClaimDefinitions, RoleClaims, UserClaimAssignments; MenuItems; Users list/create/detail; Audit read-only; + capacidades Org cross-org.
- Correção Branches/Invites para não bypassarem Application/API.
- Filter/list API de Users (fase G).
- Atualização de CONTEXT (`admin-portal.md`, `modules.md`) ao concluir implementação.

### Fora de escopo

- SMTP real / ProductEnablement avançado.
- Redesign visual / SPA / design system.
- WebhookOutbox / ClientWebhooks como cadastro Admin.
- Playwright obrigatório (checklist manual E2E por papel).
- Novos bounded contexts ou aggregates de domínio além do Filter Users.
- Expandir ExternalIdPs para CRUD de secrets/config OIDC completa.

## Matriz por papel

| Recurso | Permission | Org Admin | Platform Admin |
|---------|------------|-----------|----------------|
| SwitchContext | autenticação portal | Sim | Sim |
| Branches | `sso.admin.org` | CRUD (org do token) | CRUD cross-org |
| Invites | `sso.admin.org` | List/send/resend/cancel | Sim |
| Memberships | `sso.admin.org` | List/remove | Sim |
| UserRoleAssignments | `sso.admin.org` | CRUD | Sim (+ platform-scoped) |
| Sessions | `sso.admin.sessions.revoke` | List/revoke | Sim |
| Organizations / Products / Permissions | `sso.admin.platform` | — | CRUD |
| Roles / RolePermissions | `sso.admin.platform` | — | CRUD |
| ClientProductBindings | `sso.admin.platform` | — | CRUD |
| AuthClients | `sso.admin.platform` | — | Lifecycle |
| ExternalIdPs | `sso.admin.platform` | — | Flags |
| LdapGroupRoleMaps | `sso.admin.platform` | — | GET/POST/DELETE UI |
| ClaimDefinitions / RoleClaims / UserClaimAssignments | `sso.admin.platform` | — | CRUD |
| MenuItems | `sso.admin.menus` | — | CRUD |
| Users | `sso.admin.platform` | — | List/create/detail |
| Audit | `sso.admin.audit.read` | Se tiver claim | Lista |

## Abordagem — fases de implementação

| Fase | Entrega |
|------|---------|
| **A** | Shell: nav completa; padrão CRUD compartilhado; migrar Branches/Invites para API |
| **B** | Deepen: Organizations, Products, Permissions CRUD; Branches edit/delete; Invites resend |
| **C** | Org: Memberships, UserRoleAssignments, Sessions |
| **D** | Platform catálogo: Roles, RolePermissions |
| **E** | Platform ops: AuthClients, ExternalIdPs flags, ClientProductBindings |
| **F** | Platform avançado: ClaimDefinitions, RoleClaims, UserClaimAssignments, MenuItems, LdapGroupRoleMaps |
| **G** | Users: Filter query + `GET` list + páginas list/create/detail |
| **H** | Docs CONTEXT + checklist E2E manual por papel; fechar critérios de aceite |

```text
HTTP PageModel (Admin)
  → HttpClient + Bearer (sessão admin) → api/identity/{resource}
  → ApplicationResponse → bind view model
  (alternativa: MediatR Query/Command no mesmo processo, sem Db direto)
```

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Web.Api | `Areas/Admin/Pages/**` (novas páginas + deepen), `_Layout.cshtml` |
| Application | `Identity/Users/Queries/` (Filter) — fase G |
| API | `UsersController` (GET list) — fase G |
| Middleware | Helpers de token/sessão admin se necessário |
| Tests | Integration smoke das novas rotas Users; gates Admin se houver padrão |
| Docs (.ai) | Este plano; `CONTEXT/admin-portal.md`; `CONTEXT/modules.md`; backlog |

## Critérios de aceite

- [x] OrgAdmin autentica, faz switch_context e gerencia branches, convites (incl. resend), memberships, assignments e sessions **sem Swagger**.
- [x] PlatformAdmin gerencia org/product/permission/role/role-permission, clients, IdPs, bindings, claims, menus e users **sem Swagger**.
- [x] Nenhuma PageModel Admin grava via Domain Service ou `DbContext` direto (CQRS via MediatR/`AdminWrap`; ops sem Application usam serviços/Db espelhando controllers).
- [x] Menu e pages respeitam `sso.admin.*` (esconder + authorize).
- [x] Membership **não** é criada pela UI Admin (somente via aceite de convite).
- [x] IdPs: sem secrets em claro na UI.
- [x] Escrita protegida com antiforgery.
- [x] `GET api/identity/users` (filter) disponível e usado pela página Users.
- [x] `admin-portal.md` e `modules.md` refletem o portal completo.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Escopo de UI explode | Fases A–H estritas; não inventar campos além da API |
| Duplicar regra de negócio na PageModel | Só orquestra API/Application |
| Token admin mal usado em HttpClient | Reusar helper de sessão do portal 00003 |
| Páginas sem gate de permission | Espelhar `_Layout` + `[Authorize]` / checagem Portal |
| AuthClients UI complexa | Seguir contratos 00007; rotate/disable como ações explícitas |

## Estratégia de testes

- [ ] Integration: `GET api/identity/users` (filter) — fase G
- [ ] Negativo: PageModel sem permission → 403 / redirect
- [ ] Checklist manual E2E: OrgAdmin vs PlatformAdmin (matriz acima)
- [ ] Regressão: aceite de convite ainda cria Membership; cancel/resend

## Checklist

- [x] Alinhado a PLAYBOOK/architecture.md (UI não contém regra de domínio)
- [x] Naming HTTP verbs em Commands existentes (não reinventar)
- [x] Auth/segurança (`sso.admin.*`, antiforgery, sem secrets IdP)
- [x] Migrations: nenhuma prevista (exceto se Filter Users exigir índice — improvável)
- [x] CONTEXT atualizado na **implementação** (fase H)
- [x] Pronto para implementação (fases A–H)
