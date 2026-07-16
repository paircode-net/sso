# Integrations

## Integrações presentes

| Integração | Tipo | Status |
|------------|------|--------|
| SQL Server | Persistência | `DefaultConnection` + `IdentityConnection` |
| Swagger / OpenAPI | Documentação de API (dev) | Swashbuckle no host |
| OpenIddict / OIDC | Authorization Server | `/connect/*` (Fases 2–6) |
| ASP.NET Identity | AuthN (conta/2FA) | IdentityDb |
| Entra ID | IdP externo (OIDC) | Homologável via `Sso:ExternalAuth:Entra`; catálogo `entra-homolog` |
| Google OIDC | IdP externo | `Sso:ExternalAuth:Google`; catálogo `google`; JIT/auto-link (00006) |
| LDAP / AD | IdP externo | `System.DirectoryServices.Protocols`; `/Account/LoginWithLdap`; grupo→Role |
| MediatR Notifications | In-process | Handlers logam JSON |
| Mail (`IMailService`) | E-mail | Logger MVP; capture em testes |

## Contrato para products (JWT)

Ver **[product-integration.md](product-integration.md)** — claim names, TTL, `perm_ver`, menus, fluxo de permission dinâmica.

## SDK / BFF (feature 00004)

| Artefato | Caminho | Uso |
|----------|---------|-----|
| `SSO.Client` (.NET 0.1.0) | `src/SSO.Client/` | JwtBearer, `RequiresPermission`, claim helpers, `SsoTokenClient`, **hot revoke check** (00005) |
| `@sso/client` (JS/TS 0.1.0) | `clients/js/` | Parse claims + `requirePermission` (sem verificar assinatura) |
| Sample product API | `samples/product-api/` | Aceite do SDK .NET |
| Sample BFF | `samples/sso-bff/` | Cookie/session + refresh + `switch_context` |

Pack local: `dotnet pack src/SSO.Client` · JS: `cd clients/js && npm run build`.

## Hot revocation (feature 00005)

| Artefato | Nota |
|----------|------|
| Claim `sid` | Access token; ADR-007 |
| Deny-list | `IdentityDb.RevokedSessions` |
| Status | `GET /api/identity/sessions/{sid}/status` |
| Webhook | Outbox + `X-SSO-Signature: sha256=…` (`ClientWebhookEndpoints`) |
| SLA | ≤ 60s com SDK default-on |

## Hardening / IdPs

Ver **[external-idps.md](external-idps.md)** e **[phase6-hardening.md](phase6-hardening.md)**.

## Consent + AuthClients (feature 00007)

| Artefato | Nota |
|----------|------|
| Consent Razor | `/Account/Consent` — Always/First; Never só first-party |
| Metadados | `AuthClientMetadata` (híbrido com OpenIddict) |
| Admin | `api/identity/auth-clients`, `api/identity/auth-scopes` |
| Scopes | Convenção `{product_code}.{feature}` |

Detalhe: **[product-integration.md](product-integration.md)** § AuthClients + consent.

## Integrações não presentes

- Message bus / cache distribuído / APM cloud — não no MVP
- Nested LDAP group expansion / SCIM / SAML
- Marketplace third-party / DCR público

## Contratos HTTP internos (API)

Rotas de gestão: `api/identity/{resource}`. Protocolo OIDC: `/connect/*`.
