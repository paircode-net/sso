using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Shared.Identity;

namespace SSO.Middleware.Identity
{
	public sealed class CurrentAdminContext : ICurrentAdminContext
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public CurrentAdminContext(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public bool IsAuthenticated =>
			_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

		public bool IsPlatformAdmin =>
			HasPermission(SsoAdminPermissions.Platform);

		public Guid? OrganizationId
		{
			get
			{
				var value = _httpContextAccessor.HttpContext?.User?.FindFirst(SsoClaimTypes.OrganizationId)?.Value;
				return Guid.TryParse(value, out var id) ? id : null;
			}
		}

		public void EnsureCanAccessOrganization(Guid organizationId)
		{
			if (IsPlatformAdmin)
			{
				return;
			}

			if (OrganizationId is Guid tokenOrg && tokenOrg == organizationId)
			{
				return;
			}

			throw new UnauthorizedAccessException(
				"Caller is not allowed to act on the requested organization.");
		}

		private bool HasPermission(string code)
		{
			var user = _httpContextAccessor.HttpContext?.User;
			if (user is null)
			{
				return false;
			}

			return user.FindAll(SsoClaimTypes.Permissions)
				.Any(c => string.Equals(c.Value, code, StringComparison.OrdinalIgnoreCase));
		}
	}
}
