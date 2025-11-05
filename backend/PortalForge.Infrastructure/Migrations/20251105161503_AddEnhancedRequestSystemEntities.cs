using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnhancedRequestSystemEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToUserId",
                schema: "public",
                table: "RequestApprovalSteps",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "RequestApprovalSteps",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EscalatedAt",
                schema: "public",
                table: "RequestApprovalSteps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StepTemplateId",
                schema: "public",
                table: "RequestApprovalSteps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApprovalDelegations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDelegations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalDelegations_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalDelegations_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    InAppEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DigestEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DigestFrequency = table.Column<string>(type: "text", nullable: false),
                    DisabledTypes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, defaultValue: "[]"),
                    GroupSimilarNotifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MaxGroupSize = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    GroupingTimeWindowMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    RealTimeEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MessageTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EmailSubjectTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmailBodyTemplate = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "pl"),
                    PlaceholderDefinitions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplates_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RequestAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalRequests = table.Column<int>(type: "integer", nullable: false),
                    ApprovedRequests = table.Column<int>(type: "integer", nullable: false),
                    RejectedRequests = table.Column<int>(type: "integer", nullable: false),
                    PendingRequests = table.Column<int>(type: "integer", nullable: false),
                    AverageProcessingTime = table.Column<double>(type: "double precision", nullable: false),
                    LastCalculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestAnalytics_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalSteps_AssignedToUserId",
                schema: "public",
                table: "RequestApprovalSteps",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalSteps_StepTemplateId",
                schema: "public",
                table: "RequestApprovalSteps",
                column: "StepTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDelegations_FromUserId",
                schema: "public",
                table: "ApprovalDelegations",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDelegations_IsActive_StartDate_EndDate",
                schema: "public",
                table: "ApprovalDelegations",
                columns: new[] { "IsActive", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDelegations_ToUserId",
                schema: "public",
                table: "ApprovalDelegations",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_DigestEnabled_DigestFrequency",
                table: "NotificationPreferences",
                columns: new[] { "DigestEnabled", "DigestFrequency" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_CreatedAt",
                table: "NotificationTemplates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_CreatedById",
                table: "NotificationTemplates",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsActive",
                table: "NotificationTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Type_Language_IsActive",
                table: "NotificationTemplates",
                columns: new[] { "Type", "Language", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestAnalytics_Period",
                table: "RequestAnalytics",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestAnalytics_User_Period",
                table: "RequestAnalytics",
                columns: new[] { "UserId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestAnalytics_UserId",
                table: "RequestAnalytics",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApprovalSteps_RequestApprovalStepTemplates_StepTempl~",
                schema: "public",
                table: "RequestApprovalSteps",
                column: "StepTemplateId",
                principalSchema: "public",
                principalTable: "RequestApprovalStepTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApprovalSteps_Users_AssignedToUserId",
                schema: "public",
                table: "RequestApprovalSteps",
                column: "AssignedToUserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestApprovalSteps_RequestApprovalStepTemplates_StepTempl~",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestApprovalSteps_Users_AssignedToUserId",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropTable(
                name: "ApprovalDelegations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "RequestAnalytics");

            migrationBuilder.DropIndex(
                name: "IX_RequestApprovalSteps_AssignedToUserId",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropIndex(
                name: "IX_RequestApprovalSteps_StepTemplateId",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropColumn(
                name: "EscalatedAt",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropColumn(
                name: "StepTemplateId",
                schema: "public",
                table: "RequestApprovalSteps");
        }
    }
}
