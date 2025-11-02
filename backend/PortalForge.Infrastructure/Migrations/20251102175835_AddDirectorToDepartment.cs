using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectorToDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DirectorId",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DirectorId",
                table: "Departments",
                column: "DirectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_DirectorId",
                table: "Departments",
                column: "DirectorId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_DirectorId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DirectorId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DirectorId",
                table: "Departments");
        }
    }
}
