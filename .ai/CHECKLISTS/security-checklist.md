# Security Checklist

> Status Phase 6 (feature 00001): itens marcados refletem o estado do código no repositório. AuthZ em APIs admin de audit/menus ainda é follow-up operacional.

## Entrada e validação

- [x] Inputs validados (FluentValidation / specs / ModelWrapper)
- [x] Isolamento multi-tenant via claims `organization_id` / `branch_id` + assignments (ADR-003/004)
- [x] Sem mass assignment perigoso (suppressed properties / keys configuradas)

## Autenticação / autorização

- [x] AuthN: ASP.NET Identity + OpenIddict `/connect/*`
- [x] `UseAuthentication` / `UseAuthorization` ativos
- [x] Permissions efetivas no JWT; products autorizam localmente (ADR-005)
- [x] `[Authorize]` / `[RequiresPermission]` em APIs admin sensíveis (audit/menus/revoke + CRUD Identity) — feature 00002
- [x] IdP Entra OIDC pronto para homologação (`Sso:ExternalAuth:Entra`); Google/LDAP stub (D7)

## Segredos e configuração

- [x] Client secrets de produção via env/User Secrets / KV — não commitados no Production template
- [x] Connection strings de produção fora do padrão Development commitado para LocalDB apenas
- [x] Seed secret `dev-service-secret-change-me` documentado como **dev-only**

## Dados e logs

- [x] Logs de mail/audit sem senhas
- [x] Notifications logam payloads de domínio (revisar PII em ops)
- [x] HTTPS redirection mantida

## Persistência

- [x] Queries EF parametrizadas
- [x] P-004: AutoMigrate off por padrão em Production (`Sso:Database:AutoMigrate`)
- [ ] Menor privilégio da connection string — ops

## Dependências / superfície

- [x] Swagger apenas em Development
- [x] CORS allowlist (`Sso:Cors`)
- [x] Rate limit em `/Account/*` e `/connect/token|authorize`
- [x] Lockout Identity configurável (`Sso:Lockout`)
- [x] Signing: cert de desenvolvimento no Dev; Production exige cert path (Key Vault rotation = D9 ops)
