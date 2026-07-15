using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.Branches.Commands;
using SSO.Core.Application.Identity.Branches.Queries;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/branches")]
	public sealed class BranchesController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetBranchesByFilterQueryResponse>> Get(GetBranchesByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetBranchByIdQueryResponse>> Get(GetBranchByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostBranchCommandResponse>> Post(PostBranchCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutBranchCommandResponse>> Put(PutBranchCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteBranchCommandResponse>> Delete(DeleteBranchCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
