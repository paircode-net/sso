# Release Checklist

> Feature 00001 — Fase 6. Itens N/A mantidos para CI/CD e observabilidade ainda pendentes.

## Build e testes

- [x] `dotnet build` da solution sem erros
- [x] `dotnet test` (`SSO.Tests`) verde
- [x] Migrations IdentityDb aplicáveis (`Phase6ExternalIdpsHardening` + anteriores)

## Configuração

- [x] Connection strings por ambiente (Development LocalDB; Production via env)
- [x] Seção `Sso:*` documentada (`phase6-hardening.md`, `product-integration.md`)
- [x] Cultura `pt-BR` (+ `en-US` testes)

## Segurança

- [x] AuthN/AuthZ revisados (Identity + OpenIddict + permissions JWT)
- [x] Swagger só em Development
- [ ] AuthZ em endpoints admin sensíveis — fechar antes de exposição pública ampla
- [x] Entra homologável via config; secrets fora do git

## Operação

- [x] P-004: Production **não** auto-migra por default; pipeline `dotnet ef database update`
- [x] Rollback: reverter migration + redeploy artefato anterior
- [ ] Observabilidade APM — P-002 / A definir
- [ ] Health checks — A definir

## CI/CD

- [ ] Pipeline — P-003 / A definir
- [ ] Artefato versionado
- [ ] Aprovação de release

## Comunicação

- [x] CONTEXT / Decisions atualizados (P-004 Aceito)
- [x] Doc IdP/hardening: `.ai/CONTEXT/phase6-hardening.md`
