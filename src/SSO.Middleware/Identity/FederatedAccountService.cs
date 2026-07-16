using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.ExternalIdentityProviders.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;

namespace SSO.Middleware.Identity
{
	public sealed class FederatedLoginOutcome
	{
		public bool Succeeded { get; init; }
		public User? User { get; init; }
		public string? Error { get; init; }
		public string Detail { get; init; } = string.Empty;

		public static FederatedLoginOutcome Fail(string error, string detail = "")
			=> new() { Succeeded = false, Error = error, Detail = detail };

		public static FederatedLoginOutcome Ok(User user, string detail)
			=> new() { Succeeded = true, User = user, Detail = detail };
	}

	/// <summary>Shared auto-link / JIT rules for OIDC + LDAP (F00006-D1/D3).</summary>
	public sealed class FederatedAccountService
	{
		private readonly UserManager<User> _userManager;
		private readonly IdentityDbContext _db;

		public FederatedAccountService(UserManager<User> userManager, IdentityDbContext db)
		{
			_userManager = userManager;
			_db = db;
		}

		public async Task<bool> IsJitAllowedAsync(string providerType, CancellationToken cancellationToken = default)
		{
			var idp = await ResolveIdpAsync(providerType, cancellationToken);
			return idp?.AllowJitProvisioning == true;
		}

		public async Task<ExternalIdentityProvider?> ResolveIdpAsync(
			string providerType,
			CancellationToken cancellationToken = default)
		{
			return await _db.ExternalIdentityProviders.AsNoTracking()
				.Where(x => !x.IsDeleted && x.IsEnabled && x.ProviderType == providerType)
				.OrderBy(x => x.OrganizationId == null ? 0 : 1)
				.FirstOrDefaultAsync(cancellationToken);
		}

		public static bool IsEmailVerifiedFromClaims(IEnumerable<Claim> claims, string loginProvider)
		{
			var verified = claims.FirstOrDefault(c => c.Type is "email_verified")?.Value;
			if (!string.IsNullOrWhiteSpace(verified))
			{
				return string.Equals(verified, "true", StringComparison.OrdinalIgnoreCase) || verified == "1";
			}

			// LDAP directory attributes and Entra are treated as verified sources when email is present.
			return string.Equals(loginProvider, AuthenticationSchemes.Ldap, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(loginProvider, AuthenticationSchemes.Entra, StringComparison.OrdinalIgnoreCase);
		}

		public async Task<FederatedLoginOutcome> ResolveOrProvisionAsync(
			string loginProvider,
			string providerKey,
			string? email,
			bool emailVerified,
			string? displayName,
			CancellationToken cancellationToken = default)
		{
			var existingLogin = await _userManager.FindByLoginAsync(loginProvider, providerKey);
			if (existingLogin is not null && !existingLogin.IsDeleted)
			{
				return FederatedLoginOutcome.Ok(existingLogin, "existing_login");
			}

			if (string.IsNullOrWhiteSpace(email))
			{
				return FederatedLoginOutcome.Fail("email_required", "missing_email");
			}

			var byEmail = await _userManager.FindByEmailAsync(email);
			if (byEmail is not null && !byEmail.IsDeleted)
			{
				if (!emailVerified)
				{
					return FederatedLoginOutcome.Fail(
						"email_not_verified",
						"auto_link_requires_verified_email");
				}

				var add = await _userManager.AddLoginAsync(
					byEmail,
					new UserLoginInfo(loginProvider, providerKey, loginProvider));
				if (!add.Succeeded && add.Errors.All(e => e.Code != "LoginAlreadyAssociated"))
				{
					return FederatedLoginOutcome.Fail(
						"link_failed",
						string.Join(";", add.Errors.Select(e => e.Description)));
				}

				return FederatedLoginOutcome.Ok(byEmail, "auto_linked");
			}

			var providerType = loginProvider switch
			{
				AuthenticationSchemes.Google => ExternalIdpTypes.Google,
				AuthenticationSchemes.Entra => ExternalIdpTypes.Entra,
				AuthenticationSchemes.Ldap => ExternalIdpTypes.Ldap,
				_ => loginProvider
			};

			if (!await IsJitAllowedAsync(providerType, cancellationToken))
			{
				return FederatedLoginOutcome.Fail(
					"user_not_provisioned",
					"jit_disabled");
			}

			if (!emailVerified)
			{
				return FederatedLoginOutcome.Fail(
					"email_not_verified",
					"jit_requires_verified_email");
			}

			var user = new User
			{
				UserName = email,
				Email = email,
				EmailConfirmed = true
			};
			user.MarkCreated();
			var create = await _userManager.CreateAsync(user);
			if (!create.Succeeded)
			{
				return FederatedLoginOutcome.Fail(
					"create_failed",
					string.Join(";", create.Errors.Select(e => e.Description)));
			}

			await _userManager.AddLoginAsync(user, new UserLoginInfo(loginProvider, providerKey, displayName ?? loginProvider));
			return FederatedLoginOutcome.Ok(user, "jit_created");
		}
	}
}
