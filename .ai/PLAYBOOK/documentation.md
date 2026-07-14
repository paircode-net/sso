# Playbook — Documentation

## Propósito

Manter a base `.ai/` e docs alinhadas ao código.

## Princípios

- Documentação pequena, modular e evolutiva.
- Fatos observáveis > hipóteses.
- Lacunas explicitadas como “A definir” / “Pendente de definição”.
- Decisões importantes viram Decision/ADR.

## Boas práticas

- Atualizar `.ai/CONTEXT/` quando stack, módulos ou integrações mudarem.
- Usar templates em `.ai/TEMPLATES/` para planos e ADRs.
- Registrar decisões em `.ai/CONTEXT/Decisions.md` e/ou ADR.
- Manter `README.md` da raiz enxuto; detalhes de engenharia ficam em `.ai/`.

## Padrões obrigatórios

- Ordem de consulta: `AI_MANIFEST.md` → SYSTEM_PROMPT → PLAYBOOK → CONTEXT → WORK → README → código.
- `SYSTEM_PROMPT.md` **não** contém domínio do projeto.
- Não inventar regras de negócio na documentação.
- Não alterar docs de arquitetura sem confirmação do usuário quando o impacto for estrutural.

## Exemplos

```text
Novo aggregate → atualizar CONTEXT/modules.md + eventualmente glossary.md
Nova integração HTTP → CONTEXT/integrations.md + template integration.md
Mudança de stack → CONTEXT/stack.md + dependencies.md
```

## Anti-patterns

- Docs que descrevem o ideal futuro como se fosse presente.
- Duplicar o mesmo conteúdo em vários arquivos sem link.
- README da raiz virar dump completo da arquitetura.
- Deixar TODOs de código sem refletir lacunas em CONTEXT quando forem estruturais.
