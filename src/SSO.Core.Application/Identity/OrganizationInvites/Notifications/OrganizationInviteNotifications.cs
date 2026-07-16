using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;

namespace SSO.Core.Application.Identity.OrganizationInvites.Notifications
{
	public sealed class PostOrganizationInviteNotification : INotification
	{
		public OrganizationInvite Payload { get; }
		public PostOrganizationInviteNotification(OrganizationInvite payload) => Payload = payload;
	}

	public sealed class PostOrganizationInviteNotificationHandler : INotificationHandler<PostOrganizationInviteNotification>
	{
		public Task Handle(PostOrganizationInviteNotification notification, CancellationToken cancellationToken)
			=> Task.CompletedTask;
	}

	public sealed class CancelOrganizationInviteNotification : INotification
	{
		public OrganizationInvite Payload { get; }
		public CancelOrganizationInviteNotification(OrganizationInvite payload) => Payload = payload;
	}

	public sealed class CancelOrganizationInviteNotificationHandler : INotificationHandler<CancelOrganizationInviteNotification>
	{
		public Task Handle(CancelOrganizationInviteNotification notification, CancellationToken cancellationToken)
			=> Task.CompletedTask;
	}
}
