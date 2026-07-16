# Feature Plan — 00007 Consent explícito e lifecycle de AuthClients

> Arquivo: `.ai/WORK/2026-07-16-00007-consent-authclients.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Refinamento**  
> Data: 2026-07-16  
> Depende de: Fase 2 (OpenIddict AS); **00002** para APIs admin protegidas  
> Relaciona: 00003 (UI admin de clients); D6 (consent implícito no seed MVP); D10 (Product ≠ AuthClient)

## Objetivo

Substituir o **consent implícito de desenvolvimento** por um fluxo OIDC de **consent explícito** quando necessário, e entregar **lifecycle completo de AuthClients** (CRUD, secrets, redirect URIs, scopes, rotação, disable) alinhado a multi-produto.

## Contexto

- Seed MVP: consent implícito; clients `dev-product-spa` / `dev-product-service`.
- `ClientProductBinding` liga `client_id` → Product (Fase 3/5).
- Sem gestão formal, onboarding de novos produtos depende de seed/código.

## Escopo

### Inclui

**Consent**

- Página Razor `/Account/Consent` (scopes + client name + lembrar consent).
- Persistência de consentimentos do usuário (OpenIddict authorizations / store dedicado).
- Política por client: `RequireConsent` = Always | First time | Never (trusted first-party).
- Re-consent quando scopes aumentam.
- Audit: consent granted/denied.

**AuthClients lifecycle**

- Modelo de domínio/admin sobre OpenIddict applications (não expor OpenIddict entities cruas na API pública).
- API `api/identity/auth-clients`:
  - Create (public PKCE vs confidential)
  - Update redirect URIs, post-logout URIs, scopes permitidos
  - Rotate client secret (confidential) — secret mostrado **uma vez**
  - Disable / soft-delete
  - Bind/unbind Product (`ClientProductBinding`)
- Validations: HTTPS redirects em Production; localhost só Dev.
- Seed clients permanecem; marcados `IsSystem` (não deletáveis).

**Scopes**

- Catálogo de scopes (CRUD leve) + associação a clients.
- Scopes custom de produto documentados no contract.

### Fora de escopo

- Marketplace de apps third-party com review humano.
- DCR (Dynamic Client Registration RFC) público — pode ser fase futura.
- mTLS client auth (evolução).
- UI rica (coberta por 00003; aqui API + Consent Razor bastam).

## Abordagem

### Fase A — Consent UI + policy

1. Desligar consent implícito global; trusted clients (`sso-admin`, first-party) = Never.
2. Implementar Consent page no pipeline authorize OpenIddict.
3. Testes: authorize → consent → code; deny → error.

### Fase B — AuthClient admin API

1. Commands CQRS wrapping OpenIddict `IOpenIddictApplicationManager`.
2. Secret hashing já do OpenIddict; API retorna plain secret só no create/rotate.
3. Permissions: `sso.admin.platform` (00002).

### Fase C — Docs e samples

1. Guia “Registrar novo produto/client”.
2. Atualizar `product-integration.md` + `integrations.md`.

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | `AuthClients/`, `Scopes/` (se ainda não first-class) |
| Application | CRUD handlers; Consent app services |
| Data | Possível migration se metadados extras além OpenIddict |
| Web | `/Account/Consent.cshtml`; AuthorizationController hooks |
| Middleware | OpenIddict server options (consent) |
| Seed | Flags RequireConsent por client |
| Tests | OIDC authorize com consent; API auth-clients |
| Docs | product-integration, glossary (AuthClient) |

## Critérios de aceite

- [ ] Client com `RequireConsent=Always` exibe Consent antes do code.
- [ ] Trusted client não exibe Consent.
- [ ] PlatformAdmin cria SPA PKCE e confidential service via API.
- [ ] Rotate secret invalida secret antigo; novo funciona em client_credentials.
- [ ] Disable client faz authorize/token falharem.
- [ ] Binding client→product afeta permissions no JWT (já existente) e é editável via API.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Quebrar dev DX (todo login pede consent) | Never para clients seed; docs claros |
| Vazamento de secret em logs/API responses | Retorno one-shot; never log secret |
| Redirect URI open redirect | Validação strict allowlist |
| Divergência Product vs AuthClient | UI/API reforça D10; binding obrigatório para user tokens |

## Estratégia de testes

- [ ] Integração OIDC: matriz RequireConsent
- [ ] API: create/rotate/disable
- [ ] Negativos: redirect http em Production
- [ ] Binding remove → permissions stub/vazio conforme regra documentada

## Decisões abertas

- [ ] **D-00007-1:** Consent “Remember” permanente vs tempo limitado?
- [ ] **D-00007-2:** Metadados extras na tabela própria ou só OpenIddict properties bag?
- [ ] **D-00007-3:** Scopes custom por Product — naming convention (`product.feature`)?
- [ ] **D-00007-4:** First-party clients list config vs flag no registro?

## Checklist

- [ ] 00002 permissions para APIs
- [ ] Security: redirects, secrets
- [ ] Migrations se houver
- [ ] CONTEXT + Decisions (revisar D6 para pós-MVP)
- [ ] Pronto para implementação (após D-00007-1/2)
