namespace PortalForge.Infrastructure.Auth;

public class SupabaseSettings
{
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty; // Anon key for Supabase Auth client
    public string ServiceRoleKey { get; set; } = string.Empty; // Service role key for admin operations
    public string JwtSecret { get; set; } = string.Empty; // JWT secret for token validation
}
