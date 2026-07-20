# Admin portal

> Feature 00003 — F00003-D1/D2/D3  
> Related: [admin-api-authz.md](admin-api-authz.md), ADR-003

## Access

1. Login em `/Account/Login` (`admin@sso.local` / `ChangeMe!123` no Dev).
2. Abrir `/Admin`.
3. Em **Contexto**, selecionar organização (switch_context server-side na sessão).
4. Org Admin: Filiais, Convites. Platform: Organizações, Produtos, Permissões, Auditoria.

## Convites (F00003-D2)

- Admin envia convite por e-mail (`/Admin/Invites`).
- Convidado abre `/Account/AcceptInvite?token=...`, aceita ou recusa.
- **Membership só é criada após aceite.**
- API: `api/identity/organization-invites` (+ `PATCH …/{id}/cancel`).

## Contexto (F00003-D3)

Sessão guarda o resultado do switch; claims `organization_id` / `permissions` são enriquecidas no request do portal a partir do client `sso-admin-api` (equivalente ao grant `switch_context`).
