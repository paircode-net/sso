# Feature Plan — 00005 Revogação quente e sessão como produto

> Arquivo: `.ai/WORK/2026-07-16-00005-revogacao-quente-sessao.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Pronto para implementação** (D-00005-1..4 aceitas)  
> Data: 2026-07-16  
> Depende de: Fase 4 (`IUserSessionService`, revoke OpenIddict); 00004 (SDK consome sinais)  
> Relaciona: ADR-005 (TTL curto mitiga, não elimina, staleness)

## Objetivo

Reduzir a janela em que um access token **já emitido** continua válido após logout, revoke de sessão, mudança crítica de permissões ou desligamento de usuário — tornando **sessão** um conceito de produto observável e controlável, não só tokens OpenIddict.

## Contexto

- Access TTL = 15 min; refresh = 14 dias; `perm_ver` invalida caches locais no próximo token.
- Revogação em massa de tokens já existe; efeito no access token ativo depende do TTL.
- Requisitos típicos de segurança corporativa: “desligar acesso agora” (&lt; 1 min).

## Decisões aceitas

### D-00005-1 — Store / mecanismo de hot revoke — **Aceito: A**

Deny-list em **SQL** + claim `sid` no access token. AS e SDK consultam a lista com cache curto. Redis fica para escala futura. Introspection **não** é o caminho principal.

### D-00005-2 — Hot check no SDK — **Aceito: B**

Check **ligado por padrão** em `AddSsoAuthentication`; opt-out via config (`SsoClient:RevocationCheck:Enabled=false`). Sem check = sem SLA de hot revoke (documentar em `product-integration.md`).

### D-00005-3 — SLA de revoke — **Aceito: B**

Janela máxima documentada: **≤ 60 s**. Cache default do SDK 30–60s; perfis high-security podem baixar (ex. 15s) via config.

### D-00005-4 — Webhooks — **Aceito: B**

Incluir nesta feature o **MVP de notificações**:

- Outbox de eventos `SessionRevoked`
- Webhook HMAC `session.revoked` por AuthClient registrado

## Escopo

### Inclui

**Modelo de sessão**

- Entidade ou projeção `Session` (user, client_id, org/branch ativos se souber, created, last_seen, revoked_at, reason).
- API: listar sessões do usuário (self + admin); revogar uma ou todas.
- Auditoria de revoke (`AuthAuditEvent`).

**Revogação quente**

1. Claim `sid` estável no access token na emissão.
2. Tabela deny-list SQL (`session_id → revoked_at`, TTL ≥ access lifetime).
3. SDK: check on by default (cache 30–60s) alinhado ao SLA ≤ 60s.
4. Endpoint leve no AS para o SDK consultar status do `sid` (ou batch).

**Notificações (MVP)**

1. Outbox `SessionRevoked`.
2. Delivery webhook HMAC por AuthClient (URL + secret no cadastro do client).

**Triggers de revoke automático**

- Logout `/connect/logout`
- Reset de senha / disable user / lockout admin
- Remoção de membership crítico (opcional, configurável)
- Mudança de `perm_ver` **não** invalida access imediatamente por default — documentar trade-off; opção “hard revoke on role change” para admins.

### Fora de escopo

- Redis deny-list (evolução pós-MVP desta feature).
- Introspection como mecanismo principal.
- IdP-initiated SAML logout completo.
- Device management mobile avançado (MDM).
- Fraud detection ML.
- `perm_ver.changed` webhook (só `session.revoked` no MVP).

## Abordagem

### Fase A — Sessão como API de produto

1. Definir `Session` vs apenas tokens OpenIddict (preferir view/enrich sobre tokens persistidos + metadados).
2. Endpoints `api/identity/sessions` (self) e admin revoke (permission `sso.admin.sessions.revoke`).
3. UI Account “sessões ativas” (Razor) + portal (00003) depois se couber.

### Fase B — Hot revoke MVP

1. Incluir `sid` no access token (`TokenClaimsFactory`).
2. Persist deny-list em SQL com TTL ≥ access lifetime.
3. SDK: revocation check **on by default** (cache 30–60s); opt-out documentado.
4. Documentar SLA ≤ 60s em `product-integration.md`.

### Fase C — Notificações MVP

1. Outbox de eventos `SessionRevoked` no revoke.
2. Worker/sender webhook HMAC por AuthClient (retry básico).
3. Docs: contrato do payload + assinatura.

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | `Identity/Sessions/`, eventos de auditoria |
| Application | Commands/Queries sessions; handlers logout/reset |
| Data | Migration Identity; `RevokedSessions` / Sessions; Outbox |
| Middleware / OpenIddict | `TokenClaimsFactory` (+ `sid`) |
| Infrastructures | Deny-list store; webhook sender HMAC |
| Client SDK | Revocation check default-on (00004) |
| Tests | Revoke → access antigo rejeitado ≤ 60s; webhook smoke |
| Docs | `product-integration.md` (SLA), ADR claim `sid` se necessário |

## Critérios de aceite

- [ ] Admin/usuário lista sessões e revoga uma sessão específica.
- [ ] Após revoke, refresh token da sessão falha.
- [ ] Com hot-check (default), access token da sessão revogada é rejeitado em ≤ **60 s**.
- [ ] Logout e reset password disparam revoke documentado.
- [ ] Audit registra ator, alvo, reason.
- [ ] Webhook `session.revoked` entregue (HMAC) para AuthClient com URL configurada (pelo menos 1 smoke test).

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Latência em todo request | Deny-list cacheada no SDK (30–60s) |
| SQL deny-list multi-instance | Tabela compartilhada; Redis só se métricas exigirem |
| Webhook flaky / retry storm | Outbox + backoff; HMAC; timeout curto |
| Clients sem `sid` antigo | Claim adicional; tokens antigos expirando em ≤ 15 min |
| Falso positivo (sessão errada) | Testes + reason codes |

## Estratégia de testes

- [ ] Unit: deny-list store
- [ ] Integração AS: emit → revoke → refresh negado
- [ ] Integração product middleware: access negado pós-revoke (≤ 60s com cache baixo em teste)
- [ ] Smoke: outbox → webhook HMAC
- [ ] Carga leve: check revoke budget de latência (definir X ms p95 na impl)

## Checklist

- [x] D-00005-1..4 aceitas
- [ ] ADR de claim `sid` se necessário
- [ ] Segurança: deny-list sem PII excessiva; webhook HMAC
- [ ] Migrations planejadas
- [ ] product-integration atualizado (SLA + opt-out)
- [x] Pronto para implementação
