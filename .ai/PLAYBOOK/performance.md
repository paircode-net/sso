# Playbook — Performance

## Propósito

Evitar regressões de latência e uso de recurso no padrão CQRS + EF Core atual.

## Princípios

- Medir antes de otimizar.
- Queries e writes devem ser proporcionais ao caso de uso.
- Defaults de paginação existem — respeitá-los.

## Boas práticas

- Usar queries `ByFilter` com limites ModelWrapper (default collection size 10; max 500 em produção).
- Evitar `Include`/complex loads desnecessários (há default de load complex properties — avaliar por endpoint).
- Commitar uma vez por caso de uso (`Writer.CommitAsync` após domain service).
- Preferir filtros no servidor a materializar listas grandes em memória.

## Padrões obrigatórios

- Não remover ou elevar limites máximos de coleção sem justificativa e documentação.
- Em testes, limites podem ser mais restritos (max 100, query terms min 3) — não “corrigir” produção copiando valores de teste às cegas.
- Índices/constraints importantes vão no mapeamento EF / migrations (ex.: unicidade de negócio).

## Exemplos

```text
OK: GetSamplesByFilter respeita page size do ModelWrapper
OK: Specification de existência usa consulta pontual
NÃO: Carregar todos os Samples para validar duplicidade em memória sem necessidade
```

## Anti-patterns

- N+1 queries em novos relacionamentos.
- Logging síncrono pesado de payloads grandes em notifications.
- Migrations DML gigantes sem plano.
- Otimização prematura que quebra a separação de camadas.
