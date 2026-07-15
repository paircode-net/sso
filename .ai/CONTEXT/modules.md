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

## Context: Identity (ADR-006 — Fases 0–2)

| Aspecto | Detalhe |
|---------|---------|
| Schema / DbContext | `IdentityDb` / `IdentityDbContext` |
| Auditoria | `IdentityAuditableEntity` (`CreatedAt`, `UpdatedAt`, `DeletedAt`, `IsDeleted`) |
| Soft-delete | Delete services marcam `IsDeleted` (não removem fisicamente) |
| Seed | `IdentitySeed` (org/product/user/membership + OpenIddict clients/scopes) |
| Wiring | `AddIdentityFoundation` + `/connect/*` + Razor login |
| OIDC | `AuthorizationController`; grant `urn:sso:params:oauth:grant-type:switch_context` |
| Permissions | `IEffectivePermissionsResolver` stub → `sso.access` |

### Aggregates implementados

| Aggregate | Tabela | Rotas | Notas |
|-----------|--------|-------|-------|
| Organization | `Organizations` | `api/identity/organizations` | Code único (ativo) |
| Product | `Products` | `api/identity/products` | Code único (ativo) |
| Membership | `Memberships` | `api/identity/memberships` | UserId + OrganizationId únicos (ativo) |
| User | AspNetUsers (+ audit cols) | `api/identity/users` POST/GET `{id}` | `IdentityUser<Guid>` + `IDomainEntityBase`; senha via `UserManager` |

### Auth clients (OpenIddict seed — não são aggregate Domain ainda)

| ClientId | Tipo | Uso |
|----------|------|-----|
| `dev-product-spa` | public / PKCE | SPA de desenvolvimento |
| `dev-product-service` | confidential | client_credentials |

### Pendente (fases seguintes)

`Branch`, `Role`, `ClaimDefinition`, `Permission`, AuthClient/Scope como domínio, `Session`, `ExternalIdentityProvider`, `AuthAuditEvent`, ProductEnablement, assignments, motor efetivo real.

## Serviços de infraestrutura transversais

| Serviço | Interface | Implementação | Status |
|---------|-----------|---------------|--------|
| Mail | `IMailService` | `MailService` | Stub; DI comentado |
| Permissions efetivas | `IEffectivePermissionsResolver` | `EffectivePermissionsResolver` | Stub membership → `sso.access` |

## Shared

`SSO.Shared/Identity/SsoClaimTypes.cs` — claim types, grant `switch_context`, client ids de seed.
