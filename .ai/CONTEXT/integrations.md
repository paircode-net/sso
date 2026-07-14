# Integrations

## Integrações presentes

| Integração | Tipo | Status |
|------------|------|--------|
| SQL Server | Persistência | Configurada via `DefaultConnection` |
| Swagger / OpenAPI | Documentação de API (dev) | Swashbuckle no host |
| MediatR Notifications | In-process | Handlers apenas logam JSON |
| Mail (`IMailService`) | Serviço externo (pretendido) | Stub; DI comentado |
| Forge | Metadados de scaffold | `.forge/project.json` |

## Integrações não presentes

- Identity Providers externos (Entra ID, Auth0, etc.) — **Pendente de definição**
- Message bus (RabbitMQ, Azure Service Bus, MassTransit) — não encontrado
- HTTP clients para APIs terceiras — não encontrado
- Cache distribuído (Redis) — não encontrado
- Telemetria cloud (App Insights) — não encontrado

## Contratos HTTP internos (API)

Base observada: Controllers REST JSON sob `api/{context}/{resource}`.

Exemplo Sample: `api/default/samples`.

Arquivo `.http` em Web.Api ainda referencia `/weatherforecast/` (legado de template) — não é contrato oficial.

## A definir

- Provedores de identidade e protocolo (OIDC/OAuth2) se este repo for o SSO real
- Canal de e-mail / templates
- Event-driven integrations pós-commit (além de notifications in-process)
- Ambientes e secrets management
