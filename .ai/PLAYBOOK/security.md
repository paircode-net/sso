# Playbook — Security

## Propósito

Minimizar riscos enquanto AuthN/AuthZ ainda estão pendentes de definição no produto.

## Princípios

- Least privilege e defesa em profundidade.
- Nunca commitir segredos.
- Não expor detalhe interno em respostas de erro de produção.
- Tratar autenticação/autorização como requisito explícito — não “assumir anônimo forever”.

## Boas práticas

- Validar entrada via FluentValidation / ModelWrapper / Domain specs.
- Preferir falha explícita (BadRequest / erros de domínio) a estados inconsistentes.
- Connection strings e secrets apenas em configuração de ambiente / secret store (não hardcoded).
- Ao introduzir auth: registrar em Middleware (`AddAuthentication`/`UseAuthentication`) e documentar em CONTEXT + ADR.

## Padrões obrigatórios

- Estado atual: **sem autenticação/autorização efetiva**. Hooks estão comentados em `SSO.Middleware` e não há `[Authorize]` nos controllers. Qualquer feature sensível **deve** endereçar AuthN/AuthZ antes de ir a produção.
- Não logar payloads com PII/segredos (notifications hoje logam JSON — revisar ao adicionar dados sensíveis).
- Não enfraquecer HTTPS redirection sem justificativa.
- Migrations automáticas em startup são aceitas no scaffold; em produção a estratégia deve ser definida (risco operacional).

## Exemplos

```text
OK: CreateSampleSpecificationsValidator impede descrição duplicada
OK: Secrets em User Secrets / variáveis de ambiente / Key Vault (a definir)
NÃO: Commitar token/password em appsettings.json
NÃO: Expor stack trace completo ao cliente em produção
```

## Anti-patterns

- Entregar endpoints mutáveis em ambiente compartilhado sem autenticação.
- Confiar só em validação do cliente.
- Usar InMemory/SQL com dados reais sensíveis em testes locais commitáveis.
- Habilitar auth parcialmente (só `UseAuthorization` sem scheme) e considerar “protegido”.
