using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SSO.Middleware.Identity
{
	/// <summary>
	/// Enriches Admin portal User claims from session before Razor antiforgery runs.
	/// Must be registered after <c>UseSession</c> + <c>UseAuthentication</c> and before endpoints.
	/// </summary>
	public sealed class AdminPortalEnrichmentMiddleware
	{
		private readonly RequestDelegate _next;

		public AdminPortalEnrichmentMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, IAdminPortalContextService portal)
		{
			if (context.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase)
				&& context.User?.Identity?.IsAuthenticated == true)
			{
				await portal.EnsureEnrichedAsync(context.RequestAborted);
			}

			await _next(context);
		}
	}

	public static class AdminPortalEnrichmentMiddlewareExtensions
	{
		public static IApplicationBuilder UseAdminPortalEnrichment(this IApplicationBuilder app)
			=> app.UseMiddleware<AdminPortalEnrichmentMiddleware>();
	}
}
