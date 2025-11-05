using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatAIPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var permissionId = Guid.NewGuid();

            migrationBuilder.InsertData(
                schema: "public",
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Category", "CreatedAt" },
                values: new object[]
                {
                    permissionId,
                    "chatai.use",
                    "Allows access to AI Chat features including translation and standard chat",
                    "AI",
                    DateTime.UtcNow
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "Permissions",
                keyColumn: "Name",
                keyValue: "chatai.use");
        }
    }
}
