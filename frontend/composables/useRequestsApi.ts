import type {
  RequestTemplate,
  Request,
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

  // Request Templates
  const getAvailableTemplates = async () => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/request-templates/available`,
        {
          headers: getAuthHeaders()
        }
      ) as { templates: RequestTemplate[] }
      return response.templates
    } catch (error) {
      console.error('Error fetching available templates:', error)
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
      ) as { templates: RequestTemplate[] }
      return response.templates
    } catch (error) {
      console.error('Error fetching all templates:', error)
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
      ) as { template: RequestTemplate }
      return response.template
    } catch (error) {
      console.error('Error fetching template:', error)
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
      console.error('Error creating template:', error)
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
      console.error('Error updating template:', error)
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
      console.error('Error deleting template:', error)
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
      console.error('Error seeding templates:', error)
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
      ) as { requests: Request[] }
      return response.requests
    } catch (error) {
      console.error('Error fetching my requests:', error)
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
      ) as { requests: Request[] }
      return response.requests
    } catch (error) {
      console.error('Error fetching requests to approve:', error)
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
      console.error('Error submitting request:', error)
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
      console.error('Error approving request step:', error)
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
      console.error('Error rejecting request step:', error)
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
      ) as { requests: Request[] }
      return response.requests
    } catch (error) {
      console.error('Error fetching pending approvals:', error)
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
      ) as { items: Request[] }
      return response.items
    } catch (error) {
      console.error('Error fetching approvals history:', error)
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
      console.error('Error fetching request details:', error)
      throw error
    }
  }

  const addComment = async (requestId: string, comment: string, attachments?: string) => {
    try {
      const response = await $fetch(
        `${config.public.apiUrl}/api/requests/${requestId}/comments`,
        {
          method: 'POST',
          headers: getAuthHeaders(),
          body: {
            comment,
            attachments: attachments || null
          }
        }
      ) as string
      return response
    } catch (error) {
      console.error('Error adding comment:', error)
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
      console.error('Error fetching notifications:', error)
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
    } catch (error: any) {
      // Silently handle errors for unread count - backend might not be fully implemented
      if (error?.statusCode !== 502 && error?.statusCode !== 404) {
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
      console.error('Error marking notification as read:', error)
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
      console.error('Error marking all notifications as read:', error)
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

