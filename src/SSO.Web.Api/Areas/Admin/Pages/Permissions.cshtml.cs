using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Permissions.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class PermissionsModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public PermissionsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<Permission> Items { get; set; } = new();

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

			var cmd = AdminWrap.FromAnonymous<PostPermissionCommand>(new { code = Code, name = Name });
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Permissão criada."))
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

			var cmd = AdminWrap.FromAnonymous<PutPermissionCommand>(new { id, code = Code, name = Name });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Permissão atualizada.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeletePermissionCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Permissão removida.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Items = await _reader.Query<Permission>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Code)
				.ToListAsync();
		}
	}
}
