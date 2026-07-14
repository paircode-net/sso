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
	public class DeleteSampleCommandScenarios
	{
		[TestMethod]
		public async Task DeleteSampleCommand_Should_Return_Ok()
		{
			var contextData = SamplesCollections.GetDefaultCollection();

			using (var context = DefaultDbContextExtensions.GetInMemoryDefaultDbContext().SetupSamples(contextData))
			{
				var writer = new DefaultDbContextWriter(context);

				var mockedLogger = new Mock<ILoggerFactory>();

				var mockedMediator = new Mock<IMediator>();

				var localizer = GenericHelper.CreateLocalizer<Sample>();

				var handler = new DeleteSampleCommandHandler(
					mockedLogger.Object,
					mockedMediator.Object,
					localizer,
					writer);

				var command = new DeleteSampleCommand();

				command.Project(model =>
				{
					model.Id = SamplesCollections.FromInt(2);
				});

				var result = await handler.Handle(command, default);

				Assert.AreEqual((long)HttpStatusCode.OK, result.StatusCode);
			}
		}
	}
}
