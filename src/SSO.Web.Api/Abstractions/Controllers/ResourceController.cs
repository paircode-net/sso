using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModelWrapper;

namespace SSO.Web.Api.Abstractions.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ResourceController : ControllerBase
	{
		private IMediator? _mediator;
		protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;
		public async Task<ActionResult<TResponse>> Send<TEntity, TResponse>(ApplicationRequest<TEntity, TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
			where TEntity : DomainEntityBase
			where TResponse : ApplicationResponse<TEntity>
		{
			return WrapResult(await Mediator.Send(request, cancellationToken));
		}

		public async Task<TResponse> SendRequest<TEntity, TResponse>(ApplicationRequest<TEntity, TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
			where TEntity : DomainEntityBase
			where TResponse : ApplicationResponse<TEntity>
		{
			return await Mediator.Send(request, cancellationToken);
		}

		private ActionResult WrapResult(WrapResponse response)
		{
			var objectResult = new ObjectResult(response);

			objectResult.StatusCode = response.StatusCode;

			return objectResult;
		}
	}
}
