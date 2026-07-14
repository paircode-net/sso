using MediatR;

namespace BAYSOFT.Web.Blazor.Extensions
{
	public static class IServiceScopeFactoryExtensions
	{
		public static async Task<TResponse> SendInNewScope<TResponse>(this IServiceScopeFactory serviceScopeFactory, IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(request, cancellationToken);
			}
		}
	}
}