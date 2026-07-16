using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.MenuItems.Commands;
using SSO.Core.Application.Identity.MenuItems.Queries;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/menuitems")]
	[RequiresPermission(SsoAdminPermissions.Menus)]
	public sealed class MenuItemsController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetMenuItemsByFilterQueryResponse>> Get(GetMenuItemsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetMenuItemByIdQueryResponse>> Get(GetMenuItemByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostMenuItemCommandResponse>> Post(PostMenuItemCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutMenuItemCommandResponse>> Put(PutMenuItemCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteMenuItemCommandResponse>> Delete(DeleteMenuItemCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
