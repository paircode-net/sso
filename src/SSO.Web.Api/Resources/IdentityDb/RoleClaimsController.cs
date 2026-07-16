using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.RoleClaims.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/role-claims")]
	[IgnoreAntiforgeryToken]
	[RequiresPermission(SsoAdminPermissions.Platform)]
	public sealed class RoleClaimsController : ControllerBase
	{
		private readonly IdentityDbContext _db;

		public RoleClaimsController(IdentityDbContext db)
		{
			_db = db;
		}

		[HttpGet]
		public async Task<IActionResult> List([FromQuery] Guid? roleId, CancellationToken cancellationToken)
		{
			var query = _db.AuthRoleClaims.AsNoTracking().Where(x => !x.IsDeleted);
			if (roleId is Guid rid)
			{
				query = query.Where(x => x.RoleId == rid);
			}

			var items = await query
				.OrderBy(x => x.RoleId)
				.Select(x => new
				{
					x.Id,
					role_id = x.RoleId,
					claim_definition_id = x.ClaimDefinitionId,
					x.Value
				})
				.ToListAsync(cancellationToken);

			return Ok(items);
		}

		public sealed class CreateRequest
		{
			[Required]
			public Guid RoleId { get; set; }
			[Required]
			public Guid ClaimDefinitionId { get; set; }
			[Required]
			public string Value { get; set; } = string.Empty;
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateRequest request, CancellationToken cancellationToken)
		{
			var definition = await _db.ClaimDefinitions.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ClaimDefinitionId, cancellationToken);
			if (definition is null)
			{
				return BadRequest(new { error = "claim_definition_not_found" });
			}

			if (!ClaimValueTypes.TryValidate(definition.ValueType, request.Value, out var error))
			{
				return BadRequest(new { error });
			}

			if (!await _db.AuthRoles.AnyAsync(x => !x.IsDeleted && x.Id == request.RoleId, cancellationToken))
			{
				return BadRequest(new { error = "role_not_found" });
			}

			var exists = await _db.AuthRoleClaims.AnyAsync(
				x => !x.IsDeleted
					&& x.RoleId == request.RoleId
					&& x.ClaimDefinitionId == request.ClaimDefinitionId,
				cancellationToken);
			if (exists)
			{
				return Conflict(new { error = "role_claim_exists" });
			}

			var entity = RoleClaim.Create(request.RoleId, request.ClaimDefinitionId, request.Value);
			_db.AuthRoleClaims.Add(entity);
			await _db.SaveChangesAsync(cancellationToken);

			return Ok(new { id = entity.Id });
		}

		public sealed class UpdateRequest
		{
			[Required]
			public string Value { get; set; } = string.Empty;
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest request, CancellationToken cancellationToken)
		{
			var entity = await _db.AuthRoleClaims
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id, cancellationToken);
			if (entity is null)
			{
				return NotFound();
			}

			var definition = await _db.ClaimDefinitions.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == entity.ClaimDefinitionId, cancellationToken);
			if (definition is null)
			{
				return BadRequest(new { error = "claim_definition_not_found" });
			}

			if (!ClaimValueTypes.TryValidate(definition.ValueType, request.Value, out var error))
			{
				return BadRequest(new { error });
			}

			entity.Value = request.Value;
			entity.TouchUpdated();
			await _db.SaveChangesAsync(cancellationToken);
			return Ok(new { updated = true, id });
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
		{
			var entity = await _db.AuthRoleClaims
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id, cancellationToken);
			if (entity is null)
			{
				return NotFound();
			}

			entity.MarkDeleted();
			await _db.SaveChangesAsync(cancellationToken);
			return Ok(new { deleted = true, id });
		}
	}
}
