# Diagramy Autentykacji - PortalForge

Ten folder zawiera kompleksową dokumentację architektury systemu uwierzytelniania dla projektu PortalForge.

## Zawartość

### 1. auth.md - Diagram Przepływu Autentykacji
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

### 2. journey.md - Diagram Podróży Użytkownika
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

### 3. ui.md - Diagram Architektury UI
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

## Powiązane dokumenty

- `.ai/auth-spec.md` - Specyfikacja architektury autentykacji
- `.ai/prd.md` - Product Requirements Document (User Stories US-001, US-002)
- `.claude/CLAUDE.md` - Główne zasady projektu
- `.claude/frontend.md` - Standardy frontend (Nuxt 3)
- `.claude/backend.md` - Standardy backend (.NET 8.0)

---

*Utworzono: 2025-10-16*
*Wersja: 1.0*
*Dla: PortalForge MVP - Module 2 Authentication*
