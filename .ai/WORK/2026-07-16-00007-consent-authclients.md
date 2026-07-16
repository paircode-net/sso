# Feature Plan — 00007 Consent explícito e lifecycle de AuthClients

> Arquivo: `.ai/WORK/2026-07-16-00007-consent-authclients.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Pronto para implementação** (D-00007-1..4 aceitas)  
> Data: 2026-07-16  
> Depende de: Fase 2 (OpenIddict AS); **00002** para APIs admin protegidas  
> Relaciona: 00003 (UI admin de clients); D6 (consent implícito no seed MVP); D10 (Product ≠ AuthClient)

## Objetivo

Substituir o **consent implícito de desenvolvimento** por um fluxo OIDC de **consent explícito** quando necessário, e entregar **lifecycle completo de AuthClients** (CRUD, secrets, redirect URIs, scopes, rotação, disable) alinhado a multi-produto.

## Contexto

- Seed MVP: consent implícito; clients `dev-product-spa` / `dev-product-service`.
- `ClientProductBinding` liga `client_id` → Product (Fase 3/5).
- Sem gestão formal, onboarding de novos produtos depende de seed/código.

## Decisões aceitas

### D-00007-1 — Consent “Remember” — **Aceito: C**

Política **por client**:

- First-party / `RequireConsent=Never|First`: remember **permanente** (até revoke ou scopes novos).
- `RequireConsent=Always`: TTL configurável (default **180 dias**) via metadado `ConsentRememberDays`.

### D-00007-2 — Store de metadados — **Aceito: C**

**Híbrido:**

- OpenIddict: redirects, secrets, consent type nativo, scopes permitidos.
- Tabela leve `AuthClientMetadata` (ou equivalente): `IsSystem`, `IsFirstParty`, `RequireConsent` (`Always|First|Never`), `ConsentRememberDays`, display admin.

`ClientProductBinding` permanece a ligação client → Product.

### D-00007-3 — Naming de scopes — **Aceito: A**

Scopes custom: **`{product_code}.{feature}`** (lowercase, dot-separated). Ex.: `dev-product.reports`. OIDC padrão (`openid`, `profile`, `email`) continua; authZ fina segue em claims `permissions`.

### D-00007-4 — First-party — **Aceito: A**

Flag **`IsFirstParty`** no registro/metadado. `RequireConsent=Never` **só** permitido se `IsFirstParty=true` (validação na API). Seed marca system/first-party.

## Escopo

### Inclui

**Consent**

- Página Razor `/Account/Consent` (scopes + client name + lembrar consent).
- Persistência via OpenIddict authorizations (+ TTL por metadado quando Always).
- Política por client: `RequireConsent` = Always | First | Never.
- Re-consent quando scopes aumentam.
- Audit: consent granted/denied.

**AuthClients lifecycle**

- API `api/identity/auth-clients` (wrapper sobre OpenIddict + metadados):
  - Create (public PKCE vs confidential)
  - Update redirect URIs, post-logout URIs, scopes
  - Rotate client secret (one-shot plain)
  - Disable / soft-delete (`IsSystem` não deletável)
  - Bind/unbind Product
- Validations: HTTPS redirects em Production; localhost só Dev.
- Seed clients `IsSystem` + `IsFirstParty`.

**Scopes**

- Catálogo leve + associação a clients.
- Convention `{product_code}.{feature}` documentada.

### Fora de escopo

- Marketplace third-party / review humano.
- DCR público.
- mTLS client auth.
- UI rica de clients no portal (API + Consent Razor bastam; 00003 pode consumir depois).

## Abordagem

### Fase A — Consent UI + policy

1. Metadados + seed flags first-party / Never.
2. Consent page no pipeline authorize.
3. Remember permanente vs TTL conforme D1.

### Fase B — AuthClient admin API

1. CQRS wrapping `IOpenIddictApplicationManager` + `AuthClientMetadata`.
2. Secret one-shot; permissions `sso.admin.platform`.
3. Validar `Never` ⇒ `IsFirstParty`.

### Fase C — Scopes + docs

1. Catálogo scopes + naming A.
2. Guia “Registrar novo produto/client”; atualizar `product-integration.md`.

## Critérios de aceite

- [ ] Client `RequireConsent=Always` exibe Consent antes do code.
- [ ] First-party (`Never`) não exibe Consent.
- [ ] PlatformAdmin cria SPA PKCE e confidential via API.
- [ ] Rotate secret invalida o antigo.
- [ ] Disable client faz authorize/token falharem.
- [ ] Binding client→product editável; scopes `{product_code}.{feature}` documentados.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Dev DX (consent em todo login) | Seed first-party = Never |
| Vazamento de secret | One-shot; never log |
| Open redirect | Allowlist strict |
| Never em third-party | API rejeita sem `IsFirstParty` |

## Estratégia de testes

- [ ] Matriz RequireConsent Always/First/Never
- [ ] API create/rotate/disable + Never sem first-party rejeitado
- [ ] Redirect http em Production
- [ ] Remember TTL expiry path (Always)

## Checklist

- [x] D-00007-1..4 aceitas
- [ ] 00002 permissions
- [ ] Security: redirects, secrets
- [ ] Migrations metadados
- [ ] CONTEXT + Decisions (revisar D6 pós-impl)
- [x] Pronto para implementação
