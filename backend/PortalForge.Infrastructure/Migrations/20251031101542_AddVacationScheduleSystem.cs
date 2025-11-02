using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVacationScheduleSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestApprovalStepTemplates_Departments_SpecificDepartment~",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSubstituteSelection",
                schema: "public",
                table: "RequestTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApprovalStepTemplates_Departments_SpecificDepartment~",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "SpecificDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestApprovalStepTemplates_Departments_SpecificDepartment~",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "RequiresSubstituteSelection",
                schema: "public",
                table: "RequestTemplates");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApprovalStepTemplates_Departments_SpecificDepartment~",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "SpecificDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }
    }
}
