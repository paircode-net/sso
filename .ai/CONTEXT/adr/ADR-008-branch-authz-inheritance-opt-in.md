# ADR-008 — Herança opt-in de autorização Branch (pai → filhos)

- Status: Aceito
- Data: 2026-07-16
- Decisores: produto SSO (feature 00009)

## Contexto

ADR-004 estabeleceu match **exato** de Branch no MVP (sem herança pai→filho). Organizações com muitas filiais sofrem explosão de `UserRoleAssignment`. Precisamos de herança **opcional e explícita**, sem mudar o default seguro.

Feature 00008 já emite claims tipadas (`sso_c_*`) com o mesmo match exato; a política de herança deve cobrir permissions **e** claims para evitar divergência.

## Decisão

1. **Default permanece Off** — comportamento idêntico ao ADR-004 (match exato + org-wide `BranchId` null).
2. **Opt-in em duas camadas (F00009-D1):**
   - `Organization.BranchAuthzInheritance` = `Off` | `InheritFromAncestors` (default `Off`).
   - Assignment marcar `Inheritable=true` para propagar aos descendentes.
   - Herança só ocorre quando **ambos** permitem.
3. **Mesma política para claims tipadas (F00009-D2)** — `EffectivePermissionsResolver` e `EffectiveClaimsResolver` compartilham a regra.
4. **Precedência de claims tipadas (F00009-D3):** herança clássica — se o branch ativo (ou org-wide / user override no ativo) **define** o código, esse valor vence; senão, sobe pelos ancestrais `Inheritable` até achar. Permissions continuam **união** de códigos.
5. Este ADR é **aditivo** a ADR-004 (não o substitui por completo). ADR-004 descreve o default; este descreve o caminho On.

### Algoritmo (quando Org = InheritFromAncestors)

```text
permissions = org-wide (BranchId null)
            ∪ exact (BranchId == active)
            ∪ inheritable assignments where BranchId ∈ ancestors(active)

claims tipadas:
  1. Resolver valor no branch ativo (user > role, 00008)
  2. Se código ausente no ativo → walk ancestrais (Inheritable) até achar
  3. Org-wide aplica como hoje
```

- Ancestrais via walk `ParentBranchId`; ciclos rejeitados na escrita de Branch.
- RoleClaims do papel herdado via `UserRoleAssignment` ancestral `Inheritable` entram no passo de ancestral.

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| Só flag Organization | Simples | Tudo-ou-nada; menos controle |
| Só `Inheritable` no assignment | Granular | Sem gate org; surpresa |
| Herança automática global | Menos config | Quebra ADR-004 / surpresa de authz |
| Supersede total ADR-004 | Um doc | Apaga clareza do default MVP |

## Consequências

### Positivas

- Default seguro preservado.
- Controle fino (org + assignment).
- Permissions e claims alinhados.

### Negativas / trade-offs

- Mais campos/config e matriz de testes.
- Walk de árvore na resolução (limitar profundidade; validar aciclicidade).

## Impacto no código

- Domain: `Organization.BranchAuthzInheritance`; `UserRoleAssignment.Inheritable`; `UserClaimAssignment.Inheritable`; validação cíclica em Branch.
- Resolvers: walker de ancestrais quando flag On.
- Migration + seed demo HQ→Filial com flag Off por default.
- Docs: `product-integration.md`, `business.md`.

## Referências

- Feature 00009; F00009-D1..D4
- ADR-004 (default; superseded parcialmente quando flag On)
- ADR-005, feature 00008 (claims tipadas)
