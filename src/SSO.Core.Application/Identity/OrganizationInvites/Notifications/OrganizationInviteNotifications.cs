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

	public sealed class PatchCancelOrganizationInviteNotification : INotification
	{
		public OrganizationInvite Payload { get; }
		public PatchCancelOrganizationInviteNotification(OrganizationInvite payload) => Payload = payload;
	}

	public sealed class PatchCancelOrganizationInviteNotificationHandler : INotificationHandler<PatchCancelOrganizationInviteNotification>
	{
		public Task Handle(PatchCancelOrganizationInviteNotification notification, CancellationToken cancellationToken)
			=> Task.CompletedTask;
	}

	public sealed class PatchResendOrganizationInviteNotification : INotification
	{
		public OrganizationInvite Payload { get; }
		public PatchResendOrganizationInviteNotification(OrganizationInvite payload) => Payload = payload;
	}

	public sealed class PatchResendOrganizationInviteNotificationHandler : INotificationHandler<PatchResendOrganizationInviteNotification>
	{
		public Task Handle(PatchResendOrganizationInviteNotification notification, CancellationToken cancellationToken)
			=> Task.CompletedTask;
	}
}
