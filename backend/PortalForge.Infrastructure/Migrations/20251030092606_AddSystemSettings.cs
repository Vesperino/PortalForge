using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemSettings",
                columns: new[] { "Id", "Category", "Description", "Key", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 1, "Storage", "Base directory path for file storage", "Storage:BasePath", new DateTime(2025, 10, 30, 9, 0, 0, 0, DateTimeKind.Utc), null, "C:\\PortalForge\\Storage" },
                    { 2, "Storage", "Subdirectory for news images (relative to BasePath)", "Storage:NewsImagesPath", new DateTime(2025, 10, 30, 9, 0, 0, 0, DateTimeKind.Utc), null, "news-images" },
                    { 3, "Storage", "Subdirectory for documents (relative to BasePath)", "Storage:DocumentsPath", new DateTime(2025, 10, 30, 9, 0, 0, 0, DateTimeKind.Utc), null, "documents" },
                    { 4, "Storage", "Maximum file size in megabytes", "Storage:MaxFileSizeMB", new DateTime(2025, 10, 30, 9, 0, 0, 0, DateTimeKind.Utc), null, "10" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                schema: "public",
                table: "SystemSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_UpdatedBy",
                schema: "public",
                table: "SystemSettings",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings",
                schema: "public");
        }
    }
}
