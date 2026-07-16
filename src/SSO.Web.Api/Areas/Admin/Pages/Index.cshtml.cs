using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class IndexModel : AdminPageModel
	{
		public IndexModel(IAdminPortalContextService portal) : base(portal)
		{
		}

		public async Task OnGetAsync()
		{
			await Portal.EnsureEnrichedAsync();
		}
	}
}
