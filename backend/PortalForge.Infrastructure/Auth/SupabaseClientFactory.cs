using Microsoft.Extensions.Options;
using Supabase;

namespace PortalForge.Infrastructure.Auth;

public class SupabaseClientFactory : ISupabaseClientFactory
{
    private readonly SupabaseSettings _settings;
    private Client? _client;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public SupabaseClientFactory(IOptions<SupabaseSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<Client> CreateClientAsync()
    {
        if (_client != null)
            return _client;

        await _lock.WaitAsync();
        try
        {
            if (_client != null)
                return _client;

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

            _client = new Client(_settings.Url, _settings.Key, options);
            await _client.InitializeAsync();
            return _client;
        }
        finally
        {
            _lock.Release();
        }
    }
}
