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
| Identity | `IdentityDb` / `IdentityDbContext` | **Planejado** (ADR-006) — ainda não no código |

Rotas de gestão Identity (alvo): `api/identity/{resource}`. Protocolo OIDC: `/connect/*` (OpenIddict).

## Composition root

Arquivo central: `src/SSO.Middleware/Configurations.cs`

- `AddMiddleware` / `UseMiddleware` — host real
- `AddMiddlewareTest` / `UseMiddlewareTest` — host de testes
- Registra MediatR a partir de assemblies Application, Domain, Infrastructures.Services
- Aplica migrations relacionais via `UseMigrations()`

## Persistência

- Não há Repository/UoW nomeados além de Reader/Writer BAYSOFT.
- `IDefaultDbContextWriter.CommitAsync` delimita a unidade de trabalho do caso de uso.

## Autenticação na pipeline

**Estado no código hoje:** em `UseMiddleware`, `UseAuthentication` / `UseAuthorization` estão **comentados**. Em `Program.cs`, `app.UseAuthorization()` é chamado sem schemes configurados.

**Alvo decidido (ainda não implementado):**

| Tema | Decisão |
|------|---------|
| Authorization Server | OpenIddict (ADR-001) |
| AuthN / conta | ASP.NET Identity (ADR-002) |
| Contexto tenant/branch | switch-context + claims no token (ADR-003) |
| AuthZ | Contextual; sem herança Branch MVP (ADR-004) |
| Permissions no token | Todas as efetivas do contexto no JWT (ADR-005) |
| UI login/consent | Razor em `SSO.Web.Api` (F00001-D6) |

Detalhe: feature plan `.ai/WORK/2026-07-14-00001-plataforma-sso.md`.

## A definir

- Papel futuro de `SSO.Shared`
- Observabilidade e CI/CD (P-002, P-003)
- Estratégia de migrations em produção (P-004)
