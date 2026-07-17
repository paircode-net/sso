# Architecture

## Visão geral

Arquitetura **Clean Architecture + CQRS Full** em .NET 10, scaffoldada com convenções **BAYSOFT.Abstractions** e metadados **Forge** (`.forge/project.json`).

O README da raiz descreve: `Architecture CQRS Full (.NET 10.0)`.

## Camadas / projetos

| Camada (solution folder) | Projeto | Responsabilidade |
|--------------------------|---------|------------------|
| Presentations | `SSO.Web.Api` | HTTP, Controllers, Swagger, host |
| Middleware | `SSO.Middleware` | Composition root: DI, localization, EF wiring, MediatR, ModelWrapper |
| Core / Application | `SSO.Core.Application` | Commands, Queries, Notifications (casos de uso) |
| Core / Domain | `SSO.Core.Domain` | Entities, Domain Services, Specs, Validations, interfaces de infra |
| Infrastructures / Data | `SSO.Infrastructures.Data` | DbContext, mappings, migrations, Reader/Writer |
| Infrastructures / Services | `SSO.Infrastructures.Services` | Serviços externos (hoje stub de mail) |
| Shared | `SSO.Shared` | Claims/grants/client constants Identity |
| Client SDK | `SSO.Client` | JwtBearer, RequirePermission, token helpers (produtos) |
| Tests | `SSO.Tests` / `SSO.Client.Tests` | Unit + Integration |
| Samples | `samples/product-api`, `samples/sso-bff` | Aceite SDK + BFF (feature 00004) |
| JS client | `clients/js` (`@sso/client`) | Claim helpers SPA (sem verificar JWT) |

## Fluxo de uma request de escrita

```text
HTTP Controller
  → MediatR Command Handler (Application)
      → request.IsValid / ModelWrapper.Post|Put|Patch
      → MediatR Domain Service (Domain)
          → Entity validations + Specifications
          → Writer.Add/Update/Remove
      → Writer.CommitAsync
      → MediatR Notification (side effects / logging)
  → ApplicationResponse (HTTP status + wrap)
```

## Fluxo de leitura

```text
HTTP Controller
  → MediatR Query Handler
      → Reader + ModelWrapper FullSearch / GetById
  → ApplicationResponse
```

## Direção de dependências

```text
Web.Api → Middleware, Shared
Middleware → Application, Domain, Infrastructures.Data, Infrastructures.Services
Application → Domain, Shared
Infrastructures.* → Domain
Domain → Shared, BAYSOFT.Abstractions
Tests → Application, Domain, Data, Middleware, Web.Api
```

## Bounded contexts

| Context | Schema / DbContext | Status |
|---------|--------------------|--------|
| Default | `DefaultDb` / `DefaultDbContext` | Ativo (Sample) |
| Identity | `IdentityDb` / `IdentityDbContext` | **Ativo (Fase 6)** — JWT authz + IdPs externos + hardening |

Rotas de gestão: `api/identity/{resource}`. Protocolo OIDC: `/connect/*` (authorize, token, userinfo, logout, revoke; grant `switch_context`).

## Composition root

Arquivo central: `src/SSO.Middleware/Configurations.cs`

- `AddMiddleware` / `UseMiddleware` — host real
- `AddMiddlewareTest` / `UseMiddlewareTest` — host de testes
- `AddIdentityFoundation` — Identity + OpenIddict (core/server/validation)
- Registra MediatR a partir de assemblies Application, Domain, Infrastructures.Services
- Aplica migrations relacionais via `UseMigrations()` (Default + Identity)

## Persistência

- Não há Repository/UoW nomeados além de Reader/Writer BAYSOFT.
- `IDefaultDbContextWriter.CommitAsync` / `IIdentityDbContextWriter` delimitam UoW por context.
- Identity: connection `IdentityConnection` (fallback `DefaultConnection`), schema `IdentityDb`.

## Autenticação na pipeline

**Estado no código (Fase 2):**

| Tema | Status |
|------|--------|
| `UseAuthentication` / `UseAuthorization` | Habilitados em `UseMiddleware` (prod) e `TestStartup` (testes) |
| ASP.NET Identity | `User` + `IdentityRole<Guid>` + EF stores |
| OpenIddict | Core + Server (dev certs) + Validation local |
| Flows | Authorization Code+PKCE, Refresh, Client Credentials, `switch_context` |
| Controllers `/connect/*` | `AuthorizationController` + revoke |
| Claims JWT | `organization_id` / `branch_id` / `permissions` / `perm_ver` via `TokenClaimsFactory` |
| Permissions resolver | Ativo — Role→Permission + UserRoleAssignment; herança Branch **opt-in** (ADR-008) |
| Conta / e-mail / 2FA | Ativo — Razor + `RequireConfirmedAccount` + TOTP |
| Sessões | Revogação em massa via OpenIddict tokens (`IUserSessionService`) |
| Auditoria AuthN/AuthZ | `AuthAuditEvents` + `GET /api/identity/auth-audit-events` |
| Mail | `IMailService` (logger); spy `CapturingMailService` em testes |
| Menus | `MenuItem` + `GET /api/identity/menus/effective` |
| `perm_ver` | Dinâmico (policy stamps) via `IPermissionPolicyVersionProvider` |
| Product contract | `.ai/CONTEXT/product-integration.md` |
| UI login Razor | `/Account/Login` + `/Account/ExternalLogin` |
| IdPs externos | `ExternalIdentityProviders` + OIDC Entra/Google; LDAP stub |
| Hardening | CORS, rate limit, lockout, signing certs (`Sso:*`) |
| Migrations prod (P-004) | AutoMigrate **off** em Production por default |

Detalhe: feature plan `.ai/WORK/2026-07-14-00001-plataforma-sso.md`; `phase6-hardening.md`.

## A definir

- Papel futuro de `SSO.Shared`
- Observabilidade e CI/CD (P-002, P-003) — **implementado** ([00010](../WORK/2026-07-16-00010-observabilidade-cicd.md), [cicd.md](cicd.md))
- Backlog pós-MVP: [2026-07-16-backlog-pos-mvp.md](../WORK/2026-07-16-backlog-pos-mvp.md)

AuthZ admin: entregue (00002) — ver [admin-api-authz.md](admin-api-authz.md).
