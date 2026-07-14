# Feature Checklist

## Descoberta e plano

- [ ] Pedido compreendido (sem regras de negócio inventadas)
- [ ] CONTEXT/PLAYBOOK consultados
- [ ] Plano em `.ai/WORK/` (se médio/grande)
- [ ] Impactos por camada identificados

## Implementação

- [ ] Domain: entity/services/specs/validators conforme padrão
- [ ] Application: commands/queries/notifications
- [ ] Data: mapping + migration se necessário
- [ ] API: controller + rota `api/{context}/{resource}`
- [ ] DI: registros em Middleware quando novos tipos exigirem
- [ ] Localização: recursos/mensagens quando aplicável
- [ ] Classes `sealed` e naming conforme coding-style

## Qualidade

- [ ] Testes unitários
- [ ] Testes de integração (rotas corretas)
- [ ] Sem secrets commitados
- [ ] Auth/segurança considerados (mesmo que ainda “A definir”)
- [ ] Logs sem PII sensível

## Encerramento

- [ ] CONTEXT atualizado (modules/business/decisions/integrations)
- [ ] TODOs novos justificados ou removidos
- [ ] Checklist de review executado
- [ ] Riscos residuais comunicados
