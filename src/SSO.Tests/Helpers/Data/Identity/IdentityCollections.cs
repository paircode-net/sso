using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using System;

namespace SSO.Tests.Helpers.Data.Identity
{
	public static class IdentityCollections
	{
		public static Guid FromInt(int value) => Guid.Parse(value.ToString("D32"));

		public static Organization Organization(int id, string code)
		{
			var organization = new Organization
			{
				Id = FromInt(id),
				Name = $"Organization {id}",
				Code = code
			};
			organization.MarkCreated();
			return organization;
		}

		public static Product Product(int id, string code)
		{
			var product = new Product
			{
				Id = FromInt(id),
				Name = $"Product {id}",
				Code = code
			};
			product.MarkCreated();
			return product;
		}

		public static Membership Membership(int id, Guid userId, Guid organizationId)
		{
			var membership = new Membership
			{
				Id = FromInt(id),
				UserId = userId,
				OrganizationId = organizationId
			};
			membership.MarkCreated();
			return membership;
		}
	}
}
