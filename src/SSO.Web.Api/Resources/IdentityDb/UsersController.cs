using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModelWrapper;
using SSO.Core.Application.Identity.Users.Commands;
using SSO.Core.Application.Identity.Users.Queries;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[ApiController]
	[Produces("application/json")]
	[Route("api/identity/users")]
	[RequiresPermission(SsoAdminPermissions.Platform, SsoAdminPermissions.Org)]
	public sealed class UsersController : ControllerBase
	{
		private readonly IMediator _mediator;

		public UsersController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<ActionResult<GetUsersByFilterQueryResponse>> GetByFilter(
			GetUsersByFilterQuery request,
			CancellationToken cancellationToken = default)
			=> Wrap(await _mediator.Send(request, cancellationToken));

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetUserByIdQueryResponse>> Get(GetUserByIdQuery request, CancellationToken cancellationToken = default)
			=> Wrap(await _mediator.Send(request, cancellationToken));

		[HttpPost]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<ActionResult<PostUserCommandResponse>> Post(PostUserCommand request, CancellationToken cancellationToken = default)
			=> Wrap(await _mediator.Send(request, cancellationToken));

		private ActionResult Wrap(WrapResponse response)
		{
			return new ObjectResult(response) { StatusCode = response.StatusCode };
		}
	}
}
