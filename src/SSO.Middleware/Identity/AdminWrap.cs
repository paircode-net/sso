using System;
using System.Net;
using ModelWrapper;
using Newtonsoft.Json;
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
			var json = JsonConvert.SerializeObject(body, Settings);
			return JsonConvert.DeserializeObject<TRequest>(json, Settings)
				?? throw new InvalidOperationException($"Failed to build {typeof(TRequest).Name}.");
		}

		public static bool IsSuccess(WrapResponse response)
			=> response.StatusCode is >= 200 and < 300;

		public static string ErrorMessage(WrapResponse response)
			=> string.IsNullOrWhiteSpace(response.Message)
				? $"Operation failed ({response.StatusCode})."
				: response.Message;

		public static HttpStatusCode Status(WrapResponse response)
			=> (HttpStatusCode)response.StatusCode;
	}
}
