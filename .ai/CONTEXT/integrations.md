# Integrations

## Integrações presentes

| Integração | Tipo | Status |
|------------|------|--------|
| SQL Server | Persistência | `DefaultConnection` + `IdentityConnection` |
| Swagger / OpenAPI | Documentação de API (dev) | Swashbuckle no host |
| OpenIddict / OIDC | Authorization Server | `/connect/*` (Fases 2–5) |
| ASP.NET Identity | AuthN (conta/2FA) | IdentityDb |
| MediatR Notifications | In-process | Handlers logam JSON |
| Mail (`IMailService`) | E-mail | Logger MVP; capture em testes |

## Contrato para products (JWT)

Ver **[product-integration.md](product-integration.md)** — claim names, TTL, `perm_ver`, menus, fluxo de permission dinâmica.

## Integrações não presentes

- Identity Providers externos (Entra ID → Google → LDAP) — Fase 6
- Message bus / cache distribuído / APM cloud — não no MVP

## Contratos HTTP internos (API)

Rotas de gestão: `api/identity/{resource}`. Protocolo OIDC: `/connect/*`.
