# Admin portal

> Feature 00003 — F00003-D1/D2/D3 (shell MVP)  
> Feature **00011** — expansão completa dos cadastros (**implementado**)  
> Related: [admin-api-authz.md](admin-api-authz.md), ADR-003, [00011](../WORK/2026-07-20-00011-expansao-cadastros-admin.md)

## Access

1. Login em `/Account/Login` (`admin@sso.local` / `ChangeMe!123` no Dev).
2. Abrir `/Admin`.
3. Em **Contexto**, selecionar organização (switch_context server-side na sessão).
4. Navegação por `sso.admin.*` (Org vs Platform).

## Cadastros (`/Admin`)

Páginas orquestram Application (MediatR / `AdminWrap`) ou serviços equivalentes; sem Domain Service direto nas PageModels CQRS.

| Papel | Páginas |
|-------|---------|
| Org (+ Platform) | Branches, Invites (+ resend/cancel), Memberships (list/remove), UserRoleAssignments, UserClaimAssignments, Sessions (`sessions.revoke`) |
| Platform | Organizations, Products, Permissions, Roles, RolePermissions, ClientProductBindings, AuthClients, ExternalIdPs, ClaimDefinitions, RoleClaims, Users, LdapMaps, MenuItems (`menus`) |
| Audit | Audit (`audit.read`) |

## Convites (F00003-D2)

- Admin envia convite por e-mail (`/Admin/Invites`).
- Convidado abre `/Account/AcceptInvite?token=...`, aceita ou recusa.
- **Membership só é criada após aceite.**
- API: `api/identity/organization-invites` (+ `PATCH …/{id}/cancel`, `PATCH …/{id}/resend`).

## Contexto (F00003-D3)

Sessão guarda o resultado do switch; claims `organization_id` / `permissions` são enriquecidas no request do portal a partir do client `sso-admin-api` (equivalente ao grant `switch_context`).
