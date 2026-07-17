# Feature Plan — 00010 Observabilidade e CI/CD (P-002 / P-003)

> Arquivo: `.ai/WORK/2026-07-16-00010-observabilidade-cicd.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Implementado**  
> Data: 2026-07-16  
> Relaciona: P-004 (migrations via pipeline); release/security checklists

## Objetivo

Fechar **P-002 (observabilidade)** e **P-003 (CI/CD e deploy)** para operar o SSO em ambientes compartilhados/produção com build repetível, migrate controlado, health/ready, logs estruturados, métricas de AuthN/AuthZ e rastreamento básico de falhas.

## Entregue

### P-002 — Observabilidade

- Serilog no host (`SSO.Web.Api`) com enrichers + request logging sem headers sensíveis
- `LogRedaction` para Bearer/senhas/tokens
- OpenTelemetry metrics/traces com exporter plugável (`Console` / `Otlp` / `AzureMonitor` / `None`) — F00010-D1
- Métricas `SSO.Auth`: login, token issued, switch_context, rate_limited, JWT shape
- `/health/live` + `/health/ready` (Identity DB + signing configurado)
- Runbook: `.ai/PLAYBOOK/observability-runbook.md`

### P-003 — CI/CD

- Scripts agnósticos `scripts/ci/{build,test,migrate,pack-sdk}.{sh,ps1}` — F00010-D2
- GitHub Actions: `.github/workflows/ci.yml` (PR gate) + `cd.yml` (staging auto; prod com environment approval) — F00010-D4
- `Dockerfile` oficial host-agnóstico — F00010-D3
- Signing via Key Vault (`SigningCertificateResolver` + config provider) — F00010-D5 / D9
- Docs: `cicd.md`, release checklist atualizado

## Critérios de aceite

- [x] PR não mergeia com testes falhando (CI verde obrigatório).
- [x] Staging deploy documentado: migrate → app; AutoMigrate false verificado.
- [x] `/health/live` e `/health/ready` respondem conforme DB.
- [x] Login failure gera log estruturado **sem** senha + métrica incrementada.
- [x] P-002 e P-003 marcados Aceitos em `Decisions.md` com a stack escolhida.
- [x] Release checklist itens APM/pipeline atualizados.

## Decisões (fechadas 2026-07-16)

- [x] **D-00010-1:** OpenTelemetry com **exporter plugável por config**
- [x] **D-00010-2:** CI/CD com **steps agnósticos** + YAML fino
- [x] **D-00010-3:** **Dockerfile** como artefato oficial host-agnóstico
- [x] **D-00010-4:** **Staging automático** + **Production com aprovação manual** de ops
- [x] **D-00010-5:** **Key Vault reference** no CD / runtime (fecha D9)

## Checklist

- [x] Fechar D-00010-1..5
- [x] Alinhado a security.md / P-004
- [x] CONTEXT stack/dependencies/Decisions atualizados
- [x] release-checklist atualizado
- [x] Testes ObservabilityScenarios (111 SSO.Tests)
- [x] Implementado
