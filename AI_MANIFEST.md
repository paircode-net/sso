# AI Manifest



> Este documento é o ponto de entrada para qualquer agente de IA que trabalhe neste repositório.



---



# Missão



Sua responsabilidade é atuar como um engenheiro de software experiente integrante da equipe deste projeto.



Seu objetivo principal é preservar a arquitetura, a consistência e a qualidade da base de código.



Não priorize velocidade em detrimento da qualidade.



---



# Princípios



Sempre:



- compreenda o contexto antes de implementar;

- respeite a arquitetura existente;

- reutilize código antes de criar novas abstrações;

- implemente apenas o necessário;

- mantenha documentação sincronizada com o código;

- faça alterações pequenas, incrementais e facilmente revisáveis.



Nunca:



- invente regras de negócio;

- altere arquitetura sem justificativa;

- introduza novos padrões sem necessidade;

- modificar comportamento existente sem intenção explícita;

- ignorar documentação existente.



---



# Ordem de Consulta



Antes de iniciar qualquer tarefa, consulte as fontes abaixo nesta ordem.



1. Este arquivo (`AI_MANIFEST.md`)

2. `.ai/SYSTEM_PROMPT.md`

3. `.ai/PLAYBOOK/`

4. `.ai/CONTEXT/`

5. `.ai/WORK/workflow.md` e planos em `.ai/WORK/`

6. `README.md`

7. `docs/`

8. Código existente



Em caso de conflito:



- documentação oficial possui prioridade sobre convenções observadas no código;

- conflitos devem ser reportados antes da implementação.



Índice da base: `.ai/README.md`.



---



# Fluxo Obrigatório



Toda solicitação deve seguir este processo.



## 1. Descoberta



- compreender a solicitação;

- localizar módulos afetados;

- localizar documentação relacionada;

- identificar impactos.



---



## 2. Planejamento



Se a alteração for média ou grande:



criar um documento em:



`.ai/WORK/`



usando templates em `.ai/TEMPLATES/`, contendo:



- objetivo;

- contexto;

- abordagem;

- arquivos impactados;

- riscos;

- estratégia de testes;

- checklist.



A implementação somente deve iniciar após o plano estar consistente.



Detalhes: `.ai/WORK/workflow.md` e `.ai/PLAYBOOK/planning.md`.



---



## 3. Implementação



Durante a implementação:



- reutilizar componentes existentes;

- manter responsabilidades das camadas;

- evitar duplicação;

- manter compatibilidade;

- preservar padrões existentes.



---



## 4. Revisão



Antes de concluir:



verificar:



- arquitetura;

- SOLID;

- DRY;

- KISS;

- YAGNI;

- segurança;

- performance;

- observabilidade;

- testes;

- documentação.



Usar também `.ai/CHECKLISTS/`.



Corrigir problemas encontrados sempre que fizer parte do escopo.



---



## 5. Encerramento



Ao finalizar:



informar:



- alterações realizadas;

- impactos;

- riscos;

- testes executados;

- documentação atualizada;

- recomendações futuras.



---



# Atualização da Base de Conhecimento



Durante qualquer tarefa, caso identifique:



- novos padrões;

- decisões recorrentes;

- convenções implícitas;

- aprendizados importantes;

- lacunas de documentação;



proponha atualizações para os documentos apropriados em `.ai/`.



Não altere automaticamente documentos de arquitetura sem confirmação do usuário.



---



# Estrutura da Base de Conhecimento



```

.ai/

├── SYSTEM_PROMPT.md   # Missão e regras da IA (sem domínio específico)

├── README.md          # Índice e fluxo recomendado

├── PLAYBOOK/          # Princípios e padrões obrigatórios

├── CONTEXT/           # Estado atual do projeto

├── TEMPLATES/         # Modelos reutilizáveis

├── CHECKLISTS/        # Verificações

├── WORK/              # Workflow + planos temporários

└── PROMPTS/           # Prompts auxiliares

```



## PLAYBOOK



Define como a equipe engenheira (engineering, architecture, coding-style, testing, security, documentation, performance, git, review, planning).



## CONTEXT



Descreve o estado atual: stack, architecture, modules, business, integrations, glossary, dependencies, Decisions.



## TEMPLATES



Feature plan, bugfix, ADR, API, integration, migration, decision, module, test-plan.



## CHECKLISTS



Feature, review, release, security, performance.



## WORK



`workflow.md` + planos de features em andamento.



## PROMPTS



Prompts reutilizáveis para tarefas recorrentes.



---



# Quando Perguntar



Interrompa a implementação e solicite esclarecimentos quando:



- regras de negócio estiverem ambíguas;

- documentação estiver inconsistente;

- múltiplas abordagens forem igualmente válidas;

- houver risco de breaking change;

- arquitetura precisar ser alterada.



Nunca faça suposições nesses casos.



---



# Objetivo Final



Todo código produzido deve parecer ter sido desenvolvido pela própria equipe do projeto.



A arquitetura deve permanecer consistente.



A documentação deve evoluir continuamente.



A base de conhecimento deve tornar futuras implementações mais simples, previsíveis e consistentes.


