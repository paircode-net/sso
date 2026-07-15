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

## Context: Identity (ADR-006 — Fases 0–4)

| Aspecto | Detalhe |
|---------|---------|
| Schema / DbContext | `IdentityDb` / `IdentityDbContext` |
| Conta | Confirm e-mail, forgot/reset, 2FA TOTP (Razor) |
| Auditoria | `AuthAuditEvents` |
| Sessões | Revoke all tokens por subject (`POST .../sessions/{userId}/revoke`) |
| Mail | `IMailService` / logger; capture nos testes |

### Aggregates / entidades

| Aggregate | Tabela | Rotas | Notas |
|-----------|--------|-------|-------|
| Organization | `Organizations` | `api/identity/organizations` | Code único |
| Product | `Products` | `api/identity/products` | Code único |
| Membership | `Memberships` | `api/identity/memberships` | User×Org |
| User | AspNetUsers | `api/identity/users` | IdentityUser |
| Branch | `Branches` | `api/identity/branches` | `ParentBranchId` estrutural |
| Permission | `Permissions` | `api/identity/permissions` | Code único |
| Role | `AuthRoles` | `api/identity/roles` | Domain role |
| RolePermission | `RolePermissions` | `api/identity/rolepermissions` | Role→Permission |
| UserRoleAssignment | `UserRoleAssignments` | `api/identity/userroleassignments` | Contexto Org/Branch/Product |
| ClientProductBinding | `ClientProductBindings` | `api/identity/clientproductbindings` | client_id → Product |
| AuthAuditEvent | `AuthAuditEvents` | `api/identity/auth-audit-events` | Append-only |
| MenuItem | `MenuItems` | `api/identity/menuitems` + `api/identity/menus/effective` | PermissionCode → UI |

### Páginas de conta

| Página | Função |
|--------|--------|
| `/Account/Login` | Senha + lockout + redirect 2FA |
| `/Account/LoginWith2fa` | TOTP |
| `/Account/EnableAuthenticator` | Ativar TOTP (autenticado) |
| `/Account/ForgotPassword` / `ResetPassword` | Reset |
| `/Account/ConfirmEmail` | Confirmação |

### Pendente (fases seguintes)

`ClaimDefinition`, `UserClaimAssignment`, AuthClient/Scope como domínio completo, `ExternalIdentityProvider`, SMTP real, authZ nas APIs admin, ProductEnablement.

## Serviços de infraestrutura transversais

| Serviço | Interface | Implementação | Status |
|---------|-----------|---------------|--------|
| Mail | `IMailService` | `MailService` (+ `CapturingMailService` em testes) | Ativo (logger MVP) |
| Permissions efetivas | `IEffectivePermissionsResolver` | `EffectivePermissionsResolver` | Ativo (Fase 3) |
| Auditoria | `IAuthAuditService` | `AuthAuditService` | Ativo (Fase 4) |
| Sessões | `IUserSessionService` | `UserSessionService` | Ativo (Fase 4) |

## Shared

`SSO.Shared/Identity/SsoClaimTypes.cs` — claim types, grant `switch_context`, client ids de seed.
