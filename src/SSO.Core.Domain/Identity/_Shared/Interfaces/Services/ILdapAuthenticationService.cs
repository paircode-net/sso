using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity._Context.Interfaces.Services
{
	public sealed class LdapAuthResult
	{
		public bool Succeeded { get; init; }
		public string? Error { get; init; }
		public string? ProviderKey { get; init; }
		public string? DistinguishedName { get; init; }
		public string? Email { get; init; }
		public string? DisplayName { get; init; }
		public IReadOnlyList<string> Groups { get; init; } = Array.Empty<string>();

		public static LdapAuthResult Fail(string error) => new() { Succeeded = false, Error = error };

		public static LdapAuthResult Ok(
			string providerKey,
			string? dn,
			string? email,
			string? displayName,
			IReadOnlyList<string> groups)
			=> new()
			{
				Succeeded = true,
				ProviderKey = providerKey,
				DistinguishedName = dn,
				Email = email,
				DisplayName = displayName,
				Groups = groups
			};
	}

	public interface ILdapAuthenticationService
	{
		Task<LdapAuthResult> AuthenticateAsync(
			string username,
			string password,
			CancellationToken cancellationToken = default);
	}
}
