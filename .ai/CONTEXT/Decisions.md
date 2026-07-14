# Decisions

Registro leve de decisões observáveis ou pendentes. Para decisões novas formais, usar também `.ai/TEMPLATES/adr.md` / `decision.md`.

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
| D-009 | AuthN/AuthZ não habilitados no scaffold | hooks comentados | Temporária / **Pendente revisão** |
| D-010 | Mail service stub, DI desligado | `MailService` + AddDomainServices | Temporária |

## Decisões pendentes (produto / plataforma)

| ID | Tema | Status |
|----|------|--------|
| P-001 | Modelo de autenticação/autorização do SSO | Pendente de definição |
| P-002 | Stack de observabilidade | Pendente de definição |
| P-003 | CI/CD e estratégia de deploy | Pendente de definição |
| P-004 | Estratégia de migrations em produção (auto vs pipeline) | Pendente de definição |
| P-005 | Analyzers / EditorConfig / Nullable uniforme | Pendente de definição |
| P-006 | Domínio de negócio real além de Sample | Pendente de definição |
| P-007 | Papel e conteúdo de `SSO.Shared` | Pendente de definição |
| P-008 | Tratamento de auditoria (Forge auditable vs código) | Pendente de definição |

## Como evoluir este arquivo

- Registrar decisões novas no topo da tabela ou via ADR linkado.
- Marcar decisões temporárias quando forem propositalmente incompletas.
- Não registrar preferências pessoais sem impacto arquitetural.
