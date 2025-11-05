using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParallelApprovalEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceCategory",
                schema: "public",
                table: "RequestTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllowedFileTypes",
                schema: "public",
                table: "RequestTemplateFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutoCompleteSource",
                schema: "public",
                table: "RequestTemplateFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConditionalLogic",
                schema: "public",
                table: "RequestTemplateFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultValue",
                schema: "public",
                table: "RequestTemplateFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FileMaxSize",
                schema: "public",
                table: "RequestTemplateFields",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConditional",
                schema: "public",
                table: "RequestTemplateFields",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ValidationRules",
                schema: "public",
                table: "RequestTemplateFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClonedFromId",
                schema: "public",
                table: "Requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                schema: "public",
                table: "Requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ServiceCategory",
                schema: "public",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ServiceCompletedAt",
                schema: "public",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceNotes",
                schema: "public",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceStatus",
                schema: "public",
                table: "Requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                schema: "public",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EscalationTimeout",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EscalationUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsParallel",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MinimumApprovals",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ParallelGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ClonedFromId",
                schema: "public",
                table: "Requests",
                column: "ClonedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalStepTemplates_EscalationUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "EscalationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalStepTemplates_ParallelGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "ParallelGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApprovalStepTemplates_Users_EscalationUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                column: "EscalationUserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Requests_ClonedFromId",
                schema: "public",
                table: "Requests",
                column: "ClonedFromId",
                principalSchema: "public",
                principalTable: "Requests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestApprovalStepTemplates_Users_EscalationUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Requests_ClonedFromId",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ClonedFromId",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_RequestApprovalStepTemplates_EscalationUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropIndex(
                name: "IX_RequestApprovalStepTemplates_ParallelGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "ServiceCategory",
                schema: "public",
                table: "RequestTemplates");

            migrationBuilder.DropColumn(
                name: "AllowedFileTypes",
                schema: "public",
                table: "RequestTemplateFields");

            migrationBuilder.DropColumn(
                name: "AutoCompleteSource",
                schema: "public",
                table: "RequestTemplateFields");

            migrationBuilder.DropColumn(
                name: "ConditionalLogic",
                schema: "public",
                table: "RequestTemplateFields");

            migrationBuilder.DropColumn(
                name: "DefaultValue",
                schema: "public",
                table: "RequestTemplateFields");

            migrationBuilder.DropColumn(
                name: "FileMaxSize",
                schema: "public",
                table: "RequestTemplateFields");

            migrationBuilder.DropColumn(
                name: "IsConditional",
                schema: "public",
                table: "RequestTemplateFields");

            migrationBuilder.DropColumn(
                name: "ValidationRules",
                schema: "public",
                table: "RequestTemplateFields");

            migrationBuilder.DropColumn(
                name: "ClonedFromId",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "IsTemplate",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ServiceCategory",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ServiceCompletedAt",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ServiceNotes",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ServiceStatus",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Tags",
                schema: "public",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "EscalationTimeout",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "EscalationUserId",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "IsParallel",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "MinimumApprovals",
                schema: "public",
                table: "RequestApprovalStepTemplates");

            migrationBuilder.DropColumn(
                name: "ParallelGroupId",
                schema: "public",
                table: "RequestApprovalStepTemplates");
        }
    }
}
