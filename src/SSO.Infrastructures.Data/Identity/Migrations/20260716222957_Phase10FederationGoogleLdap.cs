using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase10FederationGoogleLdap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowJitProvisioning",
                schema: "IdentityDb",
                table: "ExternalIdentityProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LdapGroupRoleMaps",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    GroupIdentifier = table.Column<string>(type: "nvarchar(512)", nullable: false),
                    RoleId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    ProductId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    BranchId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LdapGroupRoleMaps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LdapGroupRoleMaps_OrganizationId_GroupIdentifier_RoleId_ProductId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                columns: new[] { "OrganizationId", "GroupIdentifier", "RoleId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LdapGroupRoleMaps",
                schema: "IdentityDb");

            migrationBuilder.DropColumn(
                name: "AllowJitProvisioning",
                schema: "IdentityDb",
                table: "ExternalIdentityProviders");
        }
    }
}
