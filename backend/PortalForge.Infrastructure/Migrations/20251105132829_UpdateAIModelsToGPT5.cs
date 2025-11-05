using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAIModelsToGPT5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update Translation Model to GPT-5 Mini
            migrationBuilder.Sql(@"
                UPDATE public.""SystemSettings""
                SET ""Value"" = 'gpt-5-mini-2025-08-07'
                WHERE ""Key"" = 'AI:TranslationModel';
            ");

            // Update Chat Model to GPT-5
            migrationBuilder.Sql(@"
                UPDATE public.""SystemSettings""
                SET ""Value"" = 'gpt-5-2025-08-07'
                WHERE ""Key"" = 'AI:ChatModel';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert Translation Model to GPT-4
            migrationBuilder.Sql(@"
                UPDATE public.""SystemSettings""
                SET ""Value"" = 'gpt-4'
                WHERE ""Key"" = 'AI:TranslationModel';
            ");

            // Revert Chat Model to GPT-4
            migrationBuilder.Sql(@"
                UPDATE public.""SystemSettings""
                SET ""Value"" = 'gpt-4'
                WHERE ""Key"" = 'AI:ChatModel';
            ");
        }
    }
}
