using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Select;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Branches.Queries
{
	public sealed class GetBranchByIdQuery : ApplicationRequest<Branch, GetBranchByIdQueryResponse>
	{
		public GetBranchByIdQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class GetBranchByIdQueryResponse : ApplicationResponse<Branch>
	{
		public GetBranchByIdQueryResponse(Tuple<int, int, WrapRequest<Branch>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetBranchByIdQueryResponse(int statusCode, WrapRequest<Branch> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetBranchByIdQueryHandler : ApplicationRequestHandler<Branch, GetBranchByIdQuery, GetBranchByIdQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetBranchByIdQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Branch> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetBranchByIdQueryResponse> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var id = request.Project(x => x.Id);
				var data = await Reader.Query<Branch>().Where(x => x.Id == id && !x.IsDeleted).Select(request).SingleOrDefaultAsync();
				if (data == null) throw new EntityNotFoundException<Branch>(Localizer);
				return new GetBranchByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetBranchByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetBranchByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
