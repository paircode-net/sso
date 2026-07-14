# Glossary

| Termo | Significado neste repositório |
|-------|------------------------------|
| **CQRS Full** | Separação de Commands, Queries e Notifications na Application, com Domain Services via MediatR |
| **BAYSOFT.Abstractions** | Pacote NuGet com bases (`DomainEntity`, `ApplicationRequestHandler`, Reader/Writer, etc.) |
| **Forge** | Metadados/codegen em `.forge/project.json` para entidades e convenções |
| **Context / Default** | Bounded context atual; schema EF `DefaultDb` |
| **Sample** | Aggregate de exemplo (não confundir com domínio SSO de produto) |
| **Application Command** | Caso de uso de escrita orquestrado na Application |
| **Application Query** | Caso de uso de leitura |
| **Notification** | Evento in-process pós-operação (`INotification`) |
| **Domain Service** | Caso de uso de domínio via MediatR (`*ServiceRequest`) |
| **Specification** | Regra de domínio consultável (ex.: unicidade) |
| **Reader / Writer** | Abstrações de persistência read/write sobre o DbContext |
| **ModelWrapper** | Biblioteca de wrap de request/response, filtros e tamanhos de coleção |
| **Middleware (projeto)** | Composition root de DI — não necessariamente middleware HTTP custom |
| **ResourceController** | Controller base da API que expõe `Send` para MediatR |
| **Scenarios** | Convenção de classes de teste MSTest |
| **SSO** | Nome do repositório/produto; capacidades reais de SSO **A definir** |

## A definir

Glossário oficial de negócio SSO (tenant, client, realm, grant, etc.) quando o domínio for especificado.
