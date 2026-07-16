using System;
using SSO.Core.Domain.Identity._Shared;
using SSO.Shared.Identity;

namespace SSO.Core.Domain.Identity.ClaimDefinitions.Entity
{
	/// <summary>Catalog entry for typed domain claims (feature 00008).</summary>
	public sealed class ClaimDefinition : IdentityAuditableEntity
	{
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		/// <summary>string | int | bool</summary>
		public string ValueType { get; set; } = ClaimValueTypes.String;
		/// <summary>Null = global; set = only that product.</summary>
		public Guid? ProductId { get; set; }
		public bool IsEnabled { get; set; } = true;

		public ClaimDefinition()
		{
		}

		public static ClaimDefinition Create(
			string code,
			string name,
			string valueType,
			Guid? productId = null,
			string? description = null)
		{
			if (string.IsNullOrWhiteSpace(code))
			{
				throw new InvalidOperationException("code_required");
			}

			if (!ClaimValueTypes.IsValid(valueType))
			{
				throw new InvalidOperationException("invalid_value_type");
			}

			var entity = new ClaimDefinition
			{
				Id = Guid.NewGuid(),
				Code = code.Trim().ToLowerInvariant(),
				Name = name,
				Description = description,
				ValueType = valueType.Trim().ToLowerInvariant(),
				ProductId = productId,
				IsEnabled = true
			};
			entity.MarkCreated();
			return entity;
		}
	}
}
