# PortalForge

[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Nuxt Version](https://img.shields.io/badge/Nuxt-3-00DC82)](https://nuxt.com/)
[![License](https://img.shields.io/badge/license-All%20Rights%20Reserved-red)](LICENSE)

> Wewnętrzny portal intranetowy dla organizacji 200+ pracowników

## 🚀 Live Demo

- **Frontend**: [https://krablab.pl/portalforge/fe/](https://krablab.pl/portalforge/fe/)
- **Backend API (Swagger)**: [https://krablab.pl/portalforge/be/swagger/index.html](https://krablab.pl/portalforge/be/swagger/index.html)

## Przegląd projektu

**PortalForge** to scentralizowana platforma komunikacji zaprojektowana do rozwiązania problemów związanych z chaosem dokumentowym i brakiem centralizacji informacji w dużych organizacjach. System dostarcza kompleksowe narzędzia do zarządzania strukturą organizacyjną, kalendarzem wydarzeń firmowych oraz komunikacją wewnętrzną.

### Kluczowe funkcje

- ✅ **Autoryzacja i uwierzytelnianie** - Pełna implementacja Supabase Auth z weryfikacją email, kontrolą dostępu opartą na rolach (Admin, Manager, HR, Marketing, Pracownik)
- ✅ **Struktura organizacyjna** - Nielimitowana hierarchia działów, wizualizacja drzewa (pan & zoom), 3 tryby widoku (Tree, Departments, List)
- ✅ **System zarządzania urlopami** - Kalendarz zespołu, automatyczne zastępstwa, wykrywanie konfliktów, email powiadomienia, background services
- ✅ **System wniosków** - Konfigurowalne szablony, wieloetapowy proces zatwierdzania, quizy, auto-routing w hierarchii
- ✅ **System newsów** - Publikacja newsów z obrazami, kategoriami, hashtagami i rich content editor
- ⚠️ **Kalendarz wydarzeń** - UI zaimplementowane, model domenowy istnieje, wymagane dokończenie use cases w backendzie
- ✅ **Powiadomienia** - System powiadomień real-time, grupowanie po kategoriach, email integration
- ✅ **Internal Services** - Katalog wewnętrznych narzędzi i linków z ikonami i kategoryzacją
- ✅ **AI Chat Assistant** - Asystent AI do wsparcia użytkowników i tłumaczeń
- ✅ **Monitoring aktywności** - Audit logs dla wszystkich działań administracyjnych
- ⚠️ **Import/Export** - UI zaimplementowane, eksport PDF/Excel wymaga dokończenia implementacji backendowej

## ✨ Status implementacji funkcjonalności

### ✅ Pełna implementacja (100%)

#### Autoryzacja i uwierzytelnianie
- Rejestracja i logowanie przez Supabase Auth
- Weryfikacja email z resend functionality (rate limiting 2 min)
- Odświeżanie tokenów (co 50 minut automatycznie)
- Kontrola dostępu oparta na rolach (5 ról: Admin, HR, Manager, Marketing, Employee)
- Session management z automatycznym wylogowaniem po 8h

#### Hierarchiczna struktura organizacyjna
- **Nielimitowana hierarchia działów** - Dowolna głębokość drzewa organizacyjnego
- **3 tryby wizualizacji** - Tree (z pan & zoom), Departments (karty hierarchiczne), List (tabela)
- **Szefowie działów i przypisania pracowników** - Automatyczne zarządzanie hierarchią
- **Uprawnienia widoczności** - Kontrola, kto może przeglądać które działy
- **Wyszukiwanie i filtrowanie** - Szybkie wyszukiwanie pracowników i działów
- **Profile pracowników** - Pełne dane kontaktowe, przełożeni, podwładni
- **Automatyczne routowanie wniosków** - Inteligentne przekierowanie do odpowiedniego przełożonego na podstawie hierarchii

#### System zarządzania urlopami
- **Automatyczne zastępstwa** - Przekierowanie do zastępcy gdy zatwierdzający jest na urlopie
- **Kalendarz urlopów zespołu** - 2 widoki: Timeline (Gantt), Calendar Grid
- **Wykrywanie konfliktów** - Alerty gdy >30% zespołu jest na urlopie (krytyczne przy >50%)
- **Email powiadomienia** - Przypomnienia o nadchodzących urlopach (7 dni, 1 dzień przed, rozpoczęcie, zakończenie)
- **Background services** - 5 zadań automatycznych (aktualizacja statusów, przypomnienia, roczne limity, wygasanie dni)
- **Statystyki zespołu** - Wykorzystanie urlopów, dni pozostałe, dni przeniesione z wygaśnięciem
- **Sick Leave (L4)** - Automatyczne zatwierdzanie zwolnień lekarskich z integracją ZUS

#### System wniosków z zaawansowanymi funkcjami
- **Request Templates** - Kreator szablonów z 6 typami pól (Text, Textarea, Number, Select, Date, Checkbox)
- **Multi-step approval flow** - Wieloetapowy proces zatwierdzania z wizualną timeline
- **6 typów zatwierdzających**:
  - Direct Supervisor (bezpośredni przełożony)
  - Role (rola w hierarchii - Manager, Director, VP, President)
  - Specific User (konkretny użytkownik)
  - Specific Department (szef działu)
  - User Group (grupa użytkowników)
  - Submitter (samoobsługa)
- **Quiz system** - Quizy wielokrotnego wyboru z progiem zdawalności na każdym etapie zatwierdzania
- **Auto-routing** - Inteligentne wyszukiwanie zatwierdzającego w hierarchii
- **Vacation integration** - Automatyczne tworzenie urlopu po zatwierdzeniu wniosku urlopowego
- **Sick leave integration** - Auto-approval zwolnień L4 z powiadomieniami
- **Comments & Attachments** - System komentarzy z możliwością załączników
- **Edit History** - Pełna historia zmian dla audytu
- **SLA monitoring** - Background job sprawdzający terminy z przypomnieniami

#### System newsów i wydarzeń
- **News System** - Publikacja newsów z rich content editor, obrazami, kategoriami
- **Hashtags** - System tagowania dla łatwego wyszukiwania
- **Categories** - 5 kategorii (Announcement, Product, HR, Tech, Event)
- **Image uploads** - Wsparcie dla obrazów w newsach
- **Events Calendar** - UI kalendarza z preview wydarzeń
- **Location picker** - Integracja Google Maps i OpenStreetMap dla lokalizacji wydarzeń

#### System powiadomień
- **Real-time notifications** - Powiadomienia w czasie rzeczywistym
- **9 typów powiadomień** - Wnioski, urlopy, zastępstwa, SLA, przypomnienia
- **Email integration** - Automatyczne emaile dla krytycznych powiadomień
- **Unread tracking** - Licznik nieprzeczytanych
- **Action URLs** - Deep linking do konkretnych akcji

#### Dodatkowe funkcjonalności
- **Internal Services** - Katalog wewnętrznych narzędzi z ikonami, kategoriami, scope (global/department)
- **AI Chat Assistant** - Chat AI do wsparcia i tłumaczeń
- **Location Services** - Geocoding z cache dla optymalizacji
- **Storage Management** - Upload plików (obrazy newsów, ikony, załączniki)
- **System Settings** - Konfigurowalne ustawienia runtime bez redeploymentu
- **Audit Logs** - Kompletny audit trail dla działań administracyjnych
- **Role Management** - Własne grupy ról z przypisywaniem uprawnień

### ⚠️ Częściowa implementacja (wymagane dokończenie)

#### Kalendarz wydarzeń (60%)
- ✅ UI kalendarza w pełni zaimplementowane (strona /dashboard/calendar)
- ✅ Model domenowy Event istnieje w backendzie
- ✅ Repository w UnitOfWork
- ❌ Brak use cases (Commands/Queries) dla zarządzania wydarzeniami
- ❌ Brak EventsController w API

**Wymagane do dokończenia:**
- Utworzenie use cases: CreateEvent, UpdateEvent, DeleteEvent, GetEvents, GetEventById
- Utworzenie EventsController z endpointami REST API
- Walidatory dla komend wydarzeń

#### Export do PDF/Excel (20%)
- ✅ UI przyciski eksportu zaimplementowane
- ✅ Endpointy API istnieją (vacation-schedules/export/pdf, /export/excel)
- ❌ Backend zwraca 501 Not Implemented
- ❌ Brak bibliotek do generowania PDF/Excel

**Wymagane do dokończenia:**
- Implementacja generowania PDF (QuestPDF lub iText)
- Implementacja generowania Excel (EPPlus lub ClosedXML)
- Template'y dla raportów urlopowych i struktury org

#### Powiadomienia UI (60%)
- ✅ Backend w pełni zaimplementowany
- ✅ NotificationBell component istnieje
- ✅ Toast notifications działają
- ❌ Brak dropdown panelu z listą powiadomień
- ❌ Brak real-time updates (polling/WebSocket)

**Wymagane do dokończenia:**
- Komponent NotificationPanel z listą
- Real-time updates (SignalR lub polling)
- Mark as read z UI

#### Moduł dokumentów (40%)
- ✅ Strona /dashboard/documents istnieje
- ✅ DocumentViewer component
- ✅ FilePreviewModal
- ❌ Brak zarządzania dokumentami (upload, lista, struktura folderów)
- ❌ Brak wersjonowania dokumentów

**Wymagane do dokończenia:**
- Backend use cases dla zarządzania dokumentami
- UI upload i lista dokumentów
- Struktura folderów
- Wersjonowanie (opcjonalnie dla przyszłości)

### ❌ Poza zakresem MVP (zaplanowane na przyszłość)
- Odzyskiwanie hasła (reset password flow nie zaimplementowany w backendzie)
- Import użytkowników z CSV/Excel (UI istnieje, backend wymaga implementacji)
- Active Directory/LDAP integration
- Full-text search
- Internal messenger/chat
- External API integrations
- Push notifications (mobile)

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

### Faza 1: Fundament - ✅ ZAKOŃCZONA (100%)
- [x] Setup projektu i struktury monorepo
- [x] Konfiguracja Supabase
- [x] Przygotowanie reguł AI i dokumentacji
- [x] Implementacja Clean Architecture w backendzie
- [x] Podstawowy przepływ uwierzytelniania (rejestracja, logowanie, weryfikacja emaila)
- [x] Setup pipeline CI/CD (GitHub Actions + Docker deployment)
- [x] Konfiguracja SMTP z Supabase dla emaili weryfikacyjnych
- [x] Rate limiting dla resend email (2 minuty cooldown)
- [x] Frontend: strony callback i verify-email z timerem
- [x] Middleware sprawdzające weryfikację emaila

### Faza 2: Struktura organizacyjna & Urlopy - ✅ ZAKOŃCZONA (100%)
- [x] Nielimitowana hierarchia działów z 3 trybami wizualizacji
- [x] System zarządzania urlopami z kalendarzem zespołu
- [x] Automatyczne routowanie wniosków w hierarchii
- [x] Zastępstwa podczas nieobecności
- [x] Kalendarz urlopów zespołu (2 widoki: Timeline, Grid)
- [x] Wykrywanie konfliktów urlopowych (alerty 30%/50%)
- [x] Email powiadomienia (7 dni, 1 dzień, start, koniec)
- [x] Background services (5 automatycznych zadań)
- [x] Sick Leave (L4) integration z auto-approval
- [x] System wniosków z konfigurowalnymi szablonami
- [x] Multi-step approval workflow z quizami
- [x] Komentarze i załączniki do wniosków
- [x] SLA monitoring z przypomnieniami

### Faza 2.5: Dodatkowe funkcjonalności - ✅ ZAKOŃCZONA (100%)
- [x] Internal Services - katalog narzędzi wewnętrznych
- [x] AI Chat Assistant - wsparcie i tłumaczenia
- [x] Location Services - geocoding z cache
- [x] Storage Management - upload plików
- [x] System Settings - runtime configuration
- [x] Audit Logs - pełny audit trail
- [x] Role Management - niestandardowe grupy ról

### Faza 3: Kalendarz i newsy - ⚠️ W TRAKCIE (85%)
- [x] System publikacji newsów z rich editor
- [x] Kategorie i hashtagi
- [x] Upload obrazów do newsów
- [x] Frontend kalendarza wydarzeń
- [x] Location picker (Google Maps/OSM)
- [ ] Backend use cases dla wydarzeń (CreateEvent, UpdateEvent, DeleteEvent, GetEvents)
- [ ] EventsController w API
- [ ] Walidatory dla komend wydarzeń

### Faza 4: Finalizacja MVP - 🔄 DO WYKONANIA
- [ ] Dokończenie systemu wydarzeń (backend use cases)
- [ ] Implementacja eksportu PDF/Excel (urlopy, struktura org)
- [ ] Dokończenie UI powiadomień (dropdown panel, real-time updates)
- [ ] Implementacja resetu hasła (backend + frontend)
- [ ] Import użytkowników z CSV/Excel (backend)
- [ ] Moduł zarządzania dokumentami
- [ ] Kompleksowe testy E2E
- [ ] Optymalizacja wydajności
- [ ] Code review i refactoring
- [ ] Dokumentacja użytkownika końcowego

### Przyszłe iteracje (Post-MVP)
- Active Directory/LDAP integration
- Full-text search
- Internal messenger/chat
- External API integrations
- Push notifications (mobile apps)
- Advanced analytics i dashboardy
- Wersjonowanie dokumentów

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

**Aktualna wersja**: 2.5.0-beta
**Ostatnia aktualizacja**: 2025-11-07
**Status**: Faza 3 w trakcie (85% - Events backend pozostaje do dokończenia)
**Postęp ogólny MVP**: ~90% zrealizowane
**Utrzymywany przez**: Zespół deweloperski

### Metryki projektu
- **Backend**: 29 repositories, 100+ use cases, 12 controllers
- **Frontend**: 50+ stron, 150+ komponentów, 20+ composables
- **Pokrycie testami**: Backend ~70%, Frontend wymaga rozszerzenia
- **Background jobs**: 6 automatycznych zadań (urlopy, powiadomienia, SLA)
- **Total LOC**: ~50,000+ linii kodu
