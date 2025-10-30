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

#### 3.1.1 System autoryzacji i zarządzania użytkownikami
- Logowanie przez Supabase Auth
- Ręczne dodawanie użytkowników przez administratora
- Import użytkowników z plików CSV/Excel
- System ról: Admin, Manager, HR, Marketing, Pracownik
- Zarządzanie sesjami i bezpieczeństwem

#### 3.1.2 Zarządzanie strukturą organizacyjną
- Tworzenie i edycja hierarchicznej struktury drzewiaste
- Przechowywanie danych pracownika:
  - Imię i nazwisko (wymagane)
  - Email służbowy (wymagane)
  - Telefon służbowy (opcjonalne)
  - Dział (wymagane)
  - Stanowisko (wymagane)
  - Przełożony (wymagane)
  - Zdjęcie profilowe (opcjonalne, placeholder domyślny)
- Wizualizacja struktury całej firmy lub pojedynczego działu
- Historia zmian w strukturze organizacyjnej
- Eksport struktury do PDF i Excel

#### 3.1.3 Kalendarz wydarzeń firmowych
- Tworzenie wydarzeń przez Admin, HR, Marketing
- System tagowania (#szkolenie, #impreza, #spotkanie)
- Targetowanie wydarzeń (wszystkie działy lub wybrane)
- Archiwizacja wydarzeń (rok wstecz)
- Przeglądanie wydarzeń w widoku kalendarza

#### 3.1.4 System newsów
- Tworzenie newsów niezależnych
- Powiązanie newsów z wydarzeniami w kalendarzu
- Autoryzacja tworzenia: Admin, HR, Marketing
- Śledzenie autorstwa newsów
- Wyświetlanie newsów na stronie głównej

#### 3.1.5 Monitoring i raporty
- Logowanie aktywności użytkowników (data/czas logowania)
- Raport aktywnych użytkowników (logowania w ostatnim tygodniu)
- Śledzenie autorstwa newsów i wydarzeń

#### 3.1.6 System wniosków ✅ ZREALIZOWANE
- Tworzenie własnych szablonów wniosków przez administratorów
- Definiowanie pól formularza (6 typów: Text, Textarea, Number, Select, Date, Checkbox)
- Konfigurowalny przepływ zatwierdzeń (Kierownik → Dyrektor)
- Opcjonalne quizy z progiem zdawalności
- Widoczność działowa (szablony per dział lub ogólnodostępne)
- Ikony Lucide dla profesjonalnego wyglądu
- Sekcja "Moje wnioski" z śledzeniem statusu
- Wizualna timeline zatwierdzania

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

#### US-002: Odzyskiwanie hasła
**Jako** użytkownik
**Chcę** móc zresetować zapomniane hasło
**Aby** odzyskać dostęp do swojego konta

**Kryteria akceptacji:**
- Link "Zapomniałeś hasła?" na stronie logowania
- Użytkownik podaje adres email
- System wysyła link do resetowania hasła
- Link jest ważny przez 1 godzinę
- Użytkownik może ustawić nowe hasło
- Po zmianie hasła wszystkie aktywne sesje są wylogowywane

#### US-003: Import użytkowników
**Jako** administrator
**Chcę** zaimportować listę użytkowników z pliku CSV/Excel
**Aby** szybko dodać wielu pracowników do systemu

**Kryteria akceptacji:**
- System akceptuje pliki CSV i Excel
- Walidacja danych przed importem
- Raport błędów importu
- Możliwość mapowania kolumn

#### US-004: Zarządzanie strukturą działu
**Jako** manager
**Chcę** edytować strukturę mojego działu
**Aby** odzwierciedlić aktualne zależności służbowe

**Kryteria akceptacji:**
- Manager może dodawać/usuwać pracowników w swoim dziale
- Manager może zmieniać stanowiska i przypisania
- Zmiany są logowane w historii
- Walidacja spójności struktury

#### US-005: Wizualizacja struktury organizacyjnej
**Jako** pracownik
**Chcę** zobaczyć strukturę organizacyjną
**Aby** zrozumieć zależności służbowe w firmie

**Kryteria akceptacji:**
- Widok drzewa organizacyjnego
- Możliwość rozwijania/zwijania działów
- Wyszukiwanie pracownika
- Wyświetlanie szczegółów pracownika

#### US-006: Tworzenie wydarzenia w kalendarzu
**Jako** HR
**Chcę** utworzyć wydarzenie firmowe
**Aby** poinformować pracowników o nadchodzących eventach

**Kryteria akceptacji:**
- Formularz tworzenia wydarzenia
- Wybór tagów i działów docelowych
- Automatyczne tworzenie newsa
- Walidacja daty i czasu

#### US-007: Przeglądanie kalendarza
**Jako** pracownik
**Chcę** przeglądać kalendarz wydarzeń
**Aby** być na bieżąco z wydarzeniami firmowymi

**Kryteria akceptacji:**
- Widok miesięczny kalendarza
- Filtrowanie po tagach
- Szczegóły wydarzenia po kliknięciu
- Oznaczenie wydarzeń dla mojego działu

#### US-008: Publikacja newsa
**Jako** Marketing
**Chcę** opublikować news
**Aby** informować pracowników o ważnych sprawach

**Kryteria akceptacji:**
- Edytor tekstu z podstawowym formatowaniem
- Możliwość dodania obrazka
- Opcja powiązania z wydarzeniem
- Podgląd przed publikacją

#### US-009: Eksport struktury organizacyjnej
**Jako** administrator
**Chcę** wyeksportować strukturę do PDF/Excel
**Aby** wykorzystać dane w dokumentach zewnętrznych

**Kryteria akceptacji:**
- Eksport całej struktury lub wybranego działu
- Format PDF z wizualizacją graficzną
- Format Excel z danymi tabelarycznymi
- Zawiera wszystkie dane pracowników

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

*Dokument utworzony: 2025-01-08*
*Wersja: 1.0 MVP*
*Następna rewizja: Po zakończeniu MVP*
