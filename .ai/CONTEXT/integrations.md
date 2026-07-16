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

## SDK / BFF (feature 00004)

| Artefato | Caminho | Uso |
|----------|---------|-----|
| `SSO.Client` (.NET 0.1.0) | `src/SSO.Client/` | JwtBearer, `RequiresPermission`, claim helpers, `SsoTokenClient` |
| `@sso/client` (JS/TS 0.1.0) | `clients/js/` | Parse claims + `requirePermission` (sem verificar assinatura) |
| Sample product API | `samples/product-api/` | Aceite do SDK .NET |
| Sample BFF | `samples/sso-bff/` | Cookie/session + refresh + `switch_context` |

Pack local: `dotnet pack src/SSO.Client` · JS: `cd clients/js && npm run build`.

## Hardening / IdPs

Ver **[phase6-hardening.md](phase6-hardening.md)** — CORS, rate limit, lockout, P-004, signing, external login.

## Integrações não presentes

- Message bus / cache distribuído / APM cloud — não no MVP
- Google/LDAP production-ready (apenas stub até enable + secrets)

## Contratos HTTP internos (API)

Rotas de gestão: `api/identity/{resource}`. Protocolo OIDC: `/connect/*`.
