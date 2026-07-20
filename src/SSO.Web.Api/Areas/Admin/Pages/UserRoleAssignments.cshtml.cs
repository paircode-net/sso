using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.UserRoleAssignments.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class UserRoleAssignmentsModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public UserRoleAssignmentsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<UserRoleAssignment> Items { get; set; } = new();
		public List<Role> Roles { get; set; } = new();
		public List<Product> Products { get; set; } = new();
		public List<Branch> Branches { get; set; } = new();

		[BindProperty]
		public Guid UserId { get; set; }

		[BindProperty]
		public Guid RoleId { get; set; }

		[BindProperty]
		public Guid ProductId { get; set; }

		[BindProperty]
		public Guid? BranchId { get; set; }

		[BindProperty]
		public bool Inheritable { get; set; }

		private bool CanManage => Portal.HasPermission(SsoAdminPermissions.Org) || Portal.IsPlatformAdmin;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			var organizationId = Portal.OrganizationId;
			var cmd = AdminWrap.FromAnonymous<PostUserRoleAssignmentCommand>(new
			{
				userId = UserId,
				roleId = RoleId,
				organizationId,
				branchId = BranchId,
				productId = ProductId,
				inheritable = Inheritable
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Atribuição criada.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!CanManage)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteUserRoleAssignmentCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Atribuição removida.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Roles = await _reader.Query<Role>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			Products = await _reader.Query<Product>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();

			if (Portal.OrganizationId is Guid orgId)
			{
				Branches = await _reader.Query<Branch>().AsNoTracking()
					.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
					.OrderBy(x => x.Name)
					.ToListAsync();

				Items = await _reader.Query<UserRoleAssignment>().AsNoTracking()
					.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
					.OrderByDescending(x => x.CreatedAt)
					.Take(200)
					.ToListAsync();
			}
			else if (Portal.IsPlatformAdmin)
			{
				Items = await _reader.Query<UserRoleAssignment>().AsNoTracking()
					.Where(x => !x.IsDeleted && x.OrganizationId == null)
					.OrderByDescending(x => x.CreatedAt)
					.Take(200)
					.ToListAsync();
			}
		}
	}
}
