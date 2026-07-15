# Modules

## Context: Default

Único bounded context implementado.

### Aggregate: Sample

| Aspecto | Detalhe |
|---------|---------|
| Entidade | `Sample` (`DomainEntity<Guid>`), propriedade `Description` (string, required, max 128) |
| Tabela | `Samples` (schema `DefaultDb`) |
| Aggregate root | Sim (Forge) |
| Auditable (Forge) | Marcado `true` no `.forge/project.json`, **sem propriedades de auditoria no código/mapping** |

#### Domain

| Tipo | Arquivos |
|------|----------|
| Entity | `Default/Samples/Entity/Sample.cs` |
| Services | `CreateSampleService`, `UpdateSampleService`, `DeleteSampleService` |
| Specification | `SampleDescriptionAlreadyExistsSpecification` |
| Entity validation | `SampleValidator` |
| Domain validations | `Create|Update|DeleteSampleSpecificationsValidator` |
| Resources | `EntitySample` resx (+ designer) |

#### Application

| Tipo | Operações |
|------|-----------|
| Commands | Post, Put, Patch, Delete |
| Queries | `GetSampleById`, `GetSamplesByFilter` |
| Notifications | Post, Put, Patch, Delete (handlers logam payload JSON) |

#### Infrastructure Data

| Tipo | Arquivo |
|------|---------|
| Context | `DefaultDbContext` |
| Reader/Writer | `DefaultDbContextReader`, `DefaultDbContextWriter` |
| Mapping | `SampleMap` |
| Migrations | `InitialDDLMigrationDefaultDbContext`, `InitialDMLMigrationDefaultDbContext` (+ snapshot) |

#### API

| Controller | Rota | Verbos |
|------------|------|--------|
| `SamplesController` | `api/default/samples` | GET, GET `{id}`, POST, PUT `{id}`, PATCH `{id}`, DELETE `{id}` |

Namespace do controller: `SSO.Web.Api.Default`  
Path: `SSO.Web.Api/Resources/DefaultDb/SamplesController.cs`

#### Tests

- Unit: Application (commands/queries/notifications), Domain services, DbContext reader/writer
- Integration: scenarios CRUD + filter em `IntegrationTests/Samples/`

## Serviços de infraestrutura transversais

| Serviço | Interface | Implementação | Status |
|---------|-----------|---------------|--------|
| Mail | `IMailService` | `MailService` | Stub vazio; registro DI **comentado** |

## Shared

`SSO.Shared` existe na solução, **sem código-fonte `.cs`**.

## TODOs observados no módulo Sample

- `TODO: SUPPRESSED RESPONSE PROPERTIES` (commands/queries)
- `TODO: COMMAND RULES` (`PostSampleCommand`)
- `TODO: ByFilterQuery RULES` (`GetSamplesByFilterQuery`)

## Context: Identity (planejado — ADR-006)

**Ainda não implementado.** Aggregates previstos no plano feature 00001:

`Organization`, `Branch`, `Product`, `User`, `Membership`, `Role`, `ClaimDefinition`, `Permission`, `RolePermission`, `UserRoleAssignment`, `UserClaimAssignment`, `ProductEnablement`, `AuthClient`, `Scope`, `Session`, `ExternalIdentityProvider`, `AuthAuditEvent` (+ stores OpenIddict/Identity).

Rotas alvo: `api/identity/{resource}`. Detalhe: `.ai/WORK/2026-07-14-00001-plataforma-sso.md`.
