using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/claim-definitions")]
	[IgnoreAntiforgeryToken]
	[RequiresPermission(SsoAdminPermissions.Platform)]
	public sealed class ClaimDefinitionsController : ControllerBase
	{
		private readonly IdentityDbContext _db;

		public ClaimDefinitionsController(IdentityDbContext db)
		{
			_db = db;
		}

		[HttpGet]
		public async Task<IActionResult> List(CancellationToken cancellationToken)
		{
			var items = await _db.ClaimDefinitions.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Code)
				.Select(x => new
				{
					x.Id,
					x.Code,
					x.Name,
					x.Description,
					value_type = x.ValueType,
					product_id = x.ProductId,
					is_enabled = x.IsEnabled
				})
				.ToListAsync(cancellationToken);

			return Ok(items);
		}

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
		{
			var item = await _db.ClaimDefinitions.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id, cancellationToken);
			if (item is null)
			{
				return NotFound();
			}

			return Ok(new
			{
				item.Id,
				item.Code,
				item.Name,
				item.Description,
				value_type = item.ValueType,
				product_id = item.ProductId,
				is_enabled = item.IsEnabled
			});
		}

		public sealed class CreateRequest
		{
			[Required]
			public string Code { get; set; } = string.Empty;
			[Required]
			public string Name { get; set; } = string.Empty;
			public string? Description { get; set; }
			public string ValueType { get; set; } = ClaimValueTypes.String;
			public Guid? ProductId { get; set; }
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateRequest request, CancellationToken cancellationToken)
		{
			if (!ClaimValueTypes.IsValid(request.ValueType))
			{
				return BadRequest(new { error = "invalid_value_type" });
			}

			var code = request.Code.Trim().ToLowerInvariant();
			if (await _db.ClaimDefinitions.AnyAsync(x => !x.IsDeleted && x.Code == code, cancellationToken))
			{
				return Conflict(new { error = "code_exists" });
			}

			var entity = ClaimDefinition.Create(
				code,
				request.Name,
				request.ValueType,
				request.ProductId,
				request.Description);
			_db.ClaimDefinitions.Add(entity);
			await _db.SaveChangesAsync(cancellationToken);

			return Ok(new { id = entity.Id, code = entity.Code });
		}

		public sealed class UpdateRequest
		{
			public string? Name { get; set; }
			public string? Description { get; set; }
			public string? ValueType { get; set; }
			public Guid? ProductId { get; set; }
			public bool ClearProductId { get; set; }
			public bool? IsEnabled { get; set; }
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest request, CancellationToken cancellationToken)
		{
			var entity = await _db.ClaimDefinitions
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id, cancellationToken);
			if (entity is null)
			{
				return NotFound();
			}

			if (request.ValueType is not null && !ClaimValueTypes.IsValid(request.ValueType))
			{
				return BadRequest(new { error = "invalid_value_type" });
			}

			if (request.Name is not null)
			{
				entity.Name = request.Name;
			}

			if (request.Description is not null)
			{
				entity.Description = request.Description;
			}

			if (request.ValueType is not null)
			{
				entity.ValueType = request.ValueType.Trim().ToLowerInvariant();
			}

			if (request.ClearProductId)
			{
				entity.ProductId = null;
			}
			else if (request.ProductId is Guid pid)
			{
				entity.ProductId = pid;
			}

			if (request.IsEnabled is bool enabled)
			{
				entity.IsEnabled = enabled;
			}

			entity.TouchUpdated();
			await _db.SaveChangesAsync(cancellationToken);
			return Ok(new { updated = true, id });
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
		{
			var entity = await _db.ClaimDefinitions
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
