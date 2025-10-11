# PortalForge - Main AI Rules

## Important Instructions
- NEVER add comments like "Generated with Claude Code" or similar attribution comments to any files
- NEVER add Co-Authored-By headers or similar metadata indicating AI assistance
- Focus on writing clean, maintainable, and well-documented code
- Always prioritize code quality and best practices over speed

## Project Overview

**PortalForge** is an internal intranet portal for organizations with 200+ employees. It's a centralized communication platform that solves documentation chaos and lack of information centralization through organizational structure management, company events calendar, and internal communication.

**Current Phase**: MVP Development - Phase 1 (Foundation)
**Timeline**: 8 weeks total
**Repository**: https://github.com/Vesperino/PortalForge.git

## Detailed Rules by Technology

For detailed coding standards and patterns, refer to specialized rule files:

- **Backend (.NET 8.0, Clean Architecture, CQRS)**: [@backend.md](.claude/backend.md)
  - Clean Architecture layers
  - CQRS with MediatR
  - Repository Pattern & Unit of Work
  - FluentValidation examples
  - Coding standards & best practices

- **Frontend (Nuxt 3, Vue 3, TypeScript, Tailwind)**: [@frontend.md](.claude/frontend.md)
  - Vue 3 Composition API patterns
  - TypeScript standards
  - Composables and Pinia stores
  - Tailwind CSS guidelines
  - Accessibility standards

- **Testing (xUnit, Vitest, Playwright)**: [@testing.md](.claude/testing.md)
  - Unit testing with xUnit + FluentAssertions + Moq
  - Frontend testing with Vitest
  - E2E testing with Playwright
  - Test organization and naming conventions

## Tech Stack

### Backend
- **Framework**: .NET 8.0 (LTS until November 2026)
- **Architecture**: Clean Architecture + CQRS (MediatR)
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL via Supabase
- **Authentication**: Supabase Auth
- **Logging**: Serilog
- **Validation**: FluentValidation
- **Testing**: xUnit + FluentAssertions + Moq

### Frontend
- **Framework**: Nuxt 3 (Vue 3 with Composition API)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **State Management**: Pinia
- **Testing**: Vitest (unit) + Playwright (E2E)
- **Utils**: VueUse

### Infrastructure
- **VCS**: Git + GitHub
- **CI/CD**: GitHub Actions
- **Containerization**: Docker
- **Database & Auth**: Supabase
- **Hosting**: VPS (to be configured)

## Project Structure (Monorepo)

```
PortalForge/
├── .ai/                              # AI Documentation & Context
├── .claude/                          # Claude Code configuration
│   ├── CLAUDE.md                    # This file - Main rules
│   ├── backend.md                   # Backend-specific rules
│   ├── frontend.md                  # Frontend-specific rules
│   └── testing.md                   # Testing standards
├── backend/
│   ├── PortalForge.Api/            # Presentation Layer
│   ├── PortalForge.Application/    # Application Layer (CQRS)
│   ├── PortalForge.Domain/         # Domain Layer
│   └── PortalForge.Infrastructure/ # Infrastructure Layer
├── frontend/                        # Nuxt 3 Application
│   ├── components/
│   ├── composables/
│   ├── pages/
│   └── stores/
├── .gitignore
├── PortalForge.sln
└── README.md
```

## Development Commands

### Backend
```bash
cd backend/PortalForge.Api
dotnet restore              # Restore dependencies
dotnet run                  # Start server
dotnet watch run           # Hot reload
dotnet test                # Run tests
dotnet ef migrations add <Name> --project ../PortalForge.Infrastructure
dotnet ef database update --project ../PortalForge.Infrastructure
```

### Frontend
```bash
cd frontend
npm install                # Install dependencies
npm run dev                # Start dev server (http://localhost:3000)
npm run build             # Production build
npm run test              # Unit tests
npm run test:e2e         # E2E tests
npm run lint             # Lint code
```

## Git Workflow & Commit Standards

### Branch Naming
- `feature/feature-name` - New features
- `fix/bug-name` - Bug fixes
- `refactor/description` - Code refactoring
- `docs/description` - Documentation updates

### Commit Message Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add employee creation endpoint
fix: resolve department validation issue
docs: update API documentation
refactor: extract validation logic to separate service
test: add tests for employee repository
chore: update dependencies
style: format code with prettier
```

### Commit Message Structure

```
<type>: <short description>

<optional detailed description>

<optional footer>
```

Example:
```
feat: implement employee CSV import

Add functionality to import employees from CSV files.
Includes validation and error reporting.

Closes #123
```

## Do NOT

- ❌ **Do NOT** edit `appsettings.json` directly - use `appsettings.Development.json`
- ❌ **Do NOT** commit secrets, API keys, or passwords
- ❌ **Do NOT** commit `node_modules/`, `bin/`, `obj/` folders
- ❌ **Do NOT** force push to `main` branch
- ❌ **Do NOT** skip migrations - always generate EF migrations
- ❌ **Do NOT** use `any` type in TypeScript
- ❌ **Do NOT** bypass authentication/authorization checks
- ❌ **Do NOT** ignore TypeScript/ESLint errors
- ❌ **Do NOT** use `async void` (except event handlers)
- ❌ **Do NOT** block async code with `.Result` or `.Wait()`
- ❌ **Do NOT** create commands/queries without validators
- ❌ **Do NOT** access database directly from controllers
- ❌ **Do NOT** put business logic in controllers

## MVP Scope Reminders

### ✅ IN SCOPE (MVP)
- User authentication (Supabase Auth)
- Role-based access control (Admin, Manager, HR, Marketing, Employee)
- Organizational structure management (hierarchical tree)
- Calendar of company events
- News system
- User activity monitoring
- CSV/Excel import of users
- PDF/Excel export of org structure

### ❌ OUT OF SCOPE (Future)
- Request system with workflows
- Document management and versioning
- Active Directory/LDAP integration
- Dedicated admin panel
- Email/Push notifications
- Full-text search
- Internal chat/messenger
- External API integrations

## Key Business Rules

1. **Every employee must have**: First name, Last name, Email, Department, Position, Supervisor
2. **Only Admin/HR** can import users via CSV/Excel
3. **Only Admin/HR/Marketing** can create news and events
4. **Managers** can only edit their department structure
5. **Events** are archived after 1 year
6. **Sessions** expire after 8 hours of inactivity
7. **Passwords** must be hashed with bcrypt
8. **API responses** must be < 500ms for 95% of requests

## Supabase Configuration

**Project**: https://mqowlgphivdosieakzjb.supabase.co

**Environment Variables:**
- Frontend: `frontend/.env` (gitignored)
  - `NUXT_PUBLIC_SUPABASE_URL`
  - `NUXT_PUBLIC_SUPABASE_KEY` (anon key)
- Backend: `backend/PortalForge.Api/.env` (gitignored)
  - `SUPABASE_URL`
  - `SUPABASE_SERVICE_ROLE_KEY` (secret!)
  - `CONNECTION_STRING`

**⚠️ SECURITY**: Never commit `.env` files! Always use `.env.example` templates.

## Support & References

- **Documentation**: [.ai/prd.md](../.ai/prd.md), [.ai/tech-stack.md](../.ai/tech-stack.md)
- **.NET Docs**: https://learn.microsoft.com/en-us/dotnet/
- **Nuxt Docs**: https://nuxt.com/docs
- **Supabase Docs**: https://supabase.com/docs
- **Tailwind Docs**: https://tailwindcss.com/docs
- **MediatR Docs**: https://github.com/jbogard/MediatR/wiki
- **FluentValidation Docs**: https://docs.fluentvalidation.net/

---

**Last Updated**: 2025-10-11
**Version**: 2.0.0
