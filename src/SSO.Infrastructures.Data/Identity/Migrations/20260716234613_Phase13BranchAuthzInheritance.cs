using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase13BranchAuthzInheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Inheritable",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inheritable",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BranchAuthzInheritance",
                schema: "IdentityDb",
                table: "Organizations",
                type: "NVARCHAR(32)",
                nullable: false,
                defaultValue: "Off");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inheritable",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropColumn(
                name: "Inheritable",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropColumn(
                name: "BranchAuthzInheritance",
                schema: "IdentityDb",
                table: "Organizations");
        }
    }
}
