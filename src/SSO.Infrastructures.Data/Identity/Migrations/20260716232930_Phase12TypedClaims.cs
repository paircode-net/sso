using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase12TypedClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthRoleClaims",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    RoleId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    ClaimDefinitionId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(512)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthRoleClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClaimDefinitions",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    ValueType = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    ProductId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserClaimAssignments",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    UserId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    ClaimDefinitionId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(512)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true),
                    BranchId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true),
                    ProductId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaimAssignments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthRoleClaims_RoleId_ClaimDefinitionId",
                schema: "IdentityDb",
                table: "AuthRoleClaims",
                columns: new[] { "RoleId", "ClaimDefinitionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimDefinitions_Code",
                schema: "IdentityDb",
                table: "ClaimDefinitions",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaimAssignments_UserId_ClaimDefinitionId_OrganizationId_BranchId_ProductId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                columns: new[] { "UserId", "ClaimDefinitionId", "OrganizationId", "BranchId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthRoleClaims",
                schema: "IdentityDb");

            migrationBuilder.DropTable(
                name: "ClaimDefinitions",
                schema: "IdentityDb");

            migrationBuilder.DropTable(
                name: "UserClaimAssignments",
                schema: "IdentityDb");
        }
    }
}
