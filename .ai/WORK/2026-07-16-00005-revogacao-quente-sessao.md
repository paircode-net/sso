# Feature Plan — 00005 Revogação quente e sessão como produto

> Arquivo: `.ai/WORK/2026-07-16-00005-revogacao-quente-sessao.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Implementado** (2026-07-16)  
> Data: 2026-07-16  
> Depende de: Fase 4 (`IUserSessionService`, revoke OpenIddict); 00004 (SDK consome sinais)  
> Relaciona: ADR-005, ADR-007

## Objetivo

Reduzir a janela em que um access token **já emitido** continua válido após logout, revoke de sessão, mudança crítica de permissões ou desligamento de usuário — tornando **sessão** um conceito de produto observável e controlável, não só tokens OpenIddict.

## Decisões aceitas

| ID | Escolha |
|----|---------|
| D-00005-1 | Deny-list **SQL** + claim `sid` |
| D-00005-2 | Hot check SDK **on by default** (opt-out) |
| D-00005-3 | SLA **≤ 60 s** (cache 30–60s) |
| D-00005-4 | Webhooks MVP: outbox + HMAC `session.revoked` |

## Entregue

| Item | Caminho / nota |
|------|----------------|
| `UserSession` + `RevokedSession` + outbox + `ClientWebhookEndpoint` | Domain + IdentityDb migration `Phase9HotRevocationSessions` |
| Claim `sid` | `TokenClaimsFactory` + ADR-007 |
| APIs | `GET/POST api/identity/sessions/*`, status público `{sid}/status` |
| UI | `/Account/Sessions` |
| Triggers | Logout, password reset, admin bulk revoke |
| SDK | `SsoClient:RevocationCheck` default on |
| Webhook | `WebhookOutboxSenderHostedService` HMAC-SHA256 |
| Docs | `product-integration.md`, integrations |

## Critérios de aceite

- [x] Admin/usuário lista sessões e revoga uma sessão específica
- [x] Após revoke, refresh/switch_context com `sid` revogado → `invalid_grant`
- [x] Hot-check default; SLA ≤ 60s documentado
- [x] Logout e reset password disparam revoke
- [x] Audit `session.revoked` / `tokens.revoked`
- [x] Outbox enfileira `session.revoked` quando há endpoint (teste integração)

## Checklist

- [x] D-00005-1..4 aceitas
- [x] ADR-007
- [x] Migration Phase9
- [x] product-integration atualizado
- [x] Implementado
