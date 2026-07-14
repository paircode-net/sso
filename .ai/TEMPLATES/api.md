# API Design — {{recurso}}

## Recurso

- Context: {{Default | }}
- Nome: {{ }}
- Rota base: `api/{{context}}/{{resource}}`

## Endpoints

| Verb | Path | Request | Response | Auth |
|------|------|---------|----------|------|
| GET | / | Query ByFilter | 200 | {{A definir}} |
| GET | /{id} | Query ById | 200/404 | |
| POST | / | Command Post | 201 | |
| PUT | /{id} | Command Put | 200 | |
| PATCH | /{id} | Command Patch | 200 | |
| DELETE | /{id} | Command Delete | 200 | |

## Contratos / wrapping

- Usa ModelWrapper / ApplicationRequest-Response: Sim (padrão do repo)
- Propriedades suprimidas:
- Regras de filtro:

## Erros

| Situação | Status | Mensagem/localizer |
|----------|--------|--------------------|
| Validação | 400 | |
| Não encontrado | {{ }} | |

## Compatibilidade

- Breaking change? Sim/Não
- Versionamento: **A definir**

## Testes

- [ ] Integration scenarios por verb
- [ ] Rotas iguais às do controller

## Notas

- 
