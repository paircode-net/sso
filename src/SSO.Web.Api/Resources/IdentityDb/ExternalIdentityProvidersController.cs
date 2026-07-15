using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/external-identity-providers")]
	[AllowAnonymous]
	public sealed class ExternalIdentityProvidersController : ControllerBase
	{
		private readonly IdentityDbContext _dbContext;
		private readonly SsoHardeningOptions _options;

		public ExternalIdentityProvidersController(
			IdentityDbContext dbContext,
			SsoHardeningOptions options)
		{
			_dbContext = dbContext;
			_options = options;
		}

		[HttpGet]
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
				ldapStatus = _options.ExternalAuth?.Ldap?.Enabled == true ? "stub" : "disabled"
			};

			return Ok(new { providers = fromDb, runtime });
		}
	}
}
