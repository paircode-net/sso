using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Branches;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class BranchInheritanceScenarios
	{
		[TestMethod]
		public void BranchAncestry_Should_Return_Nearest_First()
		{
			var hq = new Branch
			{
				Id = IdentitySeed.DevBranchHqId,
				OrganizationId = IdentitySeed.DevOrganizationId,
				ParentBranchId = null,
				Code = "hq",
				Name = "HQ"
			};
			var filial = new Branch
			{
				Id = IdentitySeed.DevBranchFilialId,
				OrganizationId = IdentitySeed.DevOrganizationId,
				ParentBranchId = IdentitySeed.DevBranchHqId,
				Code = "filial",
				Name = "Filial"
			};

			var ancestors = BranchAncestry.GetAncestorIds(new[] { hq, filial }, IdentitySeed.DevBranchFilialId);
			CollectionAssert.AreEqual(new[] { IdentitySeed.DevBranchHqId }, ancestors.ToList());
		}

		[TestMethod]
		public void BranchAncestry_Should_Detect_Cycle()
		{
			var a = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
			var b = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
			var branches = new[]
			{
				new Branch { Id = a, OrganizationId = IdentitySeed.DevOrganizationId, ParentBranchId = b, Code = "a", Name = "A" },
				new Branch { Id = b, OrganizationId = IdentitySeed.DevOrganizationId, ParentBranchId = a, Code = "b", Name = "B" }
			};

			Assert.IsTrue(BranchAncestry.WouldCreateCycle(branches, branches[0]));
			Assert.IsTrue(BranchAncestry.WouldCreateCycle(
				branches,
				new Branch { Id = a, OrganizationId = IdentitySeed.DevOrganizationId, ParentBranchId = a, Code = "a", Name = "A" }));
		}

		[TestMethod]
		public async Task Default_Off_Should_Not_Inherit_Hq_Permissions_To_Filial()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectivePermissionsResolver>();

			var permissions = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchFilialId,
				SsoClients.DevSpaClientId);

			Assert.IsFalse(permissions.Contains(IdentitySeed.PermissionHqReports));
			CollectionAssert.Contains(permissions.ToList(), IdentitySeed.PermissionFilialOps);
		}

		[TestMethod]
		public async Task On_Plus_Inheritable_Should_Grant_Ancestor_Permission_On_Child()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectivePermissionsResolver>();

			var org = await db.Organizations.FirstAsync(x => x.Id == IdentitySeed.DevOrganizationId);
			org.BranchAuthzInheritance = BranchAuthzInheritancePolicies.InheritFromAncestors;
			org.TouchUpdated();

			var hqAssignment = await db.UserRoleAssignments.FirstAsync(x =>
				!x.IsDeleted
				&& x.UserId == IdentitySeed.DevUserId
				&& x.RoleId == IdentitySeed.DevRoleHqManagerId
				&& x.BranchId == IdentitySeed.DevBranchHqId);
			hqAssignment.Inheritable = true;
			hqAssignment.TouchUpdated();
			await db.SaveChangesAsync();

			var permissions = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchFilialId,
				SsoClients.DevSpaClientId);

			CollectionAssert.Contains(permissions.ToList(), IdentitySeed.PermissionHqReports);
			CollectionAssert.Contains(permissions.ToList(), IdentitySeed.PermissionFilialOps);
			CollectionAssert.Contains(permissions.ToList(), IdentitySeed.PermissionAccess);
		}

		[TestMethod]
		public async Task On_Without_Inheritable_Should_Not_Inherit()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectivePermissionsResolver>();

			var org = await db.Organizations.FirstAsync(x => x.Id == IdentitySeed.DevOrganizationId);
			org.BranchAuthzInheritance = BranchAuthzInheritancePolicies.InheritFromAncestors;
			await db.SaveChangesAsync();

			// HQ assignment remains Inheritable=false (seed default)
			var permissions = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchFilialId,
				SsoClients.DevSpaClientId);

			Assert.IsFalse(permissions.Contains(IdentitySeed.PermissionHqReports));
		}

		[TestMethod]
		public async Task Claims_Child_Value_Should_Win_Over_Inheritable_Ancestor()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectiveClaimsResolver>();

			var org = await db.Organizations.FirstAsync(x => x.Id == IdentitySeed.DevOrganizationId);
			org.BranchAuthzInheritance = BranchAuthzInheritancePolicies.InheritFromAncestors;

			// Ancestor (HQ) inheritable claim
			db.UserClaimAssignments.Add(UserClaimAssignment.Create(
				IdentitySeed.DevUserId,
				IdentitySeed.DevClaimCanExportId,
				"true",
				IdentitySeed.DevProductId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchHqId,
				inheritable: true));

			// Child (Filial) explicit override
			db.UserClaimAssignments.Add(UserClaimAssignment.Create(
				IdentitySeed.DevUserId,
				IdentitySeed.DevClaimCanExportId,
				"false",
				IdentitySeed.DevProductId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchFilialId,
				inheritable: false));

			await db.SaveChangesAsync();

			var claims = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchFilialId,
				SsoClients.DevSpaClientId);

			Assert.AreEqual("false", claims[IdentitySeed.ClaimCanExport]);
		}

		[TestMethod]
		public async Task Claims_Missing_On_Child_Should_Fill_From_Nearest_Ancestor()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectiveClaimsResolver>();

			var org = await db.Organizations.FirstAsync(x => x.Id == IdentitySeed.DevOrganizationId);
			org.BranchAuthzInheritance = BranchAuthzInheritancePolicies.InheritFromAncestors;

			db.UserClaimAssignments.Add(UserClaimAssignment.Create(
				IdentitySeed.DevUserId,
				IdentitySeed.DevClaimCanExportId,
				"true",
				IdentitySeed.DevProductId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchHqId,
				inheritable: true));

			await db.SaveChangesAsync();

			var claims = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchFilialId,
				SsoClients.DevSpaClientId);

			Assert.AreEqual("true", claims[IdentitySeed.ClaimCanExport]);
		}

		[TestMethod]
		public async Task Put_Organization_Should_Accept_BranchAuthzInheritance()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var payload = new
			{
				id = IdentitySeed.DevOrganizationId,
				name = "Dev Organization",
				code = "dev-org",
				branchAuthzInheritance = BranchAuthzInheritancePolicies.InheritFromAncestors
			};

			var response = await client.PutAsync(
				$"/api/identity/organizations/{IdentitySeed.DevOrganizationId}",
				new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var org = await db.Organizations.AsNoTracking().FirstAsync(x => x.Id == IdentitySeed.DevOrganizationId);
			Assert.AreEqual(BranchAuthzInheritancePolicies.InheritFromAncestors, org.BranchAuthzInheritance);
		}

		[TestMethod]
		public async Task Update_Branch_Parent_To_Self_Should_Fail()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreateOrgAdminClient(server, IdentitySeed.DevOrganizationId);

			// Point Filial parent at itself → cycle
			var payload = new
			{
				id = IdentitySeed.DevBranchFilialId,
				organizationId = IdentitySeed.DevOrganizationId,
				parentBranchId = IdentitySeed.DevBranchFilialId,
				name = "Filial",
				code = "filial"
			};

			var response = await client.PutAsync(
				$"/api/identity/branches/{IdentitySeed.DevBranchFilialId}",
				new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

			Assert.IsTrue(
				response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity or HttpStatusCode.InternalServerError,
				$"Expected validation failure, got {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
		}
	}
}
