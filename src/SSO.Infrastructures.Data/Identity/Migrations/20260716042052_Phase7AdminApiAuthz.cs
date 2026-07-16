using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase7AdminApiAuthz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                type: "UNIQUEIDENTIFIER",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "UNIQUEIDENTIFIER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                type: "UNIQUEIDENTIFIER",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "UNIQUEIDENTIFIER",
                oldNullable: true);
        }
    }
}
