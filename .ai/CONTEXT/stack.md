# Stack

## Runtime / linguagem

| Item | Valor | Fonte |
|------|--------|-------|
| .NET TFM | `net10.0` | todos os `.csproj` |
| Linguagem | C# | solução |
| Hosting | ASP.NET Core Minimal Hosting (`Program.cs`) | `SSO.Web.Api` |
| Solution | `SSO.sln` | raiz |

## Frameworks e bibliotecas principais

| Área | Tecnologia | Notas |
|------|------------|-------|
| API | ASP.NET Core Controllers | Swagger via Swashbuckle 10.1.1 |
| CQRS / Mediation | MediatR | registrado em `SSO.Middleware` |
| Abstrações | `BAYSOFT.Abstractions` 10.0.3 | Domain |
| API shaping | ModelWrapper | paginação, Post/Select, filtros |
| Validação | FluentValidation (via BAYSOFT) | Entity + Domain validators |
| ORM | EF Core 10.0.x + SQL Server | `SSO.Infrastructures.Data` |
| Testes | MSTest 4.1.0, Moq 4.20.72, coverlet 8.0.0 | `SSO.Tests` |
| Test host | `Microsoft.AspNetCore.TestHost` | Web.Api + Tests |
| Localização | ASP.NET Localization | default runtime `pt-BR` |
| Scaffold | `.forge/project.json` | convenções Forge |

## Persistência

- Provider produção/dev: SQL Server (`ConnectionStrings:DefaultConnection`)
- Dev observado: LocalDB `SSO_DB`
- Testes: EF Core InMemory
- Migrations: EF Core, pasta `Default/Migrations`, auto-apply no startup

## Observabilidade

| Item | Status |
|------|--------|
| `ILogger` / `ILoggerFactory` | Em uso nos handlers |
| Serilog | Não presente |
| Application Insights / APM | Não presente |
| Stack de logs estruturados | **Pendente de definição** |

## AuthN / AuthZ

| Item | Status no código |
|------|------------------|
| Authentication middleware | Habilitado (`UseAuthentication` / `UseAuthorization`) |
| ASP.NET Identity | Presente — `User` / stores em `IdentityDbContext` |
| OpenIddict | 7.5.0 — AspNetCore + EntityFrameworkCore |
| Signing keys (dev) | `AddDevelopmentEncryptionCertificate` / `AddDevelopmentSigningCertificate` |
| Signing keys (prod) | Key Vault + rotação — pendente (D9) |
| JWT com permissions / switch-context | Ativo — motor Role→Permission por Org/Branch/Product (ADR-005, ADR-003, ADR-004) |
| UI login/consent | Login + forgot/reset/confirm/2FA Razor (D6) |

## Qualidade / CI

| Item | Status |
|------|--------|
| EditorConfig / StyleCop / analyzers | Não configurados |
| Pipeline CI/CD | Não presente |
| Docker | Não presente |

## A definir

- Observabilidade (Serilog, App Insights, etc.) — P-002
- CI/CD e containers — P-003
- Padrão de analyzers / Nullable consistente em todos os projetos — P-005
