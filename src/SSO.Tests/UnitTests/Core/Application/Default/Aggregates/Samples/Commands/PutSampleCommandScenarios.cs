using SSO.Core.Application.Default.Samples.Commands;
using SSO.Infrastructures.Data.Default;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Default;
using SSO.Tests.Helpers.Data.Default.Samples;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using SSO.Core.Domain.Default.Samples.Entity;

namespace SSO.Tests.UnitTests.Core.Application.Default.Samples.Commands
{
	[TestClass]
	public class PutSampleCommandScenarios
	{
		[TestMethod]
		public async Task PATCH_Sample_Should_Return_Ok()
		{
			var contextData = SamplesCollections.GetDefaultCollection();

			using (var context = DefaultDbContextExtensions.GetInMemoryDefaultDbContext().SetupSamples(contextData))
			{
				var writer = new DefaultDbContextWriter(context);

				var mockedLogger = new Mock<ILoggerFactory>();

				var mockedMediator = new Mock<IMediator>();

				var localizer = GenericHelper.CreateLocalizer<Sample>();

				var handler = new PutSampleCommandHandler(
					mockedLogger.Object,
					mockedMediator.Object,
					localizer,
					writer);

				var command = new PutSampleCommand();

				command.Project(model =>
				{
					model.Id = SamplesCollections.FromInt(1);
					model.Description = "Sample - 001 [put]";
				});

				var result = await handler.Handle(command, default);

				Assert.AreEqual((long)HttpStatusCode.OK, result.StatusCode);
			}
		}
	}
}
