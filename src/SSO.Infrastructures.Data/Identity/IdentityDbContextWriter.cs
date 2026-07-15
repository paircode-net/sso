using BAYSOFT.Abstractions.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;

namespace SSO.Infrastructures.Data.Identity
{
	public sealed class IdentityDbContextWriter : Writer, IIdentityDbContextWriter
	{
		public IdentityDbContextWriter(IdentityDbContext context)
			: base(context)
		{
		}
	}
}
