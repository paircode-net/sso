# Feature Plan — 00006 Federação Google OIDC e LDAP

> Arquivo: `.ai/WORK/2026-07-16-00006-federacao-google-ldap.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Pronto para implementação** (D-00006-1..4 aceitas)  
> Data: 2026-07-16  
> Depende de: Fase 6 (Entra homologável; Google/LDAP **stubs**; catálogo `ExternalIdentityProviders`)  
> Decisão base: D7 — ordem Entra → Google → LDAP

## Objetivo

Completar a federação de identidade externa: **Google OIDC operacional** (não stub) e **LDAP/AD bind** para autenticação corporativa legada, reusando o fluxo `/Account/ExternalLogin` e o catálogo por Organization — com JIT configurável, auto-link por e-mail verificado e MVP de mapeamento grupo LDAP → Role.

## Contexto

- Entra ID OIDC pronto para homologação via `Sso:ExternalAuth:Entra`.
- Google OIDC e LDAP existem como stubs (`ILdapAuthenticationStub`).
- Multi-org: IdPs podem ser habilitados por organização (catálogo + flags).

## Decisões aceitas

### D-00006-1 — JIT provisioning — **Aceito: C**

Flag **por organização / IdP** no catálogo (`AllowJitProvisioning`). **Default off** = pre-provision only. JIT cria `User` local no primeiro login só quando habilitado.

### D-00006-2 — Biblioteca LDAP — **Aceito: A**

**System.DirectoryServices.Protocols** (`LdapConnection`), atrás de `ILdapAuthenticationService`. Cross-platform adequado ao host .NET 10.

### D-00006-3 — Auto-link por e-mail — **Aceito: B**

Se e-mail do IdP vier **verificado** (`email_verified` OIDC; atributo confiável no LDAP) e existir `User` local único com o mesmo e-mail → **link automático** do `UserLogin`. Sem verificação → não linka.

### D-00006-4 — Grupos LDAP → Roles — **Aceito: B**

**MVP nesta feature:** config de mapeamento `group DN/CN → RoleId` por org; no login LDAP bem-sucedido, sincronizar `UserRoleAssignment` conforme membership nos grupos mapeados. Nested groups / sync contínuo fora do MVP (só no momento do login).

## Escopo

### Inclui

**Google OIDC**

- Config `Sso:ExternalAuth:Google` (ClientId/Secret, callback).
- Handler OIDC alinhado ao padrão Entra (stub removido).
- Link/unlink via Identity `UserLogins`; auto-link se e-mail verificado (D3).
- JIT só se flag do IdP/org (D1).
- Mapeamento claims Google → profile; exigir `email_verified` para link/JIT.
- Botão no Login se provider enabled.
- Docs de homologação (secrets, redirect URIs).

**LDAP / Active Directory**

- Adapter real com **System.DirectoryServices.Protocols**.
- Options: Host, Port, UseSsl, BaseDn, BindDn pattern, timeouts; enforce TLS em Production.
- UX: `LoginWithLdap` (ou seção no Login).
- Mapear entrada → User local (key estável: `objectGUID` / DN); auto-link/JIT conforme D1/D3.
- **Mapeamento grupo → Role** (config + sync no login) — D4.
- Lockout / rate limit; auditoria sem logar senha.

**Comum**

- Admin: enable/disable provider; flag JIT; CRUD mínimo de mapeamentos LDAP→Role (API e/ou portal).
- Secrets só via config/KV.

### Fora de escopo

- SAML 2.0 IdP.
- SCIM provisioning completo.
- Nested group expansion / sync contínuo em background (só sync no login).
- Social IdPs adicionais (GitHub, Apple, etc.).

## Abordagem

### Google

1. Remover stub; wiring simétrico ao Entra.
2. Seed `ExternalIdentityProviders` Google; `AllowJitProvisioning=false` default.
3. Callback: verify email → auto-link ou JIT ou reject.
4. Homologação Google Cloud OAuth.

### LDAP

1. `ILdapAuthenticationService` + adapter SDS.Protocols.
2. `LoginWithLdap`; pós-bind: Identity cookie → OIDC authorize se `returnUrl`.
3. Carregar grupos do user; aplicar mapeamentos → `UserRoleAssignment`.
4. Feature flag `Sso:ExternalAuth:Ldap:Enabled` + enforce TLS prod.

### Fases de entrega

| Fase | Entrega |
|------|---------|
| 6.1 | Google OIDC real + auto-link/JIT flags + docs + testes |
| 6.2 | LDAP adapter + UI + TLS enforce |
| 6.3 | Mapeamento grupo → Role (config + sync no login) |

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | Ports LDAP; regras linking/JIT; entidade mapeamento grupo→Role |
| Application | External login; LDAP login; sync assignments |
| Infrastructures.Services | `LdapAuthenticationService` (SDS.Protocols) |
| Middleware | `AddAuthentication` Google; options |
| Web | `/Account/ExternalLogin`, `LoginWithLdap` |
| Data | Migration: `AllowJitProvisioning`; `LdapGroupRoleMaps` (ou equivalente) |
| Tests | Mocks LDAP; TestAuthHandler Google path |
| Docs | `external-idps.md` / phase6-hardening; integrations |

## Critérios de aceite

- [ ] Google: login homologável cria/associa user (JIT se flag) ou auto-link se e-mail verificado.
- [ ] LDAP: bind válido autentica; inválido falha genérica.
- [ ] Production rejeita LDAP plain sem TLS (enforce on).
- [ ] Mapeamento grupo→Role aplica assignments no login LDAP.
- [ ] Stubs removidos ou deprecated.
- [ ] Audit federated login success/failure.
- [ ] D7 atualizado (Google/LDAP implementados).

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Account takeover via e-mail | Exigir `email_verified`; sem verified = sem auto-link |
| LDAP credential stuffing | Rate limit + lockout |
| Mapeamento grupo incompleto (nested) | Documentar: só membership direta no MVP |
| JIT órfãos | Default JIT off |

## Estratégia de testes

- [ ] Unit: claims Google; parse DN; resolução grupo→Role
- [ ] Integração: ExternalLogin com TestAuthHandler
- [ ] LDAP adapter mock + contract; TLS enforce config
- [ ] Negativos: secret errado, user disabled, org sem provider, JIT off

## Checklist

- [x] D-00006-1..4 aceitas
- [ ] Alinhado a security.md / phase6-hardening
- [ ] Secrets só config/KV
- [ ] Migrations planejadas
- [ ] CONTEXT atualizado na implementação
- [x] Pronto para implementação
