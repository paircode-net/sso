
using BAYSOFT.Abstractions.Infrastructures.Data;
using BAYSOFT.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;

namespace BAYSOFT.Infrastructures.Data.Default
{
    public sealed class DefaultDbContextWriter : Writer, IDefaultDbContextWriter
    {
        public DefaultDbContextWriter(DefaultDbContext context)
            : base(context)
        {
        }
    }
}