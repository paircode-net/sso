using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.RoleClaims.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class RoleClaimsModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;

		public RoleClaimsModel(IAdminPortalContextService portal, IdentityDbContext db) : base(portal)
		{
			_db = db;
		}

		public List<RoleClaim> Items { get; set; } = new();
		public List<Role> Roles { get; set; } = new();
		public List<ClaimDefinition> Definitions { get; set; } = new();

		[BindProperty]
		public Guid RoleId { get; set; }

		[BindProperty]
		public Guid ClaimDefinitionId { get; set; }

		[BindProperty]
		public string Value { get; set; } = string.Empty;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var definition = await _db.ClaimDefinitions.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == ClaimDefinitionId);
			if (definition is null)
			{
				Error = "Definição de claim não encontrada.";
			}
			else if (!ClaimValueTypes.TryValidate(definition.ValueType, Value, out var typeError))
			{
				Error = typeError;
			}
			else if (await _db.AuthRoleClaims.AnyAsync(x => !x.IsDeleted && x.RoleId == RoleId && x.ClaimDefinitionId == ClaimDefinitionId))
			{
				Error = "Essa role já possui esse claim.";
			}
			else
			{
				try
				{
					var entity = RoleClaim.Create(RoleId, ClaimDefinitionId, Value);
					_db.AuthRoleClaims.Add(entity);
					await _db.SaveChangesAsync();
					Message = "Claim vinculado à role.";
					Value = string.Empty;
				}
				catch (Exception ex)
				{
					Error = ex.Message;
				}
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var entity = await _db.AuthRoleClaims.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id);
			if (entity is null)
			{
				Error = "Vínculo não encontrado.";
			}
			else
			{
				entity.MarkDeleted();
				await _db.SaveChangesAsync();
				Message = "Vínculo removido.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Roles = await _db.AuthRoles.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			Definitions = await _db.ClaimDefinitions.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			Items = await _db.AuthRoleClaims.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.RoleId)
				.ToListAsync();
		}
	}
}
