# Playbook — Planning

## Propósito

Evitar implementação prematura e desalinhada.

## Princípios

- Descobrir → planejar → validar → implementar.
- Planos são leves, mas escritos.
- Ambiguidades de negócio bloqueiam código.

## Boas práticas

- Alterações pequenas: plano mental + checklist.
- Alterações médias/grandes: documento em `.ai/WORK/` via templates.
- Listar arquivos impactados por camada.
- Incluir estratégia de testes e riscos (auth, migration, breaking API).
- Preferir perguntas cedo a hipóteses.

## Padrões obrigatórios

Plano médio/grande deve conter:

- objetivo
- contexto
- abordagem
- arquivos impactados
- riscos
- estratégia de testes
- checklist

Templates: `feature-plan.md`, `bugfix.md`, `migration.md`, `api.md`, etc.

Não iniciar implementação até o plano estar consistente com PLAYBOOK/CONTEXT.

## Exemplos

```text
Feature "Clients CRUD":
  - Template feature-plan.md em .ai/WORK/2026-xx-clients.md
  - Seguir módulo Sample como referência
  - Incluir migration + testes unit/integration
```

## Anti-patterns

- Codificar enquanto ainda há 2 abordagens equivalentes sem decisão.
- Plano genérico (“ajustar API”) sem arquivos/camadas.
- Ignorar impacto em migrations e localização.
