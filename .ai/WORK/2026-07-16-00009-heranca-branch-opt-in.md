# Feature Plan — 00009 Herança de autorização Branch (opt-in)

> Arquivo: `.ai/WORK/2026-07-16-00009-heranca-branch-opt-in.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Pronto para implementação** (D-00009-1..4 aceitas; ADR-008 criado)  
> Data: 2026-07-16  
> Depende de: ADR-004 (default); motor `EffectivePermissionsResolver` / `EffectiveClaimsResolver` (00008)  
> Relaciona: 00008 (claims com a mesma política)

## Objetivo

Permitir **herança opcional e explícita** de permissões e claims tipadas de Branch pai → filhos na árvore estrutural, **sem** tornar herança automática global — preservando o default seguro do MVP (match exato).

## Contexto

- `Branch.ParentBranchId` já modela hierarquia estrutural.
- ADR-004: match **exato** de BranchId; org-wide (`BranchId` null) soma no branch ativo.
- Orgs com muitas filiais sofrem com explosão de `UserRoleAssignment`.

## Decisões aceitas

- [x] **D-00009-1:** Opt-in **Org + assignment** — `Organization.BranchAuthzInheritance` (`Off`\|`InheritFromAncestors`, default Off) **e** `Inheritable` no `UserRoleAssignment` / `UserClaimAssignment`. Herança só se ambos permitirem.
- [x] **D-00009-2:** Claims tipadas herdam com a **mesma política** que permissions.
- [x] **D-00009-3:** Claims tipadas — herança clássica: valor no branch ativo vence; se ausente, sobe ancestrais `Inheritable`. Permissions = união de códigos.
- [x] **D-00009-4:** **ADR-008** aditivo; ADR-004 permanece default (superseded parcialmente quando flag On).

## Escopo

### Inclui

- Flag Organization + `Inheritable` nos assignments (roles e user claim assignments).
- Default = **Off** (compatível ADR-004 / tokens atuais).
- Quando On: resolver permissions no `branch_id` ativo unindo org-wide + exact + ancestrais inheritable.
- Claims tipadas: mesma flag; precedência D3.
- Validação acíclica de `ParentBranchId`.
- Testes de não-regressão: flag Off ≡ MVP.
- ADR-008 + nota em ADR-004; `product-integration.md`; seed demo (HQ → Filial, default Off).

### Fora de escopo

- Herança filho → pai.
- Herança entre Organizations.
- Deny rules / permissões negativas.
- UI de visualização de árvore completa (portal 00003 depois).

## Abordagem

### Algoritmo (quando habilitado)

```text
permissions = BranchId null (org-wide)
            ∪ BranchId == active
            ∪ Inheritable && BranchId ∈ ancestors(active)
→ Distinct permission codes

claims: valor no ativo (user > role) se existir;
        senão walk ancestrais Inheritable até achar
```

- Ancestrais via walk `ParentBranchId` (rejeitar ciclos).
- Cache por (user, org, branch, product, policy_ver) se performance exigir.

### Fases

1. Domain flags + validação árvore + migration.
2. Resolvers (permissions + claims) + testes matriz.
3. Docs + seed demo.

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | `Organization`, `UserRoleAssignment`, `UserClaimAssignment`, Branch cycle spec |
| Services | `EffectivePermissionsResolver`, `EffectiveClaimsResolver` |
| Data | Migration flags |
| Tests | Matriz herança + regressão ADR-004 |
| Docs | ADR-008, product-integration, business.md |

## Critérios de aceite

- [ ] Default Off: testes Fase 3 / 00008 de não-herança continuam verdes.
- [ ] On + assignment inheritable no pai: filho recebe permissions/claims no JWT após switch_context.
- [ ] Claim no filho sobrescreve ancestral; ancestral só preenche se filho não definir.
- [ ] Ciclo de ParentBranchId rejeitado na escrita.
- [ ] Documentação e ADR-008 refletem o comportamento.

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Surpresa de authz ampliada | Default Off; audit quando flag ligada |
| Ciclos na árvore | Spec + teste |
| Divergência permissions vs claims | Mesma flag/política (D2) |
| Breaking silencioso | Feature flag + testes regressão obrigatórios |

## Estratégia de testes

- [ ] Unit resolver: Off / On / org-wide / exact / ancestor
- [ ] Unit claims: filho vence / ancestral preenche buraco
- [ ] Unit: reject cyclic parent
- [ ] Integração JWT switch_context matriz HQ/Filial
- [ ] Performance smoke profundidade 5

## Checklist

- [x] D-00009-1..4 aceitas
- [x] ADR-008 criado; ADR-004 anotado
- [ ] Migrations planejadas
- [ ] Não-regressão MVP garantida nos testes
- [ ] CONTEXT atualizado
- [x] Pronto para implementação
