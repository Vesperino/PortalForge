using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVacationAndEmploymentFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserAgent",
                schema: "public",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "AuditLogs",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Changes",
                schema: "public",
                table: "AuditLogs",
                newName: "OldValue");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_CreatedAt",
                schema: "public",
                table: "AuditLogs",
                newName: "IX_AuditLogs_Timestamp");

            migrationBuilder.AddColumn<int>(
                name: "AnnualVacationDays",
                schema: "public",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CarriedOverExpiryDate",
                schema: "public",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CarriedOverVacationDays",
                schema: "public",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CircumstantialLeaveDaysUsed",
                schema: "public",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotificationsEnabled",
                schema: "public",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmploymentStartDate",
                schema: "public",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnProbation",
                schema: "public",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OnDemandVacationDaysUsed",
                schema: "public",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProbationEndDate",
                schema: "public",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VacationDaysUsed",
                schema: "public",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsAttachments",
                schema: "public",
                table: "RequestTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSickLeaveRequest",
                schema: "public",
                table: "RequestTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVacationRequest",
                schema: "public",
                table: "RequestTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetrospectiveDays",
                schema: "public",
                table: "RequestTemplates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attachments",
                schema: "public",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeaveType",
                schema: "public",
                table: "Requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DirectorSubstituteId",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DirectorSubstituteId1",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HeadOfDepartmentSubstituteId",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HeadOfDepartmentSubstituteId1",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                schema: "public",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                schema: "public",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                schema: "public",
                table: "AuditLogs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "public",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RequestComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    Attachments = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestComments_Requests_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "public",
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestComments_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestEditHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    EditedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EditedById = table.Column<Guid>(type: "uuid", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldFormData = table.Column<string>(type: "text", nullable: false),
                    NewFormData = table.Column<string>(type: "text", nullable: false),
                    ChangeReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestEditHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestEditHistories_Requests_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "public",
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestEditHistories_Users_EditedById",
                        column: x => x.EditedById,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SickLeaves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaysCount = table.Column<int>(type: "integer", nullable: false),
                    RequiresZusDocument = table.Column<bool>(type: "boolean", nullable: false),
                    ZusDocumentUrl = table.Column<string>(type: "text", nullable: true),
                    SourceRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickLeaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SickLeaves_Requests_SourceRequestId",
                        column: x => x.SourceRequestId,
                        principalSchema: "public",
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SickLeaves_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DirectorSubstituteId1",
                table: "Departments",
                column: "DirectorSubstituteId1");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadOfDepartmentSubstituteId1",
                table: "Departments",
                column: "HeadOfDepartmentSubstituteId1");

            migrationBuilder.CreateIndex(
                name: "IX_RequestComments_RequestId",
                table: "RequestComments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestComments_UserId",
                table: "RequestComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestEditHistories_EditedById",
                table: "RequestEditHistories",
                column: "EditedById");

            migrationBuilder.CreateIndex(
                name: "IX_RequestEditHistories_RequestId",
                table: "RequestEditHistories",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SickLeaves_SourceRequestId",
                table: "SickLeaves",
                column: "SourceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SickLeaves_UserId",
                table: "SickLeaves",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_DirectorSubstituteId1",
                table: "Departments",
                column: "DirectorSubstituteId1",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_HeadOfDepartmentSubstituteId1",
                table: "Departments",
                column: "HeadOfDepartmentSubstituteId1",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_DirectorSubstituteId1",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_HeadOfDepartmentSubstituteId1",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "RequestComments");

            migrationBuilder.DropTable(
                name: "RequestEditHistories");

            migrationBuilder.DropTable(
                name: "SickLeaves");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DirectorSubstituteId1",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HeadOfDepartmentSubstituteId1",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "AnnualVacationDays",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CarriedOverExpiryDate",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CarriedOverVacationDays",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CircumstantialLeaveDaysUsed",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailNotificationsEnabled",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmploymentStartDate",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsOnProbation",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OnDemandVacationDaysUsed",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProbationEndDate",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VacationDaysUsed",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowsAttachments",
                schema: "public",
                table: "RequestTemplates");

            migrationBuilder.DropColumn(
                name: "IsSickLeaveRequest",
                schema: "public",
                table: "RequestTemplates");

            migrationBuilder.DropColumn(
                name: "IsVacationRequest",
                schema: "public",
                table: "RequestTemplates");

            migrationBuilder.DropColumn(
                name: "MaxRetrospectiveDays",
                schema: "public",
                table: "RequestTemplates");

            migrationBuilder.DropColumn(
                name: "Attachments",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "LeaveType",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DirectorSubstituteId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DirectorSubstituteId1",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HeadOfDepartmentSubstituteId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HeadOfDepartmentSubstituteId1",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "NewValue",
                schema: "public",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "public",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                schema: "public",
                table: "AuditLogs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "OldValue",
                schema: "public",
                table: "AuditLogs",
                newName: "Changes");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_Timestamp",
                schema: "public",
                table: "AuditLogs",
                newName: "IX_AuditLogs_CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                schema: "public",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                schema: "public",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                schema: "public",
                table: "AuditLogs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
