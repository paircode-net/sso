using BAYSOFT.Core.Domain.Default.Samples.Entity;
using BAYSOFT.Infrastructures.Data.Default;
using BAYSOFT.Tests.Helpers;
using BAYSOFT.Tests.Helpers.Data.Default.Samples;
using System.Net;

namespace BAYSOFT.Tests.IntegrationTests.Samples
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
				var response = await client.GetAsync($"/api/samples");

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}
	}
}
