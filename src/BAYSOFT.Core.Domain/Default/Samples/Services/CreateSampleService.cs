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
	public sealed class CreateSampleServiceRequest : DomainServiceRequest<Sample>
	{
		public CreateSampleServiceRequest(Sample payload) : base(payload)
		{
		}
	}
	public sealed class CreateSampleServiceRequestHandler
		: DomainServiceRequestHandler<Sample, CreateSampleServiceRequest>
	{
		private IDefaultDbContextWriter Writer { get; set; }
		public CreateSampleServiceRequestHandler(
			IDefaultDbContextWriter writer,
			IStringLocalizer<Sample> localizer,
			SampleValidator entityValidator,
			CreateSampleSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}
		public override async Task<Sample> Handle(
			CreateSampleServiceRequest request,
			CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);

			ValidateDomain(request.Payload);

			await Writer.AddAsync(request.Payload);

			return request.Payload;
		}
	}
}