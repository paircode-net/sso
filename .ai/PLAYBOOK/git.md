# Playbook — Git

## Propósito

Manter histórico claro e revisável.

## Princípios

- Commits pequenos e com intenção clara.
- Não commitir artefatos de build, secrets ou arquivos locais.
- Branch/PR alinhados ao escopo da mudança.

## Boas práticas

- Mensagens focadas no *porquê*.
- Separar refactor de feature quando possível.
- Incluir testes e docs no mesmo conjunto quando a mudança exige.
- Respeitar `.gitignore` (bin/obj, user secrets, etc.).

## Padrões obrigatórios

- Não fazer commit sem pedido explícito do responsável pela tarefa (quando o fluxo for assistido por IA).
- Não usar `--no-verify` / force push em main/master sem autorização explícita.
- Não commitir `appsettings` com segredos reais.
- Codegen Forge / migrations devem entrar versionados quando fizerem parte da feature.

## Exemplos

```text
OK: "Add uniqueness validation for Sample description"
OK: "Document CQRS conventions in .ai CONTEXT"
NÃO: "WIP", "fix", "updates" sem contexto
```

## Anti-patterns

- Commits misturando formatação massiva + feature.
- Incluir `bin/`, `obj/`, `.vs/`.
- Reescrever histórico compartilhado sem acordo.
