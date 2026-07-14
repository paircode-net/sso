# Release Checklist

> Vários itens abaixo ainda são **Pendente de definição** no projeto; marcar N/A quando não aplicável.

## Build e testes

- [ ] `dotnet build` da solution sem erros
- [ ] `dotnet test` (`SSO.Tests`) verde
- [ ] Migrations aplicáveis no ambiente alvo

## Configuração

- [ ] Connection strings por ambiente (sem secrets no git)
- [ ] Feature flags / configs novas documentadas
- [ ] Cultura/localization ok para o ambiente

## Segurança

- [ ] AuthN/AuthZ revisados para o ambiente (hoje: não implementados — bloquear se exposição indevida)
- [ ] Endpoints sensíveis não públicos sem proteção
- [ ] Swagger desabilitado ou protegido fora de Development (política **A definir**)

## Operação

- [ ] Estratégia de migration em produção definida (auto startup vs job)
- [ ] Rollback pensado
- [ ] Observabilidade mínima (logs/métricas) — **A definir** stack
- [ ] Health checks — **A definir** (não há no scaffold)

## CI/CD

- [ ] Pipeline — **A definir** (ausente no repo)
- [ ] Artefato versionado
- [ ] Aprovação de release

## Comunicação

- [ ] Notas de release / breaking changes
- [ ] CONTEXT/Decisions atualizados se necessário
