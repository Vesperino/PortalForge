import type { 
  RequestTemplate, 
  Request, 
  CreateRequestTemplateDto, 
  SubmitRequestDto,
  ApproveStepDto
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

  return {
    // Templates
    getAvailableTemplates,
    getAllTemplates,
    getTemplateById,
    createTemplate,
    
    // Requests
    getMyRequests,
    getRequestsToApprove,
    submitRequest,
    approveRequestStep
  }
}

