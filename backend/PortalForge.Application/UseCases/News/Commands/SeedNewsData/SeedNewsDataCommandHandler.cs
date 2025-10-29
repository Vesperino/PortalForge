using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.News.Commands.SeedNewsData;

public class SeedNewsDataCommandHandler : IRequestHandler<SeedNewsDataCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeedNewsDataCommandHandler> _logger;

    public SeedNewsDataCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SeedNewsDataCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> Handle(SeedNewsDataCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting news data seeding...");

        var users = (await _unitOfWork.UserRepository.GetAllAsync()).ToList();

        if (!users.Any())
        {
            _logger.LogWarning("No users found in database. Cannot seed news data.");
            return 0;
        }

        var firstUser = users.First();
        var count = 0;

        var newsData = new[]
        {
            new
            {
                Title = "Witamy w nowym systemie newsów!",
                Excerpt = "Nowy system newsów pozwala na łatwe tworzenie i zarządzanie aktualnościami w organizacji.",
                Content = "<h2>Nowy system newsów</h2><p>Cieszymy się, że możemy przedstawić nowy system zarządzania aktualnościami w PortalForge.</p><p><strong>Funkcje systemu:</strong></p><ul><li>Tworzenie newsów z rich text editorem</li><li>Kategoryzacja aktualności</li><li>Filtrowanie i wyszukiwanie</li><li>Śledzenie wyświetleń</li></ul><p>Zapraszamy do korzystania!</p>",
                Category = NewsCategory.Announcement,
                ImageUrl = "https://images.unsplash.com/photo-1504711434969-e33886168f5c?w=800&q=80"
            },
            new
            {
                Title = "Nowe funkcje w aplikacji PortalForge",
                Excerpt = "Odkryj najnowsze ulepszenia i funkcjonalności dodane do platformy PortalForge.",
                Content = "<h2>Co nowego?</h2><p>W ostatniej aktualizacji dodaliśmy wiele nowych funkcji:</p><h3>System newsów</h3><p>Kompletny system zarządzania aktualnościami z rich text editorem.</p><h3>Ulepszona organizacja</h3><p>Lepsze zarządzanie strukturą organizacyjną i kalendarzem wydarzeń.</p><p>To dopiero początek - więcej nowości już wkrótce!</p>",
                Category = NewsCategory.Product,
                ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=800&q=80"
            },
            new
            {
                Title = "Szkolenie z nowych funkcji systemu",
                Excerpt = "Zapraszamy wszystkich pracowników na szkolenie z obsługi nowego systemu newsów.",
                Content = "<h2>Szkolenie online</h2><p>W przyszłym tygodniu odbędzie się szkolenie online z obsługi nowego systemu newsów.</p><p><strong>Termin:</strong> Piątek, 15:00-16:00</p><p><strong>Platforma:</strong> Microsoft Teams</p><p>Link do spotkania zostanie wysłany mailowo.</p><h3>Agenda szkolenia:</h3><ul><li>Tworzenie newsów</li><li>Formatowanie treści</li><li>Dodawanie obrazów</li><li>Publikacja i zarządzanie</li></ul>",
                Category = NewsCategory.Hr,
                ImageUrl = "https://images.unsplash.com/photo-1524178232363-1fb2b075b655?w=800&q=80"
            },
            new
            {
                Title = "Aktualizacja infrastruktury technicznej",
                Excerpt = "Informujemy o planowanej aktualizacji infrastruktury technicznej platformy.",
                Content = "<h2>Planowana przerwa techniczna</h2><p>W najbliższą sobotę przeprowadzimy aktualizację infrastruktury.</p><p><strong>Czas trwania:</strong> 02:00 - 06:00</p><p><strong>Wpływ:</strong> Platforma będzie niedostępna</p><h3>Co się zmieni?</h3><ul><li>Szybsze ładowanie stron</li><li>Lepsza stabilność</li><li>Nowe funkcje bezpieczeństwa</li></ul><p>Przepraszamy za niedogodności!</p>",
                Category = NewsCategory.Tech,
                ImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800&q=80"
            },
            new
            {
                Title = "Spotkanie All-Hands - Podsumowanie Q4",
                Excerpt = "Zapraszamy na spotkanie All-Hands gdzie podsumujemy ostatni kwartał i przedstawimy plany na przyszłość.",
                Content = "<h2>Spotkanie całej firmy</h2><p>Serdecznie zapraszamy wszystkich pracowników na spotkanie All-Hands.</p><p><strong>Data:</strong> 15 listopada 2024</p><p><strong>Godzina:</strong> 14:00</p><p><strong>Miejsce:</strong> Sala konferencyjna + Teams</p><h3>Program:</h3><ol><li>Podsumowanie Q4 2024</li><li>Wyniki finansowe</li><li>Plany na Q1 2025</li><li>Pytania i odpowiedzi</li></ol><p>Spotkanie będzie również transmitowane online dla pracowników zdalnych.</p>",
                Category = NewsCategory.Event,
                ImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?w=800&q=80"
            }
        };

        foreach (var data in newsData)
        {
            var news = new Domain.Entities.News
            {
                Title = data.Title,
                Excerpt = data.Excerpt,
                Content = data.Content,
                Category = data.Category,
                ImageUrl = data.ImageUrl,
                AuthorId = firstUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-count * 2),
                Views = new Random().Next(10, 100)
            };

            await _unitOfWork.NewsRepository.CreateAsync(news);
            count++;

            _logger.LogInformation("Seeded news: {Title}", news.Title);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} news articles", count);
        return count;
    }
}
