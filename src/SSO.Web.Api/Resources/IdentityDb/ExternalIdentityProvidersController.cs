using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/external-identity-providers")]
	[IgnoreAntiforgeryToken]
	public sealed class ExternalIdentityProvidersController : ControllerBase
	{
		private readonly IdentityDbContext _dbContext;
		private readonly SsoHardeningOptions _options;

		public ExternalIdentityProvidersController(
			IdentityDbContext dbContext,
			IOptions<SsoHardeningOptions> options)
		{
			_dbContext = dbContext;
			_options = options.Value;
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> GetEnabled(CancellationToken cancellationToken)
		{
			var fromDb = await _dbContext.ExternalIdentityProviders
				.AsNoTracking()
				.Where(x => !x.IsDeleted && x.IsEnabled)
				.Select(x => new
				{
					x.Code,
					x.ProviderType,
					x.DisplayName,
					x.IsEnabled,
					x.AllowJitProvisioning,
					x.Authority,
					x.ClientId,
					x.OrganizationId
				})
				.ToListAsync(cancellationToken);

			var runtime = new
			{
				entraConfigured = _options.ExternalAuth?.Entra?.Enabled == true,
				googleConfigured = _options.ExternalAuth?.Google?.Enabled == true,
				ldapConfigured = _options.ExternalAuth?.Ldap?.Enabled == true,
				ldapStatus = _options.ExternalAuth?.Ldap?.Enabled == true ? "ready" : "disabled"
			};

			return Ok(new { providers = fromDb, runtime });
		}

		public sealed class PatchRequest
		{
			public bool? IsEnabled { get; set; }
			public bool? AllowJitProvisioning { get; set; }
		}

		[HttpPatch("{id:guid}")]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<IActionResult> Patch(
			Guid id,
			[FromBody] PatchRequest request,
			CancellationToken cancellationToken)
		{
			var idp = await _dbContext.ExternalIdentityProviders
				.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
			if (idp is null)
			{
				return NotFound();
			}

			if (request.IsEnabled is bool enabled)
			{
				idp.IsEnabled = enabled;
			}

			if (request.AllowJitProvisioning is bool jit)
			{
				idp.AllowJitProvisioning = jit;
			}

			idp.TouchUpdated();
			await _dbContext.SaveChangesAsync(cancellationToken);
			return Ok(new
			{
				idp.Id,
				idp.Code,
				idp.IsEnabled,
				idp.AllowJitProvisioning
			});
		}
	}
}
