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

- Provider produção/dev: SQL Server (`DefaultConnection` + `IdentityConnection`)
- Dev observado: LocalDB `SSO_DB`
- Testes: EF Core InMemory
- Migrations IdentityDb: pasta `Identity/Migrations`
- P-004: Production **não** auto-migra por default (`Sso:Database:AutoMigrate`)

## Observabilidade

| Item | Status |
|------|--------|
| `ILogger` / Serilog | Serilog.AspNetCore + enrichers (Environment, Machine, RequestId) |
| OpenTelemetry | Metrics + traces; exporter plugável (`Console` / `Otlp` / `AzureMonitor`) — F00010-D1 |
| Health | `/health/live`, `/health/ready` (Identity DB + signing) |
| Redaction | Sem Authorization/senhas/tokens nos logs (`LogRedaction`) |
| Métricas Auth | meter `SSO.Auth` (`sso.auth.*`, `sso.jwt.*`) |
| Runbook | `.ai/PLAYBOOK/observability-runbook.md` |

## AuthN / AuthZ

| Item | Status no código |
|------|------------------|
| Authentication middleware | Habilitado (`UseAuthentication` / `UseAuthorization`) |
| ASP.NET Identity | Presente — `User` / stores em `IdentityDbContext` |
| OpenIddict | 7.5.0 — AspNetCore + EntityFrameworkCore |
| Signing keys (dev) | `AddDevelopmentEncryptionCertificate` / `AddDevelopmentSigningCertificate` |
| Signing keys (prod) | Cert path / Key Vault (ops D9); exigido quando não usar DevelopmentCertificates |
| JWT com permissions / switch-context | Ativo — motor Role→Permission + `perm_ver` dinâmico |
| Claims tipadas | `sso_c_{code}` + `claim_ver` (00008); User override Role |
| Herança Branch | Opt-in Org + `Inheritable` (00009 / ADR-008); default Off |
| UI login/consent | Login + ExternalLogin + Consent + forgot/reset/confirm/2FA Razor (D6 / 00007) |
| AuthClients admin | `api/identity/auth-clients` + `AuthClientMetadata` (00007) |
| Menus por permission | `MenuItem` + API effective; contrato em `product-integration.md` |
| IdPs externos | Entra OIDC; Google OIDC; LDAP/AD (SDS.Protocols) — feature 00006 |
| Hardening | CORS, rate limit, lockout (`Sso:*`); ver `phase6-hardening.md` |

## Qualidade / CI

| Item | Status |
|------|--------|
| EditorConfig / StyleCop / analyzers | Não configurados |
| Pipeline CI/CD | GitHub Actions + scripts agnósticos `scripts/ci/*` (F00010-D2) — ver `cicd.md` |
| Docker | `Dockerfile` raiz (artefato oficial host-agnóstico, F00010-D3) |
| Signing prod | Key Vault certificate (`Sso:Signing:KeyVaultUri` + `KeyVaultCertificateName`, F00010-D5) |

## A definir

- Padrão de analyzers / Nullable consistente em todos os projetos — P-005
- Host concreto de deploy (Container Apps vs K8s vs App Service for Containers) — imagem já oficial
