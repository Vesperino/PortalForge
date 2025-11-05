using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDepartmentRoleAndApproverRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentRole",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApproverRole",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "ApproverType",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "DirectSupervisor",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentRole",
                schema: "public",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Employee");

            migrationBuilder.AlterColumn<string>(
                name: "ApproverType",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Role",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "DirectSupervisor");

            migrationBuilder.AddColumn<string>(
                name: "ApproverRole",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
