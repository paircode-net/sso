using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Default.Samples.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Default.Samples.Specifications
{
	public class SampleDescriptionAlreadyExistsSpecification : DomainSpecification<Sample>
    {
        private IDefaultDbContextReader Reader { get; set; }
        public SampleDescriptionAlreadyExistsSpecification(IDefaultDbContextReader reader)
        {
            Reader = reader;
            SpecificationMessage = "Já existe um registro com essa descrição!";
        }

        public override Expression<Func<Sample, bool>> ToExpression()
		{
			return sample => CheckRule(sample);
		}

		private bool CheckRule(Sample sample)
		{
			return Reader.Query<Sample>().Any(x => x.Description == sample.Description && x.Id != sample.Id);
		}
	}
}
