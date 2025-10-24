import type { Department } from '~/types'

export const mockDepartments: Department[] = [
  {
    id: 1,
    name: 'Zarząd',
    description: 'Naczelne kierownictwo firmy',
    managerId: 1
  },
  {
    id: 2,
    name: 'IT',
    description: 'Dział technologii informacyjnej',
    managerId: 2
  },
  {
    id: 3,
    name: 'HR',
    description: 'Dział zasobów ludzkich',
    managerId: 26
  },
  {
    id: 4,
    name: 'Marketing',
    description: 'Dział marketingu i komunikacji',
    managerId: 29
  },
  {
    id: 5,
    name: 'Finanse',
    description: 'Dział finansowo-księgowy',
    managerId: 22
  },
  {
    id: 6,
    name: 'Produkt',
    description: 'Zarządzanie produktem',
    managerId: 17
  }
]
