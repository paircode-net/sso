using SSO.Core.Domain.Default.Samples.Entity;
using SSO.Infrastructures.Data.Default;
using SSO.Tests.Helpers.Data.Default.Samples;

namespace SSO.Tests.Helpers.Data.Default
{
	internal static class AddMockedSamplesExtensions
	{
		public static DefaultDbContext SetupSamples(this DefaultDbContext context, List<Sample>? entities = null)
		{
			if (entities == null)
			{
				entities = SamplesCollections.GetDefaultCollection();
			}

			context.AddRange(entities);
			context.SaveChanges();

			return context;
		}
	}
}
