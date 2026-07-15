# Base de conhecimento AI (`.ai`)

Infraestrutura de documentação para desenvolvimento assistido por IA neste repositório.

Ponto de entrada da missão do agente: `AI_MANIFEST.md` (raiz).

## Estrutura

```text
.ai/
├── SYSTEM_PROMPT.md   # Missão e regras da IA (sem domínio específico)
├── README.md          # Este índice
├── PLAYBOOK/          # Como a equipe engenheira
├── CONTEXT/           # Estado atual do projeto
│   └── adr/           # Architecture Decision Records Aceitos
├── TEMPLATES/         # Modelos reutilizáveis
├── CHECKLISTS/        # Verificações com checkboxes
├── WORK/              # workflow.md + planos temporários
└── PROMPTS/           # Prompts auxiliares reutilizáveis
```

## Objetivo de cada área

### `SYSTEM_PROMPT.md`

Missão, processo, prioridade de fontes, regras de planejamento/documentação/implementação/revisão.  
**Não** colocar conhecimento específico do SSO/Sample aqui.

### `PLAYBOOK/`

| Arquivo | Uso |
|---------|-----|
| `engineering.md` | Disciplina de engenharia e escopo |
| `architecture.md` | Clean + CQRS e dependências |
| `coding-style.md` | Nomenclatura e estilo |
| `testing.md` | MSTest, rotas, helpers |
| `security.md` | Validação, secrets, auth |
| `documentation.md` | Como evoluir a base |
| `performance.md` | Queries, limites, EF |
| `git.md` | Commits e histórico |
| `review.md` | O que revisar |
| `planning.md` | Quando e como planejar |

**Quando atualizar:** ao mudar uma convenção da equipe (com confirmação se for arquitetural).

### `CONTEXT/`

| Arquivo | Uso |
|---------|-----|
| `stack.md` | Tecnologias |
| `architecture.md` | Camadas e fluxos atuais |
| `modules.md` | Aggregates e arquivos |
| `business.md` | Regras observáveis / lacunas |
| `integrations.md` | Sistemas externos |
| `glossary.md` | Termos do repo |
| `dependencies.md` | Pacotes e project refs |
| `Decisions.md` | Decisões e pendências |
| `adr/` | ADRs formais (Aceito / Depreciado) |

**Quando atualizar:** sempre que o estado do sistema mudar (novo módulo, pacote, integração, decisão).

### `TEMPLATES/`

Planos e designs reutilizáveis (`feature-plan`, `bugfix`, `adr`, `api`, `integration`, `migration`, `decision`, `module`, `test-plan`).

**Quando atualizar:** ao melhorar o processo; copiar para `WORK/` ao usar.

### `CHECKLISTS/`

`feature`, `review`, `release`, `security`, `performance`.

**Quando atualizar:** ao descobrir falha recorrente de processo.

### `WORK/`

- `workflow.md` — fluxo padrão Descoberta → Merge  
- Planos de features/bugs em andamento

**Quando atualizar:** a cada tarefa média/grande (`workflow.md` só se o processo mudar).

### `PROMPTS/`

Prompts auxiliares para tarefas recorrentes.

**Quando atualizar:** ao estabilizar um prompt útil da equipe.

## Fluxo recomendado

1. Ler `AI_MANIFEST.md`
2. Ler `SYSTEM_PROMPT.md` e o workflow
3. Consultar PLAYBOOK relevantes à tarefa
4. Ler CONTEXT (stack/architecture/modules no mínimo)
5. Planejar em `WORK/` se necessário
6. Implementar / testar / revisar com CHECKLISTS
7. Atualizar CONTEXT se o conhecimento mudou

## Relação com o código

Esta pasta **não** altera comportamento da aplicação.  
Código de produção permanece a fonte de verdade da implementação; `.ai` captura convenções e estado para agentes e humanos.
