# Backlog pós-MVP — refinamentos

> Data: 2026-07-16  
> Origem: épico 00001 concluído (Fases 0–6)  
> Status: **Planejamento / refinamento** — sem implementação

Planos de feature derivados das evolutivas prioritárias após o MVP SSO.

| # | Feature | Arquivo | Prioridade sugerida (go-live) | Prioridade sugerida (plataforma) |
|---|---------|---------|-------------------------------|----------------------------------|
| 00002 | AuthZ nas APIs admin | [2026-07-16-00002-authz-apis-admin.md](2026-07-16-00002-authz-apis-admin.md) — **implementado** | P0 | P0 |
| 00003 | Portal admin por papel | [2026-07-16-00003-portal-admin-por-papel.md](2026-07-16-00003-portal-admin-por-papel.md) — **pronto p/ implementar** | P1 | P0 |
| 00004 | SDK / BFF de integração | [2026-07-16-00004-sdk-integracao-produtos.md](2026-07-16-00004-sdk-integracao-produtos.md) | P1 | P0 |
| 00005 | Revogação quente + sessão | [2026-07-16-00005-revogacao-quente-sessao.md](2026-07-16-00005-revogacao-quente-sessao.md) | P1 | P1 |
| 00006 | Federação Google + LDAP | [2026-07-16-00006-federacao-google-ldap.md](2026-07-16-00006-federacao-google-ldap.md) | P2 | P1 |
| 00007 | Consent + AuthClients | [2026-07-16-00007-consent-authclients.md](2026-07-16-00007-consent-authclients.md) | P2 | P0 |
| 00008 | Claims tipadas | [2026-07-16-00008-claims-tipadas.md](2026-07-16-00008-claims-tipadas.md) | P2 | P1 |
| 00009 | Herança Branch opt-in | [2026-07-16-00009-heranca-branch-opt-in.md](2026-07-16-00009-heranca-branch-opt-in.md) | P3 | P2 |
| 00010 | Observabilidade + CI/CD | [2026-07-16-00010-observabilidade-cicd.md](2026-07-16-00010-observabilidade-cicd.md) | P0 (ops) | P0 (ops) |

## Dependências entre features

```text
00002 AuthZ admin ──► 00003 Portal admin
        │
        └──► 00007 Consent/AuthClients (APIs protegidas)

00004 SDK ◄── contrato atual (product-integration.md)
        │
        └──► 00005 Revogação quente (cliente consome sinais)

00006 Google/LDAP ◄── Fase 6 (stubs + Entra)

00008 Claims tipadas ◄── motor EffectivePermissions (Fase 3)

00009 Herança Branch ◄── ADR-004 (revisão / ADR novo)

00010 P-002/P-003 ── transversal (não bloqueia features de domínio)
```

## Ordem recomendada (primeiro produto em produção)

1. 00002 → 00010 (mínimo operacional) → homologar Entra (ops)  
2. 00004 → 00005  
3. 00003 (MVP do portal)  
4. 00006 / 00007 conforme demanda de clientes  
5. 00008 / 00009 quando o modelo de authz exigir

Épico base: [2026-07-14-00001-plataforma-sso.md](2026-07-14-00001-plataforma-sso.md).
