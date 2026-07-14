# Test Plan — {{feature/bug}}

## Objetivo

{{o que garantir}}

## Escopo

- Inclui:
- Exclui:

## Matriz de cenários

| ID | Tipo | Cenário | Resultado esperado |
|----|------|---------|--------------------|
| U1 | Unit Domain | | |
| U2 | Unit Application | | |
| I1 | Integration | | |

## Dados de teste

- Collections/helpers:
- Seeds necessários:

## Ambiente

- Unit: mocks / InMemory conforme padrão existente
- Integration: `ServerHelper` + `TestStartup` + InMemory
- Auth: **estado atual sem auth** — documentar se o cenário assumir anônimo

## Rotas HTTP (se integração)

Usar rota real do controller, ex.: `/api/default/samples`.

## Riscos de flaky tests

- 

## Critérios de conclusão

- [ ] Cenários da matriz implementados
- [ ] Suite relevante verde
- [ ] Regressão coberta para bugs
