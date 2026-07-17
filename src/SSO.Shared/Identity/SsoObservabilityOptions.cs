namespace SSO.Shared.Identity
{
	/// <summary>
	/// P-002 / F00010-D1: OpenTelemetry with pluggable exporter by config.
	/// </summary>
	public sealed class SsoObservabilityOptions
	{
		public const string SectionName = "Sso:Observability";

		public bool Enabled { get; set; } = true;

		/// <summary>Console | Otlp | AzureMonitor | None</summary>
		public string Exporter { get; set; } = SsoObservabilityExporters.Console;

		public string? OtlpEndpoint { get; set; } = "http://localhost:4317";

		/// <summary>Application Insights / Azure Monitor connection string when Exporter=AzureMonitor.</summary>
		public string? AzureMonitorConnectionString { get; set; }

		public bool TracingEnabled { get; set; } = true;

		public bool MetricsEnabled { get; set; } = true;

		/// <summary>0..1 ratio sampler for traces.</summary>
		public double TracingSampleRatio { get; set; } = 1.0;

		public bool SerilogRequestLogging { get; set; } = true;
	}

	public static class SsoObservabilityExporters
	{
		public const string None = "None";
		public const string Console = "Console";
		public const string Otlp = "Otlp";
		public const string AzureMonitor = "AzureMonitor";

		public static bool IsValid(string? value) =>
			string.Equals(value, None, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, Console, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, Otlp, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, AzureMonitor, StringComparison.OrdinalIgnoreCase);
	}
}
