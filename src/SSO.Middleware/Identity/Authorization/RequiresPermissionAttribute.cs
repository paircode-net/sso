using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;
using SSO.Shared.Identity;

namespace SSO.Middleware.Identity.Authorization
{
	/// <summary>
	/// Requires an authenticated Bearer (OpenIddict validation) or test scheme, and at least one permission claim.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class RequiresPermissionAttribute : AuthorizeAttribute, IAuthorizationRequirementData
	{
		private readonly PermissionRequirement _requirement;

		public RequiresPermissionAttribute(params string[] permissions)
		{
			_requirement = new PermissionRequirement(permissions);
			AuthenticationSchemes =
				OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme + "," +
				SsoTestingAuthDefaults.SchemeName;
		}

		public IEnumerable<IAuthorizationRequirement> GetRequirements()
		{
			yield return _requirement;
		}
	}
}
