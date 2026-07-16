using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.RolePermissions.Commands;
using SSO.Core.Application.Identity.RolePermissions.Queries;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/rolepermissions")]
	[RequiresPermission(SsoAdminPermissions.Platform)]
	public sealed class RolePermissionsController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetRolePermissionsByFilterQueryResponse>> Get(GetRolePermissionsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetRolePermissionByIdQueryResponse>> Get(GetRolePermissionByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostRolePermissionCommandResponse>> Post(PostRolePermissionCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutRolePermissionCommandResponse>> Put(PutRolePermissionCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteRolePermissionCommandResponse>> Delete(DeleteRolePermissionCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
