# Decisions

Registro leve de decisões observáveis ou pendentes. Para decisões novas formais, usar também `.ai/TEMPLATES/adr.md` / `decision.md` e ADRs em `.ai/CONTEXT/adr/`.

## ADRs Aceitos (feature 00001)

| ADR | Título | Arquivo |
|-----|--------|---------|
| ADR-001 | OpenIddict como Authorization Server | [adr/ADR-001-openiddict-authorization-server.md](adr/ADR-001-openiddict-authorization-server.md) |
| ADR-002 | ASP.NET Identity para AuthN | [adr/ADR-002-aspnet-identity-authn.md](adr/ADR-002-aspnet-identity-authn.md) |
| ADR-003 | Multi-tenant + switch-context | [adr/ADR-003-multi-tenant-switch-context.md](adr/ADR-003-multi-tenant-switch-context.md) |
| ADR-004 | AuthZ contextual sem herança Branch no MVP | [adr/ADR-004-contextual-authz-no-branch-inheritance.md](adr/ADR-004-contextual-authz-no-branch-inheritance.md) |
| ADR-005 | Permissões efetivas no JWT | [adr/ADR-005-permissions-in-jwt.md](adr/ADR-005-permissions-in-jwt.md) |
| ADR-006 | Bounded context Identity / IdentityDb | [adr/ADR-006-identity-bounded-context.md](adr/ADR-006-identity-bounded-context.md) |

Plano: `.ai/WORK/2026-07-14-00001-plataforma-sso.md`.

## Decisões feature 00001 (D1–D12)

| ID | Decisão | Status |
|----|---------|--------|
| F00001-D1 | Context `Identity` / `IdentityDb` separado do Sample | Aceito (ADR-006) |
| F00001-D2 | Switch-context + claims `organization_id`/`branch_id` (não header como fonte de verdade) | Aceito (ADR-003) |
| F00001-D3 | Permissões efetivas embutidas no JWT (TTL curto + reemissão no switch-context) | Aceito (ADR-005 revisado) |
| F00001-D4 | Sem herança automática Branch pai→filho no MVP | Aceito (ADR-004) |
| F00001-D5 | Identity para AuthN; roles/claims de produto no domínio SSO | Aceito (ADR-002, ADR-004) |
| F00001-D6 | Login/consent Razor em `SSO.Web.Api` no MVP | Aceito |
| F00001-D7 | IdPs: Entra ID → Google → LDAP | Aceito |
| F00001-D8 | Manter Sample até Fase 2 estável | Aceito |
| F00001-D9 | Dev: key/cert local; Prod: Key Vault + rotação | Aceito (ADR-001) |
| F00001-D10 | `Product` ≠ `AuthClient` | Aceito |
| F00001-D11 | Admin MVP = API-only | Aceito |
| F00001-D12 | Soft-delete + auditoria nas entidades Identity | Aceito (P-008 parcial) |

## Decisões observadas no código (implícitas)

| ID | Decisão | Evidência | Status |
|----|---------|-----------|--------|
| D-001 | Usar Clean Architecture + CQRS Full com BAYSOFT.Abstractions | Estrutura de soluções + handlers | Ativa |
| D-002 | Id de entidades: Guid | Forge + DomainEntity\<Guid\> | Ativa |
| D-003 | Classes preferencialmente sealed | Forge `UseSealedClasses` + código | Ativa |
| D-004 | SQL Server + EF Core como persistência | csproj Data + connection string | Ativa |
| D-005 | Composition root no projeto Middleware | `Configurations.cs` | Ativa |
| D-006 | Localização default pt-BR na API | `UseMiddleware` | Ativa |
| D-007 | Migrations aplicadas no startup | `UseMigrations()` | Ativa (reavaliar p/ produção) |
| D-008 | Testes com MSTest + InMemory + TestServer | `SSO.Tests` | Ativa |
| D-009 | AuthN/AuthZ habilitados (Fase 0) | `AddIdentityFoundation` + pipeline Auth | Ativa — supersede scaffold comentado |
| D-010 | Mail service stub, DI desligado | `MailService` + AddDomainServices | Temporária |

## Decisões pendentes (produto / plataforma)

| ID | Tema | Status |
|----|------|--------|
| P-001 | Modelo de autenticação/autorização do SSO | **Definido** (ADRs 001–005); implementação pendente (Fases 0+) |
| P-002 | Stack de observabilidade | Pendente de definição |
| P-003 | CI/CD e estratégia de deploy | Pendente de definição |
| P-004 | Estratégia de migrations em produção (auto vs pipeline) | Pendente de definição |
| P-005 | Analyzers / EditorConfig / Nullable uniforme | Pendente de definição |
| P-006 | Domínio de negócio real além de Sample | **Definido (planejamento)** — Identity context + aggregates 00001; código pendente |
| P-007 | Papel e conteúdo de `SSO.Shared` | Pendente de definição |
| P-008 | Tratamento de auditoria (Forge auditable vs código) | **Parcial** — Identity: soft-delete + auditoria (F00001-D12); restante a definir |

## Como evoluir este arquivo

- Registrar decisões novas no topo da tabela ou via ADR linkado.
- Marcar decisões temporárias quando forem propositalmente incompletas.
- Não registrar preferências pessoais sem impacto arquitetural.
