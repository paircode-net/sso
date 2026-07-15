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
| Shared | `SSO.Shared` | Projeto compartilhado (**sem arquivos .cs hoje**) |
| Tests | `SSO.Tests` | Unit + Integration |

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
| Identity | `IdentityDb` / `IdentityDbContext` | **Ativo (Fase 1)** — Identity/OpenIddict + Organization/Product/Membership/User |

Rotas de gestão: `api/identity/{resource}`. Protocolo OIDC: `/connect/*` (configurado; controllers de token/authorize na Fase 2).

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

**Estado no código (Fase 0):**

| Tema | Status |
|------|--------|
| `UseAuthentication` / `UseAuthorization` | Habilitados em `UseMiddleware` (prod) e `TestStartup` (testes) |
| ASP.NET Identity | `User` + `IdentityRole<Guid>` + EF stores |
| OpenIddict | Core + Server (dev certs) + Validation local |
| Flows habilitados (config) | Authorization Code+PKCE, Refresh, Client Credentials |
| Controllers `/connect/*` | Ainda não — passthrough preparado para Fase 2 |
| switch-context / permissions no JWT | Fase 2+ (ADRs 003–005) |
| UI login/consent Razor | Fase 2 (D6) |

Detalhe: feature plan `.ai/WORK/2026-07-14-00001-plataforma-sso.md`.

## A definir

- Papel futuro de `SSO.Shared`
- Observabilidade e CI/CD (P-002, P-003)
- Estratégia de migrations em produção (P-004)
