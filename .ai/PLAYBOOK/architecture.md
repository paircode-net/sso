# Playbook — Architecture

## Propósito

Preservar Clean Architecture + CQRS Full do repositório.

## Princípios

- Domain no centro; Infrastructure implementa interfaces do Domain.
- Application orquestra casos de uso via MediatR.
- Middleware é composition root (DI), não camada de regras.
- Presentation (Web.Api) traduz HTTP ↔ Application.

## Boas práticas

- Um bounded context por pasta de schema (ex.: `Default`).
- Aggregate por entidade raiz com Entity, Services, Specifications, Validations.
- Separar Commands, Queries e Notifications na Application.
- Side effects pós-commit via Notifications (`INotificationHandler`).
- Migrations EF no projeto de Data; aplicadas no startup (`UseMigrations`).

## Padrões obrigatórios

### Command vs Domain Service (escrita)

Fluxo obrigatório de escrita:

```text
Command Handler (Application)
  → IsValid / ModelWrapper / existência (opcional)
  → Domain Service (MediatR *ServiceRequest)
      → atribuições de valor de domínio
      → ValidateEntity + ValidateDomain (Specs)
      → Writer.Add / mutação
  → Writer.CommitAsync
  → Notification
```

| Camada | Responsável por | Exemplos |
|--------|-----------------|----------|
| **Application Command** | Orquestração | `IsValid`, `EntityNotFound`, claims→campo de auditoria de quem chama, `CommitAsync`, e-mail/link HTTP |
| **Domain Service** | Atribuições + persistência do aggregate | `Status`, `TokenHash`, `ExpiresAt`, normalizar e-mail, criar membership no accept |
| **Specification + DomainValidations** | Regras de negócio | não-pending, expirado, e-mail mismatch, unicidade |
| **EntityValidations** | Forma do dado | required, max length, e-mail format |

**Nunca** colocar no Command: `if` de regra de domínio, atribuição de estado/hashes/datas de negócio, ou criação de aggregates relacionados por regra. Se a regra existe, crie/atualize Specification e registre-a no `*SpecificationsValidator` da operação.

Referência viva: `OrganizationInvites` (Create/Accept/Decline/Cancel Services + Specs).

### Direção de dependências

```text
SSO.Web.Api → SSO.Middleware, SSO.Shared
SSO.Middleware → Application, Domain, Infrastructures.*
SSO.Core.Application → Domain, Shared
SSO.Infrastructures.* → Domain
SSO.Core.Domain → Shared (+ BAYSOFT.Abstractions)
```

- Domain **não** referencia Infrastructure.
- Novos DbContexts seguem `{Name}DbContext` + schema `{Name}Db` + Reader/Writer.
- Rotas de API: `api/{context}/{resource}` (ex.: `api/default/samples`).
- Controllers herdam `ResourceController` e ficam em `Resources/{Context}Db/`.

### Scaffold / Forge

- Metadados em `.forge/project.json`.
- Convenções: Id `Guid`, tabelas pluralizadas, classes `sealed`.

## Exemplos

```text
Novo aggregate "Client" no context Default:
  Domain/Default/Clients/{Entity,Services,Specifications,Validations}
  Application/Default/Clients/{Commands,Queries,Notifications}
  Infrastructures.Data/Default/EntityMappings/ClientMap.cs
  Web.Api/Resources/DefaultDb/ClientsController.cs
```

## Anti-patterns

- Colocar regra de negócio em Middleware ou Controller.
- **Command Handler com regra/atribuição de domínio** (status, expiração, hash, normalize, match de e-mail) em vez de Domain Service + Specification.
- Application usando `DefaultDbContext` concreto.
- Introduzir Repository/UoW paralelo sem necessidade — Writer.CommitAsync já delimita a unidade de trabalho.
- Mudar arquitetura (event bus, microserviço, etc.) sem ADR/decisão registrada.
