namespace SSO.Shared.Identity
{
	/// <summary>Scalar value types for ClaimDefinition (feature 00008 / F00008-D4).</summary>
	public static class ClaimValueTypes
	{
		public const string String = "string";
		public const string Int = "int";
		public const string Bool = "bool";

		public static bool IsValid(string? value)
			=> value is String or Int or Bool;

		public static bool TryValidate(string valueType, string? rawValue, out string? error)
		{
			error = null;
			if (!IsValid(valueType))
			{
				error = "invalid_value_type";
				return false;
			}

			if (rawValue is null)
			{
				error = "value_required";
				return false;
			}

			if (string.Equals(valueType, Int, StringComparison.OrdinalIgnoreCase))
			{
				if (!int.TryParse(rawValue, out _))
				{
					error = "value_must_be_int";
					return false;
				}
			}
			else if (string.Equals(valueType, Bool, StringComparison.OrdinalIgnoreCase))
			{
				if (!bool.TryParse(rawValue, out _))
				{
					error = "value_must_be_bool";
					return false;
				}
			}

			return true;
		}
	}

	/// <summary>JWT naming for typed claims (F00008-D1).</summary>
	public static class TypedClaimNames
	{
		public const string Prefix = "sso_c_";

		public static string ToJwtType(string code)
			=> Prefix + code.Trim().ToLowerInvariant();

		public static string? FromJwtType(string claimType)
		{
			if (string.IsNullOrWhiteSpace(claimType)
				|| !claimType.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			return claimType[Prefix.Length..];
		}
	}
}
