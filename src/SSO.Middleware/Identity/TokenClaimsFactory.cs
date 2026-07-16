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
		private readonly IEffectiveClaimsResolver _claimsResolver;
		private readonly IClaimPolicyVersionProvider _claimVersionProvider;
		private readonly IUserSessionService _sessionService;

		public TokenClaimsFactory(
			UserManager<User> userManager,
			IEffectivePermissionsResolver permissionsResolver,
			IPermissionPolicyVersionProvider policyVersionProvider,
			IEffectiveClaimsResolver claimsResolver,
			IClaimPolicyVersionProvider claimVersionProvider,
			IUserSessionService sessionService)
		{
			_userManager = userManager;
			_permissionsResolver = permissionsResolver;
			_policyVersionProvider = policyVersionProvider;
			_claimsResolver = claimsResolver;
			_claimVersionProvider = claimVersionProvider;
			_sessionService = sessionService;
		}

		public async Task<ClaimsPrincipal> CreateUserPrincipalAsync(
			User user,
			IEnumerable<string> scopes,
			string? clientId,
			Guid? organizationId,
			Guid? branchId,
			Guid? existingSessionId = null,
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

			var session = await _sessionService.EnsureSessionAsync(
				user.Id,
				clientId,
				organizationId,
				branchId,
				existingSessionId,
				cancellationToken);
			identity.SetClaim(SsoClaimTypes.SessionId, session.Id.ToString("D"));

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

			var typedClaims = await _claimsResolver.ResolveAsync(
				user.Id,
				organizationId,
				branchId,
				clientId,
				cancellationToken);

			foreach (var pair in typedClaims)
			{
				identity.SetClaim(TypedClaimNames.ToJwtType(pair.Key), pair.Value);
			}

			var claimVer = await _claimVersionProvider.GetVersionAsync(cancellationToken);
			identity.SetClaim(SsoClaimTypes.ClaimVersion, claimVer);

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
			if (claim.Type.StartsWith(TypedClaimNames.Prefix, StringComparison.OrdinalIgnoreCase))
			{
				return new[] { Destinations.AccessToken };
			}

			return claim.Type switch
			{
				Claims.Name or Claims.PreferredUsername
					=> new[] { Destinations.AccessToken, Destinations.IdentityToken },
				Claims.Email
					=> new[] { Destinations.AccessToken, Destinations.IdentityToken },
				SsoClaimTypes.OrganizationId or SsoClaimTypes.BranchId or SsoClaimTypes.Permissions
					or SsoClaimTypes.PermissionVersion or SsoClaimTypes.ClaimVersion or SsoClaimTypes.SessionId
					=> new[] { Destinations.AccessToken },
				_ => new[] { Destinations.AccessToken }
			};
		}
	}
}
