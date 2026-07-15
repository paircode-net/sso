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
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.UserRoleAssignments.Queries
{
	public sealed class GetUserRoleAssignmentByIdQuery : ApplicationRequest<UserRoleAssignment, GetUserRoleAssignmentByIdQueryResponse>
	{
		public GetUserRoleAssignmentByIdQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class GetUserRoleAssignmentByIdQueryResponse : ApplicationResponse<UserRoleAssignment>
	{
		public GetUserRoleAssignmentByIdQueryResponse(Tuple<int, int, WrapRequest<UserRoleAssignment>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetUserRoleAssignmentByIdQueryResponse(int statusCode, WrapRequest<UserRoleAssignment> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetUserRoleAssignmentByIdQueryHandler : ApplicationRequestHandler<UserRoleAssignment, GetUserRoleAssignmentByIdQuery, GetUserRoleAssignmentByIdQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetUserRoleAssignmentByIdQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<UserRoleAssignment> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetUserRoleAssignmentByIdQueryResponse> Handle(GetUserRoleAssignmentByIdQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var id = request.Project(x => x.Id);
				var data = await Reader.Query<UserRoleAssignment>().Where(x => x.Id == id && !x.IsDeleted).Select(request).SingleOrDefaultAsync();
				if (data == null) throw new EntityNotFoundException<UserRoleAssignment>(Localizer);
				return new GetUserRoleAssignmentByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetUserRoleAssignmentByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetUserRoleAssignmentByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
