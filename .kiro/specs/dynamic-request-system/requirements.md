# Requirements Document

## Introduction

Ulepszenie istniejącego systemu wniosków PortalForge w celu rozwiązania problemów z akceptacją, brakiem możliwości wskazania konkretnego użytkownika oraz topornością tworzenia szablonów. System będzie rozbudowany o zaawansowane funkcjonalności dynamicznych formularzy, lepszą obsługę wniosków urlopowych zgodnych z prawem polskim, automatyzację procesów oraz intuicyjne panele zarządzania.

## Glossary

- **Request System**: Istniejący system zarządzania wnioskami w PortalForge
- **Request Template**: Szablon wniosku (RequestTemplate) definiujący strukturę i przepływ
- **Request Instance**: Konkretny wniosek (Request) utworzony na podstawie szablonu
- **Approval Flow**: Sekwencja kroków zatwierdzania (RequestApprovalStep)
- **Form Builder**: Ulepszone narzędzie do tworzenia dynamicznych formularzy
- **Vacation Request**: Wniosek urlopowy z regułami prawa polskiego (LeaveType)
- **Notification System**: System powiadomień o statusie wniosków
- **Request Dashboard**: Panel zarządzania wnioskami
- **Field Validator**: Komponent walidujący pola formularza (RequestTemplateField)
- **Document Attachment**: Załącznik dokumentu do wniosku
- **Service Request**: Wniosek serwisowy z automatycznym routingiem

## Requirements

### Requirement 1

**User Story:** Jako administrator systemu, chcę mieć ulepszone narzędzie do tworzenia szablonów wniosków z drag-and-drop interfejsem, aby szybko i intuicyjnie budować formularze.

#### Acceptance Criteria

1. WHEN administrator otwiera kreator szablonu, THE Request System SHALL wyświetlić interfejs Form Builder z drag-and-drop funkcjonalnością
2. THE Request System SHALL rozszerzyć istniejące FieldType enum o nowe typy: FileUpload, MultiSelect, DateRange, Rating, Signature
3. THE Request System SHALL umożliwiać definiowanie zaawansowanych walidacji dla RequestTemplateField (regex, custom validators, conditional logic)
4. THE Request System SHALL umożliwiać podgląd formularza w czasie rzeczywistym podczas jego tworzenia
5. THE Request System SHALL zapisywać konfigurację pól w formacie JSON z pełną walidacją struktury

### Requirement 2

**User Story:** Jako użytkownik, chcę mieć ulepszone doświadczenie wypełniania wniosków z inteligentną walidacją i podpowiedziami, aby szybko i bezbłędnie składać wnioski.

#### Acceptance Criteria

1. WHEN użytkownik wybiera szablon wniosku, THE Request System SHALL renderować dynamiczny formularz na podstawie RequestTemplateField konfiguracji
2. THE Request System SHALL implementować walidację client-side i server-side z wykorzystaniem istniejącej infrastruktury walidacji
3. THE Request System SHALL wyświetlać inteligentne podpowiedzi i autouzupełnianie dla pól tekstowych
4. THE Request System SHALL umożliwiać zapisywanie wniosku jako Draft w istniejącej tabeli Request
5. THE Request System SHALL implementować conditional fields - pola pokazujące się na podstawie wartości innych pól

### Requirement 3

**User Story:** Jako pracownik, chcę mieć ulepszony system wniosków urlopowych z pełną obsługą prawa polskiego i automatycznym naliczaniem, aby precyzyjnie zarządzać swoimi urlopami.

#### Acceptance Criteria

1. THE Request System SHALL rozszerzyć istniejący VacationCalculationService o pełne wsparcie dla wszystkich typów urlopów z LeaveType enum
2. WHEN pracownik składa wniosek Circumstantial leave, THE Request System SHALL wymagać załączenia dokumentu przez rozszerzenie Attachments funkcjonalności
3. THE Request System SHALL integrować się z istniejącą tabelą VacationSchedule dla walidacji konfliktów terminów
4. THE Request System SHALL implementować business rules dla urlopów na żądanie (OnDemand) - maksymalnie 4 dni rocznie
5. THE Request System SHALL automatycznie tworzyć VacationSchedule record po zatwierdzeniu wniosku urlopowego

### Requirement 4

**User Story:** Jako administrator, chcę mieć ulepszone narzędzie do konfiguracji przepływów zatwierdzania z wizualnym edytorem, aby łatwo dostosowywać procesy do struktury organizacyjnej.

#### Acceptance Criteria

1. THE Request System SHALL rozszerzyć istniejący RequestApprovalStepTemplate o obsługę równoległych kroków zatwierdzania
2. THE Request System SHALL implementować wizualny edytor przepływów wykorzystujący istniejące ApproverType enum
3. THE Request System SHALL umożliwiać definiowanie conditional approval steps na podstawie wartości z FormData
4. THE Request System SHALL rozszerzyć IRequestRoutingService o obsługę dynamicznych reguł routingu
5. THE Request System SHALL implementować escalation rules - automatyczne przekierowanie po określonym czasie

### Requirement 5

**User Story:** Jako osoba zatwierdzająca, chcę mieć ulepszony dashboard z zaawansowanymi filtrami i bulk actions, aby efektywnie zarządzać dużą liczbą wniosków.

#### Acceptance Criteria

1. THE Request System SHALL ulepszyć istniejący GetRequestsToApproveQuery o zaawansowane filtrowanie i sortowanie
2. THE Request System SHALL implementować bulk approval functionality dla podobnych wniosków
3. THE Request System SHALL rozszerzyć RequestComment system o templates dla częstych odpowiedzi
4. THE Request System SHALL implementować delegation functionality - przekazywanie wniosków innym zatwierdzającym
5. THE Request System SHALL dodać SLA tracking - monitoring czasu oczekiwania na zatwierdzenie

### Requirement 6

**User Story:** Jako pracownik serwisu, chcę mieć dedykowany panel do zarządzania wnioskami serwisowymi z automatycznym routingiem, aby sprawnie realizować zadania.

#### Acceptance Criteria

1. THE Request System SHALL implementować ServiceRequestHandler wykorzystujący istniejący INotificationService
2. THE Request System SHALL rozszerzyć RequestTemplate o ServiceCategory dla automatycznego routingu do odpowiednich zespołów
3. THE Request System SHALL implementować ServiceTaskStatus tracking w ramach istniejącej struktury RequestApprovalStep
4. THE Request System SHALL umożliwiać serwisowi aktualizację statusu przez dedykowane API endpoints
5. THE Request System SHALL generować service reports wykorzystujące istniejący system raportowania

### Requirement 7

**User Story:** Jako użytkownik systemu, chcę mieć ulepszony system powiadomień z personalizacją i smart grouping, aby otrzymywać tylko istotne informacje.

#### Acceptance Criteria

1. THE Request System SHALL rozszerzyć istniejący INotificationService o smart notification grouping
2. THE Request System SHALL implementować notification preferences w User profile
3. THE Request System SHALL dodać real-time notifications wykorzystujące SignalR lub podobną technologię
4. THE Request System SHALL implementować notification templates dla różnych typów wniosków
5. THE Request System SHALL umożliwiać digest notifications - podsumowania dzienne/tygodniowe

### Requirement 8

**User Story:** Jako użytkownik, chcę mieć ulepszoną historię wniosków z zaawansowanymi opcjami zarządzania i analytics, aby mieć pełny wgląd w swoje procesy.

#### Acceptance Criteria

1. THE Request System SHALL rozszerzyć istniejący GetMyRequestsQuery o advanced filtering i search capabilities
2. THE Request System SHALL implementować RequestEditHistory tracking dla pełnej audytowalności
3. THE Request System SHALL umożliwiać cloning requests - tworzenie nowego wniosku na podstawie istniejącego
4. THE Request System SHALL implementować request cancellation workflow z approval requirements
5. THE Request System SHALL dodać personal analytics dashboard - statystyki użytkownika dotyczące jego wniosków