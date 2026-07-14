using SSO.Core.Domain.Default.Samples.Entity;
using SSO.Infrastructures.Data.Default;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Default.Samples;
using System.Net;

namespace SSO.Tests.IntegrationTests.Samples
{
	[TestClass]
	public class GetSampleByIdScenarios
	{
		[TestMethod]
		public async Task GET_Sample_By_Id_Should_Return_Ok()
		{
			var contextData = SamplesCollections.GetDefaultCollection();

			using (var client = ServerHelper.Create().SetupData<DefaultDbContext, Sample>(contextData).CreateClient())
			{
				var response = await client.GetAsync($"/api/samples/{SamplesCollections.FromInt(1)}");

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}
	}
}
