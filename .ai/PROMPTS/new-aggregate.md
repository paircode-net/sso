# Prompt auxiliar — Novo aggregate

Use após ler `AI_MANIFEST.md`, `.ai/SYSTEM_PROMPT.md`, playbooks de architecture/coding-style/testing e `CONTEXT/modules.md`.

```text
Quero adicionar o aggregate {{Nome}} no context {{Default}}.

Siga o padrão do Sample (Domain/Application/Data/API/Tests).
Não invente regras de negócio: use apenas {{lista de regras confirmadas}}.
Id: Guid. Classes sealed. Rota api/{{context}}/{{resource}}.

Camadas de escrita (obrigatório):
- Command Handler: IsValid → Domain Service → CommitAsync → Notification (sem regra/atribuição de domínio).
- Domain Service: atribuições + ValidateEntity/ValidateDomain + Writer.
- Regras: Specifications + *SpecificationsValidator; registrar em AddValidationsConfigurations.

Entregue:
1) plano em .ai/WORK/ usando TEMPLATES/module.md + feature-plan.md
2) implementação nas camadas corretas
3) testes unit + integration com a rota real
4) atualização de CONTEXT/modules.md e Decisions se necessário
```
