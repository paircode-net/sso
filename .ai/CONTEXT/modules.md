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

## Context: Identity (ADR-006 — Fases 0–3)

| Aspecto | Detalhe |
|---------|---------|
| Schema / DbContext | `IdentityDb` / `IdentityDbContext` |
| Auditoria | `IdentityAuditableEntity` |
| Soft-delete | Delete services marcam `IsDeleted` |
| Seed | Org/Product/User/Membership + Branches HQ/Filial + Role/Permission matrix + OpenIddict clients |
| Wiring | `AddIdentityFoundation` + `/connect/*` + Razor login |
| OIDC | grant `switch_context`; JWT com `permissions` efetivas |
| Permissions | `EffectivePermissionsResolver` (Role→Permission; Branch exact match) |

### Aggregates / entidades

| Aggregate | Tabela | Rotas | Notas |
|-----------|--------|-------|-------|
| Organization | `Organizations` | `api/identity/organizations` | Code único |
| Product | `Products` | `api/identity/products` | Code único |
| Membership | `Memberships` | `api/identity/memberships` | User×Org |
| User | AspNetUsers | `api/identity/users` | IdentityUser |
| Branch | `Branches` | `api/identity/branches` | `ParentBranchId` estrutural; code único por org |
| Permission | `Permissions` | `api/identity/permissions` | Code único |
| Role | `AuthRoles` | `api/identity/roles` | Domain role (≠ AspNetRoles) |
| RolePermission | `RolePermissions` | `api/identity/rolepermissions` | Role→Permission |
| UserRoleAssignment | `UserRoleAssignments` | `api/identity/userroleassignments` | User×Role×Org×Branch?×Product |
| ClientProductBinding | `ClientProductBindings` | `api/identity/clientproductbindings` | `client_id` → Product |

### Seed de autorização (dev)

| Contexto | Permissions no JWT |
|----------|-------------------|
| Org (sem branch) | `sso.access` |
| Branch HQ | `sso.access`, `hq.reports` |
| Branch Filial (filho de HQ) | `sso.access`, `filial.ops` — **sem** `hq.reports` |

### Pendente (fases seguintes)

`ClaimDefinition`, `UserClaimAssignment`, AuthClient/Scope como domínio completo, `Session`, `ExternalIdentityProvider`, `AuthAuditEvent`, ProductEnablement.

## Serviços de infraestrutura transversais

| Serviço | Interface | Implementação | Status |
|---------|-----------|---------------|--------|
| Mail | `IMailService` | `MailService` | Stub; DI comentado |
| Permissions efetivas | `IEffectivePermissionsResolver` | `EffectivePermissionsResolver` | Ativo (Fase 3) |

## Shared

`SSO.Shared/Identity/SsoClaimTypes.cs` — claim types, grant `switch_context`, client ids de seed.
