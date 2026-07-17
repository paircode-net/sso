# Release Checklist

> Feature 00001 — Fase 6 + feature 00010 (observabilidade / CI/CD).

## Build e testes

- [x] `dotnet build` da solution sem erros
- [x] `dotnet test` (`SSO.Tests`) verde
- [x] Migrations IdentityDb aplicáveis
- [x] CI PR gate: `.github/workflows/ci.yml` → `scripts/ci/build.sh` + `test.sh`

## Configuração

- [x] Connection strings por ambiente (Development LocalDB; Production via env/Key Vault)
- [x] Seção `Sso:*` documentada (`phase6-hardening.md`, `product-integration.md`, `cicd.md`)
- [x] Cultura `pt-BR` (+ `en-US` testes)
- [x] `Sso:Observability:*` (exporter plugável)
- [x] Signing prod: Key Vault (`Sso:Signing:KeyVaultUri` + `KeyVaultCertificateName`)

## Segurança

- [x] AuthN/AuthZ revisados (Identity + OpenIddict + permissions JWT)
- [x] Swagger só em Development
- [x] AuthZ em endpoints admin (`sso.admin.*` / 00002)
- [x] Entra homologável via config; secrets fora do git
- [x] Redaction de Authorization/senhas/tokens nos logs

## Operação

- [x] P-004: Production **não** auto-migra por default; pipeline `scripts/ci/migrate.sh`
- [x] Rollback: reverter migration + redeploy artefato anterior
- [x] Observabilidade APM — P-002 / OTEL + exporters (00010)
- [x] Health checks — `/health/live` + `/health/ready`
- [x] Runbook — `.ai/PLAYBOOK/observability-runbook.md`

## CI/CD

- [x] Pipeline — P-003 / GitHub Actions + scripts agnósticos (00010)
- [x] Artefato versionado — imagem Docker (`Dockerfile`) + SHA tag no CD
- [x] Aprovação de release — Environment Production com reviewers (F00010-D4)

## Comunicação

- [x] CONTEXT / Decisions atualizados (P-002/P-003 Aceitos; F00010-D1..D5)
- [x] Doc IdP/hardening: `.ai/CONTEXT/phase6-hardening.md`
- [x] Doc CI/CD: `.ai/CONTEXT/cicd.md`
