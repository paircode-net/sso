using SSO.Core.Domain.Default.Samples.Entity;

namespace SSO.Tests.Helpers.Data.Default.Samples
{
	public static class SamplesCollections
	{
		public static Guid FromInt(int value)
		{
			return Guid.Parse(value.ToString("D32"));
		}
		public static List<Sample> GetDefaultCollection()
		{
			return new List<Sample>
			{
				new Sample { Id = FromInt(1), Description = "Sample - 001" },
				new Sample { Id = FromInt(2), Description = "Sample - 002" },
				new Sample { Id = FromInt(3), Description = "Sample - 003" },
				new Sample { Id = FromInt(4), Description = "Sample - 004" },
				new Sample { Id = FromInt(5), Description = "Sample - 005" },
				new Sample { Id = FromInt(6), Description = "Sample - 006" },
				new Sample { Id = FromInt(7), Description = "Sample - 007" },
				new Sample { Id = FromInt(8), Description = "Sample - 008" },
				new Sample { Id = FromInt(9), Description = "Sample - 009" },
				new Sample { Id = FromInt(10), Description = "Sample - 010" },
			};
		}
	}
}
