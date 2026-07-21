using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Web.Api.Controllers
{
	public sealed class AuthorizationController : Controller
	{
		private readonly IOpenIddictApplicationManager _applicationManager;
		private readonly IOpenIddictAuthorizationManager _authorizationManager;
		private readonly IOpenIddictScopeManager _scopeManager;
		private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;
		private readonly TokenClaimsFactory _tokenClaimsFactory;
		private readonly IdentityDbContext _dbContext;
		private readonly IUserSessionService _sessionService;
		private readonly IAuthAuditService _auditService;
		private readonly IEffectivePermissionsResolver _permissionsResolver;
		private readonly IAdminPortalContextService _portalContext;

		public AuthorizationController(
			IOpenIddictApplicationManager applicationManager,
			IOpenIddictAuthorizationManager authorizationManager,
			IOpenIddictScopeManager scopeManager,
			SignInManager<User> signInManager,
			UserManager<User> userManager,
			TokenClaimsFactory tokenClaimsFactory,
			IdentityDbContext dbContext,
			IUserSessionService sessionService,
			IAuthAuditService auditService,
			IEffectivePermissionsResolver permissionsResolver,
			IAdminPortalContextService portalContext)
		{
			_applicationManager = applicationManager;
			_authorizationManager = authorizationManager;
			_scopeManager = scopeManager;
			_signInManager = signInManager;
			_userManager = userManager;
			_tokenClaimsFactory = tokenClaimsFactory;
			_dbContext = dbContext;
			_sessionService = sessionService;
			_auditService = auditService;
			_permissionsResolver = permissionsResolver;
			_portalContext = portalContext;
		}

		[HttpGet("~/connect/authorize")]
		[HttpPost("~/connect/authorize")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> Authorize()
		{
			var request = HttpContext.GetOpenIddictServerRequest()
				?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

			var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
			if (!result.Succeeded)
			{
				return Challenge(
					authenticationSchemes: IdentityConstants.ApplicationScheme,
					properties: new AuthenticationProperties
					{
						RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
							Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
					});
			}

			var user = await _userManager.GetUserAsync(result.Principal)
				?? throw new InvalidOperationException("The user details cannot be retrieved.");

			var application = await _applicationManager.FindByClientIdAsync(request.ClientId!)
				?? throw new InvalidOperationException("The application details cannot be found.");

			var metadata = await _dbContext.AuthClientMetadata.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == request.ClientId);
			if (metadata is not null && !metadata.IsEnabled)
			{
				return Forbid(
					authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
					properties: new AuthenticationProperties(new Dictionary<string, string?>
					{
						[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
						[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
							"The client application is disabled."
					}));
			}

			var subject = await _userManager.GetUserIdAsync(user);
			var clientAppId = await _applicationManager.GetIdAsync(application);
			var policy = metadata?.RequireConsent ?? AuthClientConsentPolicies.Never;
			var rememberDays = metadata?.ConsentRememberDays > 0 ? metadata.ConsentRememberDays : 180;
			var authorizations = new List<object>();
			await foreach (var existingAuthorization in _authorizationManager.FindAsync(
				subject: subject,
				client: clientAppId,
				status: Statuses.Valid,
				type: AuthorizationTypes.Permanent,
				scopes: request.GetScopes()))
			{
				var created = await _authorizationManager.GetCreationDateAsync(existingAuthorization);
				if (AuthClientConsentEvaluator.IsRememberExpired(policy, created, rememberDays, DateTime.UtcNow))
				{
					continue;
				}

				authorizations.Add(existingAuthorization);
			}

			var needsConsent = AuthClientConsentEvaluator.ShouldPrompt(policy, authorizations.Count > 0);

			if (needsConsent)
			{
				var returnUrl = Request.PathBase + Request.Path + QueryString.Create(
					Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList());
				return RedirectToPage(
					"/Account/Consent",
					new
					{
						returnUrl,
						clientId = request.ClientId,
						scope = string.Join(' ', request.GetScopes())
					});
			}

			var consentType = await _applicationManager.GetConsentTypeAsync(application);
			if (authorizations.Count is 0
				&& consentType == ConsentTypes.External
				&& policy == AuthClientConsentPolicies.First)
			{
				return Forbid(
					authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
					properties: new AuthenticationProperties(new Dictionary<string, string?>
					{
						[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
						[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
							"The logged in user is not allowed to access this client application."
					}));
			}

			var organizationId = await ResolveDefaultOrganizationIdAsync(user.Id);
			var principal = await _tokenClaimsFactory.CreateUserPrincipalAsync(
				user,
				request.GetScopes(),
				request.ClientId,
				organizationId,
				branchId: null,
				existingSessionId: null);

			var resources = new List<string>();
			await foreach (var resource in _scopeManager.ListResourcesAsync(principal.GetScopes()))
			{
				resources.Add(resource);
			}
			principal.SetResources(resources);

			var authorizationEntry = authorizations.LastOrDefault();
			authorizationEntry ??= await _authorizationManager.CreateAsync(
				principal: principal,
				subject: subject,
				client: clientAppId!,
				type: AuthorizationTypes.Permanent,
				scopes: principal.GetScopes());

			principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorizationEntry));
			principal.SetDestinations(GetDestinations);

			return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
		}

		[HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
		public async Task<IActionResult> Exchange()
		{
			var request = HttpContext.GetOpenIddictServerRequest()
				?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

			if (!string.IsNullOrWhiteSpace(request.ClientId))
			{
				var meta = await _dbContext.AuthClientMetadata.AsNoTracking()
					.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == request.ClientId);
				if (meta is not null && !meta.IsEnabled)
				{
					return Forbid(
						authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
						properties: new AuthenticationProperties(new Dictionary<string, string?>
						{
							[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
							[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
								"The client application is disabled."
						}));
				}
			}

			if (request.IsClientCredentialsGrantType())
			{
				var application = await _applicationManager.FindByClientIdAsync(request.ClientId!)
					?? throw new InvalidOperationException("The application details cannot be found.");

				var principal = _tokenClaimsFactory.CreateClientPrincipal(
					request.ClientId!,
					await _applicationManager.GetDisplayNameAsync(application),
					request.GetScopes());

				SsoAuthMetrics.RecordTokenIssued("client_credentials");
				return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
			}

			if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
			{
				var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
				var userId = result.Principal?.GetClaim(Claims.Subject);
				var user = userId is null ? null : await _userManager.FindByIdAsync(userId);
				if (user is null)
				{
					return Forbid(
						authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
						properties: new AuthenticationProperties(new Dictionary<string, string?>
						{
							[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
							[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
								"The token is no longer valid."
						}));
				}

				if (!await _signInManager.CanSignInAsync(user))
				{
					return Forbid(
						authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
						properties: new AuthenticationProperties(new Dictionary<string, string?>
						{
							[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
							[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
								"The user is no longer allowed to sign in."
						}));
				}

				Guid? organizationId = TryParseGuid(result.Principal?.GetClaim(SsoClaimTypes.OrganizationId))
					?? await ResolveDefaultOrganizationIdAsync(user.Id);
				Guid? branchId = TryParseGuid(result.Principal?.GetClaim(SsoClaimTypes.BranchId));
				var existingSessionId = TryParseGuid(result.Principal?.GetClaim(SsoClaimTypes.SessionId));

				if (existingSessionId is Guid sid
					&& await _sessionService.IsSessionRevokedAsync(sid))
				{
					return Forbid(
						authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
						properties: new AuthenticationProperties(new Dictionary<string, string?>
						{
							[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
							[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
								"The session has been revoked."
						}));
				}

				var principal = await _tokenClaimsFactory.CreateUserPrincipalAsync(
					user,
					request.GetScopes().Any() ? request.GetScopes() : result.Principal!.GetScopes(),
					request.ClientId,
					organizationId,
					branchId,
					existingSessionId);

				principal.SetDestinations(GetDestinations);
				SsoAuthMetrics.RecordTokenIssued(
					request.IsAuthorizationCodeGrantType() ? "authorization_code" : "refresh_token");
				return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
			}

			if (request.GrantType == SsoGrantTypes.SwitchContext)
			{
				return await HandleSwitchContextAsync(request);
			}

			throw new InvalidOperationException("The specified grant type is not supported.");
		}

		[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
		[HttpGet("~/connect/userinfo")]
		public async Task<IActionResult> Userinfo()
		{
			var userId = User.GetClaim(Claims.Subject);
			var user = userId is null ? null : await _userManager.FindByIdAsync(userId);
			if (user is null)
			{
				return Challenge(
					authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
					properties: new AuthenticationProperties(new Dictionary<string, string?>
					{
						[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
						[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
							"The specified access token is bound to an account that no longer exists."
					}));
			}

			var claims = new Dictionary<string, object>(StringComparer.Ordinal)
			{
				[Claims.Subject] = await _userManager.GetUserIdAsync(user)
			};

			if (User.HasScope(Scopes.Email))
			{
				claims[Claims.Email] = await _userManager.GetEmailAsync(user) ?? string.Empty;
				claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
			}

			if (User.HasScope(Scopes.Profile))
			{
				claims[Claims.Name] = await _userManager.GetUserNameAsync(user) ?? string.Empty;
				claims[Claims.PreferredUsername] = await _userManager.GetUserNameAsync(user) ?? string.Empty;
			}

			var organizationId = User.GetClaim(SsoClaimTypes.OrganizationId);
			if (!string.IsNullOrWhiteSpace(organizationId))
			{
				claims[SsoClaimTypes.OrganizationId] = organizationId;
			}

			var branchId = User.GetClaim(SsoClaimTypes.BranchId);
			if (!string.IsNullOrWhiteSpace(branchId))
			{
				claims[SsoClaimTypes.BranchId] = branchId;
			}

			claims[SsoClaimTypes.Permissions] = User.GetClaims(SsoClaimTypes.Permissions).ToArray();

			return Ok(claims);
		}

		[HttpGet("~/connect/logout")]
		[HttpPost("~/connect/logout")]
		public async Task<IActionResult> Logout()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is not null)
			{
				await _sessionService.RevokeAllForUserAsync(user.Id, "connect.logout");
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.Logout,
					AuthAuditOutcomes.Success,
					userId: user.Id,
					email: user.Email,
					ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()));
			}

			await _signInManager.SignOutAsync();
			await _portalContext.ClearContextAsync();
			return SignOut(
				authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
				properties: new AuthenticationProperties
				{
					RedirectUri = "/"
				});
		}

		private async Task<IActionResult> HandleSwitchContextAsync(OpenIddictRequest request)
		{
			var result = await HttpContext.AuthenticateAsync(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
			if (!result.Succeeded)
			{
				result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
			}

			if (!result.Succeeded)
			{
				SsoAuthMetrics.RecordSwitchContextFailure("invalid_token");
				return Forbid(
					authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
					properties: new AuthenticationProperties(new Dictionary<string, string?>
					{
						[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
						[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
							"The access token is missing or invalid."
					}));
			}

			var userId = result.Principal?.GetClaim(Claims.Subject);
			var user = userId is null ? null : await _userManager.FindByIdAsync(userId);
			if (user is null)
			{
				SsoAuthMetrics.RecordSwitchContextFailure("user_not_found");
				return Forbid(
					authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
					properties: new AuthenticationProperties(new Dictionary<string, string?>
					{
						[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
						[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
							"The token is no longer valid."
					}));
			}

			if (!Guid.TryParse(request.GetParameter(SsoClaimTypes.OrganizationId)?.ToString(), out var organizationId))
			{
				SsoAuthMetrics.RecordSwitchContextFailure("missing_organization");
				return Forbid(
					authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
					properties: new AuthenticationProperties(new Dictionary<string, string?>
					{
						[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidRequest,
						[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
							"organization_id is required."
					}));
			}

			Guid? branchId = null;
			var branchRaw = request.GetParameter(SsoClaimTypes.BranchId)?.ToString();
			if (!string.IsNullOrWhiteSpace(branchRaw))
			{
				if (!Guid.TryParse(branchRaw, out var parsedBranch))
				{
					SsoAuthMetrics.RecordSwitchContextFailure("invalid_branch");
					return Forbid(
						authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
						properties: new AuthenticationProperties(new Dictionary<string, string?>
						{
							[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidRequest,
							[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
								"branch_id is invalid."
						}));
				}

				branchId = parsedBranch;

				var branchOk = await _dbContext.Branches.AsNoTracking().AnyAsync(x =>
					!x.IsDeleted && x.Id == branchId.Value && x.OrganizationId == organizationId);
				if (!branchOk)
				{
					SsoAuthMetrics.RecordSwitchContextFailure("branch_mismatch");
					return Forbid(
						authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
						properties: new AuthenticationProperties(new Dictionary<string, string?>
						{
							[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidRequest,
							[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
								"branch_id does not belong to the requested organization."
						}));
				}
			}

			var hasMembership = await _dbContext.Memberships.AsNoTracking().AnyAsync(x =>
				!x.IsDeleted && x.UserId == user.Id && x.OrganizationId == organizationId);

			if (!hasMembership)
			{
				var platformPerms = await _permissionsResolver.ResolveAsync(
					user.Id,
					organizationId: null,
					branchId: null,
					SsoClients.AdminApiClientId);
				var isPlatform = platformPerms.Any(p =>
					string.Equals(p, SsoAdminPermissions.Platform, StringComparison.OrdinalIgnoreCase));

				if (!isPlatform)
				{
					SsoAuthMetrics.RecordSwitchContextFailure("no_membership");
					return Forbid(
						authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
						properties: new AuthenticationProperties(new Dictionary<string, string?>
						{
							[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
							[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
								"The user does not belong to the requested organization."
						}));
				}
			}

			var scopes = request.GetScopes().Any()
				? request.GetScopes()
				: result.Principal!.GetScopes();

			var existingSessionId = TryParseGuid(result.Principal?.GetClaim(SsoClaimTypes.SessionId));
			if (existingSessionId is Guid sid
				&& await _sessionService.IsSessionRevokedAsync(sid))
			{
				SsoAuthMetrics.RecordSwitchContextFailure("session_revoked");
				return Forbid(
					authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
					properties: new AuthenticationProperties(new Dictionary<string, string?>
					{
						[OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
						[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
							"The session has been revoked."
					}));
			}

			var principal = await _tokenClaimsFactory.CreateUserPrincipalAsync(
				user,
				scopes,
				request.ClientId ?? result.Principal!.GetClaim(Claims.ClientId) ?? result.Principal.GetClaim("client_id"),
				organizationId,
				branchId,
				existingSessionId);

			principal.SetDestinations(GetDestinations);
			SsoAuthMetrics.RecordSwitchContextSuccess();
			SsoAuthMetrics.RecordTokenIssued(SsoGrantTypes.SwitchContext);
			return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
		}

		private async Task<Guid?> ResolveDefaultOrganizationIdAsync(Guid userId)
		{
			return await _dbContext.Memberships
				.AsNoTracking()
				.Where(x => !x.IsDeleted && x.UserId == userId)
				.OrderBy(x => x.CreatedAt)
				.Select(x => (Guid?)x.OrganizationId)
				.FirstOrDefaultAsync();
		}

		private static Guid? TryParseGuid(string? value)
			=> Guid.TryParse(value, out var guid) ? guid : null;

		private static IEnumerable<string> GetDestinations(Claim claim)
		{
			switch (claim.Type)
			{
				case Claims.Name or Claims.PreferredUsername or Claims.Email:
					yield return Destinations.AccessToken;
					yield return Destinations.IdentityToken;
					yield break;

				default:
					yield return Destinations.AccessToken;
					yield break;
			}
		}
	}
}
