using Supabase;

namespace PortalForge.Infrastructure.Auth;

public interface ISupabaseClientFactory
{
    Task<Client> CreateClientAsync();
}
