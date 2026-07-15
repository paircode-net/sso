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
using SSO.Core.Domain.Identity.Users.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Users.Queries
{
	public sealed class GetUserByIdQuery : ApplicationRequest<User, GetUserByIdQueryResponse>
	{
		public GetUserByIdQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			ConfigSuppressedProperties(x => x.PasswordHash);
			ConfigSuppressedProperties(x => x.SecurityStamp);
			ConfigSuppressedProperties(x => x.ConcurrencyStamp);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class GetUserByIdQueryResponse : ApplicationResponse<User>
	{
		public GetUserByIdQueryResponse(Tuple<int, int, WrapRequest<User>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple)
			: base(tuple)
		{
		}

		public GetUserByIdQueryResponse(int statusCode, WrapRequest<User> request, object data, string message = "Successful operation!", long? resultCount = null)
			: base(statusCode, request, data, message, resultCount)
		{
		}
	}

	public sealed class GetUserByIdQueryHandler : ApplicationRequestHandler<User, GetUserByIdQuery, GetUserByIdQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetUserByIdQueryHandler(
			ILoggerFactory logger,
			IMediator mediator,
			IStringLocalizer<User> localizer,
			IIdentityDbContextReader reader)
		{
			Logger = logger;
			Localizer = localizer;
			Reader = reader;
		}

		public override async Task<GetUserByIdQueryResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var id = request.Project(x => x.Id);

				var data = await Reader
					.Query<User>()
					.Where(x => x.Id == id && !x.IsDeleted)
					.Select(request)
					.SingleOrDefaultAsync(cancellationToken);

				if (data == null)
				{
					throw new EntityNotFoundException<User>(Localizer);
				}

				return new GetUserByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetUserByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetUserByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
