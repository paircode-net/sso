# Admin portal

> Feature 00003 — F00003-D1/D2/D3 (shell MVP)  
> Feature **00011** — expansão completa dos cadastros (planejado)  
> Related: [admin-api-authz.md](admin-api-authz.md), ADR-003, [00011](../WORK/2026-07-20-00011-expansao-cadastros-admin.md)

## Access

1. Login em `/Account/Login` (`admin@sso.local` / `ChangeMe!123` no Dev).
2. Abrir `/Admin`.
3. Em **Contexto**, selecionar organização (switch_context server-side na sessão).
4. Org Admin: Filiais, Convites. Platform: Organizações, Produtos, Permissões, Auditoria.

## Estado dos cadastros

| Estado | Superfície |
|--------|------------|
| Entregue (00003 thin) | Branches (list+create), Invites (list/send/cancel), Orgs/Products/Permissions (lista), Audit (read-only), SwitchContext |
| Dívida / planejado (00011) | CRUD completo nas páginas thin; Roles, RolePermissions, Memberships, UserRoleAssignments, Sessions, AuthClients, IdPs, ClientProductBindings, Claims*, MenuItems, LdapGroupRoleMaps, Users; resend de convite; UI só via API |

Detalhe do inventário e fases A–H: [2026-07-20-00011-expansao-cadastros-admin.md](../WORK/2026-07-20-00011-expansao-cadastros-admin.md).

## Convites (F00003-D2)

- Admin envia convite por e-mail (`/Admin/Invites`).
- Convidado abre `/Account/AcceptInvite?token=...`, aceita ou recusa.
- **Membership só é criada após aceite.**
- API: `api/identity/organization-invites` (+ `PATCH …/{id}/cancel`).
- Resend na UI: previsto na 00011.

## Contexto (F00003-D3)

Sessão guarda o resultado do switch; claims `organization_id` / `permissions` são enriquecidas no request do portal a partir do client `sso-admin-api` (equivalente ao grant `switch_context`).
