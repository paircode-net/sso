using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.OrganizationInvites.Commands;
using SSO.Core.Application.Identity.OrganizationInvites.Queries;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/organization-invites")]
	[RequiresPermission(SsoAdminPermissions.Org, SsoAdminPermissions.Platform)]
	public sealed class OrganizationInvitesController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetOrganizationInvitesByFilterQueryResponse>> Get(
			GetOrganizationInvitesByFilterQuery request,
			CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostOrganizationInviteCommandResponse>> Post(
			PostOrganizationInviteCommand request,
			CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPatch("{id:Guid}/cancel")]
		public async Task<ActionResult<PatchCancelOrganizationInviteCommandResponse>> PatchCancel(
			PatchCancelOrganizationInviteCommand request,
			CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
