using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Shared.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Middleware.Identity
{
	public sealed class TokenClaimsFactory
	{
		private readonly UserManager<User> _userManager;
		private readonly IEffectivePermissionsResolver _permissionsResolver;
		private readonly IPermissionPolicyVersionProvider _policyVersionProvider;

		public TokenClaimsFactory(
			UserManager<User> userManager,
			IEffectivePermissionsResolver permissionsResolver,
			IPermissionPolicyVersionProvider policyVersionProvider)
		{
			_userManager = userManager;
			_permissionsResolver = permissionsResolver;
			_policyVersionProvider = policyVersionProvider;
		}

		public async Task<ClaimsPrincipal> CreateUserPrincipalAsync(
			User user,
			IEnumerable<string> scopes,
			string? clientId,
			Guid? organizationId,
			Guid? branchId,
			CancellationToken cancellationToken = default)
		{
			var identity = new ClaimsIdentity(
				authenticationType: TokenValidationParameters.DefaultAuthenticationType,
				nameType: Claims.Name,
				roleType: Claims.Role);

			identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user));
			identity.SetClaim(Claims.Email, await _userManager.GetEmailAsync(user));
			identity.SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user));
			identity.SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));

			if (organizationId is Guid orgId)
			{
				identity.SetClaim(SsoClaimTypes.OrganizationId, orgId.ToString("D"));
			}

			if (branchId is Guid branch)
			{
				identity.SetClaim(SsoClaimTypes.BranchId, branch.ToString("D"));
			}

			var permissions = await _permissionsResolver.ResolveAsync(
				user.Id,
				organizationId,
				branchId,
				clientId,
				cancellationToken);

			foreach (var permission in permissions)
			{
				identity.AddClaim(new Claim(SsoClaimTypes.Permissions, permission));
			}

			var permVer = await _policyVersionProvider.GetVersionAsync(cancellationToken);
			identity.SetClaim(SsoClaimTypes.PermissionVersion, permVer);

			identity.SetScopes(scopes);
			identity.SetAudiences(ResolveAudiences(clientId));
			identity.SetDestinations(GetDestinations);

			return new ClaimsPrincipal(identity);
		}

		public ClaimsPrincipal CreateClientPrincipal(string clientId, string? displayName, IEnumerable<string> scopes)
		{
			var identity = new ClaimsIdentity(
				authenticationType: TokenValidationParameters.DefaultAuthenticationType,
				nameType: Claims.Name,
				roleType: Claims.Role);

			identity.SetClaim(Claims.Subject, clientId);
			identity.SetClaim(Claims.Name, displayName ?? clientId);
			identity.SetScopes(scopes);
			identity.SetAudiences(ResolveAudiences(clientId));
			identity.SetDestinations(GetDestinations);

			return new ClaimsPrincipal(identity);
		}

		private static IEnumerable<string> ResolveAudiences(string? clientId)
		{
			if (!string.IsNullOrWhiteSpace(clientId))
			{
				yield return clientId;
			}
		}

		private static IEnumerable<string> GetDestinations(Claim claim)
		{
			return claim.Type switch
			{
				Claims.Name or Claims.PreferredUsername
					=> new[] { Destinations.AccessToken, Destinations.IdentityToken },
				Claims.Email
					=> new[] { Destinations.AccessToken, Destinations.IdentityToken },
				SsoClaimTypes.OrganizationId or SsoClaimTypes.BranchId or SsoClaimTypes.Permissions or SsoClaimTypes.PermissionVersion
					=> new[] { Destinations.AccessToken },
				_ => new[] { Destinations.AccessToken }
			};
		}
	}
}
