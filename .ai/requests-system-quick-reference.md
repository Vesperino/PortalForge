# Requests System - Quick Reference Guide

## 🎯 Overview

System wniosków PortalForge umożliwia tworzenie własnych formularzy z konfigurowalnymi polami, przepływem zatwierdzeń i opcjonalnymi quizami.

## 🚀 Quick Start (Po implementacji)

### 1. Zastosuj migrację

```bash
cd backend/PortalForge.Api
dotnet ef database update
```

### 2. Zaseeduj dane

```bash
# Uprawnienia (jeśli jeszcze nie wykonano)
POST http://localhost:5155/api/admin/seed

# Przykładowe szablony wniosków
POST http://localhost:5155/api/request-templates/seed
```

### 3. Ustaw DepartmentRole użytkownikom

```sql
-- Przykład: Ustaw role bazując na hierarchii
UPDATE "public"."Users" 
SET "DepartmentRole" = 'Manager' 
WHERE "Email" IN ('kierownik1@firma.pl', 'kierownik2@firma.pl');

UPDATE "public"."Users" 
SET "DepartmentRole" = 'Director' 
WHERE "Email" IN ('dyrektor1@firma.pl', 'dyrektor2@firma.pl');
```

## 📚 Główne Koncepcje

### DepartmentRole (Rola Działowa)

Nowe pole w tabeli Users określające pozycję w hierarchii:
- **Employee** (domyślnie) - Zwykły pracownik
- **Manager** - Kierownik (może zatwierdzać wnioski etap 1)
- **Director** - Dyrektor (może zatwierdzać wnioski etap 2)

### RequestTemplate (Szablon Wniosku)

Definicja formularza zawierająca:
- Pola formularza (dynamiczne)
- Etapy zatwierdzania
- Pytania quizowe (opcjonalnie)
- Widoczność (dział lub wszystkie)

### Approval Workflow

```
User submits request
    ↓
Manager reviews (etap 1)
    ↓ (jeśli wymaga quizu → rozwiąż quiz)
    ↓ Approve
Director reviews (etap 2)
    ↓ (jeśli wymaga quizu → rozwiąż quiz)
    ↓ Approve
Request approved ✓
```

## 🎨 Przykładowe Szablony (Seed Data)

| Nazwa | Kategoria | Dział | Etapy | Quiz |
|-------|-----------|-------|-------|------|
| Zamówienie sprzętu IT | Hardware | IT | M→D | Nie |
| Szkolenie zewnętrzne | Training | Wszystkie | M→D | Tak (80%) |
| Dostęp do systemów R&D | Security | IT | M→D | Tak (80%) |
| Urlop szkoleniowy | HR | Wszystkie | M | Nie |
| Licencja na oprogramowanie | Software | Wszystkie | M→D | Nie |

## 🔑 API Endpoints

### Szablony

```http
# Lista wszystkich (admin)
GET /api/request-templates
Authorization: Bearer {token}
Permission: requests.manage_templates

# Dostępne dla użytkownika
GET /api/request-templates/available
Authorization: Bearer {token}

# Szczegóły szablonu
GET /api/request-templates/{id}
Authorization: Bearer {token}

# Tworzenie
POST /api/request-templates
Authorization: Bearer {token}
Permission: requests.manage_templates
Body: CreateRequestTemplateDto

# Seed przykładowych
POST /api/request-templates/seed
Authorization: Bearer {token}
Permission: requests.manage_templates
```

### Wnioski

```http
# Moje wnioski
GET /api/requests/my-requests
Authorization: Bearer {token}
Permission: requests.view

# Wnioski do zatwierdzenia
GET /api/requests/to-approve
Authorization: Bearer {token}
Permission: requests.approve

# Złożenie wniosku
POST /api/requests
Authorization: Bearer {token}
Permission: requests.create
Body: {
  "requestTemplateId": "guid",
  "priority": "Standard|Urgent",
  "formData": "{\"field1\":\"value1\"}"
}

# Zatwierdzenie kroku
POST /api/requests/{requestId}/steps/{stepId}/approve
Authorization: Bearer {token}
Permission: requests.approve
Body: {
  "comment": "Wygląda dobrze!"
}
```

## 🎯 Typowe Scenariusze

### Scenariusz 1: Utworzenie szablonu "Delegacja służbowa"

1. Admin → Panel Admin → Szablony wniosków → Nowy szablon
2. **Krok 1:**
   - Nazwa: "Delegacja służbowa"
   - Opis: "Wniosek o wyjazd służbowy"
   - Ikona: "Briefcase"
   - Kategoria: "Travel"
   - Dział: (puste - dla wszystkich)
3. **Krok 2:** Dodaj pola:
   - Miejsce (Text, wymagane)
   - Data wyjazdu (Date, wymagane)
   - Data powrotu (Date, wymagane)
   - Cel (Textarea, wymagane)
   - Koszt (Number, wymagane, min: 0)
4. **Krok 3:** Przepływ:
   - Etap 1: Kierownik (bez quizu)
   - Etap 2: Dyrektor (bez quizu)
5. **Krok 4:** Pomiń (brak quizu)
6. Zapisz

### Scenariusz 2: Złożenie wniosku przez użytkownika

1. Dashboard → Wnioski → Nowy wniosek
2. Wybierz "Delegacja służbowa"
3. Wypełnij formularz:
   ```json
   {
     "miejsce": "Warszawa",
     "dataWyjazdu": "2025-11-15",
     "dataPowrotu": "2025-11-17",
     "cel": "Konferencja IT Summit 2025",
     "koszt": 2500
   }
   ```
4. Priorytet: Standard
5. Złóż wniosek
6. Wniosek nr REQ-2025-0042 został utworzony
7. Status: W trakcie oceny (U Kierownika)

### Scenariusz 3: Zatwierdzanie z quizem

1. Kierownik → Dashboard → (wniosek pojawia się automatycznie)
2. Zobacz szczegóły
3. Zatwierdź
4. System przekazuje do Dyrektora
5. Dyrektor → Zobacz szczegóły
6. Próba zatwierdzenia → "Wymaga quizu"
7. Rozwiąż quiz:
   - Pytanie 1: Odpowiedź A ✓
   - Pytanie 2: Odpowiedź C ✓
   - Wynik: 100% (zdany!)
8. Teraz może zatwierdzić
9. Status wniosku: Zatwierdzony ✓

## 🛠️ Typy Pól Formularza

| Typ | Opis | Opcje konfiguracji |
|-----|------|-------------------|
| Text | Pojedyncza linia tekstu | Placeholder, Required |
| Textarea | Wieloliniowy tekst | Placeholder, Required |
| Number | Liczba | Min, Max, Required |
| Select | Lista wyboru | Options (JSON), Required |
| Date | Wybór daty | Required |
| Checkbox | Pole wyboru | Required |

### Przykład konfiguracji Select:

```json
{
  "label": "Rodzaj urlopu",
  "fieldType": "Select",
  "options": "[{\"value\":\"training\",\"label\":\"Szkoleniowy\"},{\"value\":\"unpaid\",\"label\":\"Bezpłatny\"}]"
}
```

## 🎓 Quiz System

### Struktura pytania:

```json
{
  "question": "Jakie są obowiązki po szkoleniu?",
  "options": [
    {"value": "a", "label": "Podzielić się wiedzą", "isCorrect": true},
    {"value": "b", "label": "Brak obowiązków", "isCorrect": false},
    {"value": "c", "label": "Tylko praca", "isCorrect": false}
  ]
}
```

### Obliczanie wyniku:

```
Poprawne odpowiedzi: 4 / 5
Wynik: (4 / 5) * 100 = 80%
Próg: 80%
Status: Zdany ✓
```

## 🔐 Uprawnienia

| Uprawnienie | Role | Opis |
|-------------|------|------|
| `requests.view` | Wszystkie | Przeglądanie wniosków |
| `requests.create` | Wszystkie | Składanie wniosków |
| `requests.approve` | Manager, HR, Admin | Zatwierdzanie wniosków |
| `requests.manage_templates` | Admin | Zarządzanie szablonami |

## 🎨 Dostępne Ikony (wybrane)

### Hardware
Laptop, Monitor, Smartphone, Tablet, Keyboard, Mouse, Headphones, Printer, Server, HardDrive

### Dokumenty
FileText, File, Folder, Upload, Download, ClipboardList, ClipboardCheck

### Ludzie
Users, User, UserPlus, UserCheck

### Czas
Calendar, Clock, Bell

### Komunikacja
Mail, MessageSquare, Phone, Video, Camera

### Narzędzia
Settings, Tool, Wrench, Package

### Biznes
ShoppingCart, CreditCard, DollarSign, TrendingUp, BarChart, PieChart

### Bezpieczeństwo
Shield, Lock, Unlock, Key

### Status
AlertCircle, CheckCircle, XCircle, Info, HelpCircle

### Inne
Star, Heart, Bookmark, Flag, Tag, MapPin, Globe, Home, Building, Briefcase, Book, GraduationCap, Award, Target

## 🐛 Troubleshooting

### Problem: Szablon nie widoczny dla użytkownika

**Możliwe przyczyny:**
- Szablon ma ustawiony `DepartmentId` inny niż dział użytkownika
- Szablon jest nieaktywny (`IsActive = false`)

**Rozwiązanie:**
- Zmień `DepartmentId` na `null` (dla wszystkich)
- Lub upewnij się, że DepartmentId zgadza się z działem użytkownika
- Sprawdź `IsActive = true`

### Problem: Nie można zatwierdzić wniosku

**Możliwe przyczyny:**
- Użytkownik nie jest wyznaczonym zatwierdzającym
- Krok nie jest w statusie InReview
- Wymagany quiz nie został rozwiązany lub nie zdany

**Rozwiązanie:**
- Sprawdź `ApproverId` vs zalogowany użytkownik
- Sprawdź status kroku
- Rozwiąż quiz i uzyskaj wynik ≥ PassingScore

### Problem: Wniosek "zawiesił się"

**Możliwe przyczyny:**
- Użytkownik nie ma przełożonego (SupervisorId = null)
- Przełożony nie ma odpowiedniego DepartmentRole

**Rozwiązanie:**
- Przypisz SupervisorId użytkownikowi
- Ustaw DepartmentRole przełożonemu (Manager lub Director)

## 📊 Monitoring

### Zapytania diagnostyczne:

```sql
-- Wnioski w trakcie
SELECT * FROM "public"."Requests" 
WHERE "Status" = 'InReview';

-- Kroki oczekujące na zatwierdzenie
SELECT r."RequestNumber", ras."StepOrder", u."Email" as "Approver"
FROM "public"."RequestApprovalSteps" ras
JOIN "public"."Requests" r ON ras."RequestId" = r."Id"
JOIN "public"."Users" u ON ras."ApproverId" = u."Id"
WHERE ras."Status" = 'InReview';

-- Quizy niezdane
SELECT r."RequestNumber", ras."QuizScore", rt."PassingScore"
FROM "public"."RequestApprovalSteps" ras
JOIN "public"."Requests" r ON ras."RequestId" = r."Id"
JOIN "public"."RequestTemplates" rt ON r."RequestTemplateId" = rt."Id"
WHERE ras."Status" = 'SurveyFailed';
```

## 💡 Best Practices

### Tworzenie Szablonów

1. **Używaj opisowych nazw** - użytkownicy szukają po nazwie
2. **Kategorie konsekwentne** - np. IT, HR, Finance, Training
3. **Pola wymagane tylko gdy konieczne** - im mniej tym lepiej
4. **Quizy krótkie** - 3-5 pytań optymalnie
5. **Testuj szablon** - złóż testowy wniosek przed publikacją

### Przepływ Zatwierdzeń

1. **1 etap wystarczy** dla prostych wniosków (np. urlopy)
2. **2 etapy** dla kosztownych decyzji (sprzęt, szkolenia)
3. **Quiz tylko gdy uzasadnione** - bezpieczeństwo, compliance
4. **Próg 70-80%** jest optymalny

### Organizacja Szablonów

1. **IT:** Sprzęt, oprogramowanie, dostępy
2. **HR:** Urlopy, szkolenia, benefity
3. **Finance:** Wydatki, delegacje, zwroty
4. **All departments:** Szkolenia, konferencje, parking

## 🔄 Lifecycle Request

```
[Created] 
    ↓
[InReview] - Step 1 (Manager)
    ↓ Quiz? → Solve → Pass?
    ↓ Approve
[InReview] - Step 2 (Director)  
    ↓ Quiz? → Solve → Pass?
    ↓ Approve
[Approved] ✓

Możliwe ścieżki alternatywne:
- Reject na dowolnym etapie → [Rejected]
- Quiz failed → [AwaitingSurvey] → retry quiz
```

## 📖 Przykłady Użycia API

### Pobranie dostępnych szablonów

```typescript
const { getAvailableTemplates } = useRequestsApi()
const templates = await getAvailableTemplates()

// Response:
{
  "templates": [
    {
      "id": "uuid",
      "name": "Zamówienie sprzętu IT",
      "icon": "Laptop",
      "category": "Hardware",
      "departmentId": "IT",
      "fields": [...],
      "approvalStepTemplates": [...]
    }
  ]
}
```

### Złożenie wniosku

```typescript
const { submitRequest } = useRequestsApi()
const result = await submitRequest({
  requestTemplateId: templateId,
  priority: 'Standard',
  formData: {
    device: 'laptop',
    justification: 'Stary laptop jest zbyt wolny'
  }
})

// Response:
{
  "id": "uuid",
  "requestNumber": "REQ-2025-0042",
  "message": "Request submitted successfully"
}
```

### Zatwierdzenie kroku

```typescript
const { approveRequestStep } = useRequestsApi()
const result = await approveRequestStep(requestId, stepId, {
  comment: 'Akceptuję, uzasadnienie jest przekonujące'
})

// Response:
{
  "success": true,
  "message": "Step approved, moved to next approver"
}
```

## 🎯 UI Komponenty

### IconPicker
```vue
<IconPicker v-model="selectedIcon" />
<!-- Wybór z 80+ ikon Lucide -->
```

### RequestTimeline
```vue
<RequestTimeline :steps="request.approvalSteps" />
<!-- Wizualna timeline z statusami -->
```

### QuizModal
```vue
<QuizModal
  :questions="quizQuestions"
  :passing-score="80"
  @close="closeModal"
  @submit="handleQuizSubmit"
/>
<!-- Interaktywny quiz z oceną -->
```

## 📝 Checklisty

### ✅ Przed publikacją szablonu

- [ ] Nazwa jest opisowa i zrozumiała
- [ ] Opis wyjaśnia cel wniosku
- [ ] Ikona jest adekwatna
- [ ] Kategoria ustawiona
- [ ] Pola formularza są konieczne i wystarczające
- [ ] Przepływ zatwierdzania odpowiada procesowi biznesowemu
- [ ] Jeśli quiz - pytania są jasne i jednoznaczne
- [ ] Próg zdawalności jest rozsądny (70-80%)
- [ ] Testowy wniosek przeszedł pomyślnie

### ✅ Przed złożeniem wniosku (user)

- [ ] Wszystkie wymagane pola wypełnione
- [ ] Dane są poprawne i aktualne
- [ ] Priorytet odpowiada rzeczywistości
- [ ] Zrozumiałem szacowany czas procesowania

### ✅ Przed zatwierdzeniem (approver)

- [ ] Przejrzałem szczegóły wniosku
- [ ] Zweryfikowałem uzasadnienie
- [ ] Jeśli quiz - uzyskałem wymagany wynik
- [ ] Decyzja jest zgodna z polityką firmy

## 🔗 Powiązane Dokumenty

- [ADR 003: Requests System Architecture](.ai/decisions/003-requests-system-architecture.md)
- [Progress Report: Implementation](.ai/progress/2025-10-30-requests-system-implementation.md)
- [PRD - Product Requirements](.ai/prd.md)
- [Backend Rules](.claude/backend.md)
- [Frontend Rules](.claude/frontend.md)

---

**Ostatnia aktualizacja:** 2025-10-30
**Status:** ✅ Zaimplementowane i przetestowane

