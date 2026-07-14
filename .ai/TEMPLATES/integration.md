# Integration — {{nome}}

## Objetivo

{{por que integrar}}

## Sistema externo

- Nome:
- Protocolo: HTTP | Message Bus | SMTP | Outro
- Ambiente(s):

## Contrato

- Operações:
- Auth com o externo:
- Timeouts / retries: **A definir** se não houver padrão

## Design interno

| Item | Escolha |
|------|---------|
| Interface no Domain | `I{{Service}}` em `Interfaces/Infrastructures/...` |
| Implementação | `SSO.Infrastructures.Services` |
| Registro DI | `AddDomainServicesConfigurations` / Middleware |
| Trigger | Command / Notification / Outro |

## Configuração / secrets

- Chaves de config:
- Onde vivem os segredos: **A definir**

## Falhas e resiliência

- Comportamento se o externo falhar:
- Idempotência:

## Testes

- [ ] Unit com mock da interface
- [ ] Contrato/smoke (se aplicável)

## Documentação

- [ ] Atualizar CONTEXT/integrations.md
- [ ] Atualizar dependencies.md se houver pacote novo

## Riscos

- 
