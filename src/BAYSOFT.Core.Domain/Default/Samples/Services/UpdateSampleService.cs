using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using BAYSOFT.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;
using BAYSOFT.Core.Domain.Default.Samples.Entity;
using BAYSOFT.Core.Domain.Default.Samples.Validations.DomainValidations;
using BAYSOFT.Core.Domain.Default.Samples.Validations.EntityValidations;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace BAYSOFT.Core.Domain.Default.Samples.Services
{
	public sealed class UpdateSampleServiceRequest : DomainServiceRequest<Sample>
	{
		public UpdateSampleServiceRequest(Sample payload) : base(payload)
		{
		}
	}
	public sealed class UpdateSampleServiceRequestHandler
		: DomainServiceRequestHandler<Sample, UpdateSampleServiceRequest>
	{
		private IDefaultDbContextWriter Writer { get; set; }
		public UpdateSampleServiceRequestHandler(
			IDefaultDbContextWriter writer,
			IStringLocalizer<Sample> localizer,
			SampleValidator entityValidator,
			UpdateSampleSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}
		public override async Task<Sample> Handle(
			UpdateSampleServiceRequest request,
			CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);

			ValidateDomain(request.Payload);

			// Do the update

			return request.Payload;
		}
	}
}