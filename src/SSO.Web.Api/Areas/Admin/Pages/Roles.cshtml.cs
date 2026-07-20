using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Roles.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class RolesModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public RolesModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<Role> Items { get; set; } = new();

		[BindProperty(SupportsGet = true)]
		public Guid? EditId { get; set; }

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public string Name { get; set; } = string.Empty;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();

			if (EditId is Guid id)
			{
				var item = Items.FirstOrDefault(x => x.Id == id);
				if (item is not null)
				{
					Code = item.Code;
					Name = item.Name;
				}
			}

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PostRoleCommand>(new { code = Code, name = Name });
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Role criada."))
			{
				Code = string.Empty;
				Name = string.Empty;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostUpdateAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PutRoleCommand>(new { id, code = Code, name = Name });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Role atualizada.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteRoleCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Role removida.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Items = await _reader.Query<Role>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Code)
				.ToListAsync();
		}
	}
}
