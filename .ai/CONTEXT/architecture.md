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
| Default | `DefaultDb` / `DefaultDbContext` | Ativo (única implementação) |

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

Em `UseMiddleware`: `UseAuthentication` / `UseAuthorization` estão **comentados**.

Em `Program.cs`: `app.UseAuthorization()` é chamado, mas sem schemes de autenticação configurados.

## A definir

- Evolução do produto SSO além do scaffold Sample
- Papel futuro de `SSO.Shared`
- Estratégia de multi-contexto (novos schemas) e boundaries oficiais
- AuthN/AuthZ e padrões de segurança de API
