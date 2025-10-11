# PortalForge - Project Context for Claude AI

## Project Overview

**PortalForge** is an internal intranet portal for organizations with 200+ employees. It's a centralized communication platform that solves documentation chaos and lack of information centralization through organizational structure management, company events calendar, and internal communication.

**Current Phase**: MVP Development - Phase 1 (Foundation)
**Timeline**: 8 weeks total
**Repository**: https://github.com/Vesperino/PortalForge.git

## Tech Stack

### Backend
- **Framework**: .NET 8.0 (LTS until November 2026)
- **Architecture**: Clean Architecture + CQRS (MediatR)
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL via Supabase
- **Authentication**: Supabase Auth
- **Logging**: Serilog
- **Testing**: xUnit + FluentAssertions

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
│   ├── prd.md                       # Product Requirements Document
│   ├── tech-stack.md                # Tech Stack Analysis
│   ├── backend/                     # Backend documentation
│   └── frontend/                    # Frontend documentation
├── .claude/                          # Claude-specific configs
│   └── commands/                    # Custom slash commands
├── backend/
│   └── PortalForge.Api/            # .NET 8.0 Web API
│       ├── Controllers/
│       ├── Program.cs
│       └── PortalForge.csproj
├── frontend/                         # Nuxt 3 Application
│   ├── components/
│   ├── composables/
│   ├── pages/
│   └── stores/
├── .gitignore
├── CLAUDE.md                         # This file - Project context
├── PortalForge.sln                   # Visual Studio Solution
└── README.md
```

## Development Commands

### Backend
```bash
# Navigate to backend
cd backend/PortalForge.Api

# Restore dependencies
dotnet restore

# Run development server
dotnet run

# Run with watch (hot reload)
dotnet watch run

# Run tests
dotnet test

# Build
dotnet build

# EF Core migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Frontend
```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Development server (http://localhost:3000)
npm run dev

# Build for production
npm run build

# Run production preview
npm run preview

# Run tests
npm run test
npm run test:e2e

# Lint and format
npm run lint
npm run format
```

### Git Workflow
```bash
# Check status
git status

# Stage changes
git add .

# Commit (will trigger pre-commit hooks)
git commit -m "feat: description"

# Push to remote
git push

# Create feature branch
git checkout -b feature/feature-name
```

## Code Style & Conventions

### Backend (.NET)
- **Naming**: PascalCase for classes, methods, properties; camelCase for local variables
- **Architecture**: Follow Clean Architecture layers (Domain, Application, Infrastructure, Api)
- **CQRS**: Use MediatR for commands and queries
- **Async**: Always use async/await for I/O operations
- **Validation**: Use FluentValidation for input validation
- **Error Handling**: Use Result pattern or custom exceptions
- **Logging**: Use Serilog with structured logging

#### .NET Best Practices
- Use nullable reference types (`#nullable enable`)
- Implement repository pattern in Infrastructure layer
- Use value objects for domain primitives
- Follow SOLID principles
- Write XML documentation for public APIs
- Use dependency injection for all services
- Implement unit tests with AAA pattern (Arrange, Act, Assert)
- Use FluentAssertions for readable test assertions

#### Error Handling Guidelines
- Handle errors and edge cases at the beginning of methods
- Use early returns for error conditions to avoid deeply nested if statements
- Place the happy path last in the method for improved readability
- Avoid unnecessary else statements; use if-return pattern instead
- Use guard clauses to handle preconditions and invalid states early
- Implement proper error logging with Serilog
- Return meaningful error messages to the client

### Frontend (Vue/Nuxt)
- **Components**: Use Composition API with `<script setup>`
- **TypeScript**: Always use TypeScript, avoid `any`
- **Naming**: PascalCase for components, camelCase for composables
- **Styling**: Use Tailwind CSS utility classes
- **State**: Use Pinia for global state, composables for local state
- **Imports**: Leverage Nuxt auto-imports
- **Formatting**: Prettier with 2-space indentation

#### Vue/Nuxt Best Practices
- Use functional, declarative programming - avoid classes
- Prefer named exports for composables and utils
- Use descriptive variable names with auxiliary verbs (e.g., isLoading, hasError)
- File structure: imports, types, composables, main logic, template
- Use `defineProps` and `defineEmits` with TypeScript interfaces
- Implement proper prop validation
- Use `computed` for derived state instead of methods
- Leverage VueUse composables for common functionality
- Use Nuxt's auto-imported components - no need for manual imports
- Implement proper loading and error states in components

#### TypeScript Guidelines
- Always define interfaces for props, emits, and API responses
- Use `type` for unions and intersections, `interface` for object shapes
- Avoid `any` - use `unknown` if type is truly unknown
- Use generics for reusable components and functions
- Leverage TypeScript's strict mode
- Define proper return types for functions

#### Tailwind CSS Guidelines
- Use utility classes directly in templates
- Use `@apply` directive sparingly - only for frequently repeated patterns
- Implement responsive design with breakpoint prefixes (sm:, md:, lg:, xl:)
- Use Tailwind's color palette and spacing scale consistently
- Leverage dark mode with `dark:` variant when needed
- Use arbitrary values `[]` only when necessary
- Group related utilities logically (layout, spacing, colors, typography)

#### Accessibility Guidelines
- Use semantic HTML elements (button, nav, main, article, etc.)
- Implement proper ARIA labels and roles
- Ensure keyboard navigation works correctly
- Maintain proper heading hierarchy (h1 → h2 → h3)
- Use `aria-label` or `aria-labelledby` for elements without visible text
- Implement focus states for interactive elements
- Test with screen readers when possible

### Commit Messages
Follow Conventional Commits:
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `chore:` - Maintenance tasks
- `style:` - Code style changes

## Documentation Standards

### Code Documentation
- **Backend**: Use XML documentation comments for public APIs
- **Frontend**: Use JSDoc comments for complex functions
- **README**: Keep updated in each major folder
- **ADR**: Document architectural decisions in `.ai/decisions/`

### AI Documentation (`.ai/` folder)
All AI-assisted development should maintain:
1. **Context Files**: Current state and decisions
2. **Progress Logs**: What was done and why
3. **Technical Decisions**: Rationale for tech choices
4. **API Contracts**: Backend/Frontend interface definitions
5. **Database Schema**: Current and migration history

## Do NOT

- ❌ **Do NOT** edit `appsettings.json` directly - use `appsettings.Development.json`
- ❌ **Do NOT** commit secrets, API keys, or passwords
- ❌ **Do NOT** commit `node_modules/`, `bin/`, `obj/` folders
- ❌ **Do NOT** force push to `main` branch
- ❌ **Do NOT** skip migrations - always generate EF migrations
- ❌ **Do NOT** use `any` type in TypeScript
- ❌ **Do NOT** bypass authentication/authorization checks
- ❌ **Do NOT** ignore TypeScript/ESLint errors

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

## Current Phase: Phase 1 (Foundation - 2 weeks)

**Goals:**
1. Backend project setup with Clean Architecture
2. Frontend Nuxt 3 initialization
3. Supabase configuration (database + auth)
4. Basic authentication flow
5. CI/CD pipeline setup
6. Core domain models

**Completed:**
- [x] Initialize Nuxt 3 project ✅
- [x] Configure Supabase credentials ✅
- [x] Setup .env files (safely ignored in git) ✅

**Next Steps:**
- [ ] Setup backend Clean Architecture structure
- [ ] Run Supabase database migrations
- [ ] Implement Supabase auth in backend
- [ ] Implement Supabase auth in frontend
- [ ] Create first API endpoint with authentication
- [ ] Create CI/CD workflows

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

## Key Business Rules

1. **Every employee must have**: First name, Last name, Email, Department, Position, Supervisor
2. **Only Admin/HR** can import users via CSV/Excel
3. **Only Admin/HR/Marketing** can create news and events
4. **Managers** can only edit their department structure
5. **Events** are archived after 1 year
6. **Sessions** expire after 8 hours of inactivity
7. **Passwords** must be hashed with bcrypt
8. **API responses** must be < 500ms for 95% of requests

## Support & References

- **Documentation**: [.ai/prd.md](.ai/prd.md), [.ai/tech-stack.md](.ai/tech-stack.md)
- **.NET Docs**: https://learn.microsoft.com/en-us/dotnet/
- **Nuxt Docs**: https://nuxt.com/docs
- **Supabase Docs**: https://supabase.com/docs
- **Tailwind Docs**: https://tailwindcss.com/docs

---

**Last Updated**: 2025-10-08
**Version**: 1.0.0
