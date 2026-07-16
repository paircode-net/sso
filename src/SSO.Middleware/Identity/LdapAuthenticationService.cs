using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Shared.Identity;
using System.DirectoryServices.Protocols;
using System.Net;

namespace SSO.Middleware.Identity
{
	public sealed class LdapAuthenticationService : ILdapAuthenticationService
	{
		private readonly LdapOptions _options;
		private readonly IHostEnvironment _environment;
		private readonly ILogger<LdapAuthenticationService> _logger;

		public LdapAuthenticationService(
			IOptions<SsoHardeningOptions> hardening,
			IHostEnvironment environment,
			ILogger<LdapAuthenticationService> logger)
		{
			_options = hardening.Value.ExternalAuth?.Ldap ?? new LdapOptions();
			_environment = environment;
			_logger = logger;
		}

		public Task<LdapAuthResult> AuthenticateAsync(
			string username,
			string password,
			CancellationToken cancellationToken = default)
		{
			if (!_options.Enabled)
			{
				return Task.FromResult(LdapAuthResult.Fail("ldap_disabled"));
			}

			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				return Task.FromResult(LdapAuthResult.Fail("invalid_credentials"));
			}

			if (string.IsNullOrWhiteSpace(_options.Host))
			{
				return Task.FromResult(LdapAuthResult.Fail("ldap_misconfigured"));
			}

			if (_environment.IsProduction()
				&& _options.EnforceTlsInProduction
				&& !_options.UseSsl)
			{
				_logger.LogError("LDAP rejected: TLS required in Production (EnforceTlsInProduction).");
				return Task.FromResult(LdapAuthResult.Fail("ldap_tls_required"));
			}

			try
			{
				return Task.FromResult(AuthenticateCore(username.Trim(), password));
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "LDAP authentication failed for {User}", username);
				return Task.FromResult(LdapAuthResult.Fail("invalid_credentials"));
			}
		}

		private LdapAuthResult AuthenticateCore(string username, string password)
		{
			var bindDn = (_options.BindDnPattern ?? "{username}")
				.Replace("{username}", username, StringComparison.OrdinalIgnoreCase)
				.Replace("{BaseDn}", _options.BaseDn ?? string.Empty, StringComparison.OrdinalIgnoreCase);

			using var connection = new LdapConnection(new LdapDirectoryIdentifier(_options.Host, _options.Port));
			connection.SessionOptions.ProtocolVersion = 3;
			connection.SessionOptions.SecureSocketLayer = _options.UseSsl;
			connection.Timeout = TimeSpan.FromSeconds(Math.Clamp(_options.TimeoutSeconds, 1, 60));
			connection.AuthType = AuthType.Basic;
			connection.Credential = new NetworkCredential(bindDn, password);
			connection.Bind();

			var filter = (_options.UserSearchFilter ?? "(sAMAccountName={username})")
				.Replace("{username}", EscapeFilter(username), StringComparison.OrdinalIgnoreCase);

			var request = new SearchRequest(
				_options.BaseDn ?? string.Empty,
				filter,
				SearchScope.Subtree,
				"distinguishedName",
				"mail",
				"userPrincipalName",
				"displayName",
				"cn",
				"objectGUID",
				"memberOf");

			var response = (SearchResponse)connection.SendRequest(request);
			var entry = response.Entries.Cast<SearchResultEntry>().FirstOrDefault();
			if (entry is null)
			{
				// Bind succeeded with UPN-style credentials; synthesize minimal profile.
				return LdapAuthResult.Ok(
					providerKey: bindDn,
					dn: bindDn,
					email: username.Contains('@', StringComparison.Ordinal) ? username : null,
					displayName: username,
					groups: Array.Empty<string>());
			}

			var dn = entry.DistinguishedName;
			var email = FirstAttr(entry, "mail")
				?? FirstAttr(entry, "userPrincipalName")
				?? (username.Contains('@', StringComparison.Ordinal) ? username : null);
			var displayName = FirstAttr(entry, "displayName") ?? FirstAttr(entry, "cn") ?? username;
			var objectGuid = TryReadObjectGuid(entry);
			var providerKey = objectGuid ?? dn;
			var groups = entry.Attributes["memberOf"]?.GetValues(typeof(string))
				?.Cast<string>()
				.ToList()
				?? new List<string>();

			return LdapAuthResult.Ok(providerKey, dn, email, displayName, groups);
		}

		private static string? FirstAttr(SearchResultEntry entry, string name)
		{
			if (entry.Attributes[name] is null || entry.Attributes[name].Count == 0)
			{
				return null;
			}

			return entry.Attributes[name][0]?.ToString();
		}

		private static string? TryReadObjectGuid(SearchResultEntry entry)
		{
			try
			{
				if (entry.Attributes["objectGUID"] is null || entry.Attributes["objectGUID"].Count == 0)
				{
					return null;
				}

				var raw = entry.Attributes["objectGUID"][0];
				if (raw is byte[] bytes && bytes.Length == 16)
				{
					return new Guid(bytes).ToString("D");
				}
			}
			catch
			{
				// ignore
			}

			return null;
		}

		private static string EscapeFilter(string value)
		{
			var sb = new StringBuilder(value.Length);
			foreach (var c in value)
			{
				sb.Append(c switch
				{
					'\\' => @"\5c",
					'*' => @"\2a",
					'(' => @"\28",
					')' => @"\29",
					'\0' => @"\00",
					_ => c.ToString()
				});
			}

			return sb.ToString();
		}
	}
}
