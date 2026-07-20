using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class LdapMapsModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;

		public LdapMapsModel(IAdminPortalContextService portal, IdentityDbContext db) : base(portal)
		{
			_db = db;
		}

		public List<LdapGroupRoleMap> Items { get; set; } = new();
		public List<Organization> Organizations { get; set; } = new();
		public List<Role> Roles { get; set; } = new();
		public List<Product> Products { get; set; } = new();

		[BindProperty]
		public Guid OrganizationId { get; set; }

		[BindProperty]
		public string GroupIdentifier { get; set; } = string.Empty;

		[BindProperty]
		public Guid RoleId { get; set; }

		[BindProperty]
		public Guid ProductId { get; set; }

		[BindProperty]
		public Guid? BranchId { get; set; }

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

			try
			{
				var map = LdapGroupRoleMap.Create(OrganizationId, GroupIdentifier, RoleId, ProductId, BranchId);
				_db.LdapGroupRoleMaps.Add(map);
				await _db.SaveChangesAsync();
				Message = "Mapeamento criado.";
				GroupIdentifier = string.Empty;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var map = await _db.LdapGroupRoleMaps.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
			if (map is null)
			{
				Error = "Mapeamento não encontrado.";
			}
			else
			{
				map.MarkDeleted();
				await _db.SaveChangesAsync();
				Message = "Mapeamento removido.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Organizations = await _db.Organizations.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
			Roles = await _db.AuthRoles.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			Products = await _db.Products.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
			Items = await _db.LdapGroupRoleMaps.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.GroupIdentifier)
				.ToListAsync();
		}
	}
}
