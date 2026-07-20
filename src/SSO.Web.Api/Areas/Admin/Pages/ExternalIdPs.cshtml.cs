using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.ExternalIdentityProviders.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class ExternalIdPsModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;

		public ExternalIdPsModel(IAdminPortalContextService portal, IdentityDbContext db) : base(portal)
		{
			_db = db;
		}

		public List<ExternalIdentityProvider> Items { get; set; } = new();

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostToggleEnabledAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var idp = await _db.ExternalIdentityProviders.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
			if (idp is null)
			{
				Error = "Provedor não encontrado.";
			}
			else
			{
				idp.IsEnabled = !idp.IsEnabled;
				idp.TouchUpdated();
				await _db.SaveChangesAsync();
				Message = $"Provedor {(idp.IsEnabled ? "habilitado" : "desabilitado")}.";
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostToggleJitAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var idp = await _db.ExternalIdentityProviders.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
			if (idp is null)
			{
				Error = "Provedor não encontrado.";
			}
			else
			{
				idp.AllowJitProvisioning = !idp.AllowJitProvisioning;
				idp.TouchUpdated();
				await _db.SaveChangesAsync();
				Message = $"JIT provisioning {(idp.AllowJitProvisioning ? "habilitado" : "desabilitado")}.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Items = await _db.ExternalIdentityProviders.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Code)
				.ToListAsync();
		}
	}
}
