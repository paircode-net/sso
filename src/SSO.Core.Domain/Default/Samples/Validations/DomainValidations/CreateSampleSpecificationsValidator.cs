using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Default.Samples.Entity;
using SSO.Core.Domain.Default.Samples.Specifications;

namespace SSO.Core.Domain.Default.Samples.Validations.DomainValidations
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