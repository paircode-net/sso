# Business

## Escopo atual observável

O repositório chama-se **SSO**, mas o código implementado hoje é um **scaffold CQRS** com o aggregate de exemplo **Sample**.

### Regras de negócio inferíveis do código (Sample)

| Regra | Evidência |
|-------|-----------|
| Sample possui descrição obrigatória | Validator / mapping (`Required`, length 128) |
| Descrição não pode duplicar outra existente | `SampleDescriptionAlreadyExistsSpecification` + validators de create/update |
| Identificador é Guid | `DomainEntity<Guid>`, rotas `{id:Guid}` |
| Criação retorna HTTP 201 | `PostSampleCommandHandler` → `HttpStatusCode.Created` |
| Seed inicial com 15 samples | Migration DML `InitialDMLMigrationDefaultDbContext` |

Não há documentação formal de domínio SSO (login, tokens, applications, tenants, etc.) no repositório.

## Público / atores

**A definir.**

## Capacidades de produto esperadas (SSO)

**A definir.** Exemplos típicos (não confirmados neste repo): autenticação, autorização, emissão de tokens, gestão de clientes, consentimento, etc.

## Idioma / UX de mensagens

- Cultura padrão da API: `pt-BR`
- Testes suportam também `en-US`
- Mensagens de operação bem-sucedida usam localizer (ex.: `"Successful operation!"` via recursos)

## Fontes de verdade de negócio

Enquanto não houver docs de domínio oficiais:

1. Especificações e validators no Domain
2. Testes (`*Scenarios`) que codificam expectativas
3. Forge (estrutura de entidades)

Qualquer regra SSO real deve ser esclarecida com o product owner / equipe — **não inventar**.
