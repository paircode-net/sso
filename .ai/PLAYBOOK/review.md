# Playbook — Review

## Propósito

Padronizar o que deve ser verificado antes de concluir uma mudança.

## Princípios

- Revisar arquitetura antes de detalhes cosméticos.
- Buscar regressões e ambiguidades de negócio.
- Checklist > memória.

## Boas práticas

- Usar `.ai/CHECKLISTS/review-checklist.md` e checklists temáticos.
- Conferir se a mudança toca a camada certa.
- Validar nomes contra `coding-style.md`.
- Conferir se testes cobrem o novo comportamento e rotas corretas.
- Verificar se CONTEXT/Decisions precisam atualização.

## Padrões obrigatórios

Antes de encerrar, verificar:

1. Arquitetura e direção de dependências
2. SOLID / DRY / KISS / YAGNI
3. Segurança (auth, secrets, validação)
4. Performance básica (queries, limites)
5. Observabilidade (logs sem vazamento)
6. Testes
7. Documentação

Reportar riscos residuais explicitamente.

## Exemplos

```text
OK: PR que adiciona Command + Domain Service + Map + Controller + testes + nota em modules.md
NÃO: PR com só controller escrevendo no DbContext “porque era mais simples”
```

## Anti-patterns

- Review só de estilo.
- Ignorar TODOs novos introduzidos sem tracking.
- Aprovar quebra de contrato HTTP sem menção no plano/ADR.
