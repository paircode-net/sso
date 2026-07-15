using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.Roles.Commands;
using SSO.Core.Application.Identity.Roles.Queries;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/roles")]
	public sealed class RolesController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetRolesByFilterQueryResponse>> Get(GetRolesByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetRoleByIdQueryResponse>> Get(GetRoleByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostRoleCommandResponse>> Post(PostRoleCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutRoleCommandResponse>> Put(PutRoleCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteRoleCommandResponse>> Delete(DeleteRoleCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
