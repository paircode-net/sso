using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class SwitchContextModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;
		private readonly UserManager<User> _userManager;

		public SwitchContextModel(
			IAdminPortalContextService portal,
			IdentityDbContext db,
			UserManager<User> userManager) : base(portal)
		{
			_db = db;
			_userManager = userManager;
		}

		[BindProperty]
		public Guid OrganizationId { get; set; }

		[BindProperty]
		public Guid? BranchId { get; set; }

		public List<Organization> Organizations { get; set; } = new();
		public List<Branch> Branches { get; set; } = new();

		public async Task OnGetAsync()
		{
			await LoadAsync();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			try
			{
				await Portal.SwitchContextAsync(OrganizationId, BranchId);
				Message = "Contexto atualizado (switch_context server-side).";
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			await Portal.EnsureEnrichedAsync();
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return;
			}

			if (Portal.IsPlatformAdmin)
			{
				Organizations = await _db.Organizations.AsNoTracking()
					.Where(x => !x.IsDeleted)
					.OrderBy(x => x.Name)
					.ToListAsync();
			}
			else
			{
				var orgIds = await _db.Memberships.AsNoTracking()
					.Where(x => !x.IsDeleted && x.UserId == user.Id)
					.Select(x => x.OrganizationId)
					.ToListAsync();
				Organizations = await _db.Organizations.AsNoTracking()
					.Where(x => !x.IsDeleted && orgIds.Contains(x.Id))
					.OrderBy(x => x.Name)
					.ToListAsync();
			}

			var activeOrg = Portal.OrganizationId ?? Organizations.FirstOrDefault()?.Id;
			if (activeOrg is Guid orgId)
			{
				OrganizationId = orgId;
				Branches = await _db.Branches.AsNoTracking()
					.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
					.OrderBy(x => x.Name)
					.ToListAsync();
			}
		}
	}
}
