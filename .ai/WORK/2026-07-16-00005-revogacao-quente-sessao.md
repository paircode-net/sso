# Feature Plan — 00005 Revogação quente e sessão como produto

> Arquivo: `.ai/WORK/2026-07-16-00005-revogacao-quente-sessao.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Refinamento**  
> Data: 2026-07-16  
> Depende de: Fase 4 (`IUserSessionService`, revoke OpenIddict); idealmente 00004 (cliente consome sinais)  
> Relaciona: ADR-005 (TTL curto mitiga, não elimina, staleness)

## Objetivo

Reduzir a janela em que um access token **já emitido** continua válido após logout, revoke de sessão, mudança crítica de permissões ou desligamento de usuário — tornando **sessão** um conceito de produto observável e controlável, não só tokens OpenIddict.

## Contexto

- Access TTL = 15 min; refresh = 14 dias; `perm_ver` invalida caches locais no próximo token.
- Revogação em massa de tokens já existe; efeito no access token ativo depende do TTL.
- Requisitos típicos de segurança corporativa: “desligar acesso agora” (&lt; 1 min).

## Escopo

### Inclui

**Modelo de sessão**

- Entidade ou projeção `Session` (user, client_id, org/branch ativos se souber, created, last_seen, revoked_at, reason).
- API: listar sessões do usuário (self + admin); revogar uma ou todas.
- Auditoria de revoke (`AuthAuditEvent`).

**Revogação quente (escolher 1+ mecanismos — ver decisões)**

1. **Deny-list / version stamp no AS**
   - Claim `sid` ou `token_ver` / `session_id` no access token.
   - Store distribuído (SQL ou Redis): `session_id → revoked`.
   - Middleware no AS e **recomendação** de middleware no product SDK (00004) checando stamp (introspection curta ou cache local + push).
2. **Token introspection** (`/connect/introspect`) para resources confidential.
3. **Back-channel logout / eventos**
   - Webhook ou mensagem `session.revoked` / `perm_ver.changed` para products registrados.
4. **Reduzir TTL access** em perfis high-security (config) — complemento, não substituto.

**Triggers de revoke automático**

- Logout `/connect/logout`
- Reset de senha / disable user / lockout admin
- Remoção de membership crítico (opcional, configurável)
- Mudança de `perm_ver` **não** precisa invalidar access imediatamente se TTL curto — documentar trade-off; opção “hard revoke on role change” para admins.

### Fora de escopo

- IdP-initiated SAML logout completo (além de OIDC back-channel básico).
- Device management mobile avançado (MDM).
- Fraud detection ML.

## Abordagem

### Fase A — Sessão como API de produto

1. Definir `Session` vs apenas tokens OpenIddict (preferir view/enrich sobre tokens persistidos + metadados).
2. Endpoints `api/identity/sessions` (self) e admin revoke (permission `sso.admin.sessions.revoke`).
3. UI Account “sessões ativas” (Razor) + portal (00003) depois.

### Fase B — Hot revoke MVP

1. Incluir `sid` (session id) estável no access token na emissão.
2. Persist deny-list em SQL (MVP sem Redis) com TTL ≥ access lifetime.
3. Extensão no SDK: `AddSsoTokenRevocationCheck` (cache memory + periodic refresh ou check on each request com cache 30s).
4. Documentar: products **devem** habilitar o check para cumprir SLA de revoke.

### Fase C — Notificações (opcional)

1. Outbox de eventos `SessionRevoked`.
2. Webhook HMAC por AuthClient.

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | `Identity/Sessions/`, eventos de auditoria |
| Application | Commands/Queries sessions; handlers logout/reset |
| Data | Migration Identity; tabela `RevokedSessions` ou equivalente |
| Middleware / OpenIddict | `TokenClaimsFactory` (+ `sid`); introspection se adotado |
| Infrastructures | Store deny-list; webhook sender |
| Client SDK | Check de revogação (00004) |
| Tests | Revoke → access antigo rejeitado no product middleware |
| Docs | `product-integration.md` (SLA revoke), ADR novo se mudar claims |

## Critérios de aceite

- [ ] Admin/usuário lista sessões e revoga uma sessão específica.
- [ ] Após revoke, refresh token da sessão falha.
- [ ] Com hot-check habilitado, access token da sessão revogada é rejeitado em ≤ N segundos (N acordado, ex. 60).
- [ ] Logout e reset password disparam revoke documentado.
- [ ] Audit registra ator, alvo, reason.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Latência em todo request (introspection) | Deny-list local cacheada; TTL curto do cache |
| SQL deny-list não escala multi-instance sem sticky | Tabela compartilhada; ou Redis na fase seguinte |
| Quebra de clients que não leem `sid` | Claim adicional; validação só se product opt-in no SDK |
| Falso positivo (revoga sessão errada) | Testes + UI de confirmação; reason codes |

## Estratégia de testes

- [ ] Unit: deny-list store
- [ ] Integração AS: emit → revoke → refresh negado
- [ ] Integração product middleware: access negado pós-revoke
- [ ] Carga leve: check revoke não adiciona &gt; X ms p95 (definir budget)

## Decisões abertas

- [ ] **D-00005-1:** Deny-list SQL vs Redis vs só introspection?
- [ ] **D-00005-2:** Hot check obrigatório no SDK ou opt-in?
- [ ] **D-00005-3:** SLA alvo de revoke (15s / 60s / 15min=status quo)?
- [ ] **D-00005-4:** Webhooks nesta feature ou épico separado?

## Checklist

- [ ] ADR de claim `sid` / token_ver se necessário
- [ ] Segurança: deny-list sem PII excessiva
- [ ] Migrations planejadas
- [ ] product-integration atualizado
- [ ] Pronto para implementação (após D-00005-1 e D-00005-3)
