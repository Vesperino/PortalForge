using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventFieldsToNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                schema: "public",
                table: "News",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EventDateTime",
                schema: "public",
                table: "News",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventHashtag",
                schema: "public",
                table: "News",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventLocation",
                schema: "public",
                table: "News",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEvent",
                schema: "public",
                table: "News",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_News_DepartmentId",
                schema: "public",
                table: "News",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_News_EventDateTime",
                schema: "public",
                table: "News",
                column: "EventDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_News_IsEvent",
                schema: "public",
                table: "News",
                column: "IsEvent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_News_DepartmentId",
                schema: "public",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_EventDateTime",
                schema: "public",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_IsEvent",
                schema: "public",
                table: "News");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "public",
                table: "News");

            migrationBuilder.DropColumn(
                name: "EventDateTime",
                schema: "public",
                table: "News");

            migrationBuilder.DropColumn(
                name: "EventHashtag",
                schema: "public",
                table: "News");

            migrationBuilder.DropColumn(
                name: "EventLocation",
                schema: "public",
                table: "News");

            migrationBuilder.DropColumn(
                name: "IsEvent",
                schema: "public",
                table: "News");
        }
    }
}
