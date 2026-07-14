using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using ModelWrapper.Extensions.GetNotifications;
using System.Net;

namespace BAYSOFT.Web.Blazor.Components.Helpers
{
	public static class ResponseNotificationsHelper
	{
		public static bool HandleNotifications<T>(this ApplicationResponse<T> response, [FromServices] ISnackbar Snackbar, bool notifyOnSuccess = true) where T : DomainEntityBase
		{
			if (response != null)
			{
				var successStatusCodes = new List<int>
				{
					(int)HttpStatusCode.OK,
					(int)HttpStatusCode.Created,
					(int)HttpStatusCode.Accepted,
					(int)HttpStatusCode.NoContent
				};

				if (successStatusCodes.Any(code => code == response.StatusCode))
				{
					if (notifyOnSuccess)
						Snackbar.Add(response.Message, Severity.Success);
					return true;
				}
				else
				{
					Snackbar.Add(response.Notifications.GetMessage(), Severity.Warning);
					if (response.Notifications.HasRequest())
						foreach (var requestNotifications in response.Notifications.GetRequest())
						{
							Snackbar.Add(requestNotifications.Value.ToString(), Severity.Warning);
						}
					if (response.Notifications.HasEntity())
						foreach (var entityNotifications in response.Notifications.GetEntity())
						{
							if (entityNotifications.Value is string message)
							{
								Snackbar.Add(message, Severity.Warning);
							}
							if (entityNotifications.Value is string[] messages)
							{
								foreach (var m in messages)
								{
									Snackbar.Add(m, Severity.Warning);
								}
							}
						}
					if (response.Notifications.HasDomain())
						foreach (var domainNotifications in response.Notifications.GetDomain())
						{
							Snackbar.Add(domainNotifications, Severity.Warning);
						}
				}
			}
			return false;
		}
	}
}
