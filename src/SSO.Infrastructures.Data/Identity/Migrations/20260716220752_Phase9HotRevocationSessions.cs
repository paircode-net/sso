using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase9HotRevocationSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientWebhookEndpoints",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1024)", nullable: false),
                    HmacSecret = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientWebhookEndpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RevokedSessions",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    SessionId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    UserId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevokedSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    UserId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true),
                    BranchId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokeReason = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebhookOutbox",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1024)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookOutbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientWebhookEndpoints_ClientId",
                schema: "IdentityDb",
                table: "ClientWebhookEndpoints",
                column: "ClientId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RevokedSessions_ExpiresAt",
                schema: "IdentityDb",
                table: "RevokedSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RevokedSessions_SessionId",
                schema: "IdentityDb",
                table: "RevokedSessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                schema: "IdentityDb",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_RevokedAt",
                schema: "IdentityDb",
                table: "UserSessions",
                columns: new[] { "UserId", "RevokedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookOutbox_Status_NextAttemptAt",
                schema: "IdentityDb",
                table: "WebhookOutbox",
                columns: new[] { "Status", "NextAttemptAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientWebhookEndpoints",
                schema: "IdentityDb");

            migrationBuilder.DropTable(
                name: "RevokedSessions",
                schema: "IdentityDb");

            migrationBuilder.DropTable(
                name: "UserSessions",
                schema: "IdentityDb");

            migrationBuilder.DropTable(
                name: "WebhookOutbox",
                schema: "IdentityDb");
        }
    }
}
