using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.OrganizationInvites.Commands;
using SSO.Middleware.Identity;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;

namespace SSO.Tests.UnitTests.Middleware
{
	[TestClass]
	public class AdminWrapScenarios
	{
		[TestMethod]
		public void FromAnonymous_PostOrganizationInvite_Populates_Model_For_Post()
		{
			var orgId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
			var cmd = AdminWrap.FromAnonymous<PostOrganizationInviteCommand>(new
			{
				organizationId = orgId,
				email = "invitee@test.local"
			});

			var data = cmd.Post();

			Assert.AreEqual(orgId, data.OrganizationId);
			Assert.AreEqual("invitee@test.local", data.Email);
		}

		[TestMethod]
		public void ErrorMessage_Prefers_Notification_Details()
		{
			var response = new ModelWrapper.WrapResponse
			{
				StatusCode = 400,
				Message = "Unsuccessful operation!",
				Notifications = new System.Collections.Generic.Dictionary<string, object>
				{
					[ModelWrapper.Utilities.Constants.CONST_NOTIFICATIONS_MESSAGE] = "Email is required.",
					[ModelWrapper.Utilities.Constants.CONST_NOTIFICATIONS_ENTITY] =
						new System.Collections.Generic.Dictionary<string, object>
						{
							["email"] = new[] { "'Email' must not be empty." }
						}
				}
			};

			var message = AdminWrap.ErrorMessage(response);

			StringAssert.Contains(message, "Email");
			Assert.IsFalse(string.Equals(message, "Unsuccessful operation!", StringComparison.Ordinal));
		}
	}
}
