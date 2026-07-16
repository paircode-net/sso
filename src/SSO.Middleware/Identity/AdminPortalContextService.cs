using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;

namespace SSO.Middleware.Identity
{
	public interface IAdminPortalContextService
	{
		Guid? OrganizationId { get; }
		Guid? BranchId { get; }
		IReadOnlyList<string> Permissions { get; }
		bool HasPermission(string code);
		bool IsPlatformAdmin { get; }
		bool IsOrgAdmin { get; }
		Task EnsureEnrichedAsync(CancellationToken cancellationToken = default);
		Task SwitchContextAsync(Guid organizationId, Guid? branchId, CancellationToken cancellationToken = default);
		Task ClearContextAsync();
	}

	public sealed class AdminPortalContextService : IAdminPortalContextService
	{
		public const string SessionOrgKey = "AdminPortal.OrganizationId";
		public const string SessionBranchKey = "AdminPortal.BranchId";

		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<User> _userManager;
		private readonly IEffectivePermissionsResolver _permissionsResolver;
		private readonly IdentityDbContext _dbContext;

		private IReadOnlyList<string> _permissions = Array.Empty<string>();
		private bool _enriched;

		public AdminPortalContextService(
			IHttpContextAccessor httpContextAccessor,
			UserManager<User> userManager,
			IEffectivePermissionsResolver permissionsResolver,
			IdentityDbContext dbContext)
		{
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
			_permissionsResolver = permissionsResolver;
			_dbContext = dbContext;
		}

		public Guid? OrganizationId
		{
			get
			{
				var raw = _httpContextAccessor.HttpContext?.Session.GetString(SessionOrgKey);
				return Guid.TryParse(raw, out var id) ? id : null;
			}
		}

		public Guid? BranchId
		{
			get
			{
				var raw = _httpContextAccessor.HttpContext?.Session.GetString(SessionBranchKey);
				return Guid.TryParse(raw, out var id) ? id : null;
			}
		}

		public IReadOnlyList<string> Permissions => _permissions;
		public bool IsPlatformAdmin => HasPermission(SsoAdminPermissions.Platform);
		public bool IsOrgAdmin => HasPermission(SsoAdminPermissions.Org);

		public bool HasPermission(string code)
			=> _permissions.Any(p => string.Equals(p, code, StringComparison.OrdinalIgnoreCase));

		public async Task EnsureEnrichedAsync(CancellationToken cancellationToken = default)
		{
			if (_enriched)
			{
				return;
			}

			var http = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("No HttpContext.");

			var user = await _userManager.GetUserAsync(http.User);
			if (user is null)
			{
				_permissions = Array.Empty<string>();
				_enriched = true;
				return;
			}

			var permissionSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var permission in await _permissionsResolver.ResolveAsync(
				user.Id,
				organizationId: null,
				branchId: null,
				SsoClients.AdminApiClientId,
				cancellationToken))
			{
				permissionSet.Add(permission);
			}

			if (OrganizationId is Guid activeOrg)
			{
				foreach (var permission in await _permissionsResolver.ResolveAsync(
					user.Id,
					activeOrg,
					BranchId,
					SsoClients.AdminApiClientId,
					cancellationToken))
				{
					permissionSet.Add(permission);
				}
			}
			else
			{
				// Bootstrap: org-admins need to enter /Admin before switch_context.
				var membershipOrgIds = await _dbContext.Memberships.AsNoTracking()
					.Where(x => !x.IsDeleted && x.UserId == user.Id)
					.Select(x => x.OrganizationId)
					.Distinct()
					.ToListAsync(cancellationToken);

				foreach (var membershipOrgId in membershipOrgIds)
				{
					foreach (var permission in await _permissionsResolver.ResolveAsync(
						user.Id,
						membershipOrgId,
						branchId: null,
						SsoClients.AdminApiClientId,
						cancellationToken))
					{
						if (permission.StartsWith("sso.admin.", StringComparison.OrdinalIgnoreCase))
						{
							permissionSet.Add(permission);
						}
					}
				}
			}

			_permissions = permissionSet.OrderBy(x => x).ToList();

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString("D")),
				new Claim("sub", user.Id.ToString("D")),
				new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
				new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.ToString("D"))
			};

			if (OrganizationId is Guid orgId)
			{
				claims.Add(new Claim(SsoClaimTypes.OrganizationId, orgId.ToString("D")));
			}

			if (BranchId is Guid branchId)
			{
				claims.Add(new Claim(SsoClaimTypes.BranchId, branchId.ToString("D")));
			}

			foreach (var permission in _permissions)
			{
				claims.Add(new Claim(SsoClaimTypes.Permissions, permission));
			}

			var identity = new ClaimsIdentity(claims, authenticationType: IdentityConstants.ApplicationScheme);
			http.User = new ClaimsPrincipal(identity);
			_enriched = true;
		}

		public async Task SwitchContextAsync(
			Guid organizationId,
			Guid? branchId,
			CancellationToken cancellationToken = default)
		{
			var http = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("No HttpContext.");
			var user = await _userManager.GetUserAsync(http.User)
				?? throw new UnauthorizedAccessException();

			var hasMembership = await _dbContext.Memberships.AsNoTracking().AnyAsync(
				x => !x.IsDeleted && x.UserId == user.Id && x.OrganizationId == organizationId,
				cancellationToken);

			var platformPerms = await _permissionsResolver.ResolveAsync(
				user.Id,
				organizationId: null,
				branchId: null,
				SsoClients.AdminApiClientId,
				cancellationToken);

			var isPlatform = platformPerms.Any(p =>
				string.Equals(p, SsoAdminPermissions.Platform, StringComparison.OrdinalIgnoreCase));

			if (!hasMembership && !isPlatform)
			{
				throw new UnauthorizedAccessException("Sem membership nesta organização.");
			}

			if (branchId is Guid bId)
			{
				var branchOk = await _dbContext.Branches.AsNoTracking().AnyAsync(
					x => !x.IsDeleted && x.Id == bId && x.OrganizationId == organizationId,
					cancellationToken);
				if (!branchOk)
				{
					throw new InvalidOperationException("Branch não pertence à organização.");
				}
			}

			http.Session.SetString(SessionOrgKey, organizationId.ToString("D"));
			if (branchId is Guid branch)
			{
				http.Session.SetString(SessionBranchKey, branch.ToString("D"));
			}
			else
			{
				http.Session.Remove(SessionBranchKey);
			}

			_enriched = false;
			await EnsureEnrichedAsync(cancellationToken);
		}

		public Task ClearContextAsync()
		{
			var session = _httpContextAccessor.HttpContext?.Session;
			session?.Remove(SessionOrgKey);
			session?.Remove(SessionBranchKey);
			_enriched = false;
			_permissions = Array.Empty<string>();
			return Task.CompletedTask;
		}
	}
}
