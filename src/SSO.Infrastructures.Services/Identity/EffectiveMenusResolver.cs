using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.MenuItems.Entity;

namespace SSO.Infrastructures.Services.Identity
{
	public sealed class EffectiveMenusResolver : IEffectiveMenusResolver
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IEffectivePermissionsResolver _permissionsResolver;

		public EffectiveMenusResolver(
			IIdentityDbContextReader reader,
			IEffectivePermissionsResolver permissionsResolver)
		{
			_reader = reader;
			_permissionsResolver = permissionsResolver;
		}

		public async Task<IReadOnlyList<EffectiveMenuDto>> ResolveAsync(
			Guid userId,
			Guid? organizationId,
			Guid? branchId,
			string? clientId,
			CancellationToken cancellationToken = default)
		{
			if (organizationId is null || string.IsNullOrWhiteSpace(clientId))
			{
				return Array.Empty<EffectiveMenuDto>();
			}

			var productId = await _reader.Query<ClientProductBinding>()
				.AsNoTracking()
				.Where(x => !x.IsDeleted && x.ClientId == clientId)
				.Select(x => (Guid?)x.ProductId)
				.FirstOrDefaultAsync(cancellationToken);

			if (productId is null)
			{
				return Array.Empty<EffectiveMenuDto>();
			}

			var permissions = await _permissionsResolver.ResolveAsync(
				userId,
				organizationId,
				branchId,
				clientId,
				cancellationToken);

			if (permissions.Count == 0)
			{
				return Array.Empty<EffectiveMenuDto>();
			}

			var permissionSet = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);

			var menus = await _reader.Query<MenuItem>()
				.AsNoTracking()
				.Where(x => !x.IsDeleted && x.ProductId == productId.Value)
				.OrderBy(x => x.SortOrder)
				.ThenBy(x => x.Title)
				.Select(x => new EffectiveMenuDto
				{
					Id = x.Id,
					Code = x.Code,
					Title = x.Title,
					Route = x.Route,
					PermissionCode = x.PermissionCode,
					SortOrder = x.SortOrder
				})
				.ToListAsync(cancellationToken);

			return menus
				.Where(m => permissionSet.Contains(m.PermissionCode))
				.ToList();
		}
	}
}
