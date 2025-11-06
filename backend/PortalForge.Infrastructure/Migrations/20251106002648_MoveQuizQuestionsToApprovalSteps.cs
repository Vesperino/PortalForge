using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveQuizQuestionsToApprovalSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_RequestTemplates_RequestTemplateId",
                schema: "public",
                table: "QuizQuestions");

            migrationBuilder.RenameColumn(
                name: "RequestTemplateId",
                schema: "public",
                table: "QuizQuestions",
                newName: "RequestApprovalStepTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizQuestions_RequestTemplateId_Order",
                schema: "public",
                table: "QuizQuestions",
                newName: "IX_QuizQuestions_RequestApprovalStepTemplateId_Order");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_RequestApprovalStepTemplates_RequestApprovalS~",
                schema: "public",
                table: "QuizQuestions",
                column: "RequestApprovalStepTemplateId",
                principalSchema: "public",
                principalTable: "RequestApprovalStepTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_RequestApprovalStepTemplates_RequestApprovalS~",
                schema: "public",
                table: "QuizQuestions");

            migrationBuilder.RenameColumn(
                name: "RequestApprovalStepTemplateId",
                schema: "public",
                table: "QuizQuestions",
                newName: "RequestTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizQuestions_RequestApprovalStepTemplateId_Order",
                schema: "public",
                table: "QuizQuestions",
                newName: "IX_QuizQuestions_RequestTemplateId_Order");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_RequestTemplates_RequestTemplateId",
                schema: "public",
                table: "QuizQuestions",
                column: "RequestTemplateId",
                principalSchema: "public",
                principalTable: "RequestTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
