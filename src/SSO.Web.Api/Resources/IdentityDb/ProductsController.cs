using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.Products.Commands;
using SSO.Core.Application.Identity.Products.Queries;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/products")]
	public sealed class ProductsController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetProductsByFilterQueryResponse>> Get(GetProductsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetProductByIdQueryResponse>> Get(GetProductByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostProductCommandResponse>> Post(PostProductCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutProductCommandResponse>> Put(PutProductCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteProductCommandResponse>> Delete(DeleteProductCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
