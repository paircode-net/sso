using SSO.Core.Domain.Default.Samples.Entity;
using SSO.Infrastructures.Data.Default;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Default.Samples;
using System.Net;

namespace SSO.Tests.IntegrationTests.Samples
{
	[TestClass]
	public class GetSamplesByFilterScenarios
	{
		[TestMethod]
		public async Task GET_Samples_Should_Return_Ok()
		{
			var contextData = SamplesCollections.GetDefaultCollection();

			using (var client = ServerHelper.Create().SetupData<DefaultDbContext, Sample>(contextData).CreateClient())
			{
				var response = await client.GetAsync($"/api/default/samples");

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}
	}
}
