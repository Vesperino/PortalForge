using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizFieldsToApprovalStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PassingScore",
                schema: "public",
                table: "RequestApprovalSteps",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestApprovalStepTemplateId",
                schema: "public",
                table: "RequestApprovalSteps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalSteps_RequestApprovalStepTemplateId",
                schema: "public",
                table: "RequestApprovalSteps",
                column: "RequestApprovalStepTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApprovalSteps_RequestApprovalStepTemplates_RequestAp~",
                schema: "public",
                table: "RequestApprovalSteps",
                column: "RequestApprovalStepTemplateId",
                principalSchema: "public",
                principalTable: "RequestApprovalStepTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestApprovalSteps_RequestApprovalStepTemplates_RequestAp~",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropIndex(
                name: "IX_RequestApprovalSteps_RequestApprovalStepTemplateId",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropColumn(
                name: "PassingScore",
                schema: "public",
                table: "RequestApprovalSteps");

            migrationBuilder.DropColumn(
                name: "RequestApprovalStepTemplateId",
                schema: "public",
                table: "RequestApprovalSteps");
        }
    }
}
