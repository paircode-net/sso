using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase6ExternalIdpsHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalIdentityProviders",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true),
                    ProviderType = table.Column<string>(type: "NVARCHAR(32)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR(64)", nullable: false),
                    DisplayName = table.Column<string>(type: "NVARCHAR(128)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Authority = table.Column<string>(type: "NVARCHAR(512)", nullable: true),
                    ClientId = table.Column<string>(type: "NVARCHAR(128)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalIdentityProviders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalIdentityProviders_Code",
                schema: "IdentityDb",
                table: "ExternalIdentityProviders",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalIdentityProviders",
                schema: "IdentityDb");
        }
    }
}
