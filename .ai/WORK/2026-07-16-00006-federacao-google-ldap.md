# Feature Plan — 00006 Federação Google OIDC e LDAP

> Arquivo: `.ai/WORK/2026-07-16-00006-federacao-google-ldap.md`  
> Status: **Implementado** (2026-07-16)  
> Decisões: F00006-D1..D4 aceitas

## Decisões aceitas

| ID | Escolha |
|----|---------|
| D1 | JIT por org/IdP (default off) |
| D2 | System.DirectoryServices.Protocols |
| D3 | Auto-link se e-mail verificado |
| D4 | MVP grupo LDAP → Role no login |

## Entregue

| Item | Nota |
|------|------|
| Google OIDC | Wiring real + `MapInboundClaims=false`; seed `google` |
| FederatedAccountService | Auto-link / JIT / reject |
| LDAP | `LdapAuthenticationService` + `/Account/LoginWithLdap` + TLS enforce prod |
| LdapGroupRoleMap | Entity + API + sync no login |
| AllowJitProvisioning | Coluna + PATCH admin |
| Docs | `external-idps.md`; migration `Phase10FederationGoogleLdap` |

## Critérios de aceite

- [x] Google path com policies JIT/auto-link (homologação via config)
- [x] LDAP bind adapter + UI; TLS enforce em Production
- [x] Mapeamento grupo→Role no login
- [x] Stubs removidos (`ILdapAuthenticationStub` → serviço real/disabled)
- [x] Audit federated success/failure
- [x] D7 atualizado em Decisions
