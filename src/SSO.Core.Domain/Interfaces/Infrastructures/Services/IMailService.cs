using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Interfaces.Infrastructures.Services
{
	public interface IMailService
	{
		Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
	}
}
