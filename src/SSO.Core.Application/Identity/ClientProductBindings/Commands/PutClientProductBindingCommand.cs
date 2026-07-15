using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Put;
using SSO.Core.Application.Identity.ClientProductBindings.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ClientProductBindings.Services;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ClientProductBindings.Commands
{
	public sealed class PutClientProductBindingCommand : ApplicationRequest<ClientProductBinding, PutClientProductBindingCommandResponse>
	{
		public PutClientProductBindingCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class PutClientProductBindingCommandResponse : ApplicationResponse<ClientProductBinding>
	{
		public PutClientProductBindingCommandResponse(Tuple<int, int, WrapRequest<ClientProductBinding>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PutClientProductBindingCommandResponse(int statusCode, WrapRequest<ClientProductBinding> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PutClientProductBindingCommandHandler : ApplicationRequestHandler<ClientProductBinding, PutClientProductBindingCommand, PutClientProductBindingCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PutClientProductBindingCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<ClientProductBinding> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PutClientProductBindingCommandResponse> Handle(PutClientProductBindingCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<ClientProductBinding>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<ClientProductBinding>(Localizer);
				request.Put(data);
				await Mediator.Send(new UpdateClientProductBindingServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PutClientProductBindingNotification(data));
				return new PutClientProductBindingCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PutClientProductBindingCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PutClientProductBindingCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
