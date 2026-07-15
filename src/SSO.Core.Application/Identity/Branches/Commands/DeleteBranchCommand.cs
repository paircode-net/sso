using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using SSO.Core.Application.Identity.Branches.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Services;
using SSO.Core.Domain.Identity.Branches.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Branches.Commands
{
	public sealed class DeleteBranchCommand : ApplicationRequest<Branch, DeleteBranchCommandResponse>
	{
		public DeleteBranchCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class DeleteBranchCommandResponse : ApplicationResponse<Branch>
	{
		public DeleteBranchCommandResponse(Tuple<int, int, WrapRequest<Branch>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public DeleteBranchCommandResponse(int statusCode, WrapRequest<Branch> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class DeleteBranchCommandHandler : ApplicationRequestHandler<Branch, DeleteBranchCommand, DeleteBranchCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public DeleteBranchCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Branch> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<DeleteBranchCommandResponse> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<Branch>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<Branch>(Localizer);
				await Mediator.Send(new DeleteBranchServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new DeleteBranchNotification(data));
				return new DeleteBranchCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<DeleteBranchCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new DeleteBranchCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
