# PortalForge - System Architecture Diagram

## System Overview

This document provides comprehensive architecture diagrams for the PortalForge intranet portal system.

---

## 1. High-Level System Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Web Browser]
        Mobile[Mobile Browser]
    end

    subgraph "Frontend - Nuxt 3"
        Pages[Pages<br/>Vue Components]
        Layouts[Layouts]
        Components[Reusable Components]
        Composables[Composables<br/>Business Logic]
        Stores[Pinia Stores<br/>State Management]
        Middleware[Middleware<br/>Auth, Verified]
    end

    subgraph "Backend - .NET 8.0"
        API[API Layer<br/>Controllers]

        subgraph "Application Layer - CQRS"
            Commands[Commands<br/>Write Operations]
            Queries[Queries<br/>Read Operations]
            Handlers[MediatR Handlers]
            Validators[FluentValidation]
            Services[Application Services]
        end

        subgraph "Domain Layer"
            Entities[Domain Entities]
            ValueObjects[Value Objects]
            DomainLogic[Domain Logic]
        end

        subgraph "Infrastructure Layer"
            Repositories[Repositories]
            UnitOfWork[Unit of Work]
            EFCore[Entity Framework Core]
            BackgroundJobs[Background Jobs<br/>Hangfire]
            FileStorage[File Storage Service]
            EmailService[Email Service]
        end
    end

    subgraph "External Services"
        Supabase[Supabase<br/>Auth + PostgreSQL]
        SMTP[SMTP Server<br/>Email Delivery]
        AI[AI Service<br/>Chat & Translation]
        Maps[Maps API<br/>Geocoding]
    end

    Browser --> Pages
    Mobile --> Pages
    Pages --> Layouts
    Layouts --> Components
    Pages --> Composables
    Composables --> Stores
    Middleware --> Stores

    Composables --> API

    API --> Commands
    API --> Queries
    Commands --> Handlers
    Queries --> Handlers
    Handlers --> Validators
    Handlers --> Services
    Handlers --> Repositories

    Services --> Entities
    Entities --> ValueObjects
    Entities --> DomainLogic

    Repositories --> UnitOfWork
    UnitOfWork --> EFCore
    EFCore --> Supabase

    BackgroundJobs --> Repositories
    BackgroundJobs --> EmailService

    FileStorage --> Supabase
    EmailService --> SMTP
    Services --> AI
    Services --> Maps

    Stores --> Supabase

    style Browser fill:#E3F2FD
    style Mobile fill:#E3F2FD
    style Pages fill:#81C784
    style API fill:#FFB74D
    style Supabase fill:#BA68C8
    style BackgroundJobs fill:#FF8A65
```

---

## 2. Clean Architecture Layers

```mermaid
graph LR
    subgraph "Clean Architecture - Dependency Flow"
        Presentation[Presentation Layer<br/>API Controllers<br/>DTOs]
        Application[Application Layer<br/>Use Cases CQRS<br/>Commands & Queries<br/>Validators]
        Domain[Domain Layer<br/>Entities<br/>Business Logic<br/>Interfaces]
        Infrastructure[Infrastructure Layer<br/>Repositories<br/>EF Core<br/>External Services]
    end

    Presentation --> Application
    Application --> Domain
    Infrastructure --> Application
    Infrastructure --> Domain

    style Presentation fill:#FFE082
    style Application fill:#81C784
    style Domain fill:#64B5F6
    style Infrastructure fill:#FFB74D
```

**Key Principles:**
- **Domain** is the core - no dependencies on other layers
- **Application** orchestrates use cases, depends only on Domain
- **Infrastructure** implements interfaces defined in Domain/Application
- **Presentation** depends on Application, not on Infrastructure directly

---

## 3. CQRS Pattern with MediatR

```mermaid
sequenceDiagram
    participant Controller as API Controller
    participant Mediator as MediatR
    participant Handler as Command/Query Handler
    participant Validator as FluentValidator
    participant UoW as Unit of Work
    participant DB as Database

    Note over Controller,DB: Command Flow (Write Operation)

    Controller->>Mediator: Send(CreateEmployeeCommand)
    Mediator->>Handler: Handle(command)
    Handler->>Validator: ValidateAsync(command)
    Validator-->>Handler: ValidationResult

    alt Validation Failed
        Handler-->>Controller: ValidationException
    end

    Handler->>UoW: BeginTransaction()
    Handler->>UoW: EmployeeRepository.CreateAsync()
    UoW->>DB: INSERT
    DB-->>UoW: Success
    Handler->>UoW: CommitTransaction()
    UoW-->>Handler: TransactionComplete
    Handler-->>Mediator: Result (EmployeeId)
    Mediator-->>Controller: Result

    Note over Controller,DB: Query Flow (Read Operation)

    Controller->>Mediator: Send(GetEmployeeByIdQuery)
    Mediator->>Handler: Handle(query)
    Handler->>UoW: EmployeeRepository.GetByIdAsync()
    UoW->>DB: SELECT
    DB-->>UoW: Employee Data
    UoW-->>Handler: Employee Entity
    Handler-->>Mediator: EmployeeDto
    Mediator-->>Controller: EmployeeDto
```

---

## 4. Authentication Flow

```mermaid
sequenceDiagram
    participant User as User
    participant Frontend as Nuxt Frontend
    participant Middleware as Auth Middleware
    participant Backend as .NET API
    participant Supabase as Supabase Auth
    participant DB as PostgreSQL

    Note over User,DB: Login Flow

    User->>Frontend: Enter credentials
    Frontend->>Backend: POST /api/auth/login
    Backend->>Supabase: signInWithPassword()
    Supabase->>DB: Verify credentials
    DB-->>Supabase: User data
    Supabase-->>Backend: JWT tokens (access + refresh)
    Backend->>DB: Get user roles & permissions
    DB-->>Backend: Role data
    Backend-->>Frontend: JWT tokens + user data
    Frontend->>Frontend: Store tokens in localStorage
    Frontend-->>User: Redirect to dashboard

    Note over User,DB: Protected Route Access

    User->>Frontend: Navigate to /dashboard
    Frontend->>Middleware: Check auth status
    Middleware->>Middleware: Validate token expiry

    alt Token Expired
        Middleware->>Backend: POST /api/auth/refresh-token
        Backend->>Supabase: refreshSession()
        Supabase-->>Backend: New tokens
        Backend-->>Middleware: New JWT tokens
        Middleware->>Middleware: Update localStorage
    end

    Middleware->>Backend: API call with Authorization header
    Backend->>Backend: Validate JWT
    Backend->>DB: Check permissions
    DB-->>Backend: Authorization result
    Backend-->>Frontend: Protected data
    Frontend-->>User: Display page
```

---

## 5. Data Flow Architecture

```mermaid
graph LR
    subgraph "Frontend State Management"
        UserAction[User Action]
        Component[Vue Component]
        Composable[Composable]
        PiniaStore[Pinia Store]
    end

    subgraph "API Communication"
        HTTPClient[$fetch]
        APIEndpoint[REST API Endpoint]
    end

    subgraph "Backend Processing"
        Controller[Controller]
        MediatR[MediatR]
        Handler[Handler]
        Repository[Repository]
    end

    subgraph "Data Persistence"
        UnitOfWork[Unit of Work]
        EFCore[EF Core]
        Database[(PostgreSQL)]
    end

    UserAction --> Component
    Component --> Composable
    Composable --> PiniaStore
    PiniaStore --> HTTPClient
    HTTPClient --> APIEndpoint
    APIEndpoint --> Controller
    Controller --> MediatR
    MediatR --> Handler
    Handler --> Repository
    Repository --> UnitOfWork
    UnitOfWork --> EFCore
    EFCore --> Database

    Database -.->|Response| EFCore
    EFCore -.->|Entity| UnitOfWork
    UnitOfWork -.->|Data| Repository
    Repository -.->|DTO| Handler
    Handler -.->|Result| MediatR
    MediatR -.->|Result| Controller
    Controller -.->|JSON| APIEndpoint
    APIEndpoint -.->|Data| HTTPClient
    HTTPClient -.->|Update| PiniaStore
    PiniaStore -.->|Reactive| Component
    Component -.->|Render| UserAction

    style UserAction fill:#E1BEE7
    style Database fill:#BA68C8
    style Handler fill:#81C784
```

---

## 6. Background Jobs Architecture

```mermaid
graph TB
    subgraph "Background Job System - Hangfire"
        Scheduler[Hangfire Scheduler]

        subgraph "Vacation Jobs"
            UpdateStatus[Update Vacation Statuses<br/>Every 6 hours]
            SendReminders[Send Vacation Reminders<br/>Daily 8:00 AM]
            UpdateAllowances[Update Allowances<br/>January 1st]
            ExpireCarried[Expire Carried-Over Days<br/>March 31st]
            CarriedReminders[Carried-Over Reminders<br/>Weekly in March]
        end

        subgraph "Request Jobs"
            CheckDeadlines[Check SLA Deadlines<br/>Every hour]
            ProcessSickLeave[Process Sick Leave<br/>Every 15 minutes]
        end
    end

    subgraph "Services"
        VacationService[Vacation Schedule Service]
        NotificationService[Notification Service]
        EmailService[Email Service]
        RequestService[Request Service]
    end

    subgraph "Data Access"
        UoW[Unit of Work]
        DB[(Database)]
    end

    Scheduler --> UpdateStatus
    Scheduler --> SendReminders
    Scheduler --> UpdateAllowances
    Scheduler --> ExpireCarried
    Scheduler --> CarriedReminders
    Scheduler --> CheckDeadlines
    Scheduler --> ProcessSickLeave

    UpdateStatus --> VacationService
    SendReminders --> VacationService
    UpdateAllowances --> VacationService
    ExpireCarried --> VacationService
    CarriedReminders --> VacationService

    CheckDeadlines --> RequestService
    ProcessSickLeave --> RequestService

    VacationService --> NotificationService
    VacationService --> EmailService
    RequestService --> NotificationService
    RequestService --> EmailService

    VacationService --> UoW
    RequestService --> UoW
    NotificationService --> UoW
    UoW --> DB

    style Scheduler fill:#FF8A65
    style VacationService fill:#81C784
    style DB fill:#BA68C8
```

---

## 7. Deployment Architecture

```mermaid
graph TB
    subgraph "GitHub"
        Repo[GitHub Repository]
        Actions[GitHub Actions CI/CD]
    end

    subgraph "Build Process"
        BuildBackend[Build .NET App<br/>Docker Image]
        BuildFrontend[Build Nuxt App<br/>SSG/SSR]
        RunTests[Run Tests<br/>xUnit + Vitest]
    end

    subgraph "VPS Server"
        Docker[Docker Engine]

        subgraph "Containers"
            BackendContainer[Backend API<br/>.NET 8.0]
            FrontendContainer[Frontend<br/>Nuxt 3]
            HangfireContainer[Background Jobs<br/>Hangfire]
        end

        Nginx[Nginx<br/>Reverse Proxy]
    end

    subgraph "External Services"
        SupabaseDB[(Supabase<br/>PostgreSQL)]
        SupabaseAuth[Supabase Auth]
    end

    Internet[Internet Users]

    Repo --> Actions
    Actions --> BuildBackend
    Actions --> BuildFrontend
    Actions --> RunTests

    BuildBackend --> Docker
    BuildFrontend --> Docker

    Docker --> BackendContainer
    Docker --> FrontendContainer
    Docker --> HangfireContainer

    Internet --> Nginx
    Nginx --> FrontendContainer
    Nginx --> BackendContainer

    BackendContainer --> SupabaseDB
    BackendContainer --> SupabaseAuth
    FrontendContainer --> SupabaseAuth
    HangfireContainer --> SupabaseDB

    style Actions fill:#4CAF50
    style Docker fill:#2196F3
    style SupabaseDB fill:#BA68C8
    style Nginx fill:#FF9800
```

---

## 8. Security Architecture

```mermaid
graph TB
    subgraph "Security Layers"
        HTTPS[HTTPS/TLS<br/>Transport Security]

        subgraph "Authentication"
            JWT[JWT Tokens<br/>Access + Refresh]
            Supabase[Supabase Auth<br/>Identity Provider]
            EmailVerif[Email Verification<br/>Required]
        end

        subgraph "Authorization"
            RBAC[Role-Based Access Control<br/>5 Roles]
            OrgPerms[Organizational Permissions<br/>Department Visibility]
            Middleware[Auth Middleware<br/>Route Guards]
        end

        subgraph "Input Validation"
            FluentVal[FluentValidation<br/>Backend]
            TypeScript[TypeScript<br/>Type Safety]
            Sanitization[Input Sanitization<br/>XSS Prevention]
        end

        subgraph "Data Protection"
            PasswordHash[Password Hashing<br/>bcrypt]
            SecretMgmt[Secrets Management<br/>.env files]
            AuditLog[Audit Logging<br/>All Admin Actions]
        end
    end

    HTTPS --> JWT
    JWT --> RBAC
    RBAC --> OrgPerms
    OrgPerms --> Middleware

    Middleware --> FluentVal
    FluentVal --> Sanitization
    TypeScript --> FluentVal

    Supabase --> PasswordHash
    Supabase --> EmailVerif

    RBAC --> AuditLog
    SecretMgmt --> Supabase

    style HTTPS fill:#4CAF50
    style JWT fill:#2196F3
    style RBAC fill:#FF9800
    style AuditLog fill:#9C27B0
```

---

## Technology Stack Summary

### Frontend
- **Framework**: Nuxt 3 (Vue 3 Composition API)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **State Management**: Pinia
- **HTTP Client**: $fetch (Nuxt)
- **UI Components**: PrimeVue, custom components
- **Testing**: Vitest (unit), Playwright (E2E)

### Backend
- **Framework**: .NET 8.0 (LTS)
- **Architecture**: Clean Architecture + CQRS
- **Mediator**: MediatR
- **ORM**: Entity Framework Core
- **Validation**: FluentValidation
- **Logging**: Serilog
- **Background Jobs**: Hangfire
- **Testing**: xUnit, FluentAssertions, Moq

### Infrastructure
- **Database**: PostgreSQL (Supabase)
- **Authentication**: Supabase Auth
- **Hosting**: VPS with Docker
- **CI/CD**: GitHub Actions
- **Reverse Proxy**: Nginx
- **Email**: SMTP (configurable)

### External Services
- **Supabase**: Database + Auth + Storage
- **AI Service**: Chat and translation
- **Maps API**: Geocoding (Google Maps/OSM)
- **SMTP**: Email delivery

---

## Key Architectural Decisions

### 1. Clean Architecture
- **Why**: Separation of concerns, testability, maintainability
- **Benefit**: Easy to swap infrastructure components without affecting business logic

### 2. CQRS with MediatR
- **Why**: Separate read and write operations, single responsibility
- **Benefit**: Scalability, clear code organization, easier testing

### 3. Repository Pattern + Unit of Work
- **Why**: Abstract data access, manage transactions consistently
- **Benefit**: Testable, swappable data sources, transaction control

### 4. Supabase for Auth + DB
- **Why**: Managed PostgreSQL, built-in auth, rapid development
- **Benefit**: Reduced operational overhead, scalable, secure

### 5. Monorepo Structure
- **Why**: Single repository for frontend + backend + docs
- **Benefit**: Easier collaboration, shared docs, atomic commits

### 6. Background Jobs with Hangfire
- **Why**: Reliable job scheduling, persistent queues, retry logic
- **Benefit**: Automated tasks (vacation updates, reminders, SLA checks)

---

*Document created: 2025-11-07*
*Version: 1.0*
*For: PortalForge v2.5 - Architecture Documentation*
