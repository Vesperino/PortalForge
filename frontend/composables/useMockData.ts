import type { Employee, News, Document } from '~/types'
import { mockEmployees } from '~/data/mockEmployees'
import { mockDepartments } from '~/data/mockDepartments'
import { mockPositions } from '~/data/mockPositions'
import { mockEvents } from '~/data/mockEvents'
import { mockNews } from '~/data/mockNews'
import { mockDocuments } from '~/data/mockDocuments'

export function useMockData() {
  const enrichEmployeeWithRelations = (employee: Employee): Employee => {
    const department = mockDepartments.find(d => d.id === employee.departmentId)
    const position = mockPositions.find(p => p.id === employee.positionId)
    const supervisor = employee.supervisorId
      ? mockEmployees.find(e => e.id === employee.supervisorId)
      : undefined

    const subordinates = mockEmployees.filter(e => e.supervisorId === employee.id)

    return {
      ...employee,
      department,
      position,
      supervisor,
      subordinates: subordinates.length > 0 ? subordinates : undefined
    }
  }

  const enrichNewsWithRelations = (news: News): News => {
    const author = mockEmployees.find(e => e.id === news.authorId)
    const event = news.eventId ? mockEvents.find(ev => ev.id === news.eventId) : undefined

    return {
      ...news,
      author: author ? enrichEmployeeWithRelations(author) : undefined,
      event
    }
  }

  const enrichDocumentWithRelations = (document: Document): Document => {
    const uploader = mockEmployees.find(e => e.id === document.uploadedBy)

    return {
      ...document,
      uploader: uploader ? enrichEmployeeWithRelations(uploader) : undefined
    }
  }

  const getEmployees = () => {
    return mockEmployees.map(enrichEmployeeWithRelations)
  }

  const getEmployeeById = (id: number) => {
    const employee = mockEmployees.find(e => e.id === id)
    return employee ? enrichEmployeeWithRelations(employee) : null
  }

  const getEmployeesByDepartment = (departmentId: number) => {
    return mockEmployees
      .filter(e => e.departmentId === departmentId)
      .map(enrichEmployeeWithRelations)
  }

  const getEmployeesByRole = (role: string) => {
    return mockEmployees
      .filter(e => e.role === role)
      .map(enrichEmployeeWithRelations)
  }

  const getOrganizationTree = () => {
    const ceo = mockEmployees.find(e => !e.supervisorId)
    if (!ceo) return null

    const buildTree = (employee: Employee): Employee => {
      const enriched = enrichEmployeeWithRelations(employee)
      if (enriched.subordinates) {
        enriched.subordinates = enriched.subordinates.map(buildTree)
      }
      return enriched
    }

    return buildTree(ceo)
  }

  const getDepartments = () => {
    return mockDepartments.map(dept => {
      const manager = dept.managerId
        ? enrichEmployeeWithRelations(mockEmployees.find(e => e.id === dept.managerId)!)
        : undefined

      return {
        ...dept,
        manager
      }
    })
  }

  const getDepartmentById = (id: number) => {
    const dept = mockDepartments.find(d => d.id === id)
    if (!dept) return null

    const manager = dept.managerId
      ? enrichEmployeeWithRelations(mockEmployees.find(e => e.id === dept.managerId)!)
      : undefined

    return {
      ...dept,
      manager
    }
  }

  const getPositions = () => {
    return mockPositions
  }

  const getEvents = () => {
    return mockEvents.sort((a, b) => a.startDate.getTime() - b.startDate.getTime())
  }

  const getEventById = (id: number) => {
    return mockEvents.find(e => e.id === id) || null
  }

  const getUpcomingEvents = (limit?: number) => {
    const now = new Date()
    const upcoming = mockEvents
      .filter(e => e.startDate >= now)
      .sort((a, b) => a.startDate.getTime() - b.startDate.getTime())

    return limit ? upcoming.slice(0, limit) : upcoming
  }

  const getEventsByTag = (tag: string) => {
    return mockEvents
      .filter(e => e.tags.includes(tag as any))
      .sort((a, b) => a.startDate.getTime() - b.startDate.getTime())
  }

  const getEventsByMonth = (year: number, month: number) => {
    return mockEvents.filter(e => {
      const eventDate = new Date(e.startDate)
      return eventDate.getFullYear() === year && eventDate.getMonth() === month
    })
  }

  const getNews = () => {
    return mockNews
      .map(enrichNewsWithRelations)
      .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
  }

  const getNewsById = (id: number) => {
    const news = mockNews.find(n => n.id === id)
    return news ? enrichNewsWithRelations(news) : null
  }

  const getNewsByCategory = (category: string) => {
    return mockNews
      .filter(n => n.category === category)
      .map(enrichNewsWithRelations)
      .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
  }

  const getLatestNews = (limit: number = 5) => {
    return mockNews
      .map(enrichNewsWithRelations)
      .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
      .slice(0, limit)
  }

  const getDocuments = () => {
    return mockDocuments
      .map(enrichDocumentWithRelations)
      .sort((a, b) => b.uploadedAt.getTime() - a.uploadedAt.getTime())
  }

  const getDocumentById = (id: number) => {
    const doc = mockDocuments.find(d => d.id === id)
    return doc ? enrichDocumentWithRelations(doc) : null
  }

  const getDocumentsByCategory = (category: string) => {
    return mockDocuments
      .filter(d => d.category === category)
      .map(enrichDocumentWithRelations)
      .sort((a, b) => b.uploadedAt.getTime() - a.uploadedAt.getTime())
  }

  const searchDocuments = (query: string) => {
    const lowerQuery = query.toLowerCase()
    return mockDocuments
      .filter(d =>
        d.name.toLowerCase().includes(lowerQuery) ||
        d.description?.toLowerCase().includes(lowerQuery)
      )
      .map(enrichDocumentWithRelations)
      .sort((a, b) => b.uploadedAt.getTime() - a.uploadedAt.getTime())
  }

  return {
    // Employees
    getEmployees,
    getEmployeeById,
    getEmployeesByDepartment,
    getEmployeesByRole,
    getOrganizationTree,

    // Departments
    getDepartments,
    getDepartmentById,

    // Positions
    getPositions,

    // Events
    getEvents,
    getEventById,
    getUpcomingEvents,
    getEventsByTag,
    getEventsByMonth,

    // News
    getNews,
    getNewsById,
    getNewsByCategory,
    getLatestNews,

    // Documents
    getDocuments,
    getDocumentById,
    getDocumentsByCategory,
    searchDocuments
  }
}
