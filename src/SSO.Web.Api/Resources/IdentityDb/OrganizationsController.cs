using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.Organizations.Commands;
using SSO.Core.Application.Identity.Organizations.Queries;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/organizations")]
	public sealed class OrganizationsController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetOrganizationsByFilterQueryResponse>> Get(GetOrganizationsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetOrganizationByIdQueryResponse>> Get(GetOrganizationByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostOrganizationCommandResponse>> Post(PostOrganizationCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutOrganizationCommandResponse>> Put(PutOrganizationCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteOrganizationCommandResponse>> Delete(DeleteOrganizationCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
