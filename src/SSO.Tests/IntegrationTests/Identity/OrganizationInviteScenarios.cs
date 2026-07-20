using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Application.Identity.OrganizationInvites.Commands;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Services;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Infrastructures.Services;
using SSO.Tests.Helpers;
using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class OrganizationInviteScenarios
	{
		[TestMethod]
		public async Task Accept_Invite_Should_Create_Membership_Only_After_Accept()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var writer = scope.ServiceProvider.GetRequiredService<SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data.IIdentityDbContextWriter>();
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var mail = scope.ServiceProvider.GetRequiredService<CapturingMailService>();
			mail.Clear();

			var raw = OrganizationInviteToken.CreateRawToken();
			var invite = new OrganizationInvite
			{
				OrganizationId = IdentitySeed.DevOrganizationId,
				Email = "invitee@test.local",
				TokenHash = OrganizationInviteToken.Hash(raw),
				Status = OrganizationInviteStatuses.Pending,
				ExpiresAt = DateTime.UtcNow.AddDays(2),
				InvitedByUserId = IdentitySeed.DevUserId
			};
			invite.MarkCreated();
			db.OrganizationInvites.Add(invite);
			await db.SaveChangesAsync();

			var user = new User
			{
				Email = "invitee@test.local",
				UserName = "invitee@test.local",
				EmailConfirmed = true
			};
			user.MarkCreated();
			Assert.IsTrue((await userManager.CreateAsync(user, "ChangeMe!123")).Succeeded);

			var before = await db.Memberships.CountAsync(x =>
				!x.IsDeleted && x.UserId == user.Id && x.OrganizationId == IdentitySeed.DevOrganizationId);
			Assert.AreEqual(0, before);

			var result = await mediator.Send(new PatchAcceptOrganizationInviteCommand
			{
				RawToken = raw,
				AcceptingUserId = user.Id
			});

			Assert.IsTrue(result.Succeeded, result.Error);
			Assert.IsTrue(await db.Memberships.AnyAsync(x =>
				!x.IsDeleted && x.UserId == user.Id && x.OrganizationId == IdentitySeed.DevOrganizationId));

			var stored = await db.OrganizationInvites.AsNoTracking().SingleAsync(x => x.Id == invite.Id);
			Assert.AreEqual(OrganizationInviteStatuses.Accepted, stored.Status);
		}

		[TestMethod]
		public async Task Admin_Invites_Page_Requires_Auth()
		{
			using var client = ServerHelper.Create().CreateClient();
			var response = await client.GetAsync("/Admin/Invites");
			Assert.IsTrue(
				response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Unauthorized or HttpStatusCode.Found,
				$"Unexpected: {response.StatusCode}");
		}
	}
}
