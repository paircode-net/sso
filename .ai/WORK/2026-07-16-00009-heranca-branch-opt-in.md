# Feature Plan — 00009 Herança de autorização Branch (opt-in)

> Arquivo: `.ai/WORK/2026-07-16-00009-heranca-branch-opt-in.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Implementado** (D-00009-1..4; ADR-008)  
> Data: 2026-07-16  
> Depende de: ADR-004 (default); `EffectivePermissionsResolver` / `EffectiveClaimsResolver`  
> Relaciona: 00008 (claims com a mesma política)

## Objetivo

Permitir **herança opcional e explícita** de permissões e claims tipadas de Branch pai → filhos, preservando default seguro (match exato).

## Decisões aceitas

- [x] **D-00009-1:** Org `BranchAuthzInheritance` + assignment `Inheritable` (ambos)
- [x] **D-00009-2:** Claims tipadas com a mesma política
- [x] **D-00009-3:** Claims — ativo vence; senão sobe ancestral inheritable
- [x] **D-00009-4:** ADR-008 aditivo

## Entrega

| Item | Detalhe |
|------|---------|
| Domain | `Organization.BranchAuthzInheritance`; `Inheritable` em UserRole/UserClaim; `BranchAncestry` + cycle spec |
| Resolvers | Permissions union + claims fill-from-ancestor |
| Migration | `Phase13BranchAuthzInheritance` (default Off / false) |
| Tests | `BranchInheritanceScenarios` + regressão ADR-004 |

## Critérios de aceite

- [x] Default Off: testes de não-herança verdes
- [x] On + Inheritable: filho recebe permissions do pai
- [x] Claim no filho sobrescreve; ancestral preenche buraco
- [x] Ciclo ParentBranchId rejeitado
- [x] Docs + ADR-008

## Checklist

- [x] D-00009-1..4 + ADR-008
- [x] Migrations
- [x] Não-regressão MVP
- [x] CONTEXT atualizado
- [x] Implementado
