import type { Department } from '~/types'

export const mockDepartments: Department[] = [
  {
    id: 1,
    name: 'Zarząd',
    description: 'Naczelne kierownictwo firmy',
    managerId: 1,
    color: '#3b82f6' // blue-500
  },
  {
    id: 2,
    name: 'IT',
    description: 'Dział technologii informacyjnej',
    managerId: 2,
    color: '#8b5cf6' // violet-500
  },
  {
    id: 3,
    name: 'HR',
    description: 'Dział zasobów ludzkich',
    managerId: 26,
    color: '#10b981' // emerald-500
  },
  {
    id: 4,
    name: 'Marketing',
    description: 'Dział marketingu i komunikacji',
    managerId: 29,
    color: '#f59e0b' // amber-500
  },
  {
    id: 5,
    name: 'Finanse',
    description: 'Dział finansowo-księgowy',
    managerId: 22,
    color: '#ef4444' // red-500
  },
  {
    id: 6,
    name: 'Produkt',
    description: 'Zarządzanie produktem',
    managerId: 17,
    color: '#ec4899' // pink-500
  }
]
