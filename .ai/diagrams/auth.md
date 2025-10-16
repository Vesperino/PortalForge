# Diagram Przepływu Autentykacji - PortalForge

## Analiza wymagań autentykacji

<authentication_analysis>

### Zidentyfikowane przepływy autentykacji:

1. **Logowanie użytkownika**
   - Użytkownik przechodzi do strony logowania
   - Wypełnia formularz (email + hasło)
   - System weryfikuje dane przez Supabase Auth
   - Sesja jest tworzona z tokenami JWT
   - Użytkownik jest przekierowywany do głównej strony

2. **Rejestracja użytkownika**
   - Użytkownik przechodzi do strony rejestracji
   - Wypełnia formularz (email + hasło + potwierdzenie hasła)
   - System tworzy konto przez Supabase Auth
   - Email weryfikacyjny jest wysyłany
   - Użytkownik potwierdza email

3. **Odzyskiwanie hasła**
   - Użytkownik klika "Zapomniałeś hasła?"
   - Podaje email
   - System wysyła link resetujący przez Supabase Auth
   - Użytkownik klika link i ustawia nowe hasło
   - Wszystkie sesje są wylogowywane

4. **Odświeżanie tokenu**
   - Middleware sprawdza ważność access_token
   - Jeśli wygasł, używa refresh_token
   - Nowy access_token jest generowany
   - Sesja jest kontynuowana

5. **Wylogowanie**
   - Użytkownik klika "Wyloguj"
   - Sesja jest niszczona
   - Ciasteczka są usuwane
   - Przekierowanie do strony logowania

### Główni aktorzy i ich interakcje:

1. **Przeglądarka (Browser)**: Inicjuje żądania użytkownika, przechowuje ciasteczka sesyjne
2. **Frontend (Nuxt 3)**: Renderuje UI, waliduje dane, zarządza stanem klienta
3. **Middleware (Nuxt + .NET)**: Weryfikuje tokeny, chroni chronione ścieżki
4. **Backend API (.NET 8.0)**: Waliduje dane biznesowe, pośredniczy w komunikacji z Supabase
5. **Supabase Auth**: Zarządza użytkownikami, generuje tokeny JWT, wysyła emaile

### Procesy weryfikacji i odświeżania tokenów:

- **Weryfikacja tokenu**: Middleware sprawdza access_token w każdym żądaniu
- **Odświeżanie tokenu**: Automatyczne odświeżanie gdy access_token wygasa, używając refresh_token
- **Timeout sesji**: 8 godzin nieaktywności prowadzi do wylogowania
- **Bezpieczeństwo**: HTTP-only cookies, HTTPS, CSRF protection

### Opis kroków autentykacji:

**Logowanie:**
1. Użytkownik wypełnia formularz logowania
2. Frontend waliduje dane (format email, długość hasła)
3. Żądanie POST jest wysyłane do API backendu
4. Backend waliduje dane (FluentValidation)
5. Backend wywołuje Supabase Auth API
6. Supabase weryfikuje hasło i generuje tokeny
7. Backend ustawia HTTP-only cookies
8. Frontend otrzymuje odpowiedź sukcesu
9. Użytkownik jest przekierowywany

**Ochrona ścieżek:**
1. Middleware przechwytuje każde żądanie
2. Sprawdza czy ścieżka wymaga autoryzacji
3. Weryfikuje access_token z ciasteczka
4. Jeśli token ważny, kontynuuje żądanie
5. Jeśli nieważny, próbuje odświeżyć token
6. Jeśli odświeżenie się nie powiedzie, przekierowuje do logowania

</authentication_analysis>

## Diagram sekwencji autentykacji

```mermaid
sequenceDiagram
    autonumber

    participant Browser as Przeglądarka
    participant Frontend as Frontend<br/>(Nuxt 3)
    participant Middleware as Middleware<br/>(Auth Guard)
    participant API as Backend API<br/>(.NET 8.0)
    participant Supabase as Supabase<br/>Auth

    Note over Browser,Supabase: Scenariusz 1: Logowanie użytkownika

    Browser->>Frontend: Otwiera stronę logowania
    activate Frontend
    Frontend-->>Browser: Wyświetla formularz logowania
    deactivate Frontend

    Browser->>Frontend: Wypełnia i wysyła formularz
    activate Frontend
    Frontend->>Frontend: Walidacja formularza

    alt Walidacja frontendowa zakończona niepowodzeniem
        Frontend-->>Browser: Wyświetla błędy walidacji
    else Walidacja frontendowa zakończona sukcesem
        Frontend->>API: POST /api/auth/login
        activate API

        API->>API: Walidacja biznesowa (FluentValidation)

        alt Walidacja backendowa zakończona niepowodzeniem
            API-->>Frontend: 400 Bad Request
            Frontend-->>Browser: Wyświetla komunikat błędu
        else Walidacja backendowa zakończona sukcesem
            API->>Supabase: signInWithPassword()
            activate Supabase

            alt Nieprawidłowe dane logowania
                Supabase-->>API: Błąd autoryzacji
                API-->>Frontend: 401 Unauthorized
                Frontend-->>Browser: Wyświetla komunikat błędu
            else Prawidłowe dane logowania
                Supabase->>Supabase: Weryfikuje hasło
                Supabase->>Supabase: Generuje access_token
                Supabase->>Supabase: Generuje refresh_token
                Supabase-->>API: Tokeny JWT + dane użytkownika
                deactivate Supabase

                API->>API: Ustawia HTTP-only cookies
                API-->>Frontend: 200 OK + dane użytkownika
                deactivate API

                Frontend->>Frontend: Aktualizuje stan sesji
                Frontend-->>Browser: Przekierowanie do głównej strony
            end
        end
    end
    deactivate Frontend

    Note over Browser,Supabase: Scenariusz 2: Dostęp do chronionej strony

    Browser->>Frontend: Żądanie chronionej strony
    activate Frontend
    Frontend->>Middleware: Sprawdza autoryzację
    activate Middleware

    Middleware->>Middleware: Odczytuje access_token z cookie

    alt Brak tokenu
        Middleware-->>Frontend: Redirect do /auth/login
        Frontend-->>Browser: Wyświetla stronę logowania
    else Token obecny
        Middleware->>API: Weryfikacja tokenu
        activate API
        API->>Supabase: getUser(access_token)
        activate Supabase

        alt Token wygasł
            Supabase-->>API: Token wygasł
            deactivate Supabase
            API->>Supabase: refreshSession(refresh_token)
            activate Supabase

            alt Refresh token nieważny
                Supabase-->>API: Błąd odświeżania
                deactivate Supabase
                API-->>Middleware: 401 Unauthorized
                deactivate API
                Middleware-->>Frontend: Redirect do /auth/login
                deactivate Middleware
                Frontend-->>Browser: Wyświetla stronę logowania
            else Refresh token ważny
                Supabase->>Supabase: Generuje nowy access_token
                Supabase-->>API: Nowy access_token
                deactivate Supabase
                API->>API: Aktualizuje cookie
                API-->>Middleware: 200 OK + dane użytkownika
                deactivate API
                Middleware-->>Frontend: Kontynuuj żądanie
                deactivate Middleware
                Frontend-->>Browser: Wyświetla chronioną stronę
            end
        else Token ważny
            Supabase-->>API: Dane użytkownika
            deactivate Supabase
            API-->>Middleware: 200 OK + dane użytkownika
            deactivate API
            Middleware-->>Frontend: Kontynuuj żądanie
            deactivate Middleware
            Frontend-->>Browser: Wyświetla chronioną stronę
        end
    end
    deactivate Frontend

    Note over Browser,Supabase: Scenariusz 3: Rejestracja nowego użytkownika

    Browser->>Frontend: Otwiera stronę rejestracji
    activate Frontend
    Frontend-->>Browser: Wyświetla formularz rejestracji

    Browser->>Frontend: Wypełnia i wysyła formularz
    Frontend->>Frontend: Walidacja formularza

    alt Walidacja zakończona niepowodzeniem
        Frontend-->>Browser: Wyświetla błędy walidacji
    else Walidacja zakończona sukcesem
        Frontend->>API: POST /api/auth/register
        activate API
        API->>API: Walidacja biznesowa

        API->>Supabase: signUp()
        activate Supabase

        alt Email już istnieje
            Supabase-->>API: Błąd konfliktu
            API-->>Frontend: 409 Conflict
            Frontend-->>Browser: Email już istnieje
        else Email unikalny
            Supabase->>Supabase: Tworzy użytkownika
            Supabase->>Supabase: Generuje link weryfikacyjny

            par Wysyła email weryfikacyjny
                Supabase->>Browser: Email weryfikacyjny
            and Zwraca odpowiedź
                Supabase-->>API: 200 OK + dane użytkownika
                deactivate Supabase
                API-->>Frontend: 200 OK
                deactivate API
                Frontend-->>Browser: Sprawdź email
            end
        end
    end
    deactivate Frontend

    Browser->>Browser: Użytkownik klika link w emailu
    Browser->>Frontend: GET /auth/callback?token=xyz
    activate Frontend
    Frontend->>API: Weryfikacja tokenu
    activate API
    API->>Supabase: verifyEmail(token)
    activate Supabase

    alt Token nieważny lub wygasły
        Supabase-->>API: Błąd weryfikacji
        deactivate Supabase
        API-->>Frontend: 400 Bad Request
        deactivate API
        Frontend-->>Browser: Link wygasł
    else Token ważny
        Supabase->>Supabase: Aktywuje konto
        Supabase-->>API: 200 OK
        deactivate Supabase
        API-->>Frontend: 200 OK
        deactivate API
        Frontend-->>Browser: Konto aktywowane
    end
    deactivate Frontend

    Note over Browser,Supabase: Scenariusz 4: Odzyskiwanie hasła

    Browser->>Frontend: Klika Zapomniałeś hasła
    activate Frontend
    Frontend-->>Browser: Wyświetla formularz resetowania

    Browser->>Frontend: Podaje email
    Frontend->>API: POST /api/auth/reset-password
    activate API
    API->>Supabase: resetPasswordForEmail()
    activate Supabase

    Supabase->>Supabase: Generuje link resetujący

    par Wysyła email resetujący
        Supabase->>Browser: Email z linkiem
    and Zwraca odpowiedź
        Supabase-->>API: 200 OK
        deactivate Supabase
        API-->>Frontend: 200 OK
        deactivate API
        Frontend-->>Browser: Sprawdź email
    end
    deactivate Frontend

    Browser->>Browser: Użytkownik klika link w emailu
    Browser->>Frontend: GET /auth/reset?token=xyz
    activate Frontend
    Frontend-->>Browser: Formularz nowego hasła

    Browser->>Frontend: Podaje nowe hasło
    Frontend->>API: POST /api/auth/update-password
    activate API
    API->>Supabase: updateUser(new_password)
    activate Supabase

    alt Token nieważny lub wygasły
        Supabase-->>API: Błąd aktualizacji
        deactivate Supabase
        API-->>Frontend: 400 Bad Request
        deactivate API
        Frontend-->>Browser: Link wygasł
    else Token ważny
        Supabase->>Supabase: Aktualizuje hasło
        Supabase->>Supabase: Unieważnia wszystkie sesje
        Supabase-->>API: 200 OK
        deactivate Supabase
        API-->>Frontend: 200 OK
        deactivate API
        Frontend-->>Browser: Hasło zmienione zaloguj się
    end
    deactivate Frontend

    Note over Browser,Supabase: Scenariusz 5: Wylogowanie

    Browser->>Frontend: Klika przycisk Wyloguj
    activate Frontend
    Frontend->>API: POST /api/auth/logout
    activate API
    API->>Supabase: signOut()
    activate Supabase
    Supabase->>Supabase: Unieważnia sesję
    Supabase-->>API: 200 OK
    deactivate Supabase
    API->>API: Usuwa HTTP-only cookies
    API-->>Frontend: 200 OK
    deactivate API
    Frontend->>Frontend: Czyści stan sesji
    Frontend-->>Browser: Przekierowanie do logowania
    deactivate Frontend
```

## Opis diagramu

Diagram przedstawia pięć głównych scenariuszy autentykacji w systemie PortalForge:

1. **Logowanie**: Pełny przepływ z walidacją na frontendzie i backendzie, generowaniem tokenów JWT przez Supabase Auth
2. **Dostęp do chronionej strony**: Mechanizm middleware sprawdzający tokeny i automatycznie odświeżający je przy wygaśnięciu
3. **Rejestracja**: Tworzenie nowego konta z weryfikacją emaila
4. **Odzyskiwanie hasła**: Proces resetowania hasła przez email z unieważnieniem wszystkich aktywnych sesji
5. **Wylogowanie**: Niszczenie sesji i czyszczenie ciasteczek

Każdy scenariusz uwzględnia przypadki sukcesu i błędów, pokazując pełne przepływy danych między warstwami aplikacji.
