using SSO.Core.Domain.Default.Samples.Entity;
using SSO.Infrastructures.Data.Default;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Default.Samples;
using Newtonsoft.Json;
using System.Net;

namespace SSO.Tests.IntegrationTests.Samples
{
	[TestClass]
	public class PatchSampleScenarios
	{
		[TestMethod]
		public async Task PATCH_Samples_Should_Return_Ok()
		{
			var contextData = SamplesCollections.GetDefaultCollection();

			var data = new Sample { Id = SamplesCollections.FromInt(2), Description = "Sample - 002 [alt]" };

			using (var client = ServerHelper.Create().SetupData<DefaultDbContext, Sample>(contextData).CreateClient())
			{
				var json = JsonConvert.SerializeObject(data);
				HttpContent content = new StringContent(json);

				var response = await client.PatchAsync($"/api/default/samples/{data.Id}", content);

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}
		[TestMethod]
		public async Task PATCH_Samples_With_Same_Description_Should_Return_BadRequest()
		{
			var contextData = SamplesCollections.GetDefaultCollection();

			var data = new Sample { Id = SamplesCollections.FromInt(2), Description = "Sample - 001" };

			using (var client = ServerHelper.Create().SetupData<DefaultDbContext, Sample>(contextData).CreateClient())
			{
				var json = JsonConvert.SerializeObject(data);
				HttpContent content = new StringContent(json);

				var response = await client.PatchAsync($"/api/default/samples/{data.Id}", content);

				Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			}
		}
	}
}
