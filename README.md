# PortalForge

Wewnętrzny portal intranetowy dla organizacji 200+ pracowników.

## Opis projektu

PortalForge to centralna platforma komunikacji wewnętrznej, która rozwiązuje problem chaosu dokumentowego i braku centralizacji informacji w dużej organizacji. System umożliwia efektywne zarządzanie strukturą organizacyjną, kalendarzem wydarzeń firmowych oraz komunikacją wewnętrzną.

## Funkcje MVP

- 🔐 **System autoryzacji** - Logowanie przez Supabase Auth z systemem ról
- 👥 **Struktura organizacyjna** - Wizualizacja i zarządzanie hierarchią pracowników
- 📅 **Kalendarz wydarzeń** - Firmowe wydarzenia z systemem tagowania
- 📰 **Newsy** - Komunikaty wewnętrzne i ogłoszenia
- 📊 **Monitoring** - Raporty aktywności użytkowników

## Tech Stack

### Backend
- .NET 8.0
- CQRS z MediatR
- Entity Framework Core
- PostgreSQL (Supabase)
- Clean Architecture

### Frontend
- Nuxt 3 (Vue 3)
- Tailwind CSS
- TypeScript
- Pinia

### Infrastruktura
- GitHub Actions (CI/CD)
- Docker
- Supabase (Database + Auth)

## Struktura projektu (Monorepo)

```
PortalForge/
├── .ai/                        # Dokumentacja AI i PRD
├── backend/
│   └── PortalForge.Api/       # .NET 8.0 Web API
├── frontend/                   # Nuxt 3 Application (To be initialized)
├── PortalForge.sln             # Visual Studio Solution
└── README.md
```

## Rozpoczęcie pracy

### Backend
```bash
cd backend/PortalForge.Api
dotnet restore
dotnet run
```

### Frontend (Do zainicjalizowania)
```bash
cd frontend
npx nuxi@latest init .
npm install
npm run dev
```

## Dokumentacja

- [Product Requirements Document](.ai/prd.md)
- [Tech Stack Analysis](.ai/tech-stack.md)

## Harmonogram

- **Faza 1**: Fundament (2 tygodnie)
- **Faza 2**: Struktura organizacyjna (3 tygodnie)
- **Faza 3**: Kalendarz i newsy (2 tygodnie)
- **Faza 4**: Testy i deployment (1 tydzień)

## Licencja

Projekt wewnętrzny - All Rights Reserved
