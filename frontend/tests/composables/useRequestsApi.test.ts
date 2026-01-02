import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useRequestsApi } from '~/composables/useRequestsApi'
import { useAuthStore } from '~/stores/auth'
import type { RequestTemplate, Request, SubmitRequestDto, Notification } from '~/types/requests'

describe('useRequestsApi', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    const authStore = useAuthStore()
    authStore.accessToken = 'mock-token'
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  describe('Template operations', () => {
    describe('getAvailableTemplates', () => {
      it('should fetch available templates successfully', async () => {
        const mockTemplates: RequestTemplate[] = [
          {
            id: '1',
            name: 'Wniosek urlopowy',
            description: 'Wniosek o urlop wypoczynkowy',
            icon: 'vacation',
            category: 'HR',
            isActive: true,
            requiresApproval: true,
            createdById: 'user-1',
            createdByName: 'Admin User',
            createdAt: '2024-01-01T00:00:00Z',
            fields: [],
            approvalStepTemplates: [],
            quizQuestions: []
          }
        ]

        global.$fetch = vi.fn().mockResolvedValue({ templates: mockTemplates })

        const { getAvailableTemplates } = useRequestsApi()
        const result = await getAvailableTemplates()

        expect(result).toEqual(mockTemplates)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/request-templates/available',
          expect.objectContaining({
            headers: expect.objectContaining({
              Authorization: 'Bearer mock-token'
            })
          })
        )
      })

      it('should throw error on fetch failure', async () => {
        const error = new Error('Network error')
        global.$fetch = vi.fn().mockRejectedValue(error)

        const { getAvailableTemplates } = useRequestsApi()

        await expect(getAvailableTemplates()).rejects.toThrow('Network error')
      })
    })

    describe('getAllTemplates', () => {
      it('should fetch all templates successfully', async () => {
        const mockTemplates: RequestTemplate[] = [
          {
            id: '1',
            name: 'Template 1',
            description: 'Description 1',
            icon: 'icon1',
            category: 'HR',
            isActive: true,
            requiresApproval: true,
            createdById: 'user-1',
            createdByName: 'Admin',
            createdAt: '2024-01-01T00:00:00Z',
            fields: [],
            approvalStepTemplates: [],
            quizQuestions: []
          },
          {
            id: '2',
            name: 'Template 2',
            description: 'Description 2',
            icon: 'icon2',
            category: 'IT',
            isActive: false,
            requiresApproval: false,
            createdById: 'user-2',
            createdByName: 'IT Admin',
            createdAt: '2024-01-02T00:00:00Z',
            fields: [],
            approvalStepTemplates: [],
            quizQuestions: []
          }
        ]

        global.$fetch = vi.fn().mockResolvedValue({ templates: mockTemplates })

        const { getAllTemplates } = useRequestsApi()
        const result = await getAllTemplates()

        expect(result).toEqual(mockTemplates)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/request-templates',
          expect.any(Object)
        )
      })
    })

    describe('getTemplateById', () => {
      it('should fetch template by ID successfully', async () => {
        const mockTemplate: RequestTemplate = {
          id: 'template-123',
          name: 'Test Template',
          description: 'Test Description',
          icon: 'test-icon',
          category: 'Test',
          isActive: true,
          requiresApproval: true,
          createdById: 'user-1',
          createdByName: 'Admin',
          createdAt: '2024-01-01T00:00:00Z',
          fields: [
            {
              id: 'field-1',
              label: 'Reason',
              fieldType: 'Textarea',
              isRequired: true,
              order: 1
            }
          ],
          approvalStepTemplates: [],
          quizQuestions: []
        }

        global.$fetch = vi.fn().mockResolvedValue({ template: mockTemplate })

        const { getTemplateById } = useRequestsApi()
        const result = await getTemplateById('template-123')

        expect(result).toEqual(mockTemplate)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/request-templates/template-123',
          expect.any(Object)
        )
      })
    })

    describe('createTemplate', () => {
      it('should create template successfully', async () => {
        const mockResponse = { id: 'new-template-id', message: 'Template created' }

        global.$fetch = vi.fn().mockResolvedValue(mockResponse)

        const { createTemplate } = useRequestsApi()
        const result = await createTemplate({
          name: 'New Template',
          description: 'New Description',
          icon: 'new-icon',
          category: 'HR',
          requiresApproval: true,
          fields: [],
          approvalStepTemplates: []
        })

        expect(result).toEqual(mockResponse)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/request-templates',
          expect.objectContaining({
            method: 'POST',
            body: expect.objectContaining({
              name: 'New Template'
            })
          })
        )
      })
    })

    describe('updateTemplate', () => {
      it('should update template successfully', async () => {
        const mockResponse = { success: true, message: 'Template updated' }

        global.$fetch = vi.fn().mockResolvedValue(mockResponse)

        const { updateTemplate } = useRequestsApi()
        const result = await updateTemplate('template-id', {
          name: 'Updated Name',
          isActive: false
        })

        expect(result).toEqual(mockResponse)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/request-templates/template-id',
          expect.objectContaining({
            method: 'PUT',
            body: expect.objectContaining({
              name: 'Updated Name',
              isActive: false
            })
          })
        )
      })
    })

    describe('deleteTemplate', () => {
      it('should delete template successfully', async () => {
        const mockResponse = { success: true, message: 'Template deleted' }

        global.$fetch = vi.fn().mockResolvedValue(mockResponse)

        const { deleteTemplate } = useRequestsApi()
        const result = await deleteTemplate('template-id')

        expect(result).toEqual(mockResponse)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/request-templates/template-id',
          expect.objectContaining({
            method: 'DELETE'
          })
        )
      })
    })

    describe('seedTemplates', () => {
      it('should seed templates successfully', async () => {
        const mockResponse = { message: 'Templates seeded', count: 5 }

        global.$fetch = vi.fn().mockResolvedValue(mockResponse)

        const { seedTemplates } = useRequestsApi()
        const result = await seedTemplates()

        expect(result).toEqual(mockResponse)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/request-templates/seed',
          expect.objectContaining({
            method: 'POST'
          })
        )
      })
    })
  })

  describe('Request operations', () => {
    describe('getMyRequests', () => {
      it('should fetch user requests successfully', async () => {
        const mockRequests: Request[] = [
          {
            id: 'req-1',
            requestNumber: 'REQ-2024-001',
            requestTemplateId: 'template-1',
            requestTemplateName: 'Vacation Request',
            requestTemplateIcon: 'vacation',
            submittedById: 'user-1',
            submittedByName: 'John Doe',
            submittedAt: '2024-01-15T10:00:00Z',
            priority: 'Standard',
            formData: '{}',
            status: 'InReview',
            approvalSteps: []
          }
        ]

        global.$fetch = vi.fn().mockResolvedValue({ requests: mockRequests })

        const { getMyRequests } = useRequestsApi()
        const result = await getMyRequests()

        expect(result).toEqual(mockRequests)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/requests/my-requests',
          expect.any(Object)
        )
      })
    })

    describe('getRequestsToApprove', () => {
      it('should fetch requests to approve successfully', async () => {
        const mockRequests: Request[] = [
          {
            id: 'req-2',
            requestNumber: 'REQ-2024-002',
            requestTemplateId: 'template-1',
            requestTemplateName: 'Sick Leave',
            requestTemplateIcon: 'sick',
            submittedById: 'user-2',
            submittedByName: 'Jane Smith',
            submittedAt: '2024-01-16T09:00:00Z',
            priority: 'Urgent',
            formData: '{}',
            status: 'InReview',
            approvalSteps: []
          }
        ]

        global.$fetch = vi.fn().mockResolvedValue({ requests: mockRequests })

        const { getRequestsToApprove } = useRequestsApi()
        const result = await getRequestsToApprove()

        expect(result).toEqual(mockRequests)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/requests/to-approve',
          expect.any(Object)
        )
      })
    })

    describe('submitRequest', () => {
      it('should submit request successfully', async () => {
        const mockResponse = {
          id: 'new-req-id',
          requestNumber: 'REQ-2024-003',
          message: 'Request submitted'
        }

        global.$fetch = vi.fn().mockResolvedValue(mockResponse)

        const submitData: SubmitRequestDto = {
          requestTemplateId: 'template-1',
          priority: 'Standard',
          formData: { reason: 'Annual vacation' }
        }

        const { submitRequest } = useRequestsApi()
        const result = await submitRequest(submitData)

        expect(result).toEqual(mockResponse)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/requests',
          expect.objectContaining({
            method: 'POST',
            body: expect.objectContaining({
              requestTemplateId: 'template-1',
              priority: 'Standard',
              formData: JSON.stringify({ reason: 'Annual vacation' })
            })
          })
        )
      })
    })

    describe('approveRequestStep', () => {
      it('should approve request step successfully', async () => {
        const mockResponse = { success: true, message: 'Step approved' }

        global.$fetch = vi.fn().mockResolvedValue(mockResponse)

        const { approveRequestStep } = useRequestsApi()
        const result = await approveRequestStep('req-1', 'step-1', { comment: 'Approved' })

        expect(result).toEqual(mockResponse)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/requests/req-1/steps/step-1/approve',
          expect.objectContaining({
            method: 'POST',
            body: { comment: 'Approved' }
          })
        )
      })
    })

    describe('rejectRequestStep', () => {
      it('should reject request step successfully', async () => {
        const mockResponse = { success: true, message: 'Step rejected' }

        global.$fetch = vi.fn().mockResolvedValue(mockResponse)

        const { rejectRequestStep } = useRequestsApi()
        const result = await rejectRequestStep('req-1', 'step-1', { reason: 'Invalid dates' })

        expect(result).toEqual(mockResponse)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/requests/req-1/steps/step-1/reject',
          expect.objectContaining({
            method: 'POST',
            body: { reason: 'Invalid dates' }
          })
        )
      })
    })

    describe('getPendingApprovals', () => {
      it('should fetch pending approvals successfully', async () => {
        const mockRequests: Request[] = []

        global.$fetch = vi.fn().mockResolvedValue({ requests: mockRequests })

        const { getPendingApprovals } = useRequestsApi()
        const result = await getPendingApprovals()

        expect(result).toEqual(mockRequests)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/requests/pending-approvals',
          expect.any(Object)
        )
      })
    })

    describe('getApprovalsHistory', () => {
      it('should fetch approvals history successfully', async () => {
        const mockRequests: Request[] = []

        global.$fetch = vi.fn().mockResolvedValue({ items: mockRequests })

        const { getApprovalsHistory } = useRequestsApi()
        const result = await getApprovalsHistory()

        expect(result).toEqual(mockRequests)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/requests/approvals-history',
          expect.any(Object)
        )
      })
    })

    describe('getRequestById', () => {
      it('should fetch request by ID successfully', async () => {
        const mockRequest = {
          id: 'req-123',
          requestNumber: 'REQ-2024-123',
          status: 'Approved'
        }

        global.$fetch = vi.fn().mockResolvedValue(mockRequest)

        const { getRequestById } = useRequestsApi()
        const result = await getRequestById('req-123')

        expect(result).toEqual(mockRequest)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/requests/req-123',
          expect.any(Object)
        )
      })
    })
  })

  describe('Notification operations', () => {
    describe('getNotifications', () => {
      it('should fetch notifications with default parameters', async () => {
        const mockNotifications: Notification[] = [
          {
            id: 'notif-1',
            type: 'RequestApproved',
            title: 'Request Approved',
            message: 'Your vacation request has been approved',
            isRead: false,
            createdAt: '2024-01-15T10:00:00Z'
          }
        ]

        global.$fetch = vi.fn().mockResolvedValue({ notifications: mockNotifications })

        const { getNotifications } = useRequestsApi()
        const result = await getNotifications()

        expect(result).toEqual(mockNotifications)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/notifications?unreadOnly=false&pageNumber=1&pageSize=20',
          expect.any(Object)
        )
      })

      it('should fetch unread notifications only', async () => {
        global.$fetch = vi.fn().mockResolvedValue({ notifications: [] })

        const { getNotifications } = useRequestsApi()
        await getNotifications(true, 2, 10)

        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/notifications?unreadOnly=true&pageNumber=2&pageSize=10',
          expect.any(Object)
        )
      })
    })

    describe('getUnreadCount', () => {
      it('should fetch unread count successfully', async () => {
        global.$fetch = vi.fn().mockResolvedValue({ count: 5 })

        const { getUnreadCount } = useRequestsApi()
        const result = await getUnreadCount()

        expect(result).toBe(5)
        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/notifications/unread-count',
          expect.any(Object)
        )
      })

      it('should return 0 on 502 error', async () => {
        const error = { statusCode: 502 }
        global.$fetch = vi.fn().mockRejectedValue(error)

        const { getUnreadCount } = useRequestsApi()
        const result = await getUnreadCount()

        expect(result).toBe(0)
      })

      it('should return 0 on 404 error', async () => {
        const error = { statusCode: 404 }
        global.$fetch = vi.fn().mockRejectedValue(error)

        const { getUnreadCount } = useRequestsApi()
        const result = await getUnreadCount()

        expect(result).toBe(0)
      })

      it('should return 0 on other errors', async () => {
        const error = { statusCode: 500 }
        global.$fetch = vi.fn().mockRejectedValue(error)

        const { getUnreadCount } = useRequestsApi()
        const result = await getUnreadCount()

        expect(result).toBe(0)
      })
    })

    describe('markAsRead', () => {
      it('should mark notification as read successfully', async () => {
        global.$fetch = vi.fn().mockResolvedValue(undefined)

        const { markAsRead } = useRequestsApi()
        await markAsRead('notif-1')

        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/notifications/notif-1/mark-read',
          expect.objectContaining({
            method: 'PATCH'
          })
        )
      })

      it('should throw error on failure', async () => {
        const error = new Error('Notification not found')
        global.$fetch = vi.fn().mockRejectedValue(error)

        const { markAsRead } = useRequestsApi()

        await expect(markAsRead('invalid-id')).rejects.toThrow('Notification not found')
      })
    })

    describe('markAllAsRead', () => {
      it('should mark all notifications as read successfully', async () => {
        global.$fetch = vi.fn().mockResolvedValue(undefined)

        const { markAllAsRead } = useRequestsApi()
        await markAllAsRead()

        expect(global.$fetch).toHaveBeenCalledWith(
          'http://localhost:5155/api/notifications/mark-all-read',
          expect.objectContaining({
            method: 'PATCH'
          })
        )
      })
    })
  })

  describe('Authorization header', () => {
    it('should use correct authorization token from auth store', async () => {
      const authStore = useAuthStore()
      authStore.accessToken = 'custom-test-token'

      global.$fetch = vi.fn().mockResolvedValue({ templates: [] })

      const { getAvailableTemplates } = useRequestsApi()
      await getAvailableTemplates()

      expect(global.$fetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: 'Bearer custom-test-token'
          })
        })
      )
    })
  })
})
