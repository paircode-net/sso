using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Tests.Helpers;
using System.Net;
using System.Text;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class ProductsScenarios
	{
		[TestMethod]
		public async Task POST_Product_Should_Return_Created()
		{
			var product = new Product { Name = "HR", Code = "hr" };

			using var client = ServerHelper.Create().CreateClient();

			var response = await client.PostAsync(
				"/api/identity/products",
				new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json"));

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		}
	}
}
