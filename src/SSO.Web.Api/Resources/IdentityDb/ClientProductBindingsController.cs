using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.ClientProductBindings.Commands;
using SSO.Core.Application.Identity.ClientProductBindings.Queries;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/clientproductbindings")]
	[RequiresPermission(SsoAdminPermissions.Platform)]
	public sealed class ClientProductBindingsController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetClientProductBindingsByFilterQueryResponse>> Get(GetClientProductBindingsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetClientProductBindingByIdQueryResponse>> Get(GetClientProductBindingByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostClientProductBindingCommandResponse>> Post(PostClientProductBindingCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutClientProductBindingCommandResponse>> Put(PutClientProductBindingCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteClientProductBindingCommandResponse>> Delete(DeleteClientProductBindingCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
