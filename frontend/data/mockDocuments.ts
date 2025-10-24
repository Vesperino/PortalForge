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
    category: 'policy',
    fileType: 'pdf',
    size: 2621440,
    uploadedBy: 26,
    uploadedAt: addDays(-120),
    description: 'Regulamin pracy obowiązujący w PortalForge - zasady, prawa i obowiązki pracowników.'
  },
  {
    id: 2,
    name: 'Polityka RODO.pdf',
    category: 'policy',
    fileType: 'pdf',
    size: 1887437,
    uploadedBy: 26,
    uploadedAt: addDays(-100),
    description: 'Polityka ochrony danych osobowych zgodna z RODO.'
  },
  {
    id: 3,
    name: 'Polityka Prywatności.pdf',
    category: 'policy',
    fileType: 'pdf',
    size: 1258291,
    uploadedBy: 26,
    uploadedAt: addDays(-100),
    description: 'Polityka prywatności firmy i zasady przetwarzania danych.'
  },
  {
    id: 4,
    name: 'Zasady Bezpieczeństwa IT.pdf',
    category: 'policy',
    fileType: 'pdf',
    size: 3250586,
    uploadedBy: 15,
    uploadedAt: addDays(-90),
    description: 'Polityka bezpieczeństwa informacji - hasła, dostępy, urządzenia.'
  },
  {
    id: 5,
    name: 'Wzór Umowy o Pracę.docx',
    category: 'template',
    fileType: 'docx',
    size: 524288,
    uploadedBy: 26,
    uploadedAt: addDays(-150),
    description: 'Szablon umowy o pracę - do wypełnienia przez HR.'
  },
  {
    id: 6,
    name: 'Wzór Umowy NDA.docx',
    category: 'template',
    fileType: 'docx',
    size: 419430,
    uploadedBy: 26,
    uploadedAt: addDays(-150),
    description: 'Umowa o zachowaniu poufności dla pracowników i kontrahentów.'
  },
  {
    id: 7,
    name: 'Wniosek Urlopowy.docx',
    category: 'template',
    fileType: 'docx',
    size: 209715,
    uploadedBy: 26,
    uploadedAt: addDays(-200),
    description: 'Formularz wniosku o urlop wypoczynkowy.'
  },
  {
    id: 8,
    name: 'Wniosek o Delegację.docx',
    category: 'template',
    fileType: 'docx',
    size: 314572,
    uploadedBy: 26,
    uploadedAt: addDays(-200),
    description: 'Formularz wniosku o delegację służbową.'
  },
  {
    id: 9,
    name: 'Wniosek o Home Office.docx',
    category: 'template',
    fileType: 'docx',
    size: 209715,
    uploadedBy: 26,
    uploadedAt: addDays(-180),
    description: 'Formularz wniosku o pracę zdalną (dla wyjątków poza 3+2).'
  },
  {
    id: 10,
    name: 'Wniosek o Szkolenie.docx',
    category: 'template',
    fileType: 'docx',
    size: 314572,
    uploadedBy: 26,
    uploadedAt: addDays(-180),
    description: 'Formularz wniosku o dofinansowanie szkolenia/konferencji.'
  },
  {
    id: 11,
    name: 'Onboarding Checklist.pdf',
    category: 'manual',
    fileType: 'pdf',
    size: 1153433,
    uploadedBy: 27,
    uploadedAt: addDays(-60),
    description: 'Checklist dla nowych pracowników - pierwszy tydzień, miesiąc, 90 dni.'
  },
  {
    id: 12,
    name: 'Instrukcja Pracy Zdalnej.pdf',
    category: 'manual',
    fileType: 'pdf',
    size: 943718,
    uploadedBy: 26,
    uploadedAt: addDays(-70),
    description: 'Jak efektywnie pracować zdalnie - narzędzia, zasady, best practices.'
  },
  {
    id: 13,
    name: 'VPN Setup Guide.pdf',
    category: 'manual',
    fileType: 'pdf',
    size: 1572864,
    uploadedBy: 15,
    uploadedAt: addDays(-80),
    description: 'Instrukcja konfiguracji VPN do zasobów firmowych.'
  },
  {
    id: 14,
    name: 'Dostęp do Systemów Firmowych.pdf',
    category: 'manual',
    fileType: 'pdf',
    size: 2202009,
    uploadedBy: 15,
    uploadedAt: addDays(-80),
    description: 'Jak uzyskać dostęp do wszystkich systemów firmowych (Slack, Jira, GitLab, etc.).'
  },
  {
    id: 15,
    name: 'Expense Reporting Guide.pdf',
    category: 'manual',
    fileType: 'pdf',
    size: 1363148,
    uploadedBy: 23,
    uploadedAt: addDays(-90),
    description: 'Jak rozliczać wydatki służbowe - faktury, delegacje, koszty.'
  },
  {
    id: 16,
    name: 'Procedura Code Review.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 1782579,
    uploadedBy: 3,
    uploadedAt: addDays(-50),
    description: 'Proces code review w PortalForge - kiedy, jak, checklist.'
  },
  {
    id: 17,
    name: 'Deployment Process.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 2411724,
    uploadedBy: 15,
    uploadedAt: addDays(-50),
    description: 'Procedura deployment na production - staging, approvals, rollback.'
  },
  {
    id: 18,
    name: 'Incident Response Plan.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 2936012,
    uploadedBy: 15,
    uploadedAt: addDays(-40),
    description: 'Plan reakcji na incydenty - eskalacja, komunikacja, post-mortem.'
  },
  {
    id: 19,
    name: 'Rekrutacja - Proces.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 1468006,
    uploadedBy: 27,
    uploadedAt: addDays(-60),
    description: 'Proces rekrutacji od ogłoszenia do onboardingu.'
  },
  {
    id: 20,
    name: 'Performance Review Process.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 1992294,
    uploadedBy: 26,
    uploadedAt: addDays(-70),
    description: 'Procedura ocen okresowych - częstotliwość, kryteria, feedback.'
  },
  {
    id: 21,
    name: 'Brandin Guidelines 2025.pdf',
    category: 'manual',
    fileType: 'pdf',
    size: 16040755,
    uploadedBy: 29,
    uploadedAt: addDays(-30),
    description: 'Brand guidelines - logo, kolory, typografia, tone of voice.'
  },
  {
    id: 22,
    name: 'Architecture Decision Records.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 3565158,
    uploadedBy: 2,
    uploadedAt: addDays(-45),
    description: 'Szablon ADR - jak dokumentować decyzje architektoniczne.'
  },
  {
    id: 23,
    name: 'API Design Guidelines.pdf',
    category: 'manual',
    fileType: 'pdf',
    size: 2306867,
    uploadedBy: 3,
    uploadedAt: addDays(-55),
    description: 'Best practices designu REST API - naming, versioning, errors.'
  },
  {
    id: 24,
    name: 'Git Workflow.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 1153433,
    uploadedBy: 3,
    uploadedAt: addDays(-65),
    description: 'Git workflow w PortalForge - branching strategy, commit messages.'
  },
  {
    id: 25,
    name: 'Benefity 2025 - Katalog.pdf',
    category: 'manual',
    fileType: 'pdf',
    size: 4404019,
    uploadedBy: 26,
    uploadedAt: addDays(-20),
    description: 'Pełen katalog benefitów pracowniczych 2025 ze szczegółami.'
  },
  {
    id: 26,
    name: 'Database Migration Guide.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 1887437,
    uploadedBy: 5,
    uploadedAt: addDays(-35),
    description: 'Jak bezpiecznie przeprowadzać migracje baz danych.'
  },
  {
    id: 27,
    name: 'Testing Strategy.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 2726297,
    uploadedBy: 3,
    uploadedAt: addDays(-55),
    description: 'Strategia testowania - unit, integration, E2E, performance.'
  },
  {
    id: 28,
    name: 'Backup & Recovery Procedures.pdf',
    category: 'procedure',
    fileType: 'pdf',
    size: 2202009,
    uploadedBy: 15,
    uploadedAt: addDays(-40),
    description: 'Procedury backup i disaster recovery.'
  },
  {
    id: 29,
    name: 'OKRs Q1 2025.xlsx',
    category: 'manual',
    fileType: 'xlsx',
    size: 838860,
    uploadedBy: 1,
    uploadedAt: addDays(-10),
    description: 'Objectives and Key Results dla całej firmy na Q1 2025.'
  },
  {
    id: 30,
    name: 'Emergency Contacts.pdf',
    category: 'manual',
    fileType: 'pdf',
    size: 314572,
    uploadedBy: 26,
    uploadedAt: addDays(-100),
    description: 'Lista kontaktów alarmowych - IT support, HR, management.'
  }
]
