# Integrations

## Integrações presentes

| Integração | Tipo | Status |
|------------|------|--------|
| SQL Server | Persistência | `DefaultConnection` + `IdentityConnection` |
| Swagger / OpenAPI | Documentação de API (dev) | Swashbuckle no host |
| OpenIddict / OIDC | Authorization Server | `/connect/*` (Fases 2–6) |
| ASP.NET Identity | AuthN (conta/2FA) | IdentityDb |
| Microsoft Entra ID | IdP externo (OIDC) | Homologável via `Sso:ExternalAuth:Entra`; catálogo `entra-homolog` |
| Google / LDAP | IdP externo | Stubs (`google-stub` / `ldap-stub`); habilitar via config |
| MediatR Notifications | In-process | Handlers logam JSON |
| Mail (`IMailService`) | E-mail | Logger MVP; capture em testes |

## Contrato para products (JWT)

Ver **[product-integration.md](product-integration.md)** — claim names, TTL, `perm_ver`, menus, fluxo de permission dinâmica.

## Hardening / IdPs

Ver **[phase6-hardening.md](phase6-hardening.md)** — CORS, rate limit, lockout, P-004, signing, external login.

## Integrações não presentes

- Message bus / cache distribuído / APM cloud — não no MVP
- Google/LDAP production-ready (apenas stub até enable + secrets)

## Contratos HTTP internos (API)

Rotas de gestão: `api/identity/{resource}`. Protocolo OIDC: `/connect/*`.
