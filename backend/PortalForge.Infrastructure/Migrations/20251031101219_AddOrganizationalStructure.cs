using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationalStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "public",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecificDepartmentId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ParentDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeadOfDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Departments_Users_HeadOfDepartmentId",
                        column: x => x.HeadOfDepartmentId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationalPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CanViewAllDepartments = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VisibleDepartmentIds = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationalPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationalPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VacationSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubstituteUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    SourceRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacationSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VacationSchedules_Requests_SourceRequestId",
                        column: x => x.SourceRequestId,
                        principalSchema: "public",
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VacationSchedules_Users_SubstituteUserId",
                        column: x => x.SubstituteUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VacationSchedules_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                schema: "public",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalStepTemplates_SpecificDepartmentId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "SpecificDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsActive",
                table: "Departments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationalPermissions_UserId",
                table: "OrganizationalPermissions",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VacationSchedules_EndDate",
                table: "VacationSchedules",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_VacationSchedules_SourceRequestId",
                table: "VacationSchedules",
                column: "SourceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_VacationSchedules_StartDate",
                table: "VacationSchedules",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_VacationSchedules_StartDate_EndDate",
                table: "VacationSchedules",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VacationSchedules_Status",
                table: "VacationSchedules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VacationSchedules_SubstituteUserId",
                table: "VacationSchedules",
                column: "SubstituteUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VacationSchedules_UserId",
                table: "VacationSchedules",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApprovalStepTemplates_Departments_SpecificDepartment~",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "SpecificDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                schema: "public",
                table: "Users",
                column: "DepartmentId",
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

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                schema: "public",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "OrganizationalPermissions");

            migrationBuilder.DropTable(
                name: "VacationSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Users_DepartmentId",
                schema: "public",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_RequestApprovalStepTemplates_SpecificDepartmentId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpecificDepartmentId",
                schema: "public",
                table: "RequestApprovalStepTemplates");
        }
    }
}
