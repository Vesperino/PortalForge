using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EncryptExistingAPIKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear any existing plain-text API key for security
            // Admin will need to re-enter the key, which will be encrypted automatically
            migrationBuilder.Sql(@"
                UPDATE public.""SystemSettings""
                SET ""Value"" = ''
                WHERE ""Key"" = 'AI:OpenAIApiKey';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
