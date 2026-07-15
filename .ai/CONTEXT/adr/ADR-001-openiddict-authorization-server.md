# ADR-001 — OpenIddict como Authorization Server

- Status: Aceito
- Data: 2026-07-14
- Decisores: Equipe SSO / feature 00001

## Contexto

A plataforma SSO precisa emitir e revogar tokens conforme OAuth 2.1 e OpenID Connect, com suporte a Authorization Code + PKCE, Refresh Token e Client Credentials. O repositório é ASP.NET Core (.NET 10) e ainda não possui Authorization Server.

## Decisão

Usar **OpenIddict** como Authorization Server no host `SSO.Web.Api`, com store EF Core no bounded context **Identity** (`IdentityDbContext`).

- Flows prioritários: Authorization Code + PKCE, Refresh Token, Client Credentials.
- Endpoints de protocolo na convenção OpenIddict (`/connect/*`).
- Assinatura: **dev** com certificado/chave local; **produção** com Azure Key Vault e rotação de chaves (D9).
- Domain da aplicação não referencia OpenIddict diretamente — configuração e adapters na Infrastructure / Middleware.

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| IdentityServer / Duende | Maduro, rico em features | Custo de licença / acoplamento comercial |
| Auth0 / Entra ID External como AS único | Menos ops | Menor controle do modelo multi-org/branch/product |
| Implementação custom JWT | Controle total | Alto risco de segurança e manutenção |

## Consequências

### Positivas

- Padrões OAuth/OIDC com biblioteca alinhada ao ecossistema ASP.NET.
- Persistência coesa com EF já usado no projeto.
- Escopo de autenticação centralizado no mesmo deploy do SSO.

### Negativas / trade-offs

- Curva de configuração OpenIddict + Identity.
- Responsabilidade operacional de chaves e discovery em produção.

## Impacto no código

- Projetos: `SSO.Middleware` (DI), `SSO.Web.Api` (host/endpoints), `SSO.Infrastructures.Data` (store).
- Migrações: tabelas OpenIddict em `IdentityDb`.
- Breaking changes: nenhum no código atual (auth ainda desligada).

## Referências

- Feature plan: `.ai/WORK/2026-07-14-00001-plataforma-sso.md`
- `CONTEXT/Decisions.md` (F00001-D*)
- ADR-002, ADR-006
)
