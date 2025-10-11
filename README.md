# PortalForge

[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Nuxt Version](https://img.shields.io/badge/Nuxt-3-00DC82)](https://nuxt.com/)
[![License](https://img.shields.io/badge/license-All%20Rights%20Reserved-red)](LICENSE)

> Wewnętrzny portal intranetowy dla organizacji 200+ pracowników

## Przegląd projektu

**PortalForge** to scentralizowana platforma komunikacji zaprojektowana do rozwiązania problemów związanych z chaosem dokumentowym i brakiem centralizacji informacji w dużych organizacjach. System dostarcza kompleksowe narzędzia do zarządzania strukturą organizacyjną, kalendarzem wydarzeń firmowych oraz komunikacją wewnętrzną.

### Kluczowe funkcje

- 🔐 **Autoryzacja i uwierzytelnianie** - Supabase Auth z kontrolą dostępu opartą na rolach (Admin, Manager, HR, Marketing, Pracownik)
- 👥 **Struktura organizacyjna** - Wizualizacja i zarządzanie hierarchicznym drzewem z pełnymi operacjami CRUD
- 📅 **Kalendarz wydarzeń firmowych** - Zarządzanie wydarzeniami z systemem tagowania i targetowania działów
- 📰 **System newsów** - Komunikacja wewnętrzna z integracją wydarzeń
- 📊 **Monitoring aktywności** - Śledzenie i raportowanie aktywności użytkowników
- 📤 **Import/Export** - Import użytkowników z CSV/Excel, eksport struktury org do PDF/Excel

## Stos technologiczny

### Backend
- **Framework**: [.NET 8.0](https://dotnet.microsoft.com/) (LTS do listopada 2026)
- **Architektura**: Clean Architecture + wzorzec CQRS
- **Mediator**: [MediatR](https://github.com/jbogard/MediatR) do implementacji CQRS
- **ORM**: [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- **Baza danych**: [PostgreSQL](https://www.postgresql.org/) via [Supabase](https://supabase.com/)
- **Uwierzytelnianie**: Supabase Auth
- **Logowanie**: [Serilog](https://serilog.net/)
- **Walidacja**: [FluentValidation](https://fluentvalidation.net/)
- **Testy**: [xUnit](https://xunit.net/) + [FluentAssertions](https://fluentassertions.com/) + [Moq](https://github.com/moq/moq4)

### Frontend
- **Framework**: [Nuxt 3](https://nuxt.com/) (Vue 3 z Composition API)
- **Język**: [TypeScript](https://www.typescriptlang.org/)
- **Stylowanie**: [Tailwind CSS](https://tailwindcss.com/)
- **Zarządzanie stanem**: [Pinia](https://pinia.vuejs.org/)
- **Narzędzia**: [VueUse](https://vueuse.org/)
- **Testy**: [Vitest](https://vitest.dev/) (jednostkowe) + [Playwright](https://playwright.dev/) (E2E)

### Infrastruktura
- **VCS**: Git + GitHub
- **CI/CD**: GitHub Actions
- **Konteneryzacja**: Docker
- **Baza danych & Auth**: Supabase
- **Hosting**: VPS (do skonfigurowania)

## Struktura projektu

```
PortalForge/
├── .ai/                              # Dokumentacja AI i kontekst
│   ├── prd.md                       # Dokument wymagań produktowych
│   ├── tech-stack.md                # Analiza stosu technologicznego
│   ├── backend/                     # Dokumentacja backendu
│   ├── frontend/                    # Dokumentacja frontendu
│   ├── progress/                    # Logi postępu rozwoju
│   └── decisions/                   # Rekordy decyzji architektonicznych (ADR)
├── .claude/                          # Konfiguracja Claude Code
│   └── CLAUDE.md                    # Reguły AI i kontekst projektu
├── .github/
│   └── workflows/                   # Pipeline CI/CD GitHub Actions
├── backend/
│   ├── PortalForge.Api/            # Warstwa prezentacji
│   │   ├── Controllers/            # Kontrolery REST API
│   │   ├── Middleware/             # Własne middleware
│   │   ├── Dtos/                   # DTO żądań/odpowiedzi
│   │   └── Program.cs              # Punkt wejścia aplikacji
│   ├── PortalForge.Application/    # Warstwa aplikacji
│   │   ├── Common/                 # Współdzielone komponenty
│   │   │   └── Behaviors/          # Zachowania pipeline MediatR
│   │   ├── DTOs/                   # DTO aplikacji
│   │   ├── Exceptions/             # Własne wyjątki
│   │   ├── Extensions/             # Metody rozszerzające
│   │   ├── Interfaces/             # Interfejsy aplikacji
│   │   │   └── Persistence/        # Interfejsy repozytoriów
│   │   └── UseCases/               # Komendy i zapytania CQRS
│   │       ├── Employees/
│   │       │   ├── Commands/
│   │       │   └── Queries/
│   │       ├── Events/
│   │       ├── News/
│   │       └── Users/
│   ├── PortalForge.Domain/         # Warstwa domeny
│   │   ├── Entities/               # Encje domenowe
│   │   ├── Enums/                  # Wyliczenia domenowe
│   │   └── ValueObjects/           # Obiekty wartości
│   └── PortalForge.Infrastructure/ # Warstwa infrastruktury
│       ├── Configuration/          # Dostawcy konfiguracji
│       ├── Extensions/             # Rozszerzenia DI
│       ├── Persistence/            # Dostęp do danych
│       │   ├── ConnectionFactory/  # Fabryka połączeń DB
│       │   ├── Migrations/         # Migracje EF Core
│       │   └── Repositories/       # Implementacje repozytoriów
│       └── Services/               # Serwisy infrastruktury
├── frontend/                        # Aplikacja Nuxt 3
│   ├── components/                  # Komponenty Vue
│   ├── composables/                 # Funkcje composable
│   ├── layouts/                     # Komponenty layoutów
│   ├── pages/                       # Komponenty stron (auto-routing)
│   ├── stores/                      # Store Pinia
│   ├── types/                       # Typy TypeScript
│   ├── utils/                       # Funkcje narzędziowe
│   ├── nuxt.config.ts              # Konfiguracja Nuxt
│   └── package.json                 # Zależności NPM
├── .gitignore
├── CLAUDE.md                        # Kontekst główny projektu
├── PortalForge.sln                  # Plik rozwiązania Visual Studio
└── README.md                        # Ten plik
```

## Rozpoczęcie pracy

### Wymagania wstępne

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 22.x LTS](https://nodejs.org/)
- [Git](https://git-scm.com/)
- [Konto Supabase](https://supabase.com/)

### Instalacja

1. **Sklonuj repozytorium**
   ```bash
   git clone https://github.com/Vesperino/PortalForge.git
   cd PortalForge
   ```

2. **Konfiguracja Supabase**

   Utwórz pliki `.env` dla backendu i frontendu:

   **Frontend** (`frontend/.env`):
   ```env
   NUXT_PUBLIC_SUPABASE_URL=twoj_supabase_url
   NUXT_PUBLIC_SUPABASE_KEY=twoj_supabase_anon_key
   ```

   **Backend** (`backend/PortalForge.Api/.env`):
   ```env
   SUPABASE_URL=twoj_supabase_url
   SUPABASE_SERVICE_ROLE_KEY=twoj_supabase_service_role_key
   CONNECTION_STRING=twoj_postgresql_connection_string
   ```

   ⚠️ **Ważne**: Nigdy nie commituj plików `.env`. Używaj szablonów `.env.example`.

### Uruchomienie aplikacji

#### Backend

```bash
# Przejdź do katalogu backendu
cd backend/PortalForge.Api

# Przywróć zależności
dotnet restore

# Uruchom serwer deweloperski
dotnet run

# Lub uruchom z hot reload
dotnet watch run
```

API będzie dostępne pod adresem `https://localhost:5001` (lub skonfigurowanym porcie).

#### Frontend

```bash
# Przejdź do katalogu frontendu
cd frontend

# Zainstaluj zależności
npm install

# Uruchom serwer deweloperski
npm run dev
```

Aplikacja będzie dostępna pod adresem `http://localhost:3000`.

## Komendy deweloperskie

### Backend (.NET)

```bash
# Zbuduj projekt
dotnet build

# Uruchom testy
dotnet test

# Utwórz migrację EF Core
dotnet ef migrations add <NazwaMigracji> --project ../PortalForge.Infrastructure

# Zastosuj migracje do bazy danych
dotnet ef database update --project ../PortalForge.Infrastructure

# Formatuj kod
dotnet format
```

### Frontend (Nuxt)

```bash
# Zbuduj dla produkcji
npm run build

# Podgląd buildu produkcyjnego
npm run preview

# Uruchom testy jednostkowe
npm run test

# Uruchom testy E2E
npm run test:e2e

# Lintuj kod
npm run lint

# Formatuj kod
npm run format
```

## Architektura

### Architektura backendu

Backend śledzi zasady **Clean Architecture** z wyraźnym podziałem:

1. **Warstwa prezentacji (PortalForge.Api)**
   - Kontrolery REST API
   - DTO żądań/odpowiedzi
   - Middleware (uwierzytelnianie, logowanie, obsługa błędów)
   - Konfiguracja startu aplikacji

2. **Warstwa aplikacji (PortalForge.Application)**
   - Use cases (komendy i zapytania)
   - Logika biznesowa
   - Walidacja (FluentValidation)
   - Interfejsy dla infrastruktury

3. **Warstwa domeny (PortalForge.Domain)**
   - Główne encje biznesowe
   - Logika domenowa
   - Obiekty wartości
   - Zdarzenia domenowe

4. **Warstwa infrastruktury (PortalForge.Infrastructure)**
   - Dostęp do bazy danych (EF Core)
   - Implementacje repozytoriów
   - Serwisy zewnętrzne
   - Migracje

### Wzorzec CQRS z MediatR

Wszystkie operacje to komendy lub zapytania obsługiwane przez MediatR. Każda komenda/zapytanie:
- Ma własny handler
- Ma własny validator (FluentValidation)
- Jest oddzielona od innych operacji

### Workflow git

Stosujemy workflow z feature branches i Conventional Commits:

```bash
# Utwórz branch feature
git checkout -b feature/nazwa-funkcji

# Wprowadź zmiany i commituj
git add .
git commit -m "feat: dodaj nową funkcję"

# Wypchnij na remote
git push origin feature/nazwa-funkcji
```

### Konwencja commit message

Śledź [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` - Nowa funkcjonalność
- `fix:` - Naprawa błędu
- `docs:` - Zmiany w dokumentacji
- `refactor:` - Refaktoryzacja kodu
- `test:` - Dodanie testów
- `chore:` - Zadania konserwacyjne
- `style:` - Zmiany stylu kodu

## Testowanie

### Testy backendu

```bash
cd backend
dotnet test
```

Testy są zorganizowane według wzorca AAA (Arrange-Act-Assert) z FluentAssertions dla czytelnych asercji.

### Testy frontendu

```bash
cd frontend

# Testy jednostkowe z Vitest
npm run test

# Testy E2E z Playwright
npm run test:e2e
```

## Deployment

### Pipeline CI/CD

Workflow GitHub Actions obsługują:
- Automatyczne testowanie
- Sprawdzanie jakości kodu
- Budowanie obrazów Docker
- Deployment na staging/produkcję

### Wsparcie Docker

```bash
# Zbuduj obrazy Docker
docker-compose build

# Uruchom kontenery
docker-compose up
```

## Dokumentacja

- [Dokument wymagań produktowych](.ai/prd.md) - Szczegółowe wymagania funkcjonalne i niefunkcjonalne
- [Analiza stosu technologicznego](.ai/tech-stack.md) - Wybory technologiczne i uzasadnienie
- [CLAUDE.md](.claude/CLAUDE.md) - Kontekst asystenta AI i konwencje kodowania
- [Dokumentacja backendu](.ai/backend/README.md) - Dokumentacja specyficzna dla backendu
- [Dokumentacja frontendu](.ai/frontend/README.md) - Dokumentacja specyficzna dla frontendu

## Harmonogram projektu

### Faza 1: Fundament - Aktualna
- [x] Setup projektu i struktury monorepo
- [x] Konfiguracja Supabase
- [x] Przygotowanie reguł AI i dokumentacji
- [ ] Implementacja Clean Architecture w backendzie
- [ ] Podstawowy przepływ uwierzytelniania
- [ ] Setup pipeline CI/CD

### Faza 2: Struktura organizacyjna
- [ ] Operacje CRUD pracowników
- [ ] Wizualizacja struktury hierarchicznej
- [ ] Funkcjonalność importu CSV/Excel
- [ ] Funkcjonalność eksportu PDF/Excel

### Faza 3: Kalendarz i newsy
- [ ] System zarządzania wydarzeniami
- [ ] System publikacji newsów
- [ ] Integracja wydarzeń z newsami
- [ ] System tagowania i targetowania

### Faza 4: Testowanie i deployment
- [ ] Kompleksowe testy E2E
- [ ] Optymalizacja wydajności
- [ ] Deployment produkcyjny

## Kontybucje

To jest projekt wewnętrzny. Dla wytycznych dotyczących kontybucji, zapoznaj się z [.claude/CLAUDE.md](.claude/CLAUDE.md) w celu poznania standardów kodowania i konwencji.

## Wsparcie i zasoby

- **Dokumentacja .NET**: https://learn.microsoft.com/en-us/dotnet/
- **Dokumentacja Nuxt**: https://nuxt.com/docs
- **Dokumentacja Supabase**: https://supabase.com/docs
- **Dokumentacja Tailwind CSS**: https://tailwindcss.com/docs
- **Dokumentacja MediatR**: https://github.com/jbogard/MediatR/wiki
- **Dokumentacja FluentValidation**: https://docs.fluentvalidation.net/

## Licencja

All Rights Reserved - Projekt wewnętrzny

---

**Aktualna wersja**: 1.0.0-alpha
**Ostatnia aktualizacja**: 2025-10-11
**Utrzymywany przez**: Zespół deweloperski
