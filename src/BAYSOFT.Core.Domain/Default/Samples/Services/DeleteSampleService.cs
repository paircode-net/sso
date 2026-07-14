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
	public sealed class DeleteSampleServiceRequest : DomainServiceRequest<Sample>
	{
		public DeleteSampleServiceRequest(Sample payload) : base(payload)
		{
		}
	}
	public sealed class DeleteSampleServiceRequestHandler
		: DomainServiceRequestHandler<Sample, DeleteSampleServiceRequest>
	{
		private IDefaultDbContextWriter Writer { get; set; }
		public DeleteSampleServiceRequestHandler(
			IDefaultDbContextWriter writer,
			IStringLocalizer<Sample> localizer,
			SampleValidator entityValidator,
			DeleteSampleSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}
		public override async Task<Sample> Handle(
			DeleteSampleServiceRequest request,
			CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);

			ValidateDomain(request.Payload);

			Writer.Remove(request.Payload);

			return request.Payload;
		}
	}
}