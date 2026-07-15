using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity._Context.Interfaces.Services
{
	public interface IPermissionPolicyVersionProvider
	{
		/// <summary>Opaque policy etag for JWT <c>perm_ver</c>; changes when catalog/assignments change.</summary>
		Task<string> GetVersionAsync(CancellationToken cancellationToken = default);
	}
}
