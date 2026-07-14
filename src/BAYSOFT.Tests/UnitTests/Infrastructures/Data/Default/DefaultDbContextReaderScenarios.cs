using BAYSOFT.Tests.Helpers.Data.Default;
using BAYSOFT.Tests.Helpers.Data.Default.Samples;
using BAYSOFT.Core.Domain.Default.Samples.Entity;

namespace BAYSOFT.Tests.UnitTests.Infrastructures.Data.Default
{
	[TestClass]
	public class DefaultDbContextReaderScenarios
	{
		[TestMethod]
		public void DefaultDbContextReader_Query_ToList_Should_Return_All_Samples()
		{
			var defaultDbContextReader = DefaultDbContextExtensions
				.GetInMemoryDefaultDbContext()
				.SetupSamples()
				.GetDbContextReader();

			var samples = defaultDbContextReader.Query<Sample>().ToList();

			Assert.IsNotNull(samples);
			Assert.AreEqual(samples.Count, 10);
		}

		[TestMethod]
		public void DefaultDbContextReader_Query_Where_Id_Should_Return_Only_One()
		{
			var defaultDbContextReader = DefaultDbContextExtensions
				.GetInMemoryDefaultDbContext()
				.SetupSamples()
				.GetDbContextReader();

			var samples = defaultDbContextReader.Query<Sample>().Where(x => x.Id == SamplesCollections.FromInt(1)).ToList();

			Assert.IsNotNull(samples);
			Assert.AreEqual(samples.Count, 1);
		}

		[TestMethod]
		public void DefaultDbContextReader_Query_Where_Id_SingleOrDefault_Should_Return_Only_One()
		{
			var defaultDbContextReader = DefaultDbContextExtensions
				.GetInMemoryDefaultDbContext()
				.SetupSamples()
				.GetDbContextReader();

			var samples = defaultDbContextReader.Query<Sample>().Where(x => x.Id == SamplesCollections.FromInt(1)).SingleOrDefault();

			Assert.IsNotNull(samples);
		}

		[TestMethod]
		public void DefaultDbContextReader_Query_Where_Id_SingleOrDefault_Should_Return_Null()
		{
			var defaultDbContextReader = DefaultDbContextExtensions
				.GetInMemoryDefaultDbContext()
				.SetupSamples()
				.GetDbContextReader();

			var samples = defaultDbContextReader.Query<Sample>().Where(x => x.Id == SamplesCollections.FromInt(999)).SingleOrDefault();

			Assert.IsNull(samples);
		}
	}
}
