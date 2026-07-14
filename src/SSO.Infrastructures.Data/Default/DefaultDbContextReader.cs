
using BAYSOFT.Abstractions.Infrastructures.Data;
using SSO.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;

namespace SSO.Infrastructures.Data.Default
{
    public sealed class DefaultDbContextReader : Reader, IDefaultDbContextReader
    {
        public DefaultDbContextReader(DefaultDbContext context)
            : base(context)
        {
        }
    }
}