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
      const response = await $fetch<{ templates: RequestTemplate[] }>(
        `${config.public.apiUrl}/request-templates/available`,
        {
          headers: getAuthHeaders()
        }
      )
      return response.templates
    } catch (error) {
      console.error('Error fetching available templates:', error)
      throw error
    }
  }

  const getAllTemplates = async () => {
    try {
      const response = await $fetch<{ templates: RequestTemplate[] }>(
        `${config.public.apiUrl}/request-templates`,
        {
          headers: getAuthHeaders()
        }
      )
      return response.templates
    } catch (error) {
      console.error('Error fetching all templates:', error)
      throw error
    }
  }

  const getTemplateById = async (id: string) => {
    try {
      const response = await $fetch<{ template: RequestTemplate }>(
        `${config.public.apiUrl}/request-templates/${id}`,
        {
          headers: getAuthHeaders()
        }
      )
      return response.template
    } catch (error) {
      console.error('Error fetching template:', error)
      throw error
    }
  }

  const createTemplate = async (data: CreateRequestTemplateDto) => {
    try {
      const response = await $fetch<{ id: string; message: string }>(
        `${config.public.apiUrl}/request-templates`,
        {
          method: 'POST',
          headers: getAuthHeaders(),
          body: data
        }
      )
      return response
    } catch (error) {
      console.error('Error creating template:', error)
      throw error
    }
  }

  // Requests
  const getMyRequests = async () => {
    try {
      const response = await $fetch<{ requests: Request[] }>(
        `${config.public.apiUrl}/requests/my-requests`,
        {
          headers: getAuthHeaders()
        }
      )
      return response.requests
    } catch (error) {
      console.error('Error fetching my requests:', error)
      throw error
    }
  }

  const getRequestsToApprove = async () => {
    try {
      const response = await $fetch<{ requests: Request[] }>(
        `${config.public.apiUrl}/requests/to-approve`,
        {
          headers: getAuthHeaders()
        }
      )
      return response.requests
    } catch (error) {
      console.error('Error fetching requests to approve:', error)
      throw error
    }
  }

  const submitRequest = async (data: SubmitRequestDto) => {
    try {
      const response = await $fetch<{ id: string; requestNumber: string; message: string }>(
        `${config.public.apiUrl}/requests`,
        {
          method: 'POST',
          headers: getAuthHeaders(),
          body: {
            ...data,
            formData: JSON.stringify(data.formData)
          }
        }
      )
      return response
    } catch (error) {
      console.error('Error submitting request:', error)
      throw error
    }
  }

  const approveRequestStep = async (requestId: string, stepId: string, data: ApproveStepDto) => {
    try {
      const response = await $fetch<{ success: boolean; message: string }>(
        `${config.public.apiUrl}/requests/${requestId}/steps/${stepId}/approve`,
        {
          method: 'POST',
          headers: getAuthHeaders(),
          body: data
        }
      )
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

