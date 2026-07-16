# Feature Plan — 00002 AuthZ nas APIs admin

> Arquivo: `.ai/WORK/2026-07-16-00002-authz-apis-admin.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Implementado** (2026-07-16)  
> Data: 2026-07-16  
> Depende de: 00001 (entregue)  
> Bloqueia: 00003 (portal), exposição pública segura de `api/identity/*`

## Objetivo

Garantir que **todas as APIs de gestão Identity** exijam autenticação e autorização adequadas (permission codes / policies), eliminando o gap residual do security checklist antes de qualquer exposição fora do ambiente de desenvolvimento.

## Contexto

- MVP entregue com Admin API-only (D11); vários endpoints `api/identity/*` existem para CRUD e diagnóstico.
- Security checklist: `[Authorize]` em audit/menus/revoke ainda aberto.
- Architecture.md lista “AuthZ nas APIs admin sensíveis” em **A definir**.
- Sem esta feature, portal (00003) e multi-produto em staging compartilhado são inseguros.
- `EffectivePermissionsResolver` hoje retorna vazio se `organizationId` é null — impacto direto em PlatformAdmin (ver D-00002-2).

Fontes: `CHECKLISTS/security-checklist.md`, `CONTEXT/architecture.md`, ADR-005, `product-integration.md`.

## Escopo

### Inclui

- Inventário completo de controllers/rotas `api/identity/*` e classificação por sensibilidade.
- `[Authorize]` (Bearer) em todas as rotas de gestão (exceto health/public explícitos, se houver).
- Permission codes admin (coarse):
  | Code | Uso |
  |------|-----|
  | `sso.admin.platform` | Products, orgs (CRUD global), catálogo Permission/Role, bindings, IdPs |
  | `sso.admin.org` | Memberships, branches, user-role assignments no tenant do token |
  | `sso.admin.audit.read` | `GET` auth-audit-events |
  | `sso.admin.sessions.revoke` | Revogação de sessões/tokens de usuários |
  | `sso.admin.menus` | CRUD MenuItem + `GET menus/effective` (diagnóstico) |
- Extensão do resolver: assignments **platform-scoped** (`UserRoleAssignment.OrganizationId == null`) entram no JWT mesmo sem `organization_id` ativo (D-00002-2).
- Seed: Product “SSO Platform” + client `sso-admin-api` (ou reusar client confidential de gestão) + roles `PlatformAdmin` / `OrgAdmin` + RolePermissions.
- Isolamento: OrgAdmin só age na `organization_id` do token; PlatformAdmin pode cross-org em rotas de tenant.
- Negativos de teste: 401 / 403 / cross-org negado para OrgAdmin.
- Docs: `CONTEXT/admin-api-authz.md` + security checklist + architecture.

### Fora de escopo

- UI de portal (00003).
- Mudança do motor Role→Permission de produto além do necessário para platform-scoped.
- Introspection / revogação quente (00005).
- RBAC fino por recurso individual (ex.: “só editar Branch X”).

## Decisões aceitas

### D-00002-1 — Mecanismo de AuthZ na API — **Aceito**

**Pergunta:** Policies ASP.NET nomeadas vs atributo custom lendo `permissions`?

**Decisão:** `IAuthorizationRequirement` + `IAuthorizationHandler` que valida claim `permissions` (ADR-005), exposto na DX como atributo `[RequiresPermission("sso.admin.org")]` (implementa / registra policy dinâmica).

| Opção | Veredito |
|-------|----------|
| Só policies nomeadas (`AddPolicy` por code) | Rejeitada — explode configuração |
| Filtro/action checando claims manualmente | Rejeitada — fura pipeline de autorização |
| **Handler + `[RequiresPermission]`** | **Aceita** |

**Impacto:** constantes em `SSO.Shared`; registro em `Configurations` / `AddAuthorization`; controllers usam o atributo; testável unitariamente no handler.

### D-00002-2 — Contexto de org do PlatformAdmin — **Aceito**

**Pergunta:** PlatformAdmin precisa de `organization_id` no token?

**Decisão (híbrida):**

1. **Emissão JWT:** estender `EffectivePermissionsResolver` / factory para unir:
   - permissions **platform-scoped**: `UserRoleAssignment` com `OrganizationId == null` (e Product do client admin), **independente** de org ativa;
   - permissions **tenant-scoped**: regra atual (exige membership + org no token).
2. **Rotas platform** (`sso.admin.platform`, e em geral catálogo global): **não** exigem claim `organization_id`.
3. **Rotas tenant** (`sso.admin.org`, branches, memberships, assignments):
   - **OrgAdmin:** exige `organization_id` no token; recurso deve pertencer a essa org.
   - **PlatformAdmin** (`sso.admin.platform` no token): pode operar cross-org; `organizationId` do recurso vem da rota/body (não precisa bater com o token).
4. Seed: assignments `PlatformAdmin` com `OrganizationId = null`; `OrgAdmin` com org do tenant.

**Schema (obrigatório):** hoje `UserRoleAssignment.OrganizationId` é `Guid` required + validator `NotEmpty`. A feature **deve** torná-lo `Guid?`, ajustar EF/migration, validator (null = platform-scoped) e queries do resolver.

**Por quê:** o resolver atual zera tudo sem org — sem extensão, PlatformAdmin seria inutilizável antes de switch-context. Manter OrgAdmin amarrado ao token preserva ADR-003.

**Não fazer:** header `X-Organization-Id` como fonte de verdade (viola ADR-003 para o caller OrgAdmin).

### D-00002-3 — `menus/effective` — **Aceito**

**Pergunta:** Diagnóstico admin vs API de produto?

**Decisão:** permanece **diagnostic/admin only**. Exige `sso.admin.menus` (Bearer). Products continuam autorizando UI via claims `permissions` / `perm_ver` no JWT (`product-integration.md`). SDK (00004) **não** deve depender deste endpoint em runtime.

| Opção | Veredito |
|-------|----------|
| Abrir a qualquer usuário autenticado do product | Rejeitada |
| Endpoint público de product runtime | Rejeitada (duplica JWT; amplifica superfície) |
| **Admin-only + permission `sso.admin.menus`** | **Aceita** |

## Abordagem

### Fase A — Inventário e matriz

1. Listar controllers Identity e mapear rota → permission (tabela abaixo).
2. Marcar isolamento: `platform` | `tenant` | `tenant+platform-override`.

### Fase B — Resolver + AuthZ wiring

1. Estender `EffectivePermissionsResolver` (platform-scoped).
2. `PermissionRequirement` + handler + `[RequiresPermission]`.
3. Aplicar nos controllers; Bearer scheme nas APIs Identity.
4. Client/product seed admin para emissão das permissions no token.

### Fase C — Isolamento nos handlers + docs

1. Helper `IAdminOrganizationScope` (valida org do token vs recurso; bypass se platform).
2. Doc `admin-api-authz.md`; atualizar checklists.

### Matriz inicial (rotas → permission)

| Área | Permission | Isolamento |
|------|------------|------------|
| Organizations, Products, Permissions, Roles, RolePermissions, ClientProductBindings, ExternalIdPs | `sso.admin.platform` | platform |
| Branches, Memberships, UserRoleAssignments | `sso.admin.org` *ou* `sso.admin.platform` | tenant (+ override platform) |
| Users (gestão) | `sso.admin.platform` (create global) / `sso.admin.org` (leitura no tenant) — **detalhar na implementação** | híbrido |
| Auth audit GET | `sso.admin.audit.read` | platform (filtro opcional por org) |
| Sessions revoke | `sso.admin.sessions.revoke` | tenant (+ override platform) |
| MenuItems CRUD + menus/effective | `sso.admin.menus` | platform |

\* Em ações tenant, aceitar **qualquer** das permissions listadas (OR), com regra de isolamento acima.

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Shared | `SsoAdminPermissions`, talvez `RequiresPermissionAttribute` se compartilhado |
| Domain / Application | Validação de scope org nos handlers sensíveis |
| Infrastructures.Services | `EffectivePermissionsResolver` (platform-scoped) |
| Middleware | `AddAuthorization`, handler; seed client admin se wiring ali |
| Data / Seed | `IdentitySeed` — roles, permissions, assignments null-org, binding client |
| API | `Resources/IdentityDb/**/*Controller.cs` |
| Tests | Handler unit; matriz 401/403; platform sem org no token; OrgAdmin cross-org 403 |
| Docs | `CONTEXT/admin-api-authz.md`, security-checklist, architecture, Decisions |

## Critérios de aceite

- [ ] Nenhuma rota `api/identity/*` sensível responde 200 sem Bearer válido.
- [ ] Usuário com só `sso.access` recebe 403 em audit, menus, revoke, CRUD de products.
- [ ] Token **sem** `organization_id` mas com assignment platform emite `sso.admin.platform` e acessa rotas platform.
- [ ] OrgAdmin não altera dados de outra organization.
- [ ] PlatformAdmin altera recurso de outra org (override) onde a matriz permite.
- [ ] `menus/effective` retorna 403 sem `sso.admin.menus`.
- [ ] Security checklist item de `[Authorize]` marcado.
- [ ] Doc `admin-api-authz.md` publicada.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Quebrar suíte que chama API sem auth | Fixture de token admin no TestHost |
| Permission admin no JWT de product SPA | Client admin distinto; products ignoram `sso.admin.*`; binding product SSO Platform |
| Assignment `OrganizationId` null conflita com constraints EF | Validar schema; migration se coluna era required |
| Platform override amplo demais | Só com `sso.admin.platform`; auditar writes cross-org |

## Estratégia de testes

- [ ] Unit: `PermissionAuthorizationHandler`
- [ ] Unit: resolver com assignment `OrganizationId` null
- [ ] Integração: matriz anon / user / org admin / platform admin
- [ ] Integração: platform sem `organization_id` no token
- [ ] Isolamento cross-org OrgAdmin vs PlatformAdmin

## Checklist

- [x] Decisões D-00002-1..3 aceitas
- [x] Alinhado a PLAYBOOK/architecture.md e security.md / ADR-003 / ADR-005
- [ ] Naming conforme coding-style.md (na implementação)
- [ ] Auth/segurança considerados
- [x] Migration planejada: `UserRoleAssignment.OrganizationId` → `Guid?` (platform-scoped)
- [ ] CONTEXT atualizado na implementação
- [x] Pronto para implementação
