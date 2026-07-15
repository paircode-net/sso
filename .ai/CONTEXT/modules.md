# Modules

## Context: Default

Único bounded context de scaffold Sample.

### Aggregate: Sample

| Aspecto | Detalhe |
|---------|---------|
| Entidade | `Sample` (`DomainEntity<Guid>`), propriedade `Description` (string, required, max 128) |
| Tabela | `Samples` (schema `DefaultDb`) |
| Aggregate root | Sim (Forge) |

#### API

| Controller | Rota | Verbos |
|------------|------|--------|
| `SamplesController` | `api/default/samples` | GET, GET `{id}`, POST, PUT, PATCH, DELETE |

## Context: Identity (ADR-006 — Fase 0 + Fase 1)

| Aspecto | Detalhe |
|---------|---------|
| Schema / DbContext | `IdentityDb` / `IdentityDbContext` |
| Auditoria | `IdentityAuditableEntity` (`CreatedAt`, `UpdatedAt`, `DeletedAt`, `IsDeleted`) |
| Soft-delete | Delete services marcam `IsDeleted` (não removem fisicamente) |
| Seed | `IdentitySeed` (org/product/user/membership de desenvolvimento) |
| Wiring | `AddIdentityFoundation` + specs/validators em Middleware |

### Aggregates implementados

| Aggregate | Tabela | Rotas | Notas |
|-----------|--------|-------|-------|
| Organization | `Organizations` | `api/identity/organizations` | Code único (ativo) |
| Product | `Products` | `api/identity/products` | Code único (ativo) |
| Membership | `Memberships` | `api/identity/memberships` | UserId + OrganizationId únicos (ativo) |
| User | AspNetUsers (+ audit cols) | `api/identity/users` POST/GET `{id}` | `IdentityUser<Guid>` + `IDomainEntityBase`; senha via `UserManager` |

### Pendente (fases seguintes)

`Branch`, `Role`, `ClaimDefinition`, `Permission`, `AuthClient`, `Scope`, `Session`, `ExternalIdentityProvider`, `AuthAuditEvent`, ProductEnablement, assignments.

## Serviços de infraestrutura transversais

| Serviço | Interface | Implementação | Status |
|---------|-----------|---------------|--------|
| Mail | `IMailService` | `MailService` | Stub; DI comentado |

## Shared

`SSO.Shared` existe na solução, **sem código-fonte `.cs`**.
