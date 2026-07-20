using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class UserClaimAssignmentsModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;

		public UserClaimAssignmentsModel(IAdminPortalContextService portal, IdentityDbContext db) : base(portal)
		{
			_db = db;
		}

		public List<UserClaimAssignment> Items { get; set; } = new();
		public List<ClaimDefinition> Definitions { get; set; } = new();
		public List<Product> Products { get; set; } = new();

		[BindProperty]
		public Guid UserId { get; set; }

		[BindProperty]
		public Guid ClaimDefinitionId { get; set; }

		[BindProperty]
		public string Value { get; set; } = string.Empty;

		[BindProperty]
		public Guid ProductId { get; set; }

		[BindProperty]
		public Guid? BranchId { get; set; }

		[BindProperty]
		public bool Inheritable { get; set; }

		private bool CanManage => Portal.HasPermission(SsoAdminPermissions.Org) || Portal.IsPlatformAdmin;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			var organizationId = Portal.OrganizationId;
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
			else if (await _db.UserClaimAssignments.AnyAsync(x =>
				!x.IsDeleted
				&& x.UserId == UserId
				&& x.ClaimDefinitionId == ClaimDefinitionId
				&& x.OrganizationId == organizationId
				&& x.BranchId == BranchId
				&& x.ProductId == ProductId))
			{
				Error = "Já existe essa atribuição de claim.";
			}
			else
			{
				try
				{
					var entity = UserClaimAssignment.Create(UserId, ClaimDefinitionId, Value, ProductId, organizationId, BranchId, Inheritable);
					_db.UserClaimAssignments.Add(entity);
					await _db.SaveChangesAsync();
					Message = "Claim atribuído ao usuário.";
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
			if (!CanManage)
			{
				return Forbid();
			}

			var entity = await _db.UserClaimAssignments.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id);
			if (entity is null)
			{
				Error = "Atribuição não encontrada.";
			}
			else
			{
				entity.MarkDeleted();
				await _db.SaveChangesAsync();
				Message = "Atribuição removida.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Definitions = await _db.ClaimDefinitions.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			Products = await _db.Products.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();

			var query = _db.UserClaimAssignments.AsNoTracking().Where(x => !x.IsDeleted);
			if (Portal.OrganizationId is Guid orgId)
			{
				query = query.Where(x => x.OrganizationId == orgId);
			}

			Items = await query.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync();
		}
	}
}
