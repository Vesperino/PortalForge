import type {
  RequestTemplate,
  Request,
  ApprovalHistoryItem,
  CreateRequestTemplateDto,
  UpdateRequestTemplateDto,
  SubmitRequestDto,
  ApproveStepDto,
  RejectStepDto,
  Notification
} from '~/types/requests'

export const useRequestsApi = () => {
  const config = useRuntimeConfig()
  const { getAuthHeaders } = useAuth()
  const { handleError } = useApiError()

  const normalizeArrayResponse = <T>(response: unknown, keys: string[]): T[] => {
    if (Array.isArray(response)) return response as T[]

    if (response && typeof response === 'object') {
      for (const key of keys) {
        const value = (response as Record<string, unknown>)[key]
        if (Array.isArray(value)) return value as T[]
      }
    }

    return []
  }

  const extractTemplate = (response: unknown): RequestTemplate => {
    if (response && typeof response === 'object' && 'template' in response) {
      const template = (response as { template?: RequestTemplate }).template
      if (template) return template
    }
    return response as RequestTemplate
  }

  // Request Templates
  const getAvailableTemplates = async () => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/request-templates/available`,
        {
          headers: getAuthHeaders()
        }
      )
      return normalizeArrayResponse<RequestTemplate>(response, ['templates'])
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac dostepnych szablonow' })
      throw error
    }
  }

  const getAllTemplates = async () => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/request-templates`,
        {
          headers: getAuthHeaders()
        }
      )
      return normalizeArrayResponse<RequestTemplate>(response, ['templates'])
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac listy szablonow' })
      throw error
    }
  }

  const getTemplateById = async (id: string) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/request-templates/${id}`,
        {
          headers: getAuthHeaders()
        }
      )
      return extractTemplate(response)
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac szablonu' })
      throw error
    }
  }

  const createTemplate = async (data: CreateRequestTemplateDto) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/request-templates`,
        {
          method: 'POST',
          headers: getAuthHeaders(),
          body: data
        }
      ) as { id: string; message: string }
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie utworzyc szablonu' })
      throw error
    }
  }

  const updateTemplate = async (id: string, data: UpdateRequestTemplateDto) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/request-templates/${id}`,
        {
          method: 'PUT',
          headers: getAuthHeaders(),
          body: data
        }
      ) as { success: boolean; message: string }
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie zaktualizowac szablonu' })
      throw error
    }
  }

  const deleteTemplate = async (id: string) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/request-templates/${id}`,
        {
          method: 'DELETE',
          headers: getAuthHeaders()
        }
      ) as { success: boolean; message: string }
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie usunac szablonu' })
      throw error
    }
  }

  const seedTemplates = async () => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/request-templates/seed`,
        {
          method: 'POST',
          headers: getAuthHeaders()
        }
      ) as { message: string; count: number }
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie zainicjowac szablonow' })
      throw error
    }
  }

  // Requests
  const getMyRequests = async () => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/requests/my-requests`,
        {
          headers: getAuthHeaders()
        }
      )
      return normalizeArrayResponse<Request>(response, ['requests'])
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac Twoich wnioskow' })
      throw error
    }
  }

  const getRequestsToApprove = async () => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/requests/to-approve`,
        {
          headers: getAuthHeaders()
        }
      )
      return normalizeArrayResponse<Request>(response, ['requests'])
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac wnioskow do zatwierdzenia' })
      throw error
    }
  }

  const submitRequest = async (data: SubmitRequestDto) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/requests`,
        {
          method: 'POST',
          headers: getAuthHeaders(),
          body: {
            ...data,
            formData: JSON.stringify(data.formData)
          }
        }
      ) as { id: string; requestNumber: string; message: string }
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie zlozyc wniosku' })
      throw error
    }
  }

  const approveRequestStep = async (requestId: string, stepId: string, data: ApproveStepDto) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/requests/${requestId}/steps/${stepId}/approve`,
        {
          method: 'POST',
          headers: getAuthHeaders(),
          body: data
        }
      ) as { success: boolean; message: string }
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie zatwierdzic kroku wniosku' })
      throw error
    }
  }

  const rejectRequestStep = async (requestId: string, stepId: string, data: RejectStepDto) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/requests/${requestId}/steps/${stepId}/reject`,
        {
          method: 'POST',
          headers: getAuthHeaders(),
          body: data
        }
      ) as { success: boolean; message: string }
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie odrzucic kroku wniosku' })
      throw error
    }
  }

  const getPendingApprovals = async () => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/requests/pending-approvals`,
        {
          headers: getAuthHeaders()
        }
      )
      return normalizeArrayResponse<Request>(response, ['requests'])
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac oczekujacych zatwierdzen' })
      throw error
    }
  }

  const getApprovalsHistory = async () => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/requests/approvals-history`,
        {
          headers: getAuthHeaders()
        }
      )
      return normalizeArrayResponse<ApprovalHistoryItem>(response, ['items', 'requests'])
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac historii zatwierdzen' })
      throw error
    }
  }

  const getRequestById = async (requestId: string) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/requests/${requestId}`,
        {
          headers: getAuthHeaders()
        }
      )
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac szczegolow wniosku' })
      throw error
    }
  }

  const addComment = async (requestId: string, comment: string, files?: File[]) => {
    try {
      // Create FormData
      const formData = new FormData()

      // Add comment text
      if (comment && comment.trim()) {
        formData.append('comment', comment.trim())
      }

      // Add attachments directly (backend will handle upload)
      if (files && files.length > 0) {
        files.forEach(file => {
          formData.append('attachments', file)
        })
      }

      // Use native fetch for FormData to ensure proper multipart/form-data encoding
      // $fetch/ofetch sometimes has issues with FormData serialization
      const authHeaders = getAuthHeaders()
      const response = await fetch(
        `${config.public.apiUrl}/api/requests/${requestId}/comments`,
        {
          method: 'POST',
          headers: {
            ...authHeaders
            // Don't set Content-Type - browser will set it automatically with boundary
          },
          body: formData
        }
      )

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Unknown error' }))
        throw errorData
      }

      const result = await response.json()
      return result as string
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie dodac komentarza' })
      throw error
    }
  }

  // Notifications
  const getNotifications = async (unreadOnly = false, pageNumber = 1, pageSize = 20) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/notifications?unreadOnly=${unreadOnly}&pageNumber=${pageNumber}&pageSize=${pageSize}`,
        {
          headers: getAuthHeaders()
        }
      ) as { notifications: Notification[] }
      return response.notifications
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac powiadomien' })
      throw error
    }
  }

  const getUnreadCount = async () => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/notifications/unread-count`,
        {
          headers: getAuthHeaders()
        }
      ) as { count: number }
      return response.count
    } catch (error: unknown) {
      // Silently handle errors for unread count - backend might not be fully implemented
      const err = error as { statusCode?: number }
      if (err?.statusCode !== 502 && err?.statusCode !== 404) {
        console.error('Error fetching unread count:', error)
      }
      return 0 // Return 0 instead of throwing
    }
  }

  const markAsRead = async (notificationId: string) => {
    try {
      await $fetch(
        `${config.public.apiUrl}/api/notifications/${notificationId}/mark-read`,
        {
          method: 'PATCH',
          headers: getAuthHeaders()
        }
      )
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie oznaczyc powiadomienia jako przeczytane' })
      throw error
    }
  }

  const markAllAsRead = async () => {
    try {
      await $fetch(
        `${config.public.apiUrl}/api/notifications/mark-all-read`,
        {
          method: 'PATCH',
          headers: getAuthHeaders()
        }
      )
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie oznaczyc wszystkich powiadomien jako przeczytane' })
      throw error
    }
  }

  return {
    // Templates
    getAvailableTemplates,
    getAllTemplates,
    getTemplateById,
    createTemplate,
    updateTemplate,
    deleteTemplate,
    seedTemplates,

    // Requests
    getMyRequests,
    getRequestsToApprove,
    getPendingApprovals,
    getApprovalsHistory,
    getRequestById,
    submitRequest,
    approveRequestStep,
    rejectRequestStep,
    addComment,

    // Notifications
    getNotifications,
    getUnreadCount,
    markAsRead,
    markAllAsRead
  }
}

