using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAISystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemSettings",
                columns: new[] { "Category", "Description", "Key", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { "AI", "Encrypted OpenAI API key for AI features", "AI:OpenAIApiKey", DateTime.UtcNow, null, "" },
                    { "AI", "OpenAI model to use for translation (e.g., gpt-4, gpt-3.5-turbo)", "AI:TranslationModel", DateTime.UtcNow, null, "gpt-4" },
                    { "AI", "OpenAI model to use for standard chat (e.g., gpt-4, gpt-3.5-turbo)", "AI:ChatModel", DateTime.UtcNow, null, "gpt-4" },
                    { "AI", "Maximum tokens per AI request", "AI:MaxTokensPerRequest", DateTime.UtcNow, null, "4000" },
                    { "AI", "Maximum characters allowed in translation input", "AI:TranslationMaxCharacters", DateTime.UtcNow, null, "8000" },
                    { "AI", "Temperature for AI responses (0.0-1.0)", "AI:Temperature", DateTime.UtcNow, null, "0.7" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "SystemSettings",
                keyColumn: "Key",
                keyValues: new object[]
                {
                    "AI:OpenAIApiKey",
                    "AI:TranslationModel",
                    "AI:ChatModel",
                    "AI:MaxTokensPerRequest",
                    "AI:TranslationMaxCharacters",
                    "AI:Temperature"
                });
        }
    }
}
