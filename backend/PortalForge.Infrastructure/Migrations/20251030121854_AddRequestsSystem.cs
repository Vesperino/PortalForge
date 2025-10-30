using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentRole",
                schema: "public",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Employee");

            migrationBuilder.CreateTable(
                name: "RequestTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EstimatedProcessingDays = table.Column<int>(type: "integer", nullable: true),
                    PassingScore = table.Column<int>(type: "integer", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestTemplates_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_RequestTemplates_RequestTemplateId",
                        column: x => x.RequestTemplateId,
                        principalSchema: "public",
                        principalTable: "RequestTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestApprovalStepTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    ApproverRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequiresQuiz = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestApprovalStepTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestApprovalStepTemplates_RequestTemplates_RequestTempla~",
                        column: x => x.RequestTemplateId,
                        principalSchema: "public",
                        principalTable: "RequestTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedById = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Standard"),
                    FormData = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_RequestTemplates_RequestTemplateId",
                        column: x => x.RequestTemplateId,
                        principalSchema: "public",
                        principalTable: "RequestTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Users_SubmittedById",
                        column: x => x.SubmittedById,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequestTemplateFields",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FieldType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Placeholder = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    MinValue = table.Column<int>(type: "integer", nullable: true),
                    MaxValue = table.Column<int>(type: "integer", nullable: true),
                    HelpText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTemplateFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestTemplateFields_RequestTemplates_RequestTemplateId",
                        column: x => x.RequestTemplateId,
                        principalSchema: "public",
                        principalTable: "RequestTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestApprovalSteps",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequiresQuiz = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    QuizScore = table.Column<int>(type: "integer", nullable: true),
                    QuizPassed = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestApprovalSteps_Requests_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "public",
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestApprovalSteps_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizAnswers",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestApprovalStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedAnswer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAnswers_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalSchema: "public",
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAnswers_RequestApprovalSteps_RequestApprovalStepId",
                        column: x => x.RequestApprovalStepId,
                        principalSchema: "public",
                        principalTable: "RequestApprovalSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_QuizQuestionId",
                schema: "public",
                table: "QuizAnswers",
                column: "QuizQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_RequestApprovalStepId",
                schema: "public",
                table: "QuizAnswers",
                column: "RequestApprovalStepId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_RequestTemplateId_Order",
                schema: "public",
                table: "QuizQuestions",
                columns: new[] { "RequestTemplateId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalSteps_ApproverId",
                schema: "public",
                table: "RequestApprovalSteps",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalSteps_RequestId_StepOrder",
                schema: "public",
                table: "RequestApprovalSteps",
                columns: new[] { "RequestId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalSteps_Status",
                schema: "public",
                table: "RequestApprovalSteps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalStepTemplates_RequestTemplateId_StepOrder",
                schema: "public",
                table: "RequestApprovalStepTemplates",
                columns: new[] { "RequestTemplateId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestNumber",
                schema: "public",
                table: "Requests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestTemplateId",
                schema: "public",
                table: "Requests",
                column: "RequestTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status",
                schema: "public",
                table: "Requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_SubmittedAt",
                schema: "public",
                table: "Requests",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_SubmittedById",
                schema: "public",
                table: "Requests",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTemplateFields_RequestTemplateId_Order",
                schema: "public",
                table: "RequestTemplateFields",
                columns: new[] { "RequestTemplateId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestTemplates_Category",
                schema: "public",
                table: "RequestTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTemplates_CreatedById",
                schema: "public",
                table: "RequestTemplates",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTemplates_DepartmentId",
                schema: "public",
                table: "RequestTemplates",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTemplates_IsActive",
                schema: "public",
                table: "RequestTemplates",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizAnswers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RequestApprovalStepTemplates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RequestTemplateFields",
                schema: "public");

            migrationBuilder.DropTable(
                name: "QuizQuestions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RequestApprovalSteps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RequestTemplates",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "DepartmentRole",
                schema: "public",
                table: "Users");
        }
    }
}
