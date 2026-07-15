# Business

## Escopo atual no código

O repositório implementa hoje um **scaffold CQRS** com o aggregate **Sample**. O domínio SSO está **planejado e decidido** (feature 00001 + ADRs), mas **ainda não codificado**.

### Regras de negócio inferíveis do código (Sample)

| Regra | Evidência |
|-------|-----------|
| Sample possui descrição obrigatória | Validator / mapping (`Required`, length 128) |
| Descrição não pode duplicar outra existente | `SampleDescriptionAlreadyExistsSpecification` + validators de create/update |
| Identificador é Guid | `DomainEntity<Guid>`, rotas `{id:Guid}` |
| Criação retorna HTTP 201 | `PostSampleCommandHandler` → `HttpStatusCode.Created` |
| Seed inicial com 15 samples | Migration DML `InitialDMLMigrationDefaultDbContext` |

## Domínio SSO (decidido — feature 00001)

Fonte: `.ai/WORK/2026-07-14-00001-plataforma-sso.md` e `.ai/CONTEXT/adr/`.

### Capacidades de produto

- Autenticação centralizada (OAuth 2.1 / OIDC / JWT via OpenIddict)
- Gestão de usuários e credenciais (ASP.NET Identity)
- Confirmação de e-mail, reset de senha, 2FA
- Sessões, emissão/revogação de tokens, auditoria AuthN/AuthZ
- AuthZ híbrida: Roles + Claims + Permissions dinâmicas
- Multi-produto, multi-organization, multi-branch

### Atores (alvo)

| Ator | Papel |
|------|-------|
| Usuário final | Autentica; atua em uma ou mais orgs/branches |
| Administrador de organização | Gerencia memberships, branches, roles no tenant |
| Administrador de plataforma | Gerencia products, AuthClients, configuração global (via API no MVP) |
| Aplicação (AuthClient) | Consome JWT com permissões efetivas do contexto |
| IdP externo | Federação (Entra → Google → LDAP na Fase 6) |

### Regras de contexto e autorização (MVP)

| Regra | Decisão |
|-------|---------|
| Tenant = Organization | Isolamento lógico obrigatório |
| Contexto ativo via switch-context | Claims `organization_id` / `branch_id` no token; header **não** é fonte de verdade |
| Permissions no JWT | Conjunto efetivo do contexto ativo embutido no access token (ADR-005) |
| Frescor | TTL curto; re-resolução em login, refresh e switch-context |
| Branch hierárquica | Estrutura opcional; **sem** herança authz pai→filho no MVP |
| Herança Role → Permission | Permitida |
| Product ≠ AuthClient | Conceitos distintos |
| Admin UI rica | Fora do MVP (API-only + Razor login/consent) |

### Composition de autorização

```text
User → Organization → Branch → Product → Role → Claims → Permissions
```

Sempre resolvida no contexto ativo do token + product do cliente OAuth.

## Idioma / UX de mensagens

- Cultura padrão da API: `pt-BR`
- Testes suportam também `en-US`
- Mensagens de operação bem-sucedida usam localizer (ex.: `"Successful operation!"` via recursos)
- Login/consent: UI Razor no host da API (MVP)

## Fontes de verdade de negócio

1. ADRs em `CONTEXT/adr/` e decisões F00001-D* em `Decisions.md`
2. Plano `.ai/WORK/2026-07-14-00001-plataforma-sso.md`
3. Quando implementado: Specifications/validators no Domain + testes `*Scenarios`
4. Forge (estrutura de entidades)

Não inventar regras além das documentadas sem confirmação do product owner.
