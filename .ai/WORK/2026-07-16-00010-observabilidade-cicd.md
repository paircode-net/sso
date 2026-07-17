# Feature Plan — 00010 Observabilidade e CI/CD (P-002 / P-003)

> Arquivo: `.ai/WORK/2026-07-16-00010-observabilidade-cicd.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Pronto para implementação**  
> Data: 2026-07-16  
> Depende de: P-002 e P-003 hoje **Pendente de definição** em `Decisions.md`  
> Relaciona: P-004 (migrations via pipeline); release/security checklists

## Objetivo

Fechar **P-002 (observabilidade)** e **P-003 (CI/CD e deploy)** para operar o SSO em ambientes compartilhados/produção com build repetível, migrate controlado, health/ready, logs estruturados, métricas de AuthN/AuthZ e rastreamento básico de falhas.

## Contexto

- Release checklist: APM e pipeline ainda abertos.
- Production já não auto-migra (P-004); falta pipeline que aplique `dotnet ef database update`.
- AuthN sem métricas torna incidentes (IdP down, spike de 401, JWT grande) opacos.

## Escopo

### Inclui

**P-002 — Observabilidade**

- Logging estruturado (ex.: Serilog) com enrichers (Environment, Machine, RequestId).
- Correlação: `TraceIdentifier` / W3C `traceparent` nos logs.
- **Não** logar tokens, senhas, secrets, Authorization headers.
- Métricas mínimas (OpenTelemetry metrics ou EventCounters):
  - `sso.auth.login.success|failure`
  - `sso.auth.token.issued` (por grant_type)
  - `sso.auth.switch_context.success|failure`
  - `sso.auth.rate_limited`
  - `sso.jwt.permissions_count` / tamanho aproximado (histograma)
- Traces OTEL para requests `/connect/*` e `api/identity/*` (sampler configurável).
- Health checks: DB Identity, (opcional) availability signing cert loaded.
- Readiness vs liveness endpoints.
- Exporter: Console/OTLP em Dev; Application Insights ou OTLP collector em Prod (**decisão**).
- Runbook curto: “login failures spike”, “IdP errors”.

**P-003 — CI/CD**

- Pipeline CI: restore → build → test → (opcional) pack client SDK.
- Gate: testes obrigatórios em PR.
- CD (ambiente staging primeiro):
  - Build imagem container **ou** publish IIS/App Service (decisão).
  - Step explícito `ef database update` Identity (+ Default se necessário) com secret de connection.
  - Deploy app com `Sso:Database:AutoMigrate=false`.
- Artefatos: Dockerfile (se containers), `azure-pipelines.yml` / GitHub Actions.
- Environments: Dev (local), Staging, Production (aprovações).
- Variáveis/secrets: connection strings, Entra, signing PFX/KV refs — nunca commitados.
- Atualizar release checklist.

### Fora de escopo

- Multi-region active-active.
- Chaos engineering.
- SIEM completo / compliance formal (além de redaction básica).
- Feature flags SaaS (LaunchDarkly etc.) — flags locais `Sso:*` bastam.

## Abordagem

### Decisões de stack (fechar P-002/P-003)

| Tema | Opções | Recomendação de refinamento |
|------|--------|------------------------------|
| Logs | Serilog + OTEL | Serilog sinks + OTEL |
| APM | App Insights vs Grafana/OTEL | App Insights se Azure; OTLP genérico se multi-cloud |
| Host | App Service vs Container Apps vs IIS | Alinhar ao restante do ecossistema Baysoft |
| CI | GitHub Actions vs Azure DevOps | Onde o repo já estiver |

### Fases

| Fase | Entrega |
|------|---------|
| 10.1 | Serilog + health + redaction; sem APM vendor ainda |
| 10.2 | CI PR (build+test) |
| 10.3 | OTEL metrics/traces + exporter |
| 10.4 | CD Staging + ef migrate + checklist |
| 10.5 | Production environment com aprovação manual |

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Web / Middleware | Program/Configurations logging, health, OTEL |
| Infrastructures | Telemetry helpers; enrich audit opcional |
| Repo root | `Dockerfile`, `.github/workflows/*` ou `azure-pipelines.yml` |
| Docs | `stack.md`, `dependencies.md`, Decisions P-002/P-003 Aceitos, release-checklist |
| Tests | Smoke health; garantir que testes não dependam de APM externo |

## Critérios de aceite

- [ ] PR não mergeia com testes falhando (CI verde obrigatório).
- [ ] Staging deploy documentado: migrate → app; AutoMigrate false verificado.
- [ ] `/health/live` e `/health/ready` (ou equivalente) respondem conforme DB.
- [ ] Login failure gera log estruturado **sem** senha + métrica incrementada.
- [ ] P-002 e P-003 marcados Aceitos em `Decisions.md` com a stack escolhida.
- [ ] Release checklist itens APM/pipeline marcados ou atualizados com links.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| PII em logs | Redaction middleware; review security checklist |
| Custo APM | Sampling; métricas essenciais só |
| Migrate no CD quebra prod | Backup; migrate em job separado com gate; smoke pós-deploy |
| Dockerfile diverge do host real | Um path oficial documentado |

## Estratégia de testes

- [ ] Unit/integration existentes continuam no CI
- [ ] Teste de redaction (header Authorization não aparece)
- [ ] Smoke pipeline em staging (manual na primeira vez)
- [ ] Health ready = 503 se connection string inválida (teste opcional com factory)

## Decisões (fechadas 2026-07-16)

- [x] **D-00010-1:** OpenTelemetry com **exporter plugável por config** — Console em Dev; App Insights **ou** OTLP em Prod (sem travar vendor).
- [x] **D-00010-2:** CI/CD com **steps agnósticos** (scripts neutros `dotnet` build/test/migrate) + YAML fino conforme host do repo.
- [x] **D-00010-3:** **Dockerfile como artefato oficial host-agnóstico** (roda em Container Apps / K8s / App Service for Containers).
- [x] **D-00010-4:** **Staging automático** após CI verde; **Production com aprovação manual** de grupo de ops.
- [x] **D-00010-5:** **Key Vault reference completo no CD** nesta feature (fecha D9): assinatura via KV + managed identity.

## Checklist

- [ ] Fechar D-00010-1..3 **antes** de implementar CD
- [ ] Alinhado a security.md / P-004
- [ ] CONTEXT stack/dependencies/Decisions atualizados
- [ ] release-checklist atualizado
- [ ] Pronto para implementação
