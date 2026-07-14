using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Default;
using SSO.Tests.Helpers.Data.Default.Samples;
using MediatR;
using Moq;
using SSO.Core.Application.Default.Samples.Notifications;

namespace SSO.Tests.UnitTests.Core.Application.Default.Samples.Notifications
{
	[TestClass]
	public class PostSampleNotificationScenarios
	{
		[TestMethod]
		public async Task PostSampleNotification_Should_Not_Return_Exception()
		{
			var contextData = SamplesCollections.GetDefaultCollection();

			using (var context = DefaultDbContextExtensions.GetInMemoryDefaultDbContext().SetupSamples(contextData))
			{
				var mockedLoggerFactory = GenericHelper.MockILoggerFactory<PostSampleNotificationHandler>();

				var mockedMediator = new Mock<IMediator>();

				var handler = new PostSampleNotificationHandler(
					mockedLoggerFactory.Object,
					mockedMediator.Object);

				var entity = contextData.First();

				var notification = new PostSampleNotification(entity);

				await handler.Handle(notification, default);
			}
		}
	}
}
