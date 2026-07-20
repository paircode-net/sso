using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Users.Commands;
using SSO.Core.Application.Identity.Users.Queries;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class UsersModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public UsersModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<User> Items { get; set; } = new();
		public User? Detail { get; set; }

		[BindProperty(SupportsGet = true)]
		public Guid? DetailId { get; set; }

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		[BindProperty]
		public string UserName { get; set; } = string.Empty;

		[BindProperty]
		public string Password { get; set; } = string.Empty;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PostUserCommand>(new { email = Email, userName = UserName, password = Password });
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Usuário criado."))
			{
				Email = string.Empty;
				UserName = string.Empty;
				Password = string.Empty;
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			var filter = new GetUsersByFilterQuery();
			var listResponse = await _mediator.Send(filter);
			if (AdminWrap.IsSuccess(listResponse) && listResponse.Data is IEnumerable<User> users)
			{
				Items = users.OrderBy(x => x.UserName).Take(200).ToList();
			}
			else
			{
				Items = await _reader.Query<User>().AsNoTracking()
					.Where(x => !x.IsDeleted)
					.OrderBy(x => x.UserName)
					.Take(200)
					.ToListAsync();
			}

			if (DetailId is Guid id)
			{
				var byId = AdminWrap.FromAnonymous<GetUserByIdQuery>(new { id });
				var detailResponse = await _mediator.Send(byId);
				if (AdminWrap.IsSuccess(detailResponse) && detailResponse.Data is User user)
				{
					Detail = user;
				}
			}
		}
	}
}
