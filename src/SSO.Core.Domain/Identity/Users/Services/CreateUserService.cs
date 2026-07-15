using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Identity.Users.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Users.Validations.EntityValidations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Users.Services
{
	public sealed class CreateUserServiceRequest : DomainServiceRequest<User>
	{
		public CreateUserServiceRequest(User payload) : base(payload)
		{
		}
	}

	public sealed class CreateUserServiceRequestHandler
		: DomainServiceRequestHandler<User, CreateUserServiceRequest>
	{
		private UserManager<User> UserManager { get; set; }

		public CreateUserServiceRequestHandler(
			UserManager<User> userManager,
			IStringLocalizer<User> localizer,
			UserValidator entityValidator,
			CreateUserSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			UserManager = userManager;
		}

		public override async Task<User> Handle(CreateUserServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);

			var password = request.Payload.Password;
			request.Payload.Password = null;
			request.Payload.MarkCreated();

			var result = await UserManager.CreateAsync(request.Payload, password);
			if (!result.Succeeded)
			{
				var message = string.Join(" ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException(message);
			}

			return request.Payload;
		}
	}
}
