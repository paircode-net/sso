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
	public sealed class CreateMenuItemServiceRequest : DomainServiceRequest<MenuItem>
	{
		public CreateMenuItemServiceRequest(MenuItem payload) : base(payload) { }
	}

	public sealed class CreateMenuItemServiceRequestHandler
		: DomainServiceRequestHandler<MenuItem, CreateMenuItemServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreateMenuItemServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<MenuItem> localizer,
			MenuItemValidator entityValidator,
			CreateMenuItemSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<MenuItem> Handle(CreateMenuItemServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
