using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using BAYSOFT.Core.Domain.Default.Samples.Entity;
using BAYSOFT.Core.Domain.Default.Samples.Specifications;

namespace BAYSOFT.Core.Domain.Default.Samples.Validations.DomainValidations
{
    public sealed class CreateSampleSpecificationsValidator : DomainValidator<Sample>
    {
        public CreateSampleSpecificationsValidator(
            SampleDescriptionAlreadyExistsSpecification sampleDescriptionAlreadyExistsSpecification
            )
        {
            Add(
                nameof(sampleDescriptionAlreadyExistsSpecification),
                new DomainRule<Sample>(
                    sampleDescriptionAlreadyExistsSpecification.Not(),
                    sampleDescriptionAlreadyExistsSpecification.ToString()
                )
            );
        }
    }
}