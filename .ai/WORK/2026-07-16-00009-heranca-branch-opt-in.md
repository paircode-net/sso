# Feature Plan — 00009 Herança de autorização Branch (opt-in)

> Arquivo: `.ai/WORK/2026-07-16-00009-heranca-branch-opt-in.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Refinamento**  
> Data: 2026-07-16  
> Depende de: ADR-004 (sem herança no MVP); motor `EffectivePermissionsResolver`  
> Relaciona: 00008 (claims devem seguir a mesma política de herança)

## Objetivo

Permitir **herança opcional e explícita** de permissões (e claims, se 00008 existir) de Branch pai → filhos na árvore estrutural, **sem** tornar herança automática global — preservando o default seguro do MVP (match exato).

## Contexto

- `Branch.ParentBranchId` já modela hierarquia estrutural.
- ADR-004: match **exato** de BranchId; org-wide (`BranchId` null) soma no branch ativo.
- Orgs com muitas filiais sofrem com explosão de `UserRoleAssignment`.

## Escopo

### Inclui

- Flag de política por Organization (ou por Assignment): `BranchAuthzInheritance = Off | InheritFromAncestors`.
- Default = **Off** (compatível ADR-004 / tokens atuais).
- Quando On: ao resolver permissions no `branch_id` ativo, unir assignments do branch e de **ancestrais** (pai, avô, …) até a raiz.
- Documentar ordem/precedência (união de permissions; para claims tipadas, regra de override — decisão).
- Testes de não-regressão: com flag Off, comportamento idêntico ao MVP.
- Atualizar ADR-004 (supersede parcial) ou ADR-007 “opt-in inheritance”.
- Atualizar `product-integration.md` e seed de demonstração (HQ → Filial).

### Fora de escopo

- Herança filho → pai.
- Herança entre Organizations.
- Deny rules / permissões negativas (complexidade ABAC).
- UI de visualização de árvore completa (pode ser portal 00003 depois).

## Abordagem

### Modelo de configuração

| Opção | Descrição |
|-------|-----------|
| **A. Flag na Organization** | Simples; toda a org herda ou não |
| **B. Flag no UserRoleAssignment** | `Inheritable=true` no assignment do pai |
| **C. Ambos** | Org habilita feature; assignment marca se propaga |

**Recomendação:** C (org opt-in + assignment inheritable) para controle fino.

### Algoritmo (quando habilitado)

```text
effective = assignments where BranchId is null (org-wide)
          ∪ assignments where BranchId == active
          ∪ (se inheritance on) assignments inheritable where BranchId ∈ ancestors(active)
→ Distinct permission codes
```

- Ancestrais via walk `ParentBranchId` (cuidar ciclos — spec Domain).
- Cache por (user, org, branch, product, policy_ver) se performance exigir.

### Fases

1. Decisão + ADR.
2. Domain: validação árvore acíclica; flag(s).
3. Resolver + testes matriz (pai tem, filho não tem assignment próprio → com On recebe; com Off não).
4. Docs + seed demo.

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | `Branch`, `Organization` policy, `UserRoleAssignment.Inheritable?` |
| Application | `EffectivePermissionsResolver` (+ claims resolver) |
| Data | Migration flags |
| Tests | Matriz herança + regressão ADR-004 |
| Docs | ADR novo/atualização, product-integration, business.md |

## Critérios de aceite

- [ ] Default Off: testes Fase 3 de não-herança continuam verdes.
- [ ] On + assignment inheritable no pai: filho recebe permissions no JWT após switch_context.
- [ ] Ciclo de ParentBranchId rejeitado na escrita.
- [ ] Documentação e ADR refletem o comportamento.
- [ ] Performance aceitável em árvore até profundidade D (ex. 5) e N branches (definir).

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Surpresa de authz ampliada | Default Off; audit quando flag ligada |
| Ciclos na árvore | Spec + teste |
| Divergência permissions vs claims | Mesma flag/política para ambos |
| Breaking silencioso | Feature flag + testes regressão obrigatórios |

## Estratégia de testes

- [ ] Unit resolver: Off / On / org-wide / exact / ancestor
- [ ] Unit: reject cyclic parent
- [ ] Integração JWT switch_context matriz HQ/Filial
- [ ] Performance smoke profundidade 5

## Decisões abertas

- [ ] **D-00009-1:** Flag só Organization vs Assignment inheritable vs ambos?
- [ ] **D-00009-2:** Claims tipadas (00008) herdam com a mesma regra?
- [ ] **D-00009-3:** Precedência se pai e filho definem claim tipada diferente (override filho vs union)?
- [ ] **D-00009-4:** Supersede ADR-004 ou ADR aditivo?

## Checklist

- [ ] ADR revisado/criado **antes** do código
- [ ] Migrations planejadas
- [ ] Não-regressão MVP garantida nos testes
- [ ] CONTEXT atualizado
- [ ] Pronto para implementação (após D-00009-1/4)
