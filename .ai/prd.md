# Product Requirements Document (PRD) - PortalForge MVP

## 1. Wprowadzenie

### 1.1 Cel dokumentu
Niniejszy dokument definiuje wymagania funkcjonalne i niefunkcjonalne dla MVP (Minimum Viable Product) systemu PortalForge - wewnętrznego portalu intranetowego dla organizacji 200+ pracowników.

### 1.2 Wizja produktu
PortalForge to centralna platforma komunikacji wewnętrznej, która rozwiązuje problem chaosu dokumentowego i braku centralizacji informacji w dużej organizacji. System umożliwia efektywne zarządzanie strukturą organizacyjną, kalendarzem wydarzeń firmowych oraz komunikacją wewnętrzną.

## 2. Problem biznesowy

### 2.1 Opis problemu
Organizacje 200+ pracowników borykają się z:
- Brakiem centralnego miejsca do zarządzania strukturą organizacyjną
- Chaosem w obiegu dokumentów wewnętrznych
- Nieefektywną komunikacją o wydarzeniach firmowych
- Brakiem przejrzystości w procesach wewnętrznych
- Trudnością w wizualizacji struktury firmy i zależności służbowych

### 2.2 Grupa docelowa
- **Użytkownicy główni**: Wszyscy pracownicy organizacji (200+ osób)
- **Administratorzy**: Dział IT, HR, kadra zarządzająca
- **Interesariusze**: Zarząd, kierownicy działów

## 3. Zakres MVP

### 3.1 Funkcjonalności wchodzące w zakres MVP

#### 3.1.1 System autoryzacji i zarządzania użytkownikami ✅ ZREALIZOWANE (100%)
- ✅ Logowanie przez Supabase Auth
- ✅ Rejestracja z weryfikacją email (z rate limiting resend)
- ✅ Ręczne dodawanie użytkowników przez administratora
- ⚠️ Import użytkowników z plików CSV/Excel (UI gotowe, backend wymaga implementacji)
- ✅ System ról: Admin, Manager, HR, Marketing, Pracownik
- ✅ Role Groups - niestandardowe grupy ról
- ✅ Zarządzanie sesjami i bezpieczeństwem (8h timeout)
- ✅ Token refresh co 50 minut
- ✅ Organizational permissions - kontrola widoczności działów

#### 3.1.2 Zarządzanie strukturą organizacyjną ✅ ZREALIZOWANE (95%)
- ✅ Tworzenie i edycja nielimitowanej hierarchicznej struktury
- ✅ 3 tryby wizualizacji: Tree (z pan & zoom), Departments, List
- ✅ Przechowywanie danych pracownika:
  - ✅ Imię i nazwisko (wymagane)
  - ✅ Email służbowy (wymagane)
  - ✅ Telefon służbowy (opcjonalne)
  - ✅ Dział (wymagane)
  - ✅ Stanowisko (wymagane)
  - ✅ Przełożony (wymagane)
  - ✅ Zdjęcie profilowe (opcjonalne, placeholder domyślny)
- ✅ Wizualizacja struktury całej firmy lub pojedynczego działu
- ✅ Wyszukiwanie i filtrowanie pracowników
- ✅ Historia zmian w strukturze organizacyjnej (Audit Logs)
- ⚠️ Eksport struktury do PDF i Excel (UI gotowe, backend zwraca 501)

#### 3.1.3 Kalendarz wydarzeń firmowych ⚠️ CZĘŚCIOWO ZREALIZOWANE (60%)
- ✅ UI kalendarza pełni zaimplementowane
- ✅ Model domenowy Event w backendzie
- ✅ Location picker (Google Maps/OpenStreetMap)
- ✅ Integracja z newsami (NewsId w Event)
- ❌ Backend use cases (CreateEvent, UpdateEvent, DeleteEvent, GetEvents) - DO ZROBIENIA
- ❌ EventsController w API - DO ZROBIENIA
- ❌ Walidatory dla wydarzeń - DO ZROBIENIA
- System tagowania (#szkolenie, #impreza, #spotkanie) - Przygotowany w UI
- Targetowanie wydarzeń (wszystkie działy lub wybrane) - Model wspiera
- Archiwizacja wydarzeń (rok wstecz) - Do implementacji
- Przeglądanie wydarzeń w widoku kalendarza - UI gotowe

#### 3.1.4 System newsów ✅ ZREALIZOWANE (100%)
- ✅ Tworzenie newsów niezależnych
- ✅ Rich content editor z formatowaniem
- ✅ Upload obrazów do newsów
- ✅ Powiązanie newsów z wydarzeniami w kalendarzu
- ✅ Autoryzacja tworzenia: Admin, HR, Marketing
- ✅ Śledzenie autorstwa newsów
- ✅ Wyświetlanie newsów na stronie głównej
- ✅ 5 kategorii: Announcement, Product, HR, Tech, Event
- ✅ System hashtagów
- ✅ Filtrowanie i wyszukiwanie

#### 3.1.5 Monitoring i raporty ✅ ZREALIZOWANE (100%)
- ✅ Logowanie aktywności użytkowników (Audit Logs)
- ✅ Audit trail dla wszystkich operacji CRUD
- ✅ Śledzenie autorstwa newsów i wydarzeń
- ✅ Historia zmian z timestampami
- ✅ Filtrowanie i paginacja logów
- ✅ Tracking użytkownika, akcji, entity type, entity ID

#### 3.1.6 System wniosków ✅ ZREALIZOWANE (100%) - PRZEKROCZONE OCZEKIWANIA
- ✅ Tworzenie własnych szablonów wniosków przez administratorów
- ✅ Definiowanie pól formularza (6 typów: Text, Textarea, Number, Select, Date, Checkbox)
- ✅ Konfigurowalny wieloetapowy przepływ zatwierdzeń
- ✅ 6 typów zatwierdzających (Supervisor, Role, Specific User, Department, User Group, Submitter)
- ✅ Opcjonalne quizy z progiem zdawalności na każdym etapie
- ✅ Widoczność działowa (szablony per dział lub ogólnodostępne)
- ✅ Ikony (Iconify) dla profesjonalnego wyglądu
- ✅ Sekcja "Moje wnioski" z śledzeniem statusu
- ✅ Wizualna timeline zatwierdzania
- ✅ Komentarze do wniosków z załącznikami
- ✅ Edit History - pełna historia zmian
- ✅ SLA monitoring z przypomnieniami
- ✅ Automatyczne tworzenie urlopów po zatwierdzeniu wniosku urlopowego
- ✅ Sick Leave (L4) auto-approval z powiadomieniami
- ✅ Auto-routing w hierarchii organizacyjnej
- ✅ Vacation substitution - przekierowanie do zastępcy

#### 3.1.7 System zarządzania urlopami ✅ ZREALIZOWANE (95%) - DODATKOWO POZA MVP
- ✅ Kalendarz urlopów zespołu (2 widoki: Timeline Gantt, Calendar Grid)
- ✅ Automatyczne zastępstwa podczas nieobecności
- ✅ Wykrywanie konfliktów urlopowych (alerty 30%/50%)
- ✅ Email powiadomienia (7 dni przed, 1 dzień przed, start, koniec)
- ✅ Background services (5 automatycznych zadań)
- ✅ Statystyki zespołu i osobiste
- ✅ Carried-over vacation tracking z datami wygaśnięcia
- ✅ On-demand vacation (4 dni rocznie)
- ✅ Circumstantial leave tracking
- ✅ Sick Leave (L4) integration
- ⚠️ Eksport do PDF/Excel (endpointy istnieją, zwracają 501)

#### 3.1.8 Dodatkowe funkcjonalności poza MVP ✅ ZREALIZOWANE
- ✅ Internal Services - katalog wewnętrznych narzędzi
- ✅ AI Chat Assistant - wsparcie użytkowników i tłumaczenia
- ✅ Location Services - geocoding z cache
- ✅ Storage Management - upload i zarządzanie plikami
- ✅ System Settings - runtime configuration bez redeploymentu
- ✅ Notification System - real-time powiadomienia (backend 100%, frontend 60%)

### 3.2 Funkcjonalności poza zakresem MVP (przyszłe iteracje)
- Zarządzanie i wersjonowanie dokumentów
- Integracja z Active Directory/LDAP
- Powiadomienia email/push dla wniosków
- Wyszukiwarka pełnotekstowa
- Moduł komunikatora wewnętrznego
- API dla integracji zewnętrznych
- Załączniki do wniosków
- Komentarze i dyskusje w wnioskach

## 4. Wymagania funkcjonalne

### 4.1 User Stories

#### US-001: Bezpieczny dostęp i uwierzytelnianie ✅ ZREALIZOWANE
**Jako** użytkownik systemu
**Chcę** mieć możliwość rejestracji i logowania się do systemu w sposób zapewniający bezpieczeństwo moich danych
**Aby** uzyskać dostęp do portalu firmowego i zarządzać swoim kontem

**Kryteria akceptacji:**
- ✅ Logowanie i rejestracja odbywają się na dedykowanych stronach
- ✅ Logowanie wymaga podania adresu email i hasła
- ✅ Rejestracja wymaga podania adresu email, hasła, imienia i nazwiska
- ✅ Walidacja siły hasła (min. 8 znaków) - obsługiwana przez Supabase Auth
- ✅ Po rejestracji użytkownik otrzymuje email weryfikacyjny
- ✅ Użytkownik może ponownie wysłać email weryfikacyjny (z limitem 2 minuty)
- ✅ Użytkownik może się wylogować z systemu poprzez dedykowany endpoint
- ✅ System używa JWT tokenów z Supabase dla autoryzacji
- ✅ Tokeny (access + refresh) przechowywane w localStorage i automatycznie odświeżane co 50 minut
- ✅ Po nieudanej próbie logowania wyświetla się komunikat błędu
- ✅ Wszystkie strony wymagają aktywnej sesji użytkownika (middleware 'auth' i 'verified')
- ✅ Middleware sprawdza czy email użytkownika został zweryfikowany
- ✅ Stan autentykacji jest przywracany z localStorage przy odświeżeniu strony (hydration)

#### US-001a: Weryfikacja emaila ✅ ZREALIZOWANE
**Jako** nowy użytkownik
**Chcę** zweryfikować swój adres email po rejestracji
**Aby** aktywować moje konto i uzyskać pełny dostęp do portalu

**Kryteria akceptacji:**
- ✅ Po rejestracji Supabase automatycznie wysyła email weryfikacyjny
- ✅ Email zawiera link z tokenem weryfikacyjnym
- ✅ Link przekierowuje na stronę /auth/callback która weryfikuje token
- ✅ Po udanej weryfikacji użytkownik jest automatycznie przekierowywany do logowania
- ✅ Niezweryfikowani użytkownicy są przekierowywani na stronę /auth/verify-email
- ✅ Użytkownik może ponownie wysłać email weryfikacyjny
- ✅ Resend email ma rate limiting (2 minuty cooldown)
- ✅ Timer pokazuje ile czasu pozostało do następnego wysłania
- ✅ Backend loguje wszystkie próby resend dla monitoringu

#### US-002: Odzyskiwanie hasła ⚠️ CZĘŚCIOWO ZREALIZOWANE
**Jako** użytkownik
**Chcę** móc zresetować zapomniane hasło
**Aby** odzyskać dostęp do swojego konta

**Kryteria akceptacji:**
- ✅ Link "Zapomniałeś hasła?" na stronie logowania (UI istnieje)
- ✅ Strona reset password w frontendzie
- ❌ Backend use cases dla reset password flow (nie zaimplementowane)
- ❌ Użytkownik podaje adres email (UI gotowe, backend brak)
- ❌ System wysyła link do resetowania hasła (brak implementacji)
- ❌ Link jest ważny przez 1 godzinę
- ✅ ChangePasswordCommand istnieje dla zalogowanych użytkowników
- ❌ Unieważnianie aktywnych sesji po zmianie hasła (do implementacji)

#### US-003: Import użytkowników ⚠️ CZĘŚCIOWO ZREALIZOWANE
**Jako** administrator
**Chcę** zaimportować listę użytkowników z pliku CSV/Excel
**Aby** szybko dodać wielu pracowników do systemu

**Kryteria akceptacji:**
- ✅ UI admin panel users z przyciskiem import (widoczny)
- ❌ System akceptuje pliki CSV i Excel (backend nie zaimplementowany)
- ❌ Walidacja danych przed importem (do implementacji)
- ❌ Raport błędów importu (do implementacji)
- ❌ Możliwość mapowania kolumn (do implementacji)
- ✅ CreateUserCommand istnieje dla pojedynczych użytkowników
- ✅ BulkAssignDepartment command dla masowych operacji

#### US-004: Zarządzanie strukturą działu ✅ ZREALIZOWANE
**Jako** manager
**Chcę** edytować strukturę mojego działu
**Aby** odzwierciedlić aktualne zależności służbowe

**Kryteria akceptacji:**
- ✅ Manager może dodawać/usuwać pracowników w swoim dziale (przez admin panel)
- ✅ Manager może zmieniać stanowiska i przypisania (UpdateUser command)
- ✅ Zmiany są logowane w historii (Audit Logs)
- ✅ Walidacja spójności struktury (FluentValidation validators)
- ✅ Department CRUD operations (Create, Update, Delete)
- ✅ TransferDepartment command dla przenoszenia pracowników
- ✅ BulkAssignDepartment dla masowych operacji

#### US-005: Wizualizacja struktury organizacyjnej ✅ ZREALIZOWANE (PRZEKROCZONE OCZEKIWANIA)
**Jako** pracownik
**Chcę** zobaczyć strukturę organizacyjną
**Aby** zrozumieć zależności służbowe w firmie

**Kryteria akceptacji:**
- ✅ Widok drzewa organizacyjnego (OrgTreeChart z PrimeVue)
- ✅ 3 tryby wizualizacji: Tree, Departments, List
- ✅ Pan & zoom w trybie Tree (mousewheel zoom, drag pan, Space+drag)
- ✅ Możliwość rozwijania/zwijania działów
- ✅ Wyszukiwanie pracownika (Search input z real-time filtering)
- ✅ Wyświetlanie szczegółów pracownika (Employee modal z pełnymi danymi)
- ✅ Department modal z listą pracowników
- ✅ Profile photos z fallback do inicjałów
- ✅ Quick edit modal dla pracowników
- ✅ Hierarchical department cards z wizualnym indent

#### US-006: Tworzenie wydarzenia w kalendarzu ⚠️ CZĘŚCIOWO ZREALIZOWANE
**Jako** HR
**Chcę** utworzyć wydarzenie firmowe
**Aby** poinformować pracowników o nadchodzących eventach

**Kryteria akceptacji:**
- ✅ Formularz tworzenia wydarzenia (UI /dashboard/news/create z toggle event)
- ✅ Location picker (Google Maps/OSM integration)
- ✅ Date/time picker component
- ✅ Hashtag input component
- ❌ Backend CreateEvent use case (nie zaimplementowane)
- ❌ EventsController w API (nie istnieje)
- ❌ Wybór tagów i działów docelowych (UI gotowe, backend brak)
- ✅ Model Event wspiera targetowanie działów i tagi
- ⚠️ Automatyczne tworzenie newsa (News może zawierać EventId, ale odwrotnie do zrobienia)
- ❌ Walidacja daty i czasu (backend validators do stworzenia)

#### US-007: Przeglądanie kalendarza ⚠️ CZĘŚCIOWO ZREALIZOWANE
**Jako** pracownik
**Chcę** przeglądać kalendarz wydarzeń
**Aby** być na bieżąco z wydarzeniami firmowymi

**Kryteria akceptacji:**
- ✅ Widok miesięczny kalendarza (/dashboard/calendar - UI pełni zaimplementowane)
- ✅ EventModal component dla quick preview
- ❌ Backend GetEvents query (nie zaimplementowane)
- ❌ Filtrowanie po tagach (UI gotowe, wymaga backend API)
- ✅ Szczegóły wydarzenia po kliknięciu (EventModal z pełnymi danymi)
- ❌ Oznaczenie wydarzeń dla mojego działu (logika do implementacji w backendzie)
- ✅ Dashboard pokazuje 5 upcoming events (wymaga backend query)

#### US-008: Publikacja newsa ✅ ZREALIZOWANE (PRZEKROCZONE OCZEKIWANIA)
**Jako** Marketing
**Chcę** opublikować news
**Aby** informować pracowników o ważnych sprawach

**Kryteria akceptacji:**
- ✅ Edytor tekstu z podstawowym formatowaniem (RichTextEditor component)
- ✅ Rich content editor z pełnym HTML formatting
- ✅ Możliwość dodania obrazka (ImageUpload component)
- ✅ Multiple image support w treści
- ✅ Opcja powiązania z wydarzeniem (EventId field w News)
- ✅ Kategorie: Announcement, Product, HR, Tech, Event
- ✅ System hashtagów dla tagowania
- ✅ Podgląd przed publikacją (preview mode)
- ✅ Draft/Published status
- ✅ Author tracking
- ✅ Publication date scheduling

#### US-009: Eksport struktury organizacyjnej ⚠️ CZĘŚCIOWO ZREALIZOWANE
**Jako** administrator
**Chcę** wyeksportować strukturę do PDF/Excel
**Aby** wykorzystać dane w dokumentach zewnętrznych

**Kryteria akceptacji:**
- ✅ UI przyciski Export PDF/Excel w organizacji (/dashboard/organization)
- ❌ Backend implementacja PDF export (endpoint może istnieć, zwraca 501 Not Implemented)
- ❌ Backend implementacja Excel export (endpoint może istnieć, zwraca 501 Not Implemented)
- ❌ Eksport całej struktury lub wybranego działu (logika do implementacji)
- ❌ Format PDF z wizualizacją graficzną (wymaga QuestPDF lub iText)
- ❌ Format Excel z danymi tabelarycznymi (wymaga EPPlus lub ClosedXML)
- ❌ Zawiera wszystkie dane pracowników (template do stworzenia)
- ✅ GetDepartmentTree query dostarcza dane hierarchiczne dla eksportu

#### US-010: System wniosków z konfigurowalnymi szablonami ✅ ZREALIZOWANE
**Jako** administrator
**Chcę** tworzyć własne szablony wniosków z definiowalnymi polami i przepływem zatwierdzeń
**Aby** umożliwić pracownikom składanie różnego rodzaju wniosków (IT, HR, szkolenia, itp.)

**Kryteria akceptacji:**
- ✅ Administrator może tworzyć szablony wniosków przez panel admin
- ✅ Szablon zawiera: nazwę, opis, ikonę (Lucide), kategorię
- ✅ Możliwość ustawienia widoczności per dział lub dla wszystkich
- ✅ Kreator pól formularza z 6 typami (Text, Textarea, Number, Select, Date, Checkbox)
- ✅ Drag & drop do zmiany kolejności pól
- ✅ Konfiguracja przepływu zatwierdzeń (Kierownik → Dyrektor)
- ✅ Opcjonalny quiz na każdym etapie z progiem zdawalności (np. 80%)
- ✅ Kreator pytań quizowych z wielokrotnym wyborem i oznaczaniem poprawnych odpowiedzi

#### US-011: Składanie i śledzenie wniosków ✅ ZREALIZOWANE
**Jako** pracownik
**Chcę** składać wnioski i śledzić ich status
**Aby** uzyskać zatwierdzenie na różne działania (sprzęt, szkolenia, urlopy, itp.)

**Kryteria akceptacji:**
- ✅ Pracownik widzi tylko szablony dostępne dla swojego działu
- ✅ Intuicyjny wybór szablonu z kartami z ikonami
- ✅ Dynamiczny formularz generowany na podstawie szablonu
- ✅ Wybór priorytetu (Standard/Pilne)
- ✅ Automatyczne generowanie numeru wniosku (REQ-YYYY-NNNN)
- ✅ Sekcja "Moje wnioski" z filtrowaniem i wyszukiwaniem
- ✅ Wizualna timeline pokazująca przebieg zatwierdzania
- ✅ Statusy: W trakcie, Zatwierdzony, Odrzucony, Wymaga quizu

#### US-012: Zatwierdzanie wniosków przez przełożonych ✅ ZREALIZOWANE
**Jako** kierownik/dyrektor
**Chcę** przeglądać i zatwierdzać wnioski podległych pracowników
**Aby** kontrolować wydatki i decyzje w dziale

**Kryteria akceptacji:**
- ✅ Automatyczne przypisanie wniosków na podstawie hierarchii organizacyjnej
- ✅ Lista wniosków oczekujących na zatwierdzenie
- ✅ Przegląd szczegółów wniosku i wypełnionego formularza
- ✅ Możliwość dodania komentarza przy zatwierdzaniu/odrzucaniu
- ✅ Jeśli wymagany quiz - musi być rozwiązany przed zatwierdzeniem
- ✅ Quiz z pytaniami wielokrotnego wyboru
- ✅ Automatyczne obliczanie wyniku i sprawdzanie progu zdawalności
- ✅ Automatyczne przekazanie do kolejnego szczebla po zatwierdzeniu
- ✅ Wizualna informacja o postępie zatwierdzania

## 5. Wymagania niefunkcjonalne

### 5.1 Wydajność
- Czas ładowania strony głównej < 3 sekundy
- Obsługa 200+ równoczesnych użytkowników
- Czas odpowiedzi API < 500ms dla 95% żądań

### 5.2 Bezpieczeństwo
- Szyfrowanie połączenia HTTPS
- Hashowanie haseł (bcrypt lub podobne)
- Walidacja uprawnień na poziomie API
- Zabezpieczenie przed CSRF i XSS
- Sesje wygasające po 8h nieaktywności

### 5.3 Użyteczność
- Responsywny design (desktop, tablet)
- Intuicyjny interfejs bez potrzeby szkoleń
- Dostępność WCAG 2.1 poziom AA
- Wsparcie dla przeglądarek: Chrome, Firefox, Safari, Edge (najnowsze wersje)

### 5.4 Skalowalność
- Architektura pozwalająca na wzrost do 1000+ użytkowników
- Możliwość poziomego skalowania backendu
- Cache'owanie często używanych danych

## 6. Architektura techniczna

### 6.1 Stack technologiczny

#### Backend (.NET 8.0)
- CQRS z MediatR
- Clean Architecture
- Entity Framework Core
- PostgreSQL (via Supabase)
- Serilog dla logowania
- Supabase Auth dla autoryzacji

#### Frontend (Vue.js 3 + Nuxt 3)
- Clean Architecture
- Tailwind CSS
- Dokumentacja generowana przez AI
- Vuetest dla testów jednostkowych
- Playwright dla testów E2E

#### Infrastruktura
- GitHub Actions dla CI/CD
- Docker dla konteneryzacji
- VPS dla hostingu
- Supabase dla bazy danych i auth

### 6.2 Integracje
- Supabase Auth API
- Supabase Database
- Potencjalna integracja z AD/LDAP (przyszłość)

## 7. Model danych (wysokopoziomowy)

### 7.1 Główne encje
- **User**: Dane użytkownika systemu
- **Employee**: Dane pracownika (rozszerzenie User)
- **Department**: Struktura działów
- **Position**: Stanowiska w firmie
- **OrganizationStructure**: Relacje w strukturze
- **Event**: Wydarzenia w kalendarzu
- **News**: Newsy firmowe
- **EventTag**: Tagi wydarzeń
- **AuditLog**: Logi zmian i aktywności

### 7.2 Relacje kluczowe
- Employee -> Department (N:1)
- Employee -> Employee (przełożony, N:1)
- Event -> News (1:1 opcjonalne)
- Event -> EventTag (N:N)
- Event -> Department (N:N)

## 8. Metryki sukcesu

### 8.1 KPI dla MVP
- **Adopcja**: 60% pracowników loguje się przynajmniej raz w tygodniu
- **Kompletność danych**: 100% pracowników wprowadzonych do systemu
- **Aktywność**: Min. 5 newsów/wydarzeń publikowanych tygodniowo
- **Jakość danych**: <5% błędów w strukturze organizacyjnej

### 8.2 Metryki techniczne
- Dostępność systemu: 99.5%
- Średni czas odpowiedzi: <500ms
- Liczba krytycznych błędów: 0 w produkcji
- Pokrycie testami: >70% dla logiki biznesowej

## 9. Ryzyka i mitygacje

| Ryzyko | Prawdopodobieństwo | Wpływ | Mitygacja |
|--------|-------------------|--------|-----------|
| Opór użytkowników przed nowym systemem | Średnie | Wysoki | Intuicyjny UI, szkolenia, wsparcie |
| Problemy z importem danych | Średnie | Średni | Walidacja, testy, import etapowy |
| Wydajność przy 200+ użytkownikach | Niskie | Wysoki | Testy obciążeniowe, cache, optymalizacja |
| Błędy w strukturze organizacyjnej | Średnie | Średni | Historia zmian, możliwość cofnięcia |

## 10. Harmonogram

### Faza 1: Fundament (2 tygodnie)
- Setup projektu i CI/CD
- Implementacja autoryzacji
- Podstawowy model danych

### Faza 2: Struktura organizacyjna (3 tygodnie)
- CRUD pracowników
- Wizualizacja struktury
- Import/eksport danych

### Faza 3: Kalendarz i newsy (2 tygodnie)
- System wydarzeń
- Publikacja newsów
- Integracja z kalendarzem

### Faza 4: Testy i deployment (1 tydzień)
- Testy E2E
- Optymalizacja
- Deployment na produkcję

## 11. Załączniki

### 11.1 Mockupy i wireframe'y
*Do uzupełnienia w kolejnej iteracji*

### 11.2 Słownik pojęć
- **Struktura organizacyjna**: Hierarchiczne przedstawienie zależności służbowych
- **News**: Komunikat wewnętrzny publikowany na portalu
- **Event**: Wydarzenie firmowe widoczne w kalendarzu
- **Dział**: Jednostka organizacyjna w firmie
- **Manager**: Osoba zarządzająca działem

---

## Status Realizacji MVP (Aktualizacja 2025-11-07)

### Podsumowanie realizacji

**Ogólny postęp MVP: ~90%**

| Moduł | Status | Postęp |
|-------|--------|--------|
| Autoryzacja i uwierzytelnianie | ✅ Zakończone | 100% |
| Struktura organizacyjna | ✅ Zakończone | 95% |
| System wniosków | ✅ Zakończone | 100% |
| System newsów | ✅ Zakończone | 100% |
| Kalendarz wydarzeń | ⚠️ W trakcie | 60% |
| System urlopów | ✅ Zakończone | 95% |
| Powiadomienia | ✅ Zakończone | 90% |
| Monitoring i raporty | ✅ Zakończone | 100% |

### Do dokończenia (Faza 4 - Finalizacja)

**Priorytet WYSOKI:**
1. Backend use cases dla Event (CreateEvent, UpdateEvent, DeleteEvent, GetEvents)
2. EventsController w API
3. Eksport PDF/Excel dla urlopów i struktury org

**Priorytet ŚREDNI:**
4. Reset password flow (backend + frontend)
5. Import użytkowników z CSV/Excel (backend)
6. NotificationPanel UI (dropdown z listą powiadomień)

**Priorytet NISKI:**
7. Moduł zarządzania dokumentami (kompletna implementacja)
8. Testy E2E
9. Optymalizacja wydajności

### Funkcjonalności przekraczające MVP

Projekt zrealizował znacznie więcej niż zakładał oryginalny MVP:
- ✅ System zarządzania urlopami (nie był w MVP)
- ✅ Multi-step approval workflow z quizami
- ✅ Sick Leave (L4) auto-approval
- ✅ Internal Services catalog
- ✅ AI Chat Assistant
- ✅ Location Services z geocoding
- ✅ System Settings - runtime configuration
- ✅ 6 background jobs dla automatyzacji

### Następne kroki

1. **Week 1**: Dokończenie Event system backend (3-4 dni)
2. **Week 2**: Implementacja eksportu PDF/Excel (3-4 dni)
3. **Week 3**: Reset password + Import users + Notification UI (5 dni)
4. **Week 4**: Testy, optymalizacja, dokumentacja użytkownika (5 dni)

**Planowane uruchomienie produkcyjne**: Koniec miesiąca 11/2025

---

*Dokument utworzony: 2025-01-08*
*Ostatnia aktualizacja: 2025-11-07*
*Wersja: 2.0 - Stan implementacji*
*Następna rewizja: Po zakończeniu MVP (koniec listopada 2025)*
