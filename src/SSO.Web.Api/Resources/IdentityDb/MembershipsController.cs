using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.Memberships.Commands;
using SSO.Core.Application.Identity.Memberships.Queries;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/memberships")]
	public sealed class MembershipsController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetMembershipsByFilterQueryResponse>> Get(GetMembershipsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetMembershipByIdQueryResponse>> Get(GetMembershipByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostMembershipCommandResponse>> Post(PostMembershipCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutMembershipCommandResponse>> Put(PutMembershipCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteMembershipCommandResponse>> Delete(DeleteMembershipCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
