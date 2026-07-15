using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase4AccountAuditMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthAuditEvents",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "NVARCHAR(64)", nullable: false),
                    Outcome = table.Column<string>(type: "NVARCHAR(32)", nullable: false),
                    UserId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR(256)", nullable: true),
                    ClientId = table.Column<string>(type: "NVARCHAR(128)", nullable: true),
                    IpAddress = table.Column<string>(type: "NVARCHAR(64)", nullable: true),
                    Detail = table.Column<string>(type: "NVARCHAR(1024)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthAuditEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthAuditEvents_CreatedAt",
                schema: "IdentityDb",
                table: "AuthAuditEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuthAuditEvents_EventType",
                schema: "IdentityDb",
                table: "AuthAuditEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuthAuditEvents_UserId",
                schema: "IdentityDb",
                table: "AuthAuditEvents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthAuditEvents",
                schema: "IdentityDb");
        }
    }
}
