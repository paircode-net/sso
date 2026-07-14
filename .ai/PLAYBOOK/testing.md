# Playbook — Testing

## Propósito

Garantir regressões controladas com MSTest + Moq + TestServer / InMemory.

## Princípios

- Testar comportamento, não detalhes irrelevantes de implementação.
- Cobrir Application, Domain e fluxos HTTP principais.
- Testes devem ser determinísticos e isolados.

## Boas práticas

- Unitários em `SSO.Tests/UnitTests/...` espelhando a estrutura de produção.
- Integração em `SSO.Tests/IntegrationTests/{Aggregate}/`.
- Usar helpers (`ServerHelper`, collections, extensions de DbContext).
- Nomear métodos de teste com cenário e resultado esperado.
- Para API: usar a **mesma rota** do controller (`api/default/samples`, não atalhos desatualizados).

## Padrões obrigatórios

- Framework: **MSTest** (`[TestClass]`, `[TestMethod]`).
- Mocking: **Moq**.
- Host de integração: `TestServer` + `TestStartup` + EF InMemory.
- Classes: `*Scenarios`.
- Cobrir happy path e falhas de regra conhecidas (ex.: descrição duplicada → BadRequest).

## Exemplos

```text
UnitTests/Core/Application/Default/Aggregates/Samples/Commands/PostSampleCommandScenarios.cs
IntegrationTests/Samples/PostSampleScenarios.cs
Helpers/Data/Default/Samples/SamplesCollections.cs
```

```csharp
[TestMethod]
public async Task POST_Samples_Should_Return_Ok()
{
    using var client = ServerHelper.Create()
        .SetupData<DefaultDbContext, Sample>(contextData)
        .CreateClient();
    var response = await client.PostAsync("/api/default/samples", content);
    Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
}
```

## Anti-patterns

- Teste de integração apontando rota diferente do controller.
- Dependência de SQL Server real em testes unitários/integração atuais.
- Asserts fracos (`IsTrue(true)`).
- Deixar stubs sem teste quando o serviço passar a ter comportamento (`MailService`).
