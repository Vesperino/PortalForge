import type { Document } from '~/types'

const addDays = (days: number) => {
  const date = new Date()
  date.setDate(date.getDate() + days)
  return date
}

export const mockDocuments: Document[] = [
  {
    id: 1,
    name: 'Regulamin Pracy.pdf',
    category: 'regulamin',
    fileType: 'PDF',
    size: '2.5 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-120),
    description: 'Regulamin pracy obowiązujący w PortalForge - zasady, prawa i obowiązki pracowników.'
  },
  {
    id: 2,
    name: 'Polityka RODO.pdf',
    category: 'regulamin',
    fileType: 'PDF',
    size: '1.8 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-100),
    description: 'Polityka ochrony danych osobowych zgodna z RODO.'
  },
  {
    id: 3,
    name: 'Polityka Prywatności.pdf',
    category: 'regulamin',
    fileType: 'PDF',
    size: '1.2 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-100),
    description: 'Polityka prywatności firmy i zasady przetwarzania danych.'
  },
  {
    id: 4,
    name: 'Zasady Bezpieczeństwa IT.pdf',
    category: 'regulamin',
    fileType: 'PDF',
    size: '3.1 MB',
    uploadedBy: 15,
    uploadedAt: addDays(-90),
    description: 'Polityka bezpieczeństwa informacji - hasła, dostępy, urządzenia.'
  },
  {
    id: 5,
    name: 'Wzór Umowy o Pracę.docx',
    category: 'wzór',
    fileType: 'DOCX',
    size: '0.5 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-150),
    description: 'Szablon umowy o pracę - do wypełnienia przez HR.'
  },
  {
    id: 6,
    name: 'Wzór Umowy NDA.docx',
    category: 'wzór',
    fileType: 'DOCX',
    size: '0.4 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-150),
    description: 'Umowa o zachowaniu poufności dla pracowników i kontrahentów.'
  },
  {
    id: 7,
    name: 'Wniosek Urlopowy.docx',
    category: 'wzór',
    fileType: 'DOCX',
    size: '0.2 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-200),
    description: 'Formularz wniosku o urlop wypoczynkowy.'
  },
  {
    id: 8,
    name: 'Wniosek o Delegację.docx',
    category: 'wzór',
    fileType: 'DOCX',
    size: '0.3 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-200),
    description: 'Formularz wniosku o delegację służbową.'
  },
  {
    id: 9,
    name: 'Wniosek o Home Office.docx',
    category: 'wzór',
    fileType: 'DOCX',
    size: '0.2 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-180),
    description: 'Formularz wniosku o pracę zdalną (dla wyjątków poza 3+2).'
  },
  {
    id: 10,
    name: 'Wniosek o Szkolenie.docx',
    category: 'wzór',
    fileType: 'DOCX',
    size: '0.3 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-180),
    description: 'Formularz wniosku o dofinansowanie szkolenia/konferencji.'
  },
  {
    id: 11,
    name: 'Onboarding Checklist.pdf',
    category: 'instrukcja',
    fileType: 'PDF',
    size: '1.1 MB',
    uploadedBy: 27,
    uploadedAt: addDays(-60),
    description: 'Checklist dla nowych pracowników - pierwszy tydzień, miesiąc, 90 dni.'
  },
  {
    id: 12,
    name: 'Instrukcja Pracy Zdalnej.pdf',
    category: 'instrukcja',
    fileType: 'PDF',
    size: '0.9 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-70),
    description: 'Jak efektywnie pracować zdalnie - narzędzia, zasady, best practices.'
  },
  {
    id: 13,
    name: 'VPN Setup Guide.pdf',
    category: 'instrukcja',
    fileType: 'PDF',
    size: '1.5 MB',
    uploadedBy: 15,
    uploadedAt: addDays(-80),
    description: 'Instrukcja konfiguracji VPN do zasobów firmowych.'
  },
  {
    id: 14,
    name: 'Dostęp do Systemów Firmowych.pdf',
    category: 'instrukcja',
    fileType: 'PDF',
    size: '2.1 MB',
    uploadedBy: 15,
    uploadedAt: addDays(-80),
    description: 'Jak uzyskać dostęp do wszystkich systemów firmowych (Slack, Jira, GitLab, etc.).'
  },
  {
    id: 15,
    name: 'Expense Reporting Guide.pdf',
    category: 'instrukcja',
    fileType: 'PDF',
    size: '1.3 MB',
    uploadedBy: 23,
    uploadedAt: addDays(-90),
    description: 'Jak rozliczać wydatki służbowe - faktury, delegacje, koszty.'
  },
  {
    id: 16,
    name: 'Procedura Code Review.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '1.7 MB',
    uploadedBy: 3,
    uploadedAt: addDays(-50),
    description: 'Proces code review w PortalForge - kiedy, jak, checklist.'
  },
  {
    id: 17,
    name: 'Deployment Process.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '2.3 MB',
    uploadedBy: 15,
    uploadedAt: addDays(-50),
    description: 'Procedura deployment na production - staging, approvals, rollback.'
  },
  {
    id: 18,
    name: 'Incident Response Plan.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '2.8 MB',
    uploadedBy: 15,
    uploadedAt: addDays(-40),
    description: 'Plan reakcji na incydenty - eskalacja, komunikacja, post-mortem.'
  },
  {
    id: 19,
    name: 'Rekrutacja - Proces.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '1.4 MB',
    uploadedBy: 27,
    uploadedAt: addDays(-60),
    description: 'Proces rekrutacji od ogłoszenia do onboardingu.'
  },
  {
    id: 20,
    name: 'Performance Review Process.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '1.9 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-70),
    description: 'Procedura ocen okresowych - częstotliwość, kryteria, feedback.'
  },
  {
    id: 21,
    name: 'Brandin Guidelines 2025.pdf',
    category: 'instrukcja',
    fileType: 'PDF',
    size: '15.3 MB',
    uploadedBy: 29,
    uploadedAt: addDays(-30),
    description: 'Brand guidelines - logo, kolory, typografia, tone of voice.'
  },
  {
    id: 22,
    name: 'Architecture Decision Records.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '3.4 MB',
    uploadedBy: 2,
    uploadedAt: addDays(-45),
    description: 'Szablon ADR - jak dokumentować decyzje architektoniczne.'
  },
  {
    id: 23,
    name: 'API Design Guidelines.pdf',
    category: 'instrukcja',
    fileType: 'PDF',
    size: '2.2 MB',
    uploadedBy: 3,
    uploadedAt: addDays(-55),
    description: 'Best practices designu REST API - naming, versioning, errors.'
  },
  {
    id: 24,
    name: 'Git Workflow.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '1.1 MB',
    uploadedBy: 3,
    uploadedAt: addDays(-65),
    description: 'Git workflow w PortalForge - branching strategy, commit messages.'
  },
  {
    id: 25,
    name: 'Benefity 2025 - Katalog.pdf',
    category: 'instrukcja',
    fileType: 'PDF',
    size: '4.2 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-20),
    description: 'Pełen katalog benefitów pracowniczych 2025 ze szczegółami.'
  },
  {
    id: 26,
    name: 'Database Migration Guide.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '1.8 MB',
    uploadedBy: 5,
    uploadedAt: addDays(-35),
    description: 'Jak bezpiecznie przeprowadzać migracje baz danych.'
  },
  {
    id: 27,
    name: 'Testing Strategy.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '2.6 MB',
    uploadedBy: 3,
    uploadedAt: addDays(-55),
    description: 'Strategia testowania - unit, integration, E2E, performance.'
  },
  {
    id: 28,
    name: 'Backup & Recovery Procedures.pdf',
    category: 'procedura',
    fileType: 'PDF',
    size: '2.1 MB',
    uploadedBy: 15,
    uploadedAt: addDays(-40),
    description: 'Procedury backup i disaster recovery.'
  },
  {
    id: 29,
    name: 'OKRs Q1 2025.xlsx',
    category: 'instrukcja',
    fileType: 'XLSX',
    size: '0.8 MB',
    uploadedBy: 1,
    uploadedAt: addDays(-10),
    description: 'Objectives and Key Results dla całej firmy na Q1 2025.'
  },
  {
    id: 30,
    name: 'Emergency Contacts.pdf',
    category: 'instrukcja',
    fileType: 'PDF',
    size: '0.3 MB',
    uploadedBy: 26,
    uploadedAt: addDays(-100),
    description: 'Lista kontaktów alarmowych - IT support, HR, management.'
  }
]
