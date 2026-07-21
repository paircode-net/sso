using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ModelWrapper;
using ModelWrapper.Extensions.GetNotifications;
using ModelWrapper.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace SSO.Middleware.Identity
{
	/// <summary>
	/// Builds ModelWrapper ApplicationRequests the same way HTTP JSON binding does,
	/// so Admin PageModels can orchestrate Application commands without Domain/Db writes.
	/// </summary>
	public static class AdminWrap
	{
		private static readonly JsonSerializerSettings Settings = new()
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			NullValueHandling = NullValueHandling.Ignore
		};

		public static TRequest FromAnonymous<TRequest>(object body)
			where TRequest : class
		{
			var request = Activator.CreateInstance<TRequest>()
				?? throw new InvalidOperationException($"Failed to create {typeof(TRequest).Name}.");

			var model = GetWrappedModel(request);
			if (model is null)
			{
				return JsonConvert.DeserializeObject<TRequest>(
						JsonConvert.SerializeObject(body, Settings), Settings)
					?? throw new InvalidOperationException($"Failed to build {typeof(TRequest).Name}.");
			}

			// Populate the inner ModelWrapper entity directly. JsonDeserialize of DynamicObject
			// (WrapRequest) often leaves Model empty, which makes Post()/Put() emit blank entities
			// and surfaces only the generic "Unsuccessful operation!" message.
			var json = JObject.FromObject(body, JsonSerializer.Create(Settings));
			var payload = json.ToString(Formatting.None);
			JsonConvert.PopulateObject(payload, model, Settings);

			// Request-level properties (e.g. IssuedRawToken) that are not on the entity.
			foreach (var prop in typeof(TRequest).GetProperties(
				System.Reflection.BindingFlags.Public
				| System.Reflection.BindingFlags.Instance
				| System.Reflection.BindingFlags.DeclaredOnly))
			{
				if (!prop.CanWrite)
				{
					continue;
				}

				var match = json.Properties()
					.FirstOrDefault(p => string.Equals(p.Name, prop.Name, StringComparison.OrdinalIgnoreCase));
				if (match is null || match.Value.Type == JTokenType.Null)
				{
					continue;
				}

				prop.SetValue(request, match.Value.ToObject(prop.PropertyType));
			}

			typeof(TRequest).GetMethod("BindComplete", Type.EmptyTypes)?.Invoke(request, null);
			return request;
		}

		public static bool IsSuccess(WrapResponse response)
			=> response.StatusCode is >= 200 and < 300;

		public static string ErrorMessage(WrapResponse response)
		{
			var details = FormatNotifications(response.Notifications);
			if (!string.IsNullOrWhiteSpace(details))
			{
				return details;
			}

			return string.IsNullOrWhiteSpace(response.Message)
				? $"Operation failed ({response.StatusCode})."
				: response.Message;
		}

		public static HttpStatusCode Status(WrapResponse response)
			=> (HttpStatusCode)response.StatusCode;

		private static object? GetWrappedModel(object request)
		{
			var getModel = request.GetType().GetMethod("GetModel", Type.EmptyTypes);
			return getModel?.Invoke(request, null);
		}

		private static string? FormatNotifications(Dictionary<string, object>? notifications)
		{
			if (notifications is null || notifications.Count == 0)
			{
				return null;
			}

			var parts = new List<string>();

			var message = notifications.GetMessage();
			if (!string.IsNullOrWhiteSpace(message)
				&& !string.Equals(message, "Unsuccessful operation!", StringComparison.OrdinalIgnoreCase))
			{
				parts.Add(message!);
			}

			AppendGrouped(parts, notifications.GetRequest());
			AppendGrouped(parts, notifications.GetEntity());

			var domain = notifications.GetDomain();
			if (domain is { Length: > 0 })
			{
				parts.AddRange(domain.Where(x => !string.IsNullOrWhiteSpace(x)));
			}

			if (parts.Count == 0 && !string.IsNullOrWhiteSpace(message))
			{
				parts.Add(message!);
			}

			return parts.Count == 0 ? null : string.Join(" ", parts);
		}

		private static void AppendGrouped(List<string> parts, Dictionary<string, object>? grouped)
		{
			if (grouped is null)
			{
				return;
			}

			foreach (var (key, value) in grouped)
			{
				if (value is IEnumerable enumerable and not string)
				{
					foreach (var item in enumerable)
					{
						var text = item?.ToString();
						if (!string.IsNullOrWhiteSpace(text))
						{
							parts.Add($"{key}: {text}");
						}
					}
				}
				else if (!string.IsNullOrWhiteSpace(value?.ToString()))
				{
					parts.Add($"{key}: {value}");
				}
			}
		}
	}
}
