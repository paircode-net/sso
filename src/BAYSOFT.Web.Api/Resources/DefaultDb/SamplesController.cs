using BAYSOFT.Core.Application.Default.Samples.Commands;
using BAYSOFT.Core.Application.Default.Samples.Queries;
using BAYSOFT.Web.Api.Abstractions.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BAYSOFT.Web.Api.Default
{
    [Produces("application/json")]
    [Route("api/default/samples")]
    public sealed class SamplesController : ResourceController
    {
		[HttpGet]
		public async Task<ActionResult<GetSamplesByFilterQueryResponse>> Get(GetSamplesByFilterQuery request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await Send(request, cancellationToken);
		}

        [HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetSampleByIdQueryResponse>> Get(GetSampleByIdQuery request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await Send(request, cancellationToken);
		}

		[HttpPost]
		public async Task<ActionResult<PostSampleCommandResponse>> Post(PostSampleCommand request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await Send(request, cancellationToken);
		}

        [HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutSampleCommandResponse>> Put(PutSampleCommand request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await Send(request, cancellationToken);
		}

        [HttpPatch("{id:Guid}")]
		public async Task<ActionResult<PatchSampleCommandResponse>> Patch(PatchSampleCommand request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await Send(request, cancellationToken);
		}

        [HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteSampleCommandResponse>> Delete(DeleteSampleCommand request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await Send(request, cancellationToken);
		}
    }
}