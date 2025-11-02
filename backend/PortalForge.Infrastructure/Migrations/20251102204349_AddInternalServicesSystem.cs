using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalServicesSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InternalServiceCategories",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Icon = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalServiceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InternalServices",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Icon = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IconType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsGlobal = table.Column<bool>(type: "boolean", nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalServices_InternalServiceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "public",
                        principalTable: "InternalServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InternalServices_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternalServiceDepartments",
                schema: "public",
                columns: table => new
                {
                    InternalServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalServiceDepartments", x => new { x.InternalServiceId, x.DepartmentId });
                    table.ForeignKey(
                        name: "FK_InternalServiceDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternalServiceDepartments_InternalServices_InternalService~",
                        column: x => x.InternalServiceId,
                        principalSchema: "public",
                        principalTable: "InternalServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InternalServiceDepartments_DepartmentId",
                schema: "public",
                table: "InternalServiceDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalServices_CategoryId",
                schema: "public",
                table: "InternalServices",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalServices_CreatedById",
                schema: "public",
                table: "InternalServices",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InternalServiceDepartments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InternalServices",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InternalServiceCategories",
                schema: "public");
        }
    }
}
