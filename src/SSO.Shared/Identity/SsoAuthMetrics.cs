using System.Diagnostics.Metrics;

namespace SSO.Shared.Identity
{
	/// <summary>
	/// AuthN/AuthZ meters for feature 00010. Names match the observability plan.
	/// </summary>
	public static class SsoAuthMetrics
	{
		public const string MeterName = "SSO.Auth";

		private static readonly Meter Meter = new(MeterName, "1.0.0");

		public static readonly Counter<long> LoginSuccess =
			Meter.CreateCounter<long>("sso.auth.login.success");

		public static readonly Counter<long> LoginFailure =
			Meter.CreateCounter<long>("sso.auth.login.failure");

		public static readonly Counter<long> TokenIssued =
			Meter.CreateCounter<long>("sso.auth.token.issued");

		public static readonly Counter<long> SwitchContextSuccess =
			Meter.CreateCounter<long>("sso.auth.switch_context.success");

		public static readonly Counter<long> SwitchContextFailure =
			Meter.CreateCounter<long>("sso.auth.switch_context.failure");

		public static readonly Counter<long> RateLimited =
			Meter.CreateCounter<long>("sso.auth.rate_limited");

		public static readonly Histogram<int> JwtPermissionsCount =
			Meter.CreateHistogram<int>("sso.jwt.permissions_count");

		public static readonly Histogram<int> JwtApproximateSize =
			Meter.CreateHistogram<int>("sso.jwt.approximate_size");

		public static void RecordLoginSuccess(string? method = null) =>
			LoginSuccess.Add(1, new KeyValuePair<string, object?>("method", method ?? "password"));

		public static void RecordLoginFailure(string? reason = null, string? method = null) =>
			LoginFailure.Add(
				1,
				new KeyValuePair<string, object?>("method", method ?? "password"),
				new KeyValuePair<string, object?>("reason", reason ?? "unknown"));

		public static void RecordTokenIssued(string grantType) =>
			TokenIssued.Add(1, new KeyValuePair<string, object?>("grant_type", grantType));

		public static void RecordSwitchContextSuccess() => SwitchContextSuccess.Add(1);

		public static void RecordSwitchContextFailure(string? reason = null) =>
			SwitchContextFailure.Add(1, new KeyValuePair<string, object?>("reason", reason ?? "unknown"));

		public static void RecordRateLimited(string? path = null) =>
			RateLimited.Add(1, new KeyValuePair<string, object?>("path", path ?? "unknown"));

		public static void RecordJwtShape(int permissionsCount, int approximateUtf8Bytes)
		{
			JwtPermissionsCount.Record(permissionsCount);
			JwtApproximateSize.Record(approximateUtf8Bytes);
		}
	}
}
