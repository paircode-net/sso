using BAYSOFT.Tests.Helpers.Data.Default;
using BAYSOFT.Tests.Helpers.Data.Default.Samples;
using BAYSOFT.Core.Domain.Default.Samples.Entity;

namespace BAYSOFT.Tests.UnitTests.Infrastructures.Data.Default
{
	[TestClass]
	public class DefaultDbContextWriterScenarios
	{
		[TestMethod]
		public void DefaultDbContextReader_Add_Should_Not_Throw_Exception()
		{
			using (var context = DefaultDbContextExtensions.GetInMemoryDefaultDbContext().SetupSamples())
			{
				var defaultDbContextReader = context.GetDbContextReader();
				var defaultDbContextWriter = context.GetDbContextWriter();

				var entity = new Sample { Description = "new sample" };

				defaultDbContextWriter.Add(entity);
				context.SaveChanges();

				Assert.IsTrue(defaultDbContextReader.Query<Sample>().Any(x => x.Description == entity.Description));
				Assert.IsTrue(entity.Id != SamplesCollections.FromInt(0));
			}
		}
	}
}
