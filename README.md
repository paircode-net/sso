# SSO

Plataforma centralizada de **Single Sign-On** para o ecossistema de produtos: identidade única, autenticação OAuth 2.1 / OpenID Connect e autorização contextual (organização, filial e produto).

O Authorization Server usa **OpenIddict** + **ASP.NET Identity**, com APIs de gestão em Clean Architecture (.NET 10 / CQRS). Documentação de domínio e decisões: [`.ai/CONTEXT/`](.ai/CONTEXT/) · plano do épico: [`.ai/WORK/2026-07-14-00001-plataforma-sso.md`](.ai/WORK/2026-07-14-00001-plataforma-sso.md) · backlog pós-MVP: [`.ai/WORK/2026-07-16-backlog-pos-mvp.md`](.ai/WORK/2026-07-16-backlog-pos-mvp.md).

## Funcionalidades

| Área | Capacidade |
|------|------------|
| Autenticação | Login local (Razor), refresh/revoke, grant `switch_context`, JWT com claims de contexto |
| Conta | Confirmação de e-mail, forgot/reset de senha, 2FA TOTP, revogação de sessões |
| Autorização | Roles → Permissions dinâmicas; permissões efetivas no JWT (`permissions`, `perm_ver`) |
| Multi-tenant | Organizations (tenants), Branches (sem herança authz pai→filho no MVP), Products |
| Multi-produto | Bindings client → product; menus efetivos por permission |
| IdPs externos | Microsoft Entra ID (OIDC, homologável); Google OIDC e LDAP (stubs) |
| Hardening | CORS allowlist, rate limiting, lockout, signing Dev/Prod, migrations controladas em Production |
| Admin MVP | Portal Razor `/Admin` + API `api/identity/*` (`sso.admin.*`) — [admin-api-authz.md](.ai/CONTEXT/admin-api-authz.md) |
| SDK produtos | `SSO.Client` + `@sso/client` + samples BFF/API — [product-integration.md](.ai/CONTEXT/product-integration.md) |
| Auditoria | Eventos AuthN/AuthZ (`AuthAuditEvents`) |

Contrato para produtos consumidores: [`.ai/CONTEXT/product-integration.md`](.ai/CONTEXT/product-integration.md) · IdPs e hardening: [`.ai/CONTEXT/phase6-hardening.md`](.ai/CONTEXT/phase6-hardening.md).

## Stack

| Camada | Tecnologia |
|--------|------------|
| Runtime | .NET 10 (`net10.0`), C#, ASP.NET Core |
| AuthN / AS | ASP.NET Identity, OpenIddict 7.x (OIDC / OAuth 2.1 / JWT) |
| Arquitetura | Clean Architecture + CQRS (MediatR, BAYSOFT.Abstractions / Forge) |
| Persistência | EF Core + SQL Server (`DefaultDbContext`, `IdentityDbContext`) |
| API | Controllers, ModelWrapper, FluentValidation, Swagger (Development) |
| UI de conta | Razor Pages (`/Account/*`) |
| Testes | MSTest, Moq, TestHost, EF InMemory |
| Localização | `pt-BR` (padrão) |

## Solution

```text
SSO.Web.Api                 Host HTTP + Razor + /connect/*
SSO.Middleware              DI, Identity, OpenIddict, hardening
SSO.Core.Domain             Domínio Identity (+ Sample scaffold)
SSO.Core.Application        Handlers CQRS
SSO.Infrastructures.Data    EF Core / migrations
SSO.Infrastructures.Services Mail, resolvers, adapters
SSO.Shared                  Claims, options, constants
SSO.Client                  SDK produtos (JwtBearer, RequirePermission, token client)
SSO.Client.Tests            Unit do SDK
SSO.Tests                   Unit + integration
samples/product-api         Aceite SSO.Client
samples/sso-bff             BFF referência (session + refresh + switch_context)
clients/js                  Pacote @sso/client
```

## Desenvolvimento rápido

```bash
dotnet restore
dotnet build
dotnet run --project src/SSO.Web.Api
dotnet test
```

Seed de desenvolvimento: `admin@sso.local` / `ChangeMe!123`. Endpoints OIDC em `/connect/*`; Swagger apenas em Development.

### Migrations (EF)

```bash
dotnet tool install --global dotnet-ef   # se necessário
dotnet tool update --global dotnet-ef

cd src/SSO.Infrastructures.Data

# DefaultDb
dotnet ef --startup-project ../SSO.Web.Api migrations add [Nome]DefaultDbContext -c DefaultDbContext -o Default/Migrations

# IdentityDb
dotnet ef --startup-project ../SSO.Web.Api migrations add [Nome] --context IdentityDbContext -o Identity/Migrations
```

Em **Production**, auto-migrate fica desligado por padrão (`Sso:Database:AutoMigrate`); aplique schema via pipeline (`dotnet ef database update`).
