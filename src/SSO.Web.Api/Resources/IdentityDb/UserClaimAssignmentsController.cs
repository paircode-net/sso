using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/user-claim-assignments")]
	[IgnoreAntiforgeryToken]
	[RequiresPermission(SsoAdminPermissions.Org, SsoAdminPermissions.Platform)]
	public sealed class UserClaimAssignmentsController : ControllerBase
	{
		private readonly IdentityDbContext _db;

		public UserClaimAssignmentsController(IdentityDbContext db)
		{
			_db = db;
		}

		[HttpGet]
		public async Task<IActionResult> List(
			[FromQuery] Guid? userId,
			[FromQuery] Guid? organizationId,
			CancellationToken cancellationToken)
		{
			var query = _db.UserClaimAssignments.AsNoTracking().Where(x => !x.IsDeleted);
			if (userId is Guid uid)
			{
				query = query.Where(x => x.UserId == uid);
			}

			if (organizationId is Guid orgId)
			{
				query = query.Where(x => x.OrganizationId == orgId);
			}

			var items = await query
				.OrderByDescending(x => x.CreatedAt)
				.Select(x => new
				{
					x.Id,
					user_id = x.UserId,
					claim_definition_id = x.ClaimDefinitionId,
					x.Value,
					organization_id = x.OrganizationId,
					branch_id = x.BranchId,
					product_id = x.ProductId,
					inheritable = x.Inheritable
				})
				.ToListAsync(cancellationToken);

			return Ok(items);
		}

		public sealed class CreateRequest
		{
			[Required]
			public Guid UserId { get; set; }
			[Required]
			public Guid ClaimDefinitionId { get; set; }
			[Required]
			public string Value { get; set; } = string.Empty;
			[Required]
			public Guid ProductId { get; set; }
			public Guid? OrganizationId { get; set; }
			public Guid? BranchId { get; set; }
			public bool Inheritable { get; set; }
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

			var exists = await _db.UserClaimAssignments.AnyAsync(
				x => !x.IsDeleted
					&& x.UserId == request.UserId
					&& x.ClaimDefinitionId == request.ClaimDefinitionId
					&& x.OrganizationId == request.OrganizationId
					&& x.BranchId == request.BranchId
					&& x.ProductId == request.ProductId,
				cancellationToken);
			if (exists)
			{
				return Conflict(new { error = "assignment_exists" });
			}

			var entity = UserClaimAssignment.Create(
				request.UserId,
				request.ClaimDefinitionId,
				request.Value,
				request.ProductId,
				request.OrganizationId,
				request.BranchId,
				request.Inheritable);
			_db.UserClaimAssignments.Add(entity);
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
			var entity = await _db.UserClaimAssignments
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
			var entity = await _db.UserClaimAssignments
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
