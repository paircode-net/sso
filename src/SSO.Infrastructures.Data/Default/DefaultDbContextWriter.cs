
using BAYSOFT.Abstractions.Infrastructures.Data;
using SSO.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;

namespace SSO.Infrastructures.Data.Default
{
    public sealed class DefaultDbContextWriter : Writer, IDefaultDbContextWriter
    {
        public DefaultDbContextWriter(DefaultDbContext context)
            : base(context)
        {
        }
    }
}