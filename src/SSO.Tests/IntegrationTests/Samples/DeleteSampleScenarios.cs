using SSO.Core.Domain.Default.Samples.Entity;
using SSO.Infrastructures.Data.Default;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Default.Samples;
using System.Net;

namespace SSO.Tests.IntegrationTests.Samples
{
	[TestClass]
	public class DeleteSampleScenarios
	{
		[TestMethod]
		public async Task DELETE_Samples_Should_Return_Ok()
		{
			var contextData = SamplesCollections.GetDefaultCollection();

			using (var client = ServerHelper.Create().SetupData<DefaultDbContext, Sample>(contextData).CreateClient())
			{
				var response = await client.DeleteAsync($"/api/default/samples/{SamplesCollections.FromInt(2)}");

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}
		[TestMethod]
		public async Task DELETE_Samples_Where_Sample_Doesnt_Exists_Should_Return_NotFound()
		{
			var contextData = SamplesCollections.GetDefaultCollection().Where(x => x.Id != SamplesCollections.FromInt(2));

			using (var client = ServerHelper.Create().SetupData<DefaultDbContext, Sample>(contextData).CreateClient())
			{
				var response = await client.DeleteAsync($"/api/default/samples/{SamplesCollections.FromInt(2)}");

				Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
			}
		}
	}
}
