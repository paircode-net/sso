using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Memberships.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class MembershipsModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public MembershipsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<Membership> Items { get; set; } = new();
		public Dictionary<Guid, string> UserLabels { get; set; } = new();

		private bool CanManage => Portal.HasPermission(SsoAdminPermissions.Org) || Portal.IsPlatformAdmin;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			if (!RequireOrgContext())
			{
				return Page();
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

			var cmd = AdminWrap.FromAnonymous<DeleteMembershipCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Membership removida.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			var orgId = Portal.OrganizationId!.Value;
			Items = await _reader.Query<Membership>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
				.OrderByDescending(x => x.CreatedAt)
				.Take(200)
				.ToListAsync();

			var userIds = Items.Select(x => x.UserId).Distinct().ToList();
			UserLabels = await _reader.Query<User>().AsNoTracking()
				.Where(x => userIds.Contains(x.Id))
				.ToDictionaryAsync(x => x.Id, x => x.Email ?? x.UserName ?? x.Id.ToString());
		}
	}
}
