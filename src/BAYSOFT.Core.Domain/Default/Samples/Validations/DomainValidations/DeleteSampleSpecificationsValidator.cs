//using BAYSOFT.Core.Domain.Default.Samples.Specifications;
using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using BAYSOFT.Core.Domain.Default.Samples.Entity;

namespace BAYSOFT.Core.Domain.Default.Samples.Validations.DomainValidations
{
    public sealed class DeleteSampleSpecificationsValidator : DomainValidator<Sample>
    {
        public DeleteSampleSpecificationsValidator(
            // SampleDescriptionAlreadyExistsSpecification sampleDescriptionAlreadyExistsSpecification
        )
        {
            // Add(
            //     nameof(sampleDescriptionAlreadyExistsSpecification),
            //     new DomainRule<Sample>(
            //         sampleDescriptionAlreadyExistsSpecification.Not(),
            //         sampleDescriptionAlreadyExistsSpecification.ToString()
            //     )
            // );
        }
    }
}