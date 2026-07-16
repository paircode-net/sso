using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class AuditModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;

		public AuditModel(IAdminPortalContextService portal, IdentityDbContext db) : base(portal)
		{
			_db = db;
		}

		public List<AuthAuditEvent> Items { get; set; } = new();

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.HasPermission(SsoAdminPermissions.AuditRead))
			{
				return Forbid();
			}

			Items = await _db.AuthAuditEvents.AsNoTracking()
				.OrderByDescending(x => x.CreatedAt)
				.Take(50)
				.ToListAsync();
			return Page();
		}
	}
}
