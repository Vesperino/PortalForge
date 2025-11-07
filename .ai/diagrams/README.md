# PortalForge - Architecture Diagrams

Ten folder zawiera kompleksową dokumentację architektury systemu PortalForge z diagramami Mermaid.

## 📋 Zawartość

### 1. architecture.md - System Architecture Overview ✨ NEW
**Typ:** Sequence Diagram (Mermaid)

Szczegółowy diagram sekwencji pokazujący:
- Przepływ logowania użytkownika
- Dostęp do chronionych stron z weryfikacją tokenów
- Proces rejestracji z weryfikacją emaila
- Odzyskiwanie hasła
- Proces wylogowania

**Aktorzy:**
- Przeglądarka
- Frontend (Nuxt 3)
- Middleware (Auth Guard)
- Backend API (.NET 8.0)
- Supabase Auth

**Scenariusze:**
1. Logowanie użytkownika (z walidacją i obsługą błędów)
2. Dostęp do chronionej strony (z odświeżaniem tokenów)
3. Rejestracja nowego użytkownika (z emailem weryfikacyjnym)
4. Odzyskiwanie hasła (z linkiem resetującym)
5. Wylogowanie użytkownika

### 6. journey.md - User Journey Diagram
**Typ:** State Diagram (Mermaid)

Kompleksowy diagram stanów przedstawiający:
- Wszystkie możliwe ścieżki użytkownika w systemie
- Stany: Niezalogowany, Proces Autentykacji, Zalogowany
- Punkty decyzyjne i alternatywne przepływy
- Automatyczne wylogowanie po 8h nieaktywności

**Główne stany:**
1. **Niezalogowany**: Sprawdzanie autoryzacji i przekierowania
2. **Proces Autentykacji**:
   - Logowanie (z weryfikacją poświadczeń)
   - Rejestracja (z weryfikacją emaila)
   - Resetowanie hasła (z tokenem resetującym)
3. **Zalogowany Użytkownik**:
   - Korzystanie z aplikacji
   - Sprawdzanie sesji
   - Proces wylogowania

**Mechanizmy:**
- Automatyczne odświeżanie tokenów
- Timeout sesji po 8h
- Unieważnianie sesji przy zmianie hasła

### 7. ui.md - UI Architecture Diagram
**Typ:** Flowchart (Mermaid)

Wizualizacja struktury komponentów UI:
- Warstwa Middleware (auth.global.ts, guest.ts)
- Strony (Pages) - autentykacji i chronione
- Layouts (default.vue, auth.vue)
- Komponenty (Auth forms, Base components)
- Composables (useAuth, useAuthForm, useUser)
- Stores (Pinia auth store)

**Struktura:**
```
Browser
  ↓
Middleware → Pages → Layouts → Components
                         ↓
                    Composables
                         ↓
                      Stores
                         ↓
                    Backend API
```

**Komponenty:**
- AuthLoginForm.vue, AuthRegisterForm.vue, AuthResetPasswordForm.vue
- AuthHeader.vue, AuthUserMenu.vue
- BaseButton.vue, BaseInput.vue, BaseAlert.vue

## Jak używać tych diagramów

### 1. Planowanie implementacji
- Przejrzyj `auth.md` aby zrozumieć przepływ komunikacji między warstwami
- Użyj `journey.md` do identyfikacji wszystkich przypadków użycia
- Skonsultuj `ui.md` przy tworzeniu komponentów UI

### 2. Onboarding nowych członków zespołu
- Zacznij od `journey.md` aby zrozumieć perspektywę użytkownika
- Przejdź do `auth.md` aby poznać szczegóły techniczne
- Zakończ na `ui.md` aby zrozumieć strukturę kodu frontendowego

### 3. Code Review
- Weryfikuj zgodność implementacji z diagramami
- Sprawdzaj czy nowe funkcje są udokumentowane
- Aktualizuj diagramy przy zmianach architektury

### 4. Debugging
- `auth.md` pomaga zidentyfikować gdzie w przepływie występuje błąd
- `journey.md` pokazuje alternatywne ścieżki i przypadki brzegowe
- `ui.md` pomaga znaleźć odpowiedni komponent do modyfikacji

## Renderowanie diagramów

Diagramy są w formacie Mermaid i mogą być renderowane w:

- **GitHub**: Automatyczne renderowanie w plikach .md
- **VS Code**: Rozszerzenie "Markdown Preview Mermaid Support"
- **Confluence**: Plugin Mermaid
- **Online**: https://mermaid.live/

## Aktualizacja diagramów

Diagramy powinny być aktualizowane gdy:
- Dodajesz nowe strony autentykacji
- Zmieniasz przepływ autoryzacji
- Wprowadzasz nowe komponenty UI
- Modyfikujesz middleware lub stores

## 📊 Diagram Summary

| Diagram | Focus Area | Diagram Types | Status |
|---------|-----------|---------------|--------|
| architecture.md | System-wide architecture | Multiple (8 diagrams) | ✅ Complete |
| organizational-structure.md | Org structure & employees | 9 diagrams | ✅ Complete |
| vacation-system.md | Vacation management | 7 diagrams | ✅ Complete |
| request-workflow.md | Request approval system | 7 diagrams | ✅ Complete |
| auth.md | Authentication flow | Sequence diagram | ✅ Complete |
| journey.md | User journey | State diagram | ✅ Complete |
| ui.md | UI architecture | Flowchart | ✅ Complete |

**Total Diagrams**: 40+ Mermaid diagrams covering all major systems

---

## 🎯 Diagram Usage Guide

### For Developers

**New Team Member Onboarding:**
1. Start with `architecture.md` - understand overall system
2. Review `auth.md` and `journey.md` - understand user flows
3. Dive into specific system diagrams based on assigned work area

**Feature Development:**
1. Consult relevant system diagram (org-structure, vacation, request)
2. Review sequence diagrams for API integration
3. Check state machines for business logic flows

**Bug Fixing:**
1. Use sequence diagrams to trace request flow
2. Identify failing state transitions in state machines
3. Verify data flow in architecture diagrams

### For Product/Business

**Understanding Features:**
- `organizational-structure.md` - How org structure works
- `vacation-system.md` - Vacation booking and management
- `request-workflow.md` - Request approval processes

**Planning New Features:**
- Review existing flows to understand integration points
- Identify where new features fit in architecture
- Understand approval routing and automation

### For QA/Testing

**Test Planning:**
- Use state diagrams to identify all possible states
- Check sequence diagrams for error scenarios
- Verify all approval paths in request workflow

**Bug Investigation:**
- Trace actual behavior against documented flows
- Identify deviations from expected sequences
- Verify data transformations match architecture

---

## 🔧 Rendering Diagrams

Diagramy są w formacie Mermaid i mogą być renderowane w:

- **GitHub**: Automatyczne renderowanie w plikach .md
- **VS Code**: Rozszerzenie "Markdown Preview Mermaid Support" lub "Mermaid Editor"
- **Confluence**: Plugin Mermaid
- **Online**: https://mermaid.live/
- **Documentation sites**: Docusaurus, VuePress, MkDocs (native Mermaid support)

---

## 📝 Aktualizacja diagramów

Diagramy powinny być aktualizowane gdy:
- ✅ Dodajesz nowe endpointy API
- ✅ Zmieniasz przepływ approval lub biznesowy
- ✅ Wprowadzasz nowe komponenty lub serwisy
- ✅ Modyfikujesz strukturę danych (entities, DTOs)
- ✅ Dodajesz nowe background jobs
- ✅ Zmieniasz deployment lub infrastrukturę

**Proces aktualizacji:**
1. Zidentyfikuj dotknięte diagramy
2. Zaktualizuj kod Mermaid
3. Zweryfikuj rendering
4. Dodaj notatkę o zmianie w commit message
5. Review przez innego developera

---

## 🔗 Powiązane dokumenty

### Main Documentation
- `.ai/prd.md` - Product Requirements Document (updated 2025-11-07)
- `.ai/tech-stack.md` - Technology stack decisions
- `README.md` - Project overview with implementation status

### Architecture Decisions
- `.ai/decisions/` - Architecture Decision Records (ADR)
- `.ai/auth-spec.md` - Authentication specification
- `.ai/auth-and-roles-architecture.md` - Roles and permissions

### Backend Documentation
- `.ai/backend/README.md` - Backend architecture overview
- `.ai/backend/organizational-structure.md` - Org structure backend
- `.ai/backend/vacation-schedule-system.md` - Vacation backend
- `.ai/backend/requests-system.md` - Request system backend
- `.claude/backend.md` - Backend coding standards

### Frontend Documentation
- `.ai/frontend/README.md` - Frontend architecture overview
- `.ai/frontend/vacation-calendar.md` - Vacation UI
- `.ai/frontend/requests-system.md` - Request UI
- `.claude/frontend.md` - Frontend coding standards

### Implementation Progress
- `.ai/progress/` - Development progress logs
- `.ai/implementation-plan-organizational-structure.md`
- `.ai/vacation-system-implementation-plan.md`
- `.ai/implementation-summary-approval-workflow.md`

---

## 🎓 Learning Path

**Week 1: Fundamentals**
- Day 1-2: `architecture.md` - System overview
- Day 3: `auth.md` + `journey.md` - User flows
- Day 4: `ui.md` - Frontend structure
- Day 5: Review backend docs + coding standards

**Week 2: Core Systems**
- Day 1-2: `organizational-structure.md` - Understand hierarchy
- Day 3-4: `vacation-system.md` - Vacation management
- Day 5: `request-workflow.md` - Request approvals

**Week 3: Deep Dive**
- Implement small feature in each system
- Debug issues using diagrams
- Propose improvements to existing flows

---

*Utworzono: 2025-10-16*
*Ostatnia aktualizacja: 2025-11-07*
*Wersja: 2.0 - Expanded with all system diagrams*
*Dla: PortalForge v2.5 - Complete Architecture Documentation*
