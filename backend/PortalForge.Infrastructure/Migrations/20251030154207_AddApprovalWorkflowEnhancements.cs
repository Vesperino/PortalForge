using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalWorkflowEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApproverRole",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "ApproverGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproverType",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Role");

            migrationBuilder.AddColumn<Guid>(
                name: "SpecificUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ActionUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalStepTemplates_ApproverGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "ApproverGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalStepTemplates_SpecificUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "SpecificUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                schema: "public",
                table: "Notifications",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                schema: "public",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApprovalStepTemplates_RoleGroups_ApproverGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "ApproverGroupId",
                principalSchema: "public",
                principalTable: "RoleGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApprovalStepTemplates_Users_SpecificUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "SpecificUserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestApprovalStepTemplates_RoleGroups_ApproverGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestApprovalStepTemplates_Users_SpecificUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_RequestApprovalStepTemplates_ApproverGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropIndex(
                name: "IX_RequestApprovalStepTemplates_SpecificUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "ApproverGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "ApproverType",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "SpecificUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "ApproverRole",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
