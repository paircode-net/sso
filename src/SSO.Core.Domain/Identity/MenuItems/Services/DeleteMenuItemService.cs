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
	public sealed class DeleteMenuItemServiceRequest : DomainServiceRequest<MenuItem>
	{
		public DeleteMenuItemServiceRequest(MenuItem payload) : base(payload) { }
	}

	public sealed class DeleteMenuItemServiceRequestHandler
		: DomainServiceRequestHandler<MenuItem, DeleteMenuItemServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteMenuItemServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<MenuItem> localizer,
			MenuItemValidator entityValidator,
			DeleteMenuItemSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<MenuItem> Handle(DeleteMenuItemServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
