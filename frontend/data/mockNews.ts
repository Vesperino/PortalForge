import type { News } from '~/types'

const addDays = (days: number) => {
  const date = new Date()
  date.setDate(date.getDate() + days)
  return date
}

export const mockNews: News[] = [
  {
    id: 1,
    title: 'Jubileusz 5 lat PortalForge! 🎉',
    excerpt: 'Świętujemy 5 lat działalności naszej firmy! Poznaj historię naszego sukcesu i plany na przyszłość.',
    content: `
# Jubileusz 5 lat PortalForge!

To niewiarygodne, ale minęło już 5 lat odkąd PortalForge rozpoczął swoją działalność! Z małego startupu w garażu, wyrośliśmy na renomowaną firmę technologiczną z 31 pracownikami i portfelem zadowolonych klientów.

## Nasza droga do sukcesu

Wszystko zaczęło się w 2020 roku, kiedy Anna Nowak i Piotr Kowalski postanowili stworzyć platformę, która zrewolucjonizuje sposób zarządzania organizacjami. Pierwsze miesiące były trudne - praca po nocach, ograniczony budżet, niewielki zespół. Ale wiara w produkt i determinacja przyniosły efekty.

## Kluczowe kamienie milowe

- **2020** - Założenie firmy, pierwszy klient
- **2021** - Rozbudowa zespołu do 10 osób
- **2022** - Pierwsza duża runda inwestycyjna
- **2023** - Ekspansja na rynek europejski
- **2024** - Wdrożenie AI w naszych produktach
- **2025** - 31 pracowników, 100+ klientów

## Podziękowania

Chcemy podziękować wszystkim pracownikom, którzy przyczynili się do naszego sukcesu. Wasza praca, zaangażowanie i pasja są fundamentem PortalForge.

## Co dalej?

Następne 5 lat zapowiada się jeszcze lepiej! Planujemy dalszą rozbudowę zespołu, rozwój nowych produktów i ekspansję międzynarodową.

**Z okazji jubileuszu organizujemy uroczystą kolację - szczegóły wkrótce!**
    `,
    imageUrl: 'https://images.unsplash.com/photo-1511578314322-379afb476865?w=800&h=600&fit=crop',
    authorId: 1,
    createdAt: addDays(-5),
    views: 245,
    category: 'announcement',
    eventId: 6
  },
  {
    id: 2,
    title: 'Witamy nowych członków zespołu!',
    excerpt: 'W tym miesiącu do PortalForge dołączyło 3 nowych pracowników. Poznajcie Sebastiana, Karolinę i Rafała!',
    content: `
# Witamy nowych członków zespołu!

Z wielką radością witamy trzech nowych pracowników, którzy w tym miesiącu dołączyli do rodziny PortalForge!

## Sebastian Kubiak - Content Manager

Sebastian ma 6 lat doświadczenia w content marketingu. Wcześniej pracował dla międzynarodowych marek e-commerce. W wolnym czasie pasjonuje się fotografią i podróżami.

**Dział:** Marketing
**Email:** sebastian.kubiak@portalforge.pl

## Karolina Baran - Content Manager

Karolina jest absolwentką dziennikarstwa i komunikacji społecznej. Specjalizuje się w storytellingu i strategiach contentowych. Prywatnie uwielbia gotować i testować nowe przepisy.

**Dział:** Marketing
**Email:** karolina.baran@portalforge.pl

## Rafał Jankowski - HR Specialist

Rafał ma doświadczenie w rekrutacji IT i employer brandingu. Dołącza do nas z dużej korporacji, gdzie zarządzał procesami onboardingu. Fan koszykówki i gier planszowych.

**Dział:** HR
**Email:** rafal.jankowski@portalforge.pl

## Onboarding

Wszyscy nowi pracownicy przechodzą 2-tygodniowy program onboardingowy, podczas którego poznają kulturę firmy, narzędzia i zespół. Każdy ma przypisanego mentora, który pomaga w adaptacji.

**Jeśli spotkasz naszych nowych kolegów - przywitaj się i pomóż im poczuć się jak w domu!** 💙
    `,
    imageUrl: 'https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=800&h=600&fit=crop',
    authorId: 26,
    createdAt: addDays(-3),
    views: 178,
    category: 'hr'
  },
  {
    id: 5,
    title: 'Team Building - Paintball już za 3 tygodnie! 🎯',
    excerpt: 'Przygotuj się na dzień pełen adrenaliny! Paintball, grillowanie i gry zespołowe czekają na całą firmę.',
    content: `
# Team Building - Paintball już za 3 tygodnie!

Nadchodzi jeden z najbardziej wyczekiwanych eventów roku - **firmowy wyjazd integracyjny**! Tym razem czeka nas paintball w lesie, grillowanie i mnóstwo zabawy.

## Szczegóły wydarzenia

📅 **Data:** Za 3 tygodnie
📍 **Miejsce:** PaintballPark, ul. Leśna 45
🚌 **Transport:** Autokar z biura o 9:00 (wyjazd punktualnie!)
⏰ **Powrót:** Około 18:00

## Program dnia

### 9:00 - Wyjazd z biura
Zbiórka przed budynkiem. Nie spóźniaj się - autokar odjeżdża punktualnie!

### 10:00 - Przywitanie i briefing
Instruktaż bezpieczeństwa, podział na drużyny, wydanie sprzętu.

### 10:30 - 14:00 - Rozgrywki paintballowe
Seria gier zespołowych:
- Capture the Flag
- Team Deathmatch
- VIP Escort
- Last Team Standing

### 14:00 - 16:00 - Grill i relaks
Kiełbaski, szaszłyki, sałatki i napoje. Czas na rozmowy i odpoczynek.

### 16:00 - 17:30 - Gry zespołowe
Przeciąganie liny, sztafety, konkursy sprawnościowe z nagrodami!

### 18:00 - Powrót do biura

## Dress code

👕 **Sportowy i wygodny!**
- Ubrania, które możesz pobrudzić (farba z paintballa zmywa się, ale lepiej nie ryzykować ulubionych ciuchów)
- Wygodne buty sportowe lub trekkingowe
- Kurtka/bluza (w lesie może być chłodno)
- Czapka z daszkiem (opcjonalnie)

## Co zabieramy?

✅ Dobry humor i energię!
✅ Krem z filtrem (jeśli będzie słonecznie)
✅ Wodę (będzie też na miejscu)
❌ Nie musisz zabierać jedzenia - wszystko zapewnione!

## Zapisy

Potwierdź swoją obecność do końca tygodnia w systemie HR lub napisz do Moniki Lewandowskiej (monika.lewandowska@portalforge.pl).

**Uwaga:** Liczba miejsc ograniczona do 35 osób - kto pierwszy, ten lepszy!

## Bezpieczeństwo

- Pełny sprzęt ochronny (maska, kamizelka)
- Instruktorzy na miejscu
- Apteczka pierwszej pomocy
- Ubezpieczenie NNW

Nie możemy się doczekać wspólnej zabawy! To będzie dzień pełen adrenaliny, śmiechu i budowania relacji w zespole. 🎯🔫

**Do zobaczenia na polu bitwy!** 💪
    `,
    imageUrl: 'https://images.unsplash.com/photo-1588731247989-c5d8e31d4d4c?w=800&h=600&fit=crop',
    authorId: 26,
    createdAt: addDays(-20),
    views: 289,
    category: 'event',
    eventId: 3
  },
  {
    id: 3,
    title: 'Nowa wersja aplikacji v2.0 już dostępna! 🚀',
    excerpt: 'Po 6 miesiącach intensywnej pracy, z dumą prezentujemy nową wersję naszej platformy z AI, nowym UI i wydajnością.',
    content: `
# PortalForge v2.0 - największy update w historii!

To był intensywny rok! Cały zespół produktowy i techniczny pracował nad największym updatem w historii naszej platformy. I już jest - **PortalForge v2.0**!

## Co nowego?

### 🤖 Asystent AI
Zintegrowany asystent AI pomaga w codziennych zadaniach - od automatycznego tagowania dokumentów po sugestie dotyczące struktury organizacji.

### 🎨 Nowy design system
Całkowicie przeprojektowaliśmy interfejs. Nowocześniejszy, bardziej intuicyjny, dostosowany do WCAG 2.1 AA.

### ⚡ Wydajność
- 3x szybsze ładowanie stron
- 50% mniej zapytań do serwera
- Offline mode dla podstawowych funkcji

### 📱 Mobilna rewolucja
Aplikacja mobilna (iOS i Android) dostępna w sklepach. Pełna synchronizacja z wersją webową.

### 🔒 Bezpieczeństwo
- End-to-end encryption dla wrażliwych danych
- 2FA obowiązkowe dla adminów
- Audyt bezpieczeństwa przeprowadzony przez zewnętrzną firmę

## Podziękowania

Szczególne podziękowania dla:
- **Zespołu Backend** za rewizję architektury
- **Zespołu Frontend** za pixel-perfect implementację
- **DevOps** za płynne wdrożenie
- **Product Team** za badania UX

## Feedback

Czekamy na wasze opinie! Jeśli znajdziecie bugi lub macie sugestie - piszcie na product@portalforge.pl
    `,
    imageUrl: 'https://images.unsplash.com/photo-1551434678-e076c223a692?w=800&h=600&fit=crop',
    authorId: 17,
    createdAt: addDays(-7),
    views: 312,
    category: 'product'
  },
  {
    id: 6,
    title: 'Warsztat TypeScript Advanced - zapisz się już dziś! 📚',
    excerpt: 'Piotr Kowalski (CTO) poprowadzi zaawansowany warsztat TypeScript. Generics, conditional types, mapped types i więcej!',
    content: `
# Warsztat TypeScript Advanced z CTO

Masz już doświadczenie z TypeScript i chcesz poznać zaawansowane techniki? **Piotr Kowalski**, nasz CTO, poprowadzi warsztat dla developerów, którzy chcą podnieść swoje umiejętności na wyższy poziom!

## Dla kogo?

Warsztat jest przeznaczony dla developerów z **co najmniej 1 rokiem doświadczenia z TypeScript**. Jeśli znasz podstawy (typy, interfejsy, klasy), ale chcesz zgłębić bardziej zaawansowane koncepty - to warsztat dla Ciebie!

## Program warsztatu

### 1. Generics - głębsze zrozumienie (45 min)
- Generic constraints
- Multiple type parameters
- Generic utility types
- Praktyczne przykłady z naszych projektów

### 2. Conditional Types (60 min)
- Podstawy conditional types
- Infer keyword
- Distributive conditional types
- Tworzenie własnych utility types

### 3. Mapped Types (45 min)
- Transformacje typów
- Key remapping
- Template literal types
- Praktyczne zastosowania

### 4. Advanced Patterns (60 min)
- Type guards i type predicates
- Discriminated unions
- Builder pattern w TypeScript
- Dependency injection

### 5. Q&A i live coding (30 min)
- Pytania uczestników
- Rozwiązywanie problemów z prawdziwych projektów
- Best practices z doświadczenia Piotra

## Szczegóły praktyczne

📅 **Data:** Za 2 tygodnie
⏰ **Godzina:** 10:00 - 14:00 (z przerwą na lunch)
📍 **Miejsce:** Sala szkoleniowa B
🍕 **Lunch:** Pizza i napoje zapewnione!
💻 **Co zabrać:** Laptop z zainstalowanym Node.js i VS Code

## Materiały

Wszyscy uczestnicy otrzymają:
- Slajdy z prezentacji
- Przykłady kodu z warsztatu
- Listę polecanych zasobów do dalszej nauki
- Certyfikat ukończenia

## Zapisy

Liczba miejsc ograniczona do **15 osób**. Zapisy przez system HR lub email do Piotra (piotr.kowalski@portalforge.pl).

**Deadline na zapisy:** 3 dni przed warsztatem

## Wymagania wstępne

Przed warsztatem upewnij się, że:
- ✅ Masz co najmniej rok doświadczenia z TypeScript
- ✅ Znasz podstawowe typy i interfejsy
- ✅ Rozumiesz koncepcję typowania statycznego
- ✅ Masz zainstalowane środowisko deweloperskie

## Dlaczego warto?

> "TypeScript to nie tylko dodanie typów do JavaScript. To narzędzie, które zmienia sposób myślenia o kodzie i pozwala budować bardziej niezawodne aplikacje." - Piotr Kowalski

Po warsztacie będziesz w stanie:
- 🎯 Pisać bardziej type-safe kod
- 🔧 Tworzyć własne utility types
- 🚀 Wykorzystywać zaawansowane wzorce TypeScript
- 💡 Lepiej rozumieć błędy kompilatora
- 📚 Czytać i rozumieć skomplikowane typy z bibliotek

**Nie przegap tej okazji do nauki od najlepszych!** 🚀
    `,
    imageUrl: 'https://images.unsplash.com/photo-1516116216624-53e697fedbea?w=800&h=600&fit=crop',
    authorId: 2,
    createdAt: addDays(-25),
    views: 156,
    category: 'tech',
    eventId: 2
  },
  {
    id: 4,
    title: 'Q4 2024 - Podsumowanie wyników',
    excerpt: 'Najlepszy kwartał w historii firmy! Przychody wzrosły o 45%, a liczba klientów o 30%.',
    content: `
# Q4 2024 - rekordowy kwartał!

Q4 2024 był najlepszym kwartałem w historii PortalForge. Oto najważniejsze liczby:

## Wyniki finansowe

📈 **Przychody:** +45% r/r
👥 **Nowi klienci:** 28 (wzrost o 30%)
💰 **ARR:** $2.4M (+52% r/r)
📊 **Churn rate:** 3.2% (najniższy w historii!)

## Produktowe sukcesy

- Wydanie wersji 2.0
- 3 nowe główne funkcje
- 156 bugfixów i ulepszeń
- 99.97% uptime

## Zespół

W Q4 zatrudniliśmy 5 nowych osób:
- 2 Backend Developers
- 1 Frontend Developer
- 1 Product Manager
- 1 Marketing Manager

## Kultura firmy

- Employee satisfaction: 8.7/10
- eNPS: +62 (excellent!)
- 0% turnover

## Q1 2025 - co przed nami?

- Ekspansja na rynek niemiecki
- Nowy tier Enterprise
- Integracja z kolejnymi systemami
- Zespół do 35 osób

**Gratulacje dla całego zespołu! To wasza ciężka praca sprawiła, że te wyniki były możliwe.** 🎉

— Anna Nowak, CEO
    `,
    imageUrl: 'https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=800&h=600&fit=crop',
    authorId: 1,
    createdAt: addDays(-10),
    views: 289,
    category: 'announcement'
  },
  {
    id: 5,
    title: 'Team Building - Relacja z paintballa 🎯',
    excerpt: 'Wspaniały dzień pełen adrenaliny! Zobacz zdjęcia i najlepsze momenty z naszego wyjazdu integracyjnego.',
    content: `
# Paintball Team Building - relacja

Wczoraj spędziliśmy fantastyczny dzień na paintballu! Pomimo zmiennej pogody, wszyscy świetnie się bawili.

## Przebieg dnia

**9:00** - Wyjazd z biura (2 busy)
**10:00** - Przybycie, briefing bezpieczeństwa
**10:30-13:00** - Rozgrywki paintballowe
**13:00-15:00** - Grill i integracja
**15:30** - Powrót do Warszawy

## Statystyki

- 31 uczestników (100% frekwencja!)
- 5 drużyn
- 12 gier rozegranych
- 0 kontuzji 😅
- ~2000 kulek użytych

## Zwycięzcy

🥇 **1 miejsce:** Zespół "Backend Bandits"
🥈 **2 miejsce:** Zespół "Frontend Fighters"
🥉 **3 miejsce:** Zespół "Marketing Mavericks"

## MVP dnia

**Kamil Kowalczyk** - 47 trafień, niesamowita celność!

## Najlepsze momenty

- Epicki comeback zespołu HR w finale
- Piotr (CTO) ukrywający się w krzakach przez 15 minut
- Anna (CEO) prowadząca szarżę na flagę
- Afterparty przy grillu z gitarą

## Opinie uczestników

> "Najlepszy team building ever! Następnym razem może lasertag?" - Magdalena, Frontend Developer

> "Super integracja, świetna atmosfera, polecam więcej takich wyjazdów!" - Tomasz, Team Lead

> "Mięśnie bolą, ale było warto 😂" - Rafał, HR Specialist

## Podziękowania

Dziękujemy Beacie i zespołowi HR za organizację! Czekamy na kolejne wyjazdy integracyjne.

📸 **Zdjęcia dostępne na dysku firmowym:** /Team/Events/2025/Paintball
    `,
    imageUrl: 'https://images.unsplash.com/photo-1628277613967-6abca504d0ac?w=800&h=600&fit=crop',
    authorId: 29,
    createdAt: addDays(-2),
    views: 198,
    category: 'event',
    eventId: 3
  },
  {
    id: 6,
    title: 'Jak pracujemy z mikroserwisami w PortalForge',
    excerpt: 'Tech deep dive - architektura mikroserwisowa, komunikacja między serwisami, wyzwania i rozwiązania.',
    content: `
# Mikroservisy w PortalForge - case study

Przez ostatnie 2 lata stopniowo migrowaliśmy z monolitu do architektury mikroserwisowej. Dzisiaj chciałbym podzielić się naszymi doświadczeniami.

## Dlaczego mikroservisy?

Nasz monolit miał ~200k linii kodu. Deployment trwał 45 minut. Każda zmiana wymagała regresu całej aplikacji. Skalowanie było kosztowne i trudne.

## Obecna architektura

Obecnie mamy 12 mikroserwisów:

1. **Auth Service** - uwierzytelnianie i autoryzacja
2. **User Service** - zarządzanie użytkownikami
3. **Organization Service** - struktura organizacyjna
4. **Event Service** - kalendarz i wydarzenia
5. **News Service** - system newsów
6. **Document Service** - zarządzanie dokumentami
7. **Notification Service** - powiadomienia
8. **Analytics Service** - zbieranie metryk
9. **Search Service** - wyszukiwarka (Elasticsearch)
10. **Export Service** - generowanie raportów
11. **AI Service** - funkcje AI/ML
12. **Gateway** - API Gateway (Kong)

## Stack technologiczny

- **Backend:** .NET 8, C#, Clean Architecture
- **Communication:** RabbitMQ (async), gRPC (sync)
- **Databases:** PostgreSQL, MongoDB, Redis
- **Container orchestration:** Kubernetes
- **Monitoring:** Prometheus + Grafana
- **Logging:** ELK Stack

## Wzorce i best practices

### Event-driven architecture
Używamy event sourcing dla krytycznych operacji. Każda zmiana stanu generuje event, który trafia do kolejki.

### API Gateway pattern
Kong jako single entry point. Routing, rate limiting, authentication.

### Circuit Breaker
Hystrix zapobiega kaskadowym awariom.

### Service Mesh
Istio dla service-to-service communication.

## Wyzwania

❌ **Distributed tracing** - początkowo ciężko było debugować
❌ **Data consistency** - eventual consistency wymaga zmiany myślenia
❌ **DevOps complexity** - więcej serwisów = więcej pracy operacyjnej

## Rozwiązania

✅ **Jaeger** dla distributed tracing
✅ **Saga pattern** dla distributed transactions
✅ **Extensive automation** - CI/CD dla wszystkich serwisów

## Wyniki

- ⚡ Deployment z 45 min → 5 min
- 📈 Uptime: 99.5% → 99.97%
- 💰 Koszty infrastruktury: -30%
- 🚀 Velocity zespołu: +40%

## Czy warto?

**Tak, ale...** mikroserwisy to nie silver bullet. Sprawdzą się dla średnich i dużych systemów. Dla małych projektów monolit jest lepszy.

## Q&A

Masz pytania o naszą architekturę? Napisz na piotr.kowalski@portalforge.pl

— Piotr Kowalski, CTO
    `,
    imageUrl: 'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800&h=600&fit=crop',
    authorId: 2,
    createdAt: addDays(-14),
    views: 267,
    category: 'tech'
  },
  {
    id: 7,
    title: 'Remote work policy - zmiany od marca',
    excerpt: 'Nowa polityka pracy zdalnej - większa elastyczność, hybrid model 3+2, jasne zasady.',
    content: `
# Nowa polityka pracy zdalnej

Po konsultacjach z zespołem i managerami, wprowadzamy zaktualizowaną politykę remote work, która wchodzi w życie od 1 marca 2025.

## Model hybrydowy 3+2

**3 dni w biurze** (Pon, Wto, Śro)
**2 dni remote** (Czw, Pt)

### Dlaczego 3+2?

- Utrzymanie kultury firmy i więzi zespołowych
- Efektywna komunikacja face-to-face
- Elastyczność dla work-life balance
- Zgodność z preferencjami zespołu (według ankiety)

## Wyjątki

### Pełny remote
Możliwy dla:
- Pracowników z udokumentowanych powodów zdrowotnych
- Rodziców z małymi dziećmi (do 3 lat)
- Mieszkających >100km od biura

Wymaga zgody managera i HR.

### Elastyczne godziny
- Core hours: 10:00-15:00 (obecność obowiązkowa)
- Poza tym: elastyczny start/koniec

## Zasady pracy zdalnej

✅ **Must have:**
- Stabilne łącze (min. 50 Mb/s)
- Kamera podczas meetingów
- Odpowiedź w Teams do 30 min
- Dostępność telefoniczna

❌ **Don't:**
- Praca z kawiarni (RODO!)
- Wyjazdy bez zgody managera
- "Ciche godziny" podczas core hours

## Wsparcie firmy

Dla pracowników remote dofinansowujemy:
- 💺 Krzesło biurowe (do 2000 zł)
- 💻 Monitor (do 1500 zł)
- 🌐 Internet (100 zł/mc)
- ☕ Co-working (w przypadku braku odpowiednich warunków w domu)

## Feedback

Policy będzie podlegać przeglądowi co 6 miesięcy. Wasze opinie są ważne - piszcie do hr@portalforge.pl

— Beata Szczepańska, HR Manager
    `,
    imageUrl: 'https://images.unsplash.com/photo-1521791136064-7986c2920216?w=800&h=600&fit=crop',
    authorId: 26,
    createdAt: addDays(-6),
    views: 421,
    category: 'hr'
  },
  {
    id: 8,
    title: 'Accessibility first - WCAG 2.1 AA certification',
    excerpt: 'Nasza aplikacja otrzymała certyfikat WCAG 2.1 AA! Zobacz jak dbamy o dostępność dla wszystkich użytkowników.',
    content: `
# PortalForge z certyfikatem WCAG 2.1 AA! ♿

Z dumą ogłaszamy, że nasza aplikacja otrzymała oficjalny certyfikat zgodności z **WCAG 2.1 poziom AA**!

## Co to oznacza?

Web Content Accessibility Guidelines (WCAG) to międzynarodowy standard dostępności cyfrowej. Poziom AA oznacza, że nasza aplikacja jest dostępna dla osób z różnymi niepełnosprawnościami.

## Co zrobiliśmy?

### ⌨️ Keyboard navigation
Pełna obsługa klawiaturą. Każda funkcja dostępna bez myszy.

### 🔊 Screen reader support
Testowaliśmy z NVDA, JAWS i VoiceOver. Wszystkie elementy poprawnie oznaczone.

### 🎨 Kontrast i kolory
Wszystkie kombinacje kolorów spełniają wymagania kontrastu 4.5:1 (tekst) i 3:1 (UI).

### 📝 Alternatywny tekst
Każdy obraz, ikona i element graficzny ma opisowy alt text.

### 🔍 Focus indicators
Wyraźne zaznaczenie focusu dla każdego interaktywnego elementu.

### 📏 Responsive text
Tekst skaluje się do 200% bez utraty funkcjonalności.

## Proces certyfikacji

1. **Audyt wewnętrzny** (Dominika Sikorska, UX Designer)
2. **Automatyczne testy** (axe, Lighthouse, WAVE)
3. **Testy z użytkownikami** z niepełnosprawnościami
4. **Audyt zewnętrzny** przez certyfikowaną firmę
5. **Certyfikat!** 🎉

## Dlaczego to ważne?

- 15% ludzi ma jakąś formę niepełnosprawności
- To prawo, nie przywilej
- Lepsza UX dla wszystkich
- SEO benefits
- Zgodność z regulacjami (ADA, EAA)

## Co dalej?

Accessibility to ciągły proces. Planujemy:
- Regularne audyty (co 6 miesięcy)
- Szkolenia dla zespołu
- Accessibility champions w każdym zespole
- WCAG 2.2 AAA jako długoterminowy cel

## Podziękowania

Ogromne dzięki dla:
- **Dominiki Sikorskiej** - prowadzenie audytu
- **Frontend Team** - implementacja zmian
- **Product Team** - advocacy dla a11y

**Dostępność to nie feature, to fundament!**

— Natalia Krawczyk, VP Product
    `,
    imageUrl: 'https://images.unsplash.com/photo-1573164713619-24c711fe7878?w=800&h=600&fit=crop',
    authorId: 17,
    createdAt: addDays(-12),
    views: 156,
    category: 'product'
  },
  {
    id: 9,
    title: 'Benefity 2025 - co się zmienia?',
    excerpt: 'Zaktualizowany pakiet benefitów - Multisport, prywatna opieka medyczna, dofinansowanie do nauki i więcej!',
    content: `
# Benefity 2025 - jeszcze lepiej!

Słuchaliśmy waszych głosów w ankiecie i z przyjemnością przedstawiamy zaktualizowany pakiet benefitów na 2025 rok!

## Co dostajecie? 🎁

### 💪 Multisport / FitProfit
Karta na siłownię, basen, zajęcia grupowe - ponad 3000 obiektów w Polsce.

**Koszt:** 0 zł (100% z firmy)

### 🏥 Prywatna opieka medyczna
Pakiet Premium w Luxmed lub Medicover.

**Koszt:** 50 zł/mc (firma dopłaca resztę)

### 🎓 Budżet rozwojowy
Na szkolenia, konferencje, certyfikacje, książki.

**Kwota:** 5000 zł/rok na osobę

### 🍕 Lunch w biurze
Catering 2x w tygodniu (Wtorek, Czwartek).

**Koszt:** 0 zł

### 🚗 Parking
Miejsce parkingowe w garażu.

**Koszt:** 0 zł (w miarę dostępności)

### 🏖️ Dodatkowe dni wolne
- 26 dni urlopu (standardowo)
- +1 dzień za każde 2 lata stażu
- Twoje urodziny wolne!
- 24 i 31 grudnia wolne

### 💻 Sprzęt do pracy zdalnej
Dofinansowanie do home office (szczegóły w remote policy).

### 📚 Książki i kursy online
Nielimitowany dostęp do:
- O'Reilly Learning
- Udemy Business
- Pluralsight

### 🎉 Eventy firmowe
- 2 team buildingi rocznie
- Coroczny wyjazd integracyjny
- Święta firmowe

## Nowości w 2025

### 🧘 Mental health support
Dostęp do psychologa online - 4 sesje/rok za darmo.

### 👶 Dodatkowy urlop macierzyński/ojcowski
+2 tygodnie płatnego urlopu (poza ustawowym).

### 🚲 Bike to work
200 zł/mc na rower firmowy (leasing) lub komunikację miejską.

### 💰 Program poleceń
5000-10000 zł za polecenie kandydata, który zostanie zatrudniony.

## Kafeteria (do wyboru)

Masz 2000 zł/rok na wybór dodatkowych benefitów:
- Bon Sodexo
- Karty podarunkowe
- Dofinansowanie do wakacji
- Ubezpieczenie na życie

## Jak z tego korzystać?

1. Sprawdź dostępne benefity w systemie HR
2. Wybierz to, co Cię interesuje
3. Wypełnij deklarację (deadline: 31 stycznia)
4. Ciesz się!

## FAQ

**Q: Czy mogę zrezygnować z jakiegoś benefitu?**
A: Multisport i medycyna są obowiązkowe. Reszta opcjonalnie.

**Q: Co z ekwiwalentem za benefity?**
A: Nie ma opcji ekwiwalentu pieniężnego.

**Q: Czy mogę dzielić się kartą Multisport?**
A: Nie, karta jest imiennie przypisana.

Pytania? Piszcie do benefits@portalforge.pl

— Beata Szczepańska, HR Manager
    `,
    imageUrl: 'https://images.unsplash.com/photo-1556761175-4b46a572b786?w=800&h=600&fit=crop',
    authorId: 26,
    createdAt: addDays(-8),
    views: 389,
    category: 'hr'
  },
  {
    id: 10,
    title: 'Security incident - post mortem',
    excerpt: 'Transparentna relacja z niedawnego incydentu bezpieczeństwa, podjęte kroki i wnioski na przyszłość.',
    content: `
# Security Incident Post-Mortem

W duchu transparentności, chcemy poinformować o incydencie bezpieczeństwa, który miał miejsce 15 stycznia 2025. **Dane klientów są bezpieczne** - nie doszło do ich wycieku.

## Co się stało?

**15.01.2025, 14:23 CET** - Wykryliśmy nieautoryzowaną próbę dostępu do systemu monitoringu (Grafana).

## Timeline

**14:23** - Alerty o anomalnej aktywności
**14:25** - Zespół DevOps rozpoczyna investigation
**14:30** - Potwierdzenie incydentu
**14:32** - Zablokowanie podejrzanych IP
**14:45** - Forced logout wszystkich sesji
**15:00** - Full system audit
**15:30** - Komunikat do zespołu
**17:00** - Incydent opanowany

## Root cause

Skompromitowane credentials do Grafany (shared account używany przez external contractor). **Lesson learned:** nigdy więcej shared accounts.

## Co zrobiliśmy?

### Natychmiast
✅ Zablokowanie dostępu
✅ Zmiana wszystkich credentials
✅ Audit logów
✅ Komunikacja z zespołem

### W ciągu 24h
✅ Wdrożenie 2FA dla wszystkich serwisów
✅ Usunięcie wszystkich shared accounts
✅ Przegląd uprawnień wszystkich użytkowników
✅ Update security policies

### W ciągu tygodnia
✅ Security training dla całego zespołu
✅ Penetration testing przez zewnętrzną firmę
✅ Wdrożenie SIEM (Splunk)
✅ Incident response playbook

## Wnioski

1. **Zero Trust** - nigdy nie ufaj, zawsze weryfikuj
2. **Least Privilege** - minimalne wymagane uprawnienia
3. **No Shared Accounts** - każdy ma swoje credentials
4. **2FA Everywhere** - obowiązkowe dla wszystkich systemów
5. **Continuous Monitoring** - lepsza detekcja anomalii

## Zapewnienia

- ❌ Brak wycieku danych klientów
- ❌ Brak wycieku danych osobowych
- ❌ Brak wpływu na dostępność systemu
- ✅ 100% transparentność
- ✅ Ciągłe doskonalenie security

## Co dalej?

- Monthly security reviews
- Quarterly penetration tests
- Annual security audit przez Big4
- Bug bounty program (coming soon!)

Security to priorytet nr 1. Jeśli zauważysz coś podejrzanego - zgłoś na security@portalforge.pl

— Krzysztof Grabowski, DevOps Engineer
    `,
    imageUrl: 'https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=800&h=600&fit=crop',
    authorId: 15,
    createdAt: addDays(-4),
    views: 203,
    category: 'tech'
  },
  {
    id: 11,
    title: 'Employee of the Quarter - Q4 2024',
    excerpt: 'Gratulacje dla Kamila Kowalczyka - Employee of the Quarter Q4 2024! Poznajcie jego historię sukcesu.',
    content: `
# Employee of the Quarter Q4 2024 🏆

Z wielką radością ogłaszamy, że **Employee of the Quarter Q4 2024** został **Kamil Kowalczyk**, Senior Backend Developer!

## Dlaczego Kamil?

### 🚀 Techniczne osiągnięcia
- Przeprowadził migration krytycznego serwisu z monolitu do mikroservisu
- Zredukował response time o 60%
- Wprowadził caching strategy, która zmniejszyła koszty DB o 40%
- Code review champion - średnio 15 PR/tydzień

### 👥 Praca z zespołem
- Mentoring dla 2 junior developerów
- Prowadził 3 lunch & learn sessions
- Zawsze chętny do pomocy
- Pozytywna energia zarażająca cały zespół

### 💡 Inicjatywy
- Zaproponował i wdrożył nowy monitoring stack
- Stworzył internal wiki z best practices
- Przeprowadził refactoring legacy code

## Co mówią o Kamilu?

> "Kamil to developer, jakiego każdy team lead chciałby mieć. Nie tylko super kod pisze, ale jeszcze pomaga innym rosnąć." - Tomasz Wójcik, Team Lead

> "Jego code reviews to małe lekcje programowania. Uczę się od niego każdego dnia." - Adam Szymański, Junior Developer

> "Technical excellence + team player = Kamil. Zasłużone wyróżnienie!" - Marek Wiśniewski, VP Engineering

## Nagroda

- 🏆 Tytuł Employee of the Quarter
- 💰 Bonus 5000 zł brutto
- 🎟️ Voucher na weekend dla 2 osób
- 🌟 Highlight na stronie firmowej
- 📜 Certyfikat

## Fun facts o Kamilu

- ☕ Wypija średnio 6 kaw dziennie
- 🎮 Gra w CS:GO na poziomie semi-pro
- 🏃 Ukończył 3 maratony
- 🍕 Jego ulubiony lunch to pizza pepperoni

**Kamil - dziękujemy za Twój wkład i zaangażowanie! Jesteś inspiracją dla całego zespołu!** 🎉

Nominacje na Q1 2025 już otwarte - zgłaszajcie kolegów i koleżanki!

— Anna Nowak, CEO
    `,
    imageUrl: 'https://images.unsplash.com/photo-1552664730-d307ca884978?w=800&h=600&fit=crop',
    authorId: 1,
    createdAt: addDays(-15),
    views: 234,
    category: 'hr'
  },
  {
    id: 12,
    title: 'Nowy klient: TechCorp Industries',
    excerpt: 'Podpisaliśmy kontrakt z TechCorp Industries - jedna z największych firm przemysłowych w Europie!',
    content: `
# Nowy klient: TechCorp Industries 🎉

Ogromny sukces commercial! Właśnie podpisaliśmy kontrakt z **TechCorp Industries** - jedną z największych firm przemysłowych w Europie Centralnej!

## O kliencie

**TechCorp Industries**
- 🏭 5000+ pracowników
- 🌍 12 krajów
- 💰 €800M+ rocznego obrotu
- 🏗️ 23 zakłady produkcyjne

## Scope projektu

Wdrożenie PortalForge jako centralnej platformy komunikacji wewnętrznej dla wszystkich 5000 pracowników.

### Etap 1 (Q1 2025)
- Setup i konfiguracja
- Migration danych z legacy systemu
- Training dla administratorów

### Etap 2 (Q2 2025)
- Rollout dla management (500 osób)
- Customizacja pod procesy klienta
- Integration z SAP

### Etap 3 (Q3 2025)
- Rollout dla wszystkich pracowników
- Go-live w 12 krajach
- Support i optymalizacja

## Wartość kontraktu

💰 **€1.2M** na 3 lata (€400k ARR)

To nasz największy kontrakt w historii firmy!

## Co to oznacza dla zespołu?

### Więcej pracy
Będzie intensywnie! Szczególnie Q1-Q2 to będzie hot period.

### Nowe wyzwania
- Multi-language support (12 języków!)
- Integracja z enterprise systems
- Wymagania compliance (ISO, GDPR)

### Rozwój
To ogromne doświadczenie w pracy z enterprise client.

### Bonusy
Sukces tego projektu = bonusy dla całego zespołu!

## Zespół projektowy

**Project Lead:** Natalia Krawczyk
**Tech Lead:** Marek Wiśniewski
**Backend:** Kamil, Łukasz, Michał, Jakub
**Frontend:** Magdalena, Agnieszka, Joanna, Ewa
**DevOps:** Krzysztof, Bartosz
**Support:** Rafał, Paulina

## Timeline jest tight!

⏰ **Kickoff:** 1 lutego
⏰ **Go-live Etap 1:** 31 marca
⏰ **Full rollout:** 1 września

## Call to action

To projekt, który może zmienić historię naszej firmy. Dajmy z siebie wszystko! 💪

Pytania? Piszcie do natalia.krawczyk@portalforge.pl

**Let's make it happen!** 🚀

— Anna Nowak, CEO
    `,
    imageUrl: 'https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=800&h=600&fit=crop',
    authorId: 1,
    createdAt: addDays(-1),
    views: 412,
    category: 'announcement'
  },
  {
    id: 13,
    title: 'Engineering blog - first post!',
    excerpt: 'Startujemy z blogiem technicznym! Dziel się wiedzą, rozwijaj personal branding, buduj tech community.',
    content: `
# Startujemy z Engineering Blog! ✍️

Mamy świetną wiadomość dla wszystkich tech enthusiasts - startujemy z oficjalnym **PortalForge Engineering Blog**!

## Po co blog?

### 📚 Knowledge sharing
Dzielenie się wiedzą z community. Co ciekawego robimy? Jakie problemy rozwiązujemy?

### 🌟 Employer branding
Pokazanie, że jesteśmy tech-forward company. Pomoc w rekrutacji.

### 💼 Personal branding
Rozwijaj swoją markę osobistą. Występuj na konferencjach. Buduj network.

### 🎓 Learning
Najlepszy sposób na naukę to teaching others.

## Gdzie?

👉 **blog.portalforge.pl**

Już wkrótce pierwsze posty!

## Co będziemy pisać?

- 🏗️ **Architecture** - jak budujemy systemy
- ⚡ **Performance** - optymalizacje i benchmarki
- 🔒 **Security** - best practices i case studies
- 🧪 **Testing** - strategie i narzędzia
- 🚀 **DevOps** - CI/CD, infrastructure as code
- 📱 **Mobile** - iOS i Android development
- 🎨 **Frontend** - React, Vue, nowoczesne UI
- 🤖 **AI/ML** - jak wykorzystujemy AI
- 📊 **Data** - analytics i data engineering

## Chcesz pisać?

**Tak! Potrzebujemy Ciebie!**

### Kto może pisać?
Każdy technical person w firmie!

### Ile czasu to zajmie?
- Prosty post: 2-4h
- Medium post: 4-8h
- Deep dive: 8-16h

### Wsparcie
- 📝 Editor pomoże z polishem
- 🎨 Designer zrobi grafiki
- 🚀 Marketing zadba o promotion

### Korzyści
- 💰 500 zł za opublikowany post
- 🏆 Recognition w firmie
- 📈 Growth osobisty
- 🎤 Możliwość wystąpienia na konferencji

## Proces

1. **Pitch** - wyślij propozycję tematu
2. **Approval** - dostaniesz feedback
3. **Write** - pisz w swoim tempie
4. **Review** - technical i editorial review
5. **Publish** - post idzie na blog!
6. **Promote** - share na social media

## Pomysły na pierwsze posty

- "Jak zrefaktorowaliśmy legacy code bez downtime"
- "From monolith to microservices - lessons learned"
- "Building real-time collaboration features"
- "Performance optimization - 10x speed improvement"
- "Our testing strategy"

## Zgłoszenia

Chcesz napisać? Wyślij propozycję na:
📧 **engineering-blog@portalforge.pl**

Format:
- Tytuł
- Abstrakt (2-3 zdania)
- Outline (główne punkty)
- Szacowany czas dostarczenia

## Let's build tech community together! 🚀

— Piotr Kowalski, CTO
    `,
    imageUrl: 'https://images.unsplash.com/photo-1499750310107-5fef28a66643?w=800&h=600&fit=crop',
    authorId: 2,
    createdAt: addDays(-9),
    views: 167,
    category: 'tech'
  },
  {
    id: 14,
    title: 'Diversity & Inclusion - nasze zobowiązanie',
    excerpt: 'Różnorodność i inkluzywność to nie tylko buzzwords. Zobacz nasze konkretne działania i cele na 2025.',
    content: `
# Diversity & Inclusion - more than buzzwords

W PortalForge wierzymy, że różnorodność to siła. Dzisiaj chcemy podzielić się naszymi zobowiązaniami i konkretnymi działaniami.

## Obecny stan (2024)

### Gender diversity
- 45% kobiet w całej firmie
- 35% kobiet w tech (powyżej industry average!)
- 40% kobiet w leadership

### Age diversity
- 23% Gen Z (18-27)
- 52% Millennials (28-43)
- 25% Gen X (44+)

### Background
- 10% pracowników z innych krajów
- 15% first-generation college graduates

## Nasze wartości

### 🌈 Równość
Każdy zasługuje na równe traktowanie i szanse.

### 🎯 Inkluzywność
Każdy głos się liczy. Różnorodność perspektyw = lepsze decyzje.

### 🚀 Opportunity
Equal opportunities dla wszystkich, niezależnie od background.

## Konkretne działania

### Recruitment
✅ Blind CV screening (usuwamy dane demograficzne)
✅ Diverse interview panels
✅ Partnerships z organizacjami diversity
✅ Internship programs dla underrepresented groups

### Workplace
✅ Flexible working hours (family-friendly)
✅ Parental leave policy (equal dla wszystkich)
✅ Lactation room
✅ Gender-neutral bathrooms
✅ Mental health support

### Development
✅ Mentorship programs
✅ Leadership development dla all genders
✅ Conference attendance (democratic selection)
✅ Equal pay dla equal work (annual audits)

### Culture
✅ Zero tolerance dla discrimination
✅ Anonymous reporting channel
✅ Regular D&I training
✅ ERG (Employee Resource Groups)

## Cele na 2025

🎯 50% kobiet w leadership
🎯 40% kobiet w tech
🎯 20% team z innych krajów
🎯 100% equality w comp (already there!)
🎯 Launch internship program

## Wyzwania

Nie udajemy, że jest perfect:
- Tech industry jest male-dominated
- Unconscious bias exists
- Some roles trudniej zdywersyfikować

Ale pracujemy nad tym każdego dnia.

## Your voice matters

Masz sugestie? Zauważyłeś problem?

📧 **diversity@portalforge.pl**
🔒 Anonymous reporting: **/anonymous-feedback**

## Together

D&I to nie HR initiative - to responsibility wszystkich.

Bądźmy allies dla siebie nawzajem. 💜

— Beata Szczepańska, HR Manager
    `,
    imageUrl: 'https://images.unsplash.com/photo-1529156069898-49953e39b3ac?w=800&h=600&fit=crop',
    authorId: 26,
    createdAt: addDays(-11),
    views: 298,
    category: 'hr'
  },
  {
    id: 15,
    title: 'Rok w liczbach - infografika 2024',
    excerpt: '2024 w statystykach - kod, commity, PR-y, spotkania, kawa i wszystko co się wydarzyło!',
    content: `
# 2024 w liczbach 📊

Rok 2024 za nami! Zobaczcie co osiągnęliśmy w liczbach:

## Development

### Kod
- 📝 **1.2M** linii kodu napisanych
- 🔄 **8,547** pull requestów
- ✅ **7,821** code reviews
- 🐛 **2,341** bugów naprawionych
- 🚀 **156** features dostarczonych

### Commits
- 💻 **Top committer:** Kamil (2,453 commits)
- 🏆 **Best streak:** Łukasz (87 dni!)
- 📅 **Busiest day:** Czwartek
- ⏰ **Peak hour:** 10-11 AM
- ☕ **Most commits after coffee:** Piotr

### Tests
- 🧪 **12,567** unit tests
- 🔍 **1,234** integration tests
- 🎯 **Code coverage:** 87%
- ✅ **Green builds:** 94.5%

## Product

### Releases
- 🎉 **52** releases (weekly!)
- 🚀 **4** major versions
- 📦 **156** features
- 🐛 **312** bug fixes
- ⚡ **89** performance improvements

### Users
- 👥 **+127** new clients
- 📈 **+340%** user growth
- ⭐ **4.8/5** average rating
- 💬 **98%** positive feedback

### Performance
- ⚡ **-45%** average response time
- 🚀 **99.97%** uptime
- 📊 **-30%** infrastructure costs
- 💾 **-60%** database load

## Team

### Growth
- 👥 **31** people (było 24)
- 🎓 **7** new hires
- 🎯 **0%** turnover
- 😊 **8.7/10** employee satisfaction

### Meetings
- 📅 **1,247** meetings
- ⏱️ **avg 35min** per meeting
- 🎥 **78%** online, **22%** in-person
- ☕ **Most meetings:** Tuesdays

### Communication
- 💬 **87,543** Slack messages
- 📧 **12,432** emails
- 🎥 **2,341** video calls
- ☕ **Najaktywniejszy kanał:** #random

## Culture

### Events
- 🎉 **2** team buildings
- 🎓 **24** lunch & learns
- 🏆 **4** hackathony
- 🍕 **52** pizza Fridays
- 🎂 **31** urodziny świętowane

### Learning
- 📚 **127** książek przeczytanych
- 🎓 **43** kursy ukończone
- 🎤 **8** konferencji
- 💰 **€87k** na development

### Fun facts
- ☕ **12,456** kaw wypitych
- 🍕 **234** pizze zamówione
- 🎮 **156h** grania w PS5 w biurze
- 🚴 **2,341km** rowerem do biura
- 🏃 **3** maratony ukończone

## Failures & Learnings

Bo nie zawsze było idealnie:
- ❌ **3** production incidents
- 🐛 **1** critical bug w production
- ⏰ **27** deadlines missed
- 😅 **1** server accidentally deleted

**Ale nauczyliśmy się z każdego błędu!** 💪

## 2025 Goals

- 🎯 40 osób w zespole
- 🚀 200 klientów
- 💰 $3M ARR
- ⭐ 5.0 rating
- 🌍 Ekspansja międzynarodowa

## Thank you!

Dziękujemy całemu zespołowi za niesamowity rok!

Razem osiągniemy jeszcze więcej w 2025! 🚀

— Anna Nowak, CEO
    `,
    imageUrl: 'https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800&h=600&fit=crop',
    authorId: 1,
    createdAt: addDays(-20),
    views: 456,
    category: 'announcement'
  }
]
