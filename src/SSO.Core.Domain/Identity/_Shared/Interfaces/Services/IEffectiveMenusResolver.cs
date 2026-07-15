using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity._Context.Interfaces.Services
{
	public sealed class EffectiveMenuDto
	{
		public Guid Id { get; set; }
		public string Code { get; set; }
		public string Title { get; set; }
		public string Route { get; set; }
		public string PermissionCode { get; set; }
		public int SortOrder { get; set; }
	}

	public interface IEffectiveMenusResolver
	{
		Task<IReadOnlyList<EffectiveMenuDto>> ResolveAsync(
			Guid userId,
			Guid? organizationId,
			Guid? branchId,
			string? clientId,
			CancellationToken cancellationToken = default);
	}
}
