using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class ProductsModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;

		public ProductsModel(IAdminPortalContextService portal, IdentityDbContext db) : base(portal)
		{
			_db = db;
		}

		public List<Product> Items { get; set; } = new();

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Items = await _db.Products.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Name)
				.ToListAsync();
			return Page();
		}
	}
}
