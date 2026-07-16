# ADR-007 — Claim `sid` e deny-list SQL para revogação quente

- Status: Aceito
- Data: 2026-07-16
- Decisores: produto SSO (feature 00005)

## Contexto

Access tokens JWT são válidos até expirar (~15 min) mesmo após logout/revoke. Requisitos corporativos pedem “desligar acesso agora” (SLA ≤ 60s). Redis e introspection-only foram rejeitados no MVP (F00005-D1).

## Decisão

1. Emitir claim **`sid`** (Guid de `UserSession`) em todo access token de usuário.
2. Manter **deny-list em SQL** (`RevokedSessions`) consultada pelo AS e pelo SDK.
3. SDK `SSO.Client`: check **on by default** com cache 30–60s; opt-out documentado.
4. Webhook MVP `session.revoked` via outbox + HMAC (F00005-D4).

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| Redis deny-list | Latência baixa | Infra nova |
| Só introspection | Padrão OAuth | Carga/latência no AS |
| Só TTL 15 min | Zero custo | Não cumpre SLA |

## Consequências

### Positivas

- SLA ≤ 60s com stack SQL existente
- Products optam out explicitamente se não precisarem do SLA

### Negativas / trade-offs

- +1 hop HTTP (cacheado) por sid no product
- Tokens sem `sid` (pré-00005) só morrem no TTL

## Impacto no código

- `TokenClaimsFactory`, `UserSessionService`, `SSO.Client` revocation events
- Migration `Phase9HotRevocationSessions`
- Docs: `product-integration.md`

## Referências

- `.ai/WORK/2026-07-16-00005-revogacao-quente-sessao.md`
- `Decisions.md` F00005-D1..D4
