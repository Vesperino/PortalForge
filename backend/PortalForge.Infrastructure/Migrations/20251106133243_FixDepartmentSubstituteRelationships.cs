using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDepartmentSubstituteRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_DirectorSubstituteId1",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_HeadOfDepartmentSubstituteId1",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DirectorSubstituteId1",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HeadOfDepartmentSubstituteId1",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DirectorSubstituteId1",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HeadOfDepartmentSubstituteId1",
                table: "Departments");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DirectorSubstituteId",
                table: "Departments",
                column: "DirectorSubstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadOfDepartmentSubstituteId",
                table: "Departments",
                column: "HeadOfDepartmentSubstituteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_DirectorSubstituteId",
                table: "Departments",
                column: "DirectorSubstituteId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_HeadOfDepartmentSubstituteId",
                table: "Departments",
                column: "HeadOfDepartmentSubstituteId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_DirectorSubstituteId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_HeadOfDepartmentSubstituteId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DirectorSubstituteId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HeadOfDepartmentSubstituteId",
                table: "Departments");

            migrationBuilder.AddColumn<Guid>(
                name: "DirectorSubstituteId1",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HeadOfDepartmentSubstituteId1",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DirectorSubstituteId1",
                table: "Departments",
                column: "DirectorSubstituteId1");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadOfDepartmentSubstituteId1",
                table: "Departments",
                column: "HeadOfDepartmentSubstituteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_DirectorSubstituteId1",
                table: "Departments",
                column: "DirectorSubstituteId1",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_HeadOfDepartmentSubstituteId1",
                table: "Departments",
                column: "HeadOfDepartmentSubstituteId1",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
