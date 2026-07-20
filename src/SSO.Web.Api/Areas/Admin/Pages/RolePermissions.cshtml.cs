using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.RolePermissions.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class RolePermissionsModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public RolePermissionsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<RolePermission> Items { get; set; } = new();
		public List<Role> Roles { get; set; } = new();
		public List<Permission> Permissions { get; set; } = new();

		[BindProperty]
		public Guid RoleId { get; set; }

		[BindProperty]
		public Guid PermissionId { get; set; }

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

			var cmd = AdminWrap.FromAnonymous<PostRolePermissionCommand>(new { roleId = RoleId, permissionId = PermissionId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Permissão vinculada à role.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteRolePermissionCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Vínculo removido.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Roles = await _reader.Query<Role>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Code)
				.ToListAsync();
			Permissions = await _reader.Query<Permission>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Code)
				.ToListAsync();
			Items = await _reader.Query<RolePermission>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.RoleId)
				.ToListAsync();
		}
	}
}
