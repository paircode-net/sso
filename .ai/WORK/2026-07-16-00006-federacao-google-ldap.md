# Feature Plan — 00006 Federação Google OIDC e LDAP

> Arquivo: `.ai/WORK/2026-07-16-00006-federacao-google-ldap.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Refinamento**  
> Data: 2026-07-16  
> Depende de: Fase 6 (Entra homologável; Google/LDAP **stubs**; catálogo `ExternalIdentityProviders`)  
> Decisão base: D7 — ordem Entra → Google → LDAP

## Objetivo

Completar a federação de identidade externa: **Google OIDC operacional** (não stub) e **LDAP/AD bind** para autenticação corporativa legada, reusando o fluxo `/Account/ExternalLogin` e o catálogo por Organization.

## Contexto

- Entra ID OIDC pronto para homologação via `Sso:ExternalAuth:Entra`.
- Google OIDC e LDAP existem como stubs (`ILdapAuthenticationStub`).
- Multi-org: IdPs podem ser habilitados por organização (catálogo + flags).

## Escopo

### Inclui

**Google OIDC**

- Config `Sso:ExternalAuth:Google` (ClientId/Secret, callback).
- Handler ASP.NET `AddGoogle` ou OIDC genérico alinhado ao padrão Entra.
- Link/unlink de login externo à conta local (Identity `UserLogins`).
- Provisionamento: JIT (Just-In-Time) create user **ou** require pre-provision — decisão aberta.
- Mapeamento claims Google → profile (email, name).
- Botão no Login apenas se provider enabled no catálogo (e org context se aplicável).
- Testes com mock handler / TestHost challenge stub.
- Doc de homologação (checklist secrets, redirect URIs).

**LDAP / Active Directory**

- Substituir stub por adaptador real: bind (user/password) contra LDAP path.
- Options: Host, Port, UseSsl, BaseDn, BindDn pattern (`{username}@domain` ou `uid=`), timeouts.
- Fluxo UX: formulário “LDAP” ou campo domínio no Login (não OIDC redirect).
- Mapear entrada LDAP → User local (key estável: `objectGUID` / DN hash); sync de display name/email se disponíveis.
- Lockout / rate limit já existentes aplicados.
- Auditoria de sucesso/falha sem logar senha.
- Testes com LDAP em memória/docker **ou** interface mock + contract tests do adapter.

**Comum**

- Admin API: enable/disable provider por org (já parcialmente); validar secrets só via config/KV.
- Security: não permitir LDAP sem TLS em Production (config flag enforce).

### Fora de escopo

- SAML 2.0 IdP.
- SCIM provisioning completo.
- Sync contínuo de grupos LDAP → Roles (pode ser evolução; v1 = authN only + membership manual).
- Social IdPs adicionais (GitHub, Apple, etc.).

## Abordagem

### Google

1. Remover stub; wiring simétrico ao Entra.
2. Seed `ExternalIdentityProviders` Google enabled=false por default.
3. Homologação com projeto Google Cloud OAuth.

### LDAP

1. Porta `ILdapAuthenticationService` no Domain/Application; adapter em Infrastructures (novell.ldap / System.DirectoryServices.Protocols — escolher).
2. Página Razor `LoginWithLdap` ou seção no Login.
3. Após bind OK: sign-in Identity cookie → continuar OIDC authorize se houver `returnUrl`.
4. Feature flag `Sso:ExternalAuth:Ldap:Enabled`.

### Fases de entrega

| Fase | Entrega |
|------|---------|
| 6.1 | Google OIDC real + docs + testes |
| 6.2 | LDAP adapter + UI + enforce TLS prod |
| 6.3 | JIT rules + link account UX |

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | Ports LDAP; regras de linking |
| Application | External login callbacks; LDAP login command |
| Infrastructures.Services | `Google` (se adapter), `LdapAuthenticationService` |
| Middleware | `AddAuthentication` Google; options bind |
| Web | `/Account/ExternalLogin`, `LoginWithLdap`, UI botões |
| Data | Ajustes catálogo IdP se necessário |
| Tests | Adapter mocks; integration external scheme |
| Docs | `phase6-hardening.md` → `external-idps.md`; integrations |

## Critérios de aceite

- [ ] Google: login real em ambiente de homologação cria/associa usuário e completa authorize.
- [ ] LDAP: bind válido autentica; inválido falha com mensagem genérica (sem user enumeration excessiva).
- [ ] Production rejeita LDAP plain sem TLS (quando enforce on).
- [ ] Stubs removidos ou claramente deprecated.
- [ ] Audit events para federated login success/failure.
- [ ] D7 atualizado em Decisions (Google/LDAP implementados).

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Account takeover via e-mail Google não verificado | Exigir email_verified; policy de link |
| LDAP credential stuffing | Rate limit + lockout + captcha futuro |
| Dependência nativa Windows vs cross-platform | Preferir lib managed cross-platform |
| JIT cria users órfãos | Default pre-provision em corp; JIT só se flag |

## Estratégia de testes

- [ ] Unit: mapeamento claims Google; parse DN LDAP
- [ ] Integração: ExternalLogin callback com TestAuthHandler
- [ ] Contract: LDAP adapter com server de teste (Testcontainers) se viável
- [ ] Negativos: secret errado, user disabled, org sem provider

## Decisões abertas

- [ ] **D-00006-1:** JIT provisioning on/off por org?
- [ ] **D-00006-2:** Biblioteca LDAP (SDS.Protocols vs Novell)?
- [ ] **D-00006-3:** Auto-link por e-mail se já existe User local?
- [ ] **D-00006-4:** Grupos LDAP → Roles nesta feature ou depois?

## Checklist

- [ ] Alinhado a security.md / phase6-hardening
- [ ] Secrets só config/KV
- [ ] Migrations se schema IdP mudar
- [ ] CONTEXT atualizado
- [ ] Pronto para implementação (após D-00006-1/2)
