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

| Item | Status |
|------|--------|
| Authentication middleware | Comentado / não configurado |
| Authorization scheme | Não configurado (apenas `UseAuthorization` no pipeline) |
| Identity / JWT / cookies | Não presente |
| Estratégia SSO real | **Pendente de definição** |

## Qualidade / CI

| Item | Status |
|------|--------|
| EditorConfig / StyleCop / analyzers | Não configurados |
| Pipeline CI/CD | Não presente |
| Docker | Não presente |

## A definir

- Estratégia de autenticação/autorização do produto SSO
- Observabilidade (Serilog, App Insights, etc.)
- CI/CD e containers
- Padrão de analyzers / Nullable consistente em todos os projetos
