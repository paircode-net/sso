using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.Permissions.Commands;
using SSO.Core.Application.Identity.Permissions.Queries;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/permissions")]
	[RequiresPermission(SsoAdminPermissions.Platform)]
	public sealed class PermissionsController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetPermissionsByFilterQueryResponse>> Get(GetPermissionsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetPermissionByIdQueryResponse>> Get(GetPermissionByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostPermissionCommandResponse>> Post(PostPermissionCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutPermissionCommandResponse>> Put(PutPermissionCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeletePermissionCommandResponse>> Delete(DeletePermissionCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
