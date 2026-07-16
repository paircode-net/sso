# Feature Plan — 00003 Portal admin por papel

> Arquivo: `.ai/WORK/2026-07-16-00003-portal-admin-por-papel.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Implementado** (2026-07-16)  
> Data: 2026-07-16  
> Depende de: **00002** (AuthZ APIs admin) — entregue  
> Relaciona: 00007 (gestão AuthClients na UI)

## Objetivo

Entregar um **portal administrativo** (UI) com experiências distintas para **Administrador de plataforma** e **Administrador de organização**, substituindo o uso exclusivo de API/Swagger para operações do dia a dia.

## Contexto

- D11: Admin MVP = API-only; login/consent Razor já existem em `SSO.Web.Api`.
- 00003 introduz UI admin (supersede parcial de D11 para operação humana).
- 00002 protege `api/identity/*` com `sso.admin.*`.
- Atores: Platform Admin vs Org Admin (`business.md`, `admin-api-authz.md`).

## Decisões aceitas

### D-00003-1 — Stack — **Aceito: A**

**Razor Pages / Area `/Admin`** no host `SSO.Web.Api` (mesmo processo do login). Sem SPA/Blazor Server no v1.

### D-00003-2 — Vínculo usuário ↔ organização — **Aceito: convite + aceite**

A organização **não** cria `Membership` diretamente sem consentimento do usuário.

| Etapa | Comportamento |
|-------|----------------|
| Admin | Envia **convite** (e-mail) para um endereço |
| Sistema | Persiste convite `Pending` (org, e-mail, expiração, token) |
| Usuário | Aceita ou recusa via link (autenticado ou fluxo Account) |
| Só após aceite | Cria/atualiza User se necessário + **Membership** |

Fora do v1 deste fluxo: vincular user existente “no silêncio”; create+membership sem aceite.

### D-00003-3 — Contexto org/branch no portal — **Aceito: A**

Reusa o grant **`switch_context`** (ADR-003). Claims `organization_id` / `branch_id` no access token são a fonte de verdade. Cookie Identity só autentica a sessão do portal; após troca de contexto, o portal obtém novo access token (ex.: confidential/BFF no server Razor chamando `/connect/token` com o refresh + switch_context, ou fluxo equivalente server-side).

**Não** usar cookie `AdminOrganizationId` como fonte de verdade para APIs.

## Escopo

### Inclui (v1)

- Area `/Admin` autenticada (cookie Identity + chamada às APIs com Bearer obtido server-side / sessão admin).
- Navegação por `sso.admin.*` (esconder + gate; APIs já protegidas).
- **Org Admin:** branches; **convites** (listar/enviar/reenviar/cancelar); assignments após membership ativo; sessões/revoke; UI de switch org/branch via `switch_context`.
- **Platform Admin:** orgs, products, catálogo permission/role, audit read-only; AuthClients CRUD mínimo (detalhe em 00007); IdPs flags (sem secrets em claro).
- Aggregate/API de **OrganizationInvite** (novo) + páginas Accept/Decline.
- UX pt-BR; desktop-first.

### Fora de escopo (v1)

- SPA / Blazor WASM.
- Design system / white-label.
- Self-service de perfil além do Account atual.
- Joiner/mover/leaver workflows avançados.
- Playwright obrigatório (opcional).

## Abordagem

### Fases

1. **Shell `/Admin`** — layout, menu por permission, auth gate, obtenção de token admin + switch_context.
2. **OrganizationInvite** — Domain + API + e-mail + páginas Accept/Decline.
3. **Org Admin** — Branches, convites, assignments, sessões.
4. **Platform Admin** — Orgs/Products/catálogo/audit.
5. **Polish** — empty states, confirmações, docs README `/Admin`.

### Auth no Razor Area

- Login cookie (já existe).
- Server-side: client confidential `sso-admin-api` (ou público+PKCE server) para Bearer nas calls `api/identity/*`.
- Troca de contexto: form POST → `switch_context` → guarda access token na sessão server (não expor refresh ao browser se possível).

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | `OrganizationInvites/` |
| Application | Commands/Queries invites; handlers Accept |
| Data | Migration invites; seed |
| Services | Mail templates convite |
| Web | `Areas/Admin/**`, `Account/AcceptInvite` (ou similar) |
| Middleware | Session/token store helper; antiforgery |
| Tests | Invite lifecycle; Area authorize; switch_context no portal |
| Docs | `modules.md`, `business.md`, README, Decisions |

## Critérios de aceite

- [ ] OrgAdmin autentica, faz switch_context, gerencia branches/convites/assignments sem Swagger.
- [ ] Membership só nasce após aceite do convite.
- [ ] PlatformAdmin gerencia org/product/permission/audit sem Swagger.
- [ ] Usuário sem `sso.admin.*` não acessa `/Admin`.
- [ ] Escrita protegida com antiforgery.
- [ ] Contexto ativo do portal = claims do token pós-`switch_context` (ADR-003).

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Escopo de UI explode | Checklist v1 estrito |
| Token admin no server mal guardado | Session server-side; sem refresh no JS |
| Convite account-takeover por e-mail | Token one-time; expiração; e-mail confirmado no aceite |
| Duplicar lógica de negócio | UI só orquestra APIs |

## Estratégia de testes

- [ ] Unit/integration invite Pending→Accepted
- [ ] Negativo: membership sem aceite
- [ ] Area `/Admin` 401/403 sem permission
- [ ] Checklist manual E2E por papel

## Checklist

- [x] D-00003-1..3 aceitas
- [x] 00002 entregue
- [x] Alinhado a ADR-003 / security
- [ ] Naming / localization pt-BR (na implementação)
- [ ] CONTEXT + README na implementação
- [x] Pronto para implementação
