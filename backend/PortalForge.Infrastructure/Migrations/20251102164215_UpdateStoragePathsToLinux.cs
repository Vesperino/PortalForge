using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoragePathsToLinux : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update Storage:BasePath from Windows to Linux path
            migrationBuilder.Sql(@"
                UPDATE ""SystemSettings""
                SET ""Value"" = '/app/storage'
                WHERE ""Key"" = 'Storage:BasePath'
                AND ""Value"" = 'C:\\PortalForge\\Storage';
            ");

            // Update Storage:NewsImagesPath from 'news-images' to 'images'
            migrationBuilder.Sql(@"
                UPDATE ""SystemSettings""
                SET ""Value"" = 'images'
                WHERE ""Key"" = 'Storage:NewsImagesPath'
                AND ""Value"" = 'news-images';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert Storage:BasePath to Windows path
            migrationBuilder.Sql(@"
                UPDATE ""SystemSettings""
                SET ""Value"" = 'C:\\PortalForge\\Storage'
                WHERE ""Key"" = 'Storage:BasePath'
                AND ""Value"" = '/app/storage';
            ");

            // Revert Storage:NewsImagesPath to 'news-images'
            migrationBuilder.Sql(@"
                UPDATE ""SystemSettings""
                SET ""Value"" = 'news-images'
                WHERE ""Key"" = 'Storage:NewsImagesPath'
                AND ""Value"" = 'images';
            ");
        }
    }
}
