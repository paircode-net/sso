using SSO.Core.Application.Default.Samples.Queries;
using SSO.Infrastructures.Data.Default;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Default;
using SSO.Tests.Helpers.Data.Default.Samples;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using SSO.Core.Domain.Default.Samples.Entity;

namespace SSO.Tests.UnitTests.Core.Application.Default.Samples.Queries
{
	[TestClass]
	public class GetSamplesByFilterQueryScenarios
	{
		[TestMethod]
		public async Task GET_Samples_By_Filter_Should_Return_Ok()
		{
			var contextData = SamplesCollections.GetDefaultCollection();

			using (var context = DefaultDbContextExtensions.GetInMemoryDefaultDbContext().SetupSamples(contextData))
			{
				var reader = new DefaultDbContextReader(context);

				var mockedLogger = new Mock<ILoggerFactory>();

				var mockedMediator = new Mock<IMediator>();

				var localizer = GenericHelper.CreateLocalizer<Sample>();

				var handler = new GetSamplesByFilterQueryHandler(
					mockedLogger.Object,
					mockedMediator.Object,
					localizer,
					reader);

				var command = new GetSamplesByFilterQuery();

				var result = await handler.Handle(command, default);

				Assert.AreEqual((long)HttpStatusCode.OK, result.StatusCode);
				Assert.AreEqual(contextData.Count, result.ResultCount);
			}
		}
	}
}
