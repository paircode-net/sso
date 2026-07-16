namespace SSO.Shared.Identity
{
	/// <summary>Organization policy for Branch authz inheritance (feature 00009 / ADR-008).</summary>
	public static class BranchAuthzInheritancePolicies
	{
		public const string Off = "Off";
		public const string InheritFromAncestors = "InheritFromAncestors";

		public static bool IsValid(string? value)
			=> value is Off or InheritFromAncestors;

		public static bool IsEnabled(string? value)
			=> string.Equals(value, InheritFromAncestors, StringComparison.OrdinalIgnoreCase);
	}
}
