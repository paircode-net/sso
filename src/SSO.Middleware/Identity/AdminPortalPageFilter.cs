using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Shared.Identity;

namespace SSO.Middleware.Identity
{
	/// <summary>
	/// Cookie auth + admin permissions (resolved via admin client / switch_context session).
	/// </summary>
	public sealed class AdminPortalPageFilter : IAsyncPageFilter
	{
		private readonly IAdminPortalContextService _portalContext;

		public AdminPortalPageFilter(IAdminPortalContextService portalContext)
		{
			_portalContext = portalContext;
		}

		public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
			=> Task.CompletedTask;

		public async Task OnPageHandlerExecutionAsync(
			PageHandlerExecutingContext context,
			PageHandlerExecutionDelegate next)
		{
			if (!IsAdminArea(context))
			{
				await next();
				return;
			}

			if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
			{
				context.Result = new ChallengeResult();
				return;
			}

			await _portalContext.EnsureEnrichedAsync(context.HttpContext.RequestAborted);

			var hasAdmin = _portalContext.Permissions.Any(p =>
				p.StartsWith("sso.admin.", System.StringComparison.OrdinalIgnoreCase));

			if (!hasAdmin)
			{
				context.Result = new ForbidResult();
				return;
			}

			await next();
		}

		private static bool IsAdminArea(PageHandlerExecutingContext context)
		{
			if (context.ActionDescriptor is PageActionDescriptor page)
			{
				return string.Equals(page.AreaName, "Admin", System.StringComparison.OrdinalIgnoreCase);
			}

			return false;
		}
	}

	public abstract class AdminPageModel : PageModel
	{
		public IAdminPortalContextService Portal { get; }

		protected AdminPageModel(IAdminPortalContextService portal)
		{
			Portal = portal;
		}

		protected IActionResult RequirePermission(string permission)
		{
			if (!Portal.HasPermission(permission))
			{
				return Forbid();
			}

			return null!;
		}

		protected bool Can(string permission) => Portal.HasPermission(permission);
	}
}
