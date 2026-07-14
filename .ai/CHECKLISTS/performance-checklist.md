# Performance Checklist

## API / queries

- [ ] Listagens usam filtro/paginação (ModelWrapper)
- [ ] Max collection size respeitado (prod: até 500)
- [ ] Sem materializar coleções grandes sem necessidade
- [ ] Includes/complex loads justificados

## Escrita

- [ ] Um commit por caso de uso (salvo necessidade)
- [ ] Specs/validações não fazem scans O(n) evitáveis em tabelas grandes
- [ ] Índices/unicidade no banco para regras quentes

## Pipeline

- [ ] Notifications/light side effects não bloqueiam demais o request (avaliar se crescer)
- [ ] Logging de payload completo evitado em hot paths com dados grandes

## EF / migrations

- [ ] Migrations não fazem DML massivo sem plano
- [ ] Modelo evita colunas desnecessárias carregadas sempre

## Verificação

- [ ] Cenário crítico exercitado localmente
- [ ] Não há regressão óbvia de N+1 em novos relacionamentos
- [ ] Limites de teste vs produção não foram copiados incorretamente
