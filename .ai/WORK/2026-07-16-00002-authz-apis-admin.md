# Feature Plan — 00002 AuthZ nas APIs admin

> Arquivo: `.ai/WORK/2026-07-16-00002-authz-apis-admin.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Refinamento**  
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

Fontes: `CHECKLISTS/security-checklist.md`, `CONTEXT/architecture.md`, ADR-005, `product-integration.md`.

## Escopo

### Inclui

- Inventário completo de controllers/rotas `api/identity/*` e classificação por sensibilidade.
- `[Authorize]` (Bearer) em todas as rotas de gestão (exceto health/public explícitos, se houver).
- Políticas / permission codes para ações admin, por exemplo:
  - `sso.admin.platform` — products, auth clients, permissions globais, IdPs
  - `sso.admin.org` — memberships, branches, roles/assignments no tenant
  - `sso.admin.audit.read` — leitura de `auth-audit-events`
  - `sso.admin.sessions.revoke` — revogação de sessões/tokens de usuários
  - `sso.admin.menus` — CRUD MenuItem / effective (diagnóstico)
- Seed de roles/permissions admin no IdentitySeed (dev) + documentação dos códigos.
- Filtro de escopo: admin de org só enxerga/age na `organization_id` do token (após switch-context).
- Negativos de teste: 401 sem token; 403 sem permission; cross-org negado.
- Atualizar security checklist e `architecture.md` (remover lacuna).

### Fora de escopo

- UI de portal (00003).
- Mudança do motor de permissões de produto (Role→Permission contextual já existe).
- Introspection / revogação quente (00005).
- RBAC fino por recurso individual (ex.: “só editar Branch X”) — MVP admin = permission coarse + isolamento por org.

## Abordagem

### Fase A — Inventário e matriz

1. Listar todos os controllers Identity e métodos HTTP.
2. Mapear cada rota → permission mínima + regra de isolamento (org vs platform).
3. Decidir: permission no JWT (ADR-005) vs policy que lê roles Identity — **preferir permission codes no JWT** para consistência com produtos.

### Fase B — Wiring

1. Registrar policies ASP.NET (`AddAuthorization`) mapeando permission claim → policy name, ou handler genérico `RequiresPermission("code")`.
2. Aplicar `[Authorize(Policy = ...)]` (ou atributo custom) nos controllers.
3. Endpoints de diagnóstico (`menus/effective`, audit) exigem permission dedicada; não deixar abertos a qualquer usuário autenticado.
4. Garantir que `switch_context` possa emitir permissions admin de org quando o usuário tiver assignment correspondente.

### Fase C — Seed e docs

1. Role `PlatformAdmin` / `OrgAdmin` no seed com RolePermissions.
2. Documentar códigos em `glossary.md` + `product-integration.md` (seção admin) ou doc dedicado `admin-api-authz.md`.

### Camadas

| Camada | Mudança |
|--------|---------|
| Domain | Opcional: constantes de permission admin (ou Shared) |
| Application | Handlers que hoje não validam org do caller passam a receber/validar contexto |
| Middleware | Policies + `IAuthorizationHandler` |
| API | Atributos Authorize; possivelmente filtro de resource-based org |
| Tests | Integração 401/403 + happy path com token admin |
| Docs | security checklist, architecture, CONTEXT |

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Shared | `SsoClaimTypes` / constantes `SsoAdminPermissions` |
| Middleware | `Configurations.cs` (AddAuthorization policies) |
| API | `Resources/IdentityDb/**/*Controller.cs` |
| Application | Handlers sensíveis (audit, sessions, menus) — validação de org |
| Data / Seed | `IdentitySeed` — roles/permissions admin |
| Tests | Novos `*AuthzScenarios` / estender HTTP scenarios |
| Docs | `security-checklist.md`, `architecture.md`, `CONTEXT/*` |

## Critérios de aceite

- [ ] Nenhuma rota `api/identity/*` sensível responde 200 sem Bearer válido.
- [ ] Usuário com só `sso.access` recebe 403 em audit, menus CRUD, revoke, CRUD de products.
- [ ] OrgAdmin não altera dados de outra organization.
- [ ] PlatformAdmin consegue operações globais documentadas.
- [ ] Security checklist item de `[Authorize]` marcado.
- [ ] Seed documentado no README / product-integration.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Quebrar suíte de testes que chama API sem auth | Atualizar TestHost para obter token admin; fixture compartilhada |
| Permission admin “vaza” para produtos via JWT | Scopes/audiences distintos ou prefixo `sso.admin.*` + products ignoram |
| Lock-out operacional (ninguém com admin) | Seed + runbook de bootstrap do primeiro PlatformAdmin |
| Over-fetch de permissions no JWT | Manter TTL 15m; codes coarse (poucos) |

## Estratégia de testes

- [ ] Unit: policy/handler `RequiresPermission`
- [ ] Integração: matriz rota × token (anon / user / org admin / platform admin)
- [ ] Isolamento cross-org em write/read
- [ ] Smoke: login → switch_context → chamada admin com permissions no JWT

## Decisões abertas

- [ ] **D-00002-1:** Policies ASP.NET vs atributo custom lendo claim `permissions`?
- [ ] **D-00002-2:** PlatformAdmin precisa de `organization_id` no token ou opera sem contexto de org?
- [ ] **D-00002-3:** Menus effective permanece diagnostic-only (admin) ou vira API de produto autenticada?

## Checklist

- [ ] Alinhado a PLAYBOOK/architecture.md e security.md
- [ ] Naming conforme coding-style.md
- [ ] Auth/segurança considerados
- [ ] Migrations (só se novas Permission/Role no seed via DML) planejadas
- [ ] CONTEXT atualizado
- [ ] Pronto para implementação (após fechar D-00002-*)
