using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.UserRoleAssignments.Commands;
using SSO.Core.Application.Identity.UserRoleAssignments.Queries;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/userroleassignments")]
	public sealed class UserRoleAssignmentsController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetUserRoleAssignmentsByFilterQueryResponse>> Get(GetUserRoleAssignmentsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetUserRoleAssignmentByIdQueryResponse>> Get(GetUserRoleAssignmentByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostUserRoleAssignmentCommandResponse>> Post(PostUserRoleAssignmentCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutUserRoleAssignmentCommandResponse>> Put(PutUserRoleAssignmentCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteUserRoleAssignmentCommandResponse>> Delete(DeleteUserRoleAssignmentCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
