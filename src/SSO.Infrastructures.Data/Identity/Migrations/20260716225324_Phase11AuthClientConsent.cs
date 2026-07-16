using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase11AuthClientConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthClientMetadata",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsFirstParty = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RequireConsent = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    ConsentRememberDays = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthClientMetadata", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthClientMetadata_ClientId",
                schema: "IdentityDb",
                table: "AuthClientMetadata",
                column: "ClientId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthClientMetadata",
                schema: "IdentityDb");
        }
    }
}
