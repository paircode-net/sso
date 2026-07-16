using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/ldap-group-role-maps")]
	[IgnoreAntiforgeryToken]
	public sealed class LdapGroupRoleMapsController : ControllerBase
	{
		private readonly IdentityDbContext _db;

		public LdapGroupRoleMapsController(IdentityDbContext db)
		{
			_db = db;
		}

		[HttpGet]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<IActionResult> List(
			[FromQuery] Guid? organizationId,
			CancellationToken cancellationToken)
		{
			var query = _db.LdapGroupRoleMaps.AsNoTracking().Where(x => !x.IsDeleted);
			if (organizationId is Guid org)
			{
				query = query.Where(x => x.OrganizationId == org);
			}

			var items = await query
				.OrderBy(x => x.GroupIdentifier)
				.Select(x => new
				{
					x.Id,
					x.OrganizationId,
					x.GroupIdentifier,
					x.RoleId,
					x.ProductId,
					x.BranchId
				})
				.ToListAsync(cancellationToken);

			return Ok(items);
		}

		public sealed class CreateRequest
		{
			[Required]
			public Guid OrganizationId { get; set; }
			[Required]
			public string GroupIdentifier { get; set; } = string.Empty;
			[Required]
			public Guid RoleId { get; set; }
			[Required]
			public Guid ProductId { get; set; }
			public Guid? BranchId { get; set; }
		}

		[HttpPost]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<IActionResult> Create(
			[FromBody] CreateRequest request,
			CancellationToken cancellationToken)
		{
			var map = LdapGroupRoleMap.Create(
				request.OrganizationId,
				request.GroupIdentifier,
				request.RoleId,
				request.ProductId,
				request.BranchId);
			_db.LdapGroupRoleMaps.Add(map);
			await _db.SaveChangesAsync(cancellationToken);
			return Ok(new { map.Id });
		}

		[HttpDelete("{id:guid}")]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
		{
			var map = await _db.LdapGroupRoleMaps.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
			if (map is null)
			{
				return NotFound();
			}

			map.MarkDeleted();
			await _db.SaveChangesAsync(cancellationToken);
			return Ok(new { deleted = true });
		}
	}
}
