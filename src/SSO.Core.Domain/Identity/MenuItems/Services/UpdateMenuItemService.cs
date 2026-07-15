using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.MenuItems.Validations.DomainValidations;
using SSO.Core.Domain.Identity.MenuItems.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.MenuItems.Services
{
	public sealed class UpdateMenuItemServiceRequest : DomainServiceRequest<MenuItem>
	{
		public UpdateMenuItemServiceRequest(MenuItem payload) : base(payload) { }
	}

	public sealed class UpdateMenuItemServiceRequestHandler
		: DomainServiceRequestHandler<MenuItem, UpdateMenuItemServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateMenuItemServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<MenuItem> localizer,
			MenuItemValidator entityValidator,
			UpdateMenuItemSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<MenuItem> Handle(UpdateMenuItemServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
