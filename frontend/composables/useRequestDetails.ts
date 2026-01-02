import type { Request, RequestTemplate, RequestApprovalStep } from '~/types/requests'

interface UserWithId {
  id?: string
  Id?: string
}

export interface UseRequestDetailsReturn {
  request: Ref<Request | null>
  template: Ref<RequestTemplate | null>
  isLoading: Ref<boolean>
  error: Ref<string | null>
  isRequestSubmitter: ComputedRef<boolean>
  isCurrentApprover: ComputedRef<boolean>
  currentStep: ComputedRef<RequestApprovalStep | undefined>
  canAddComment: ComputedRef<boolean>
  requiresQuiz: ComputedRef<boolean>
  quizPassed: ComputedRef<boolean>
  quizScore: ComputedRef<number | null | undefined>
  hasQuizQuestions: ComputedRef<boolean>
  loadRequest: () => Promise<void>
  handleApprove: (comment: string) => Promise<void>
  handleReject: (reason: string) => Promise<void>
  handleAddComment: (data: { comment: string; attachments: File[] }) => Promise<void>
}

export function useRequestDetails(requestId: string): UseRequestDetailsReturn {
  const authStore = useAuthStore()
  const { getRequestById, getTemplateById, addComment, approveRequestStep, rejectRequestStep } = useRequestsApi()
  const toast = useNotificationToast()

  const request = ref<Request | null>(null)
  const template = ref<RequestTemplate | null>(null)
  const isLoading = ref(true)
  const error = ref<string | null>(null)

  const isRequestSubmitter = computed((): boolean => {
    if (!request.value || !authStore.user) return false
    const submitterId = request.value.submittedById
    const user = authStore.user as UserWithId
    const userId = user.id || user.Id
    return submitterId === userId
  })

  const isCurrentApprover = computed((): boolean => {
    if (!request.value || !authStore.user) return false

    const currentStepItem = request.value.approvalSteps.find(
      (s: RequestApprovalStep) => s.status === 'InReview'
    )

    if (!currentStepItem) return false

    const approverId = currentStepItem.approverId
    const user = authStore.user as UserWithId
    const userId = user.id || user.Id

    return approverId === userId
  })

  const currentStep = computed((): RequestApprovalStep | undefined => {
    if (!request.value) return undefined
    return request.value.approvalSteps.find(
      (s: RequestApprovalStep) => s.status === 'InReview'
    )
  })

  const canAddComment = computed((): boolean => {
    if (!request.value || !authStore.user) return false

    const user = authStore.user as UserWithId
    const userId = user.id || user.Id

    if (request.value.submittedById === userId) return true

    const isApprover = request.value.approvalSteps.some(
      (s: RequestApprovalStep) => s.approverId === userId
    )
    return isApprover
  })

  const requiresQuiz = computed((): boolean => {
    const step = currentStep.value
    return step?.requiresQuiz === true
  })

  const quizPassed = computed((): boolean => {
    const step = currentStep.value
    return step?.quizPassed === true
  })

  const quizScore = computed((): number | null | undefined => {
    const step = currentStep.value
    return step?.quizScore
  })

  const hasQuizQuestions = computed((): boolean => {
    const step = currentStep.value
    const questions = step?.quizQuestions
    return questions !== undefined && questions !== null && questions.length > 0
  })

  const loadRequest = async (): Promise<void> => {
    isLoading.value = true
    error.value = null

    try {
      const data = await getRequestById(requestId)
      request.value = data

      if (request.value?.requestTemplateId) {
        try {
          template.value = await getTemplateById(request.value.requestTemplateId)
        } catch (e) {
          console.warn('Failed to load request template for details mapping', e)
        }
      }
    } catch (err: unknown) {
      const errorData = err as { statusCode?: number; message?: string }
      if (errorData.statusCode === 404) {
        error.value = 'Wniosek nie zostal znaleziony'
      } else {
        error.value = errorData.message || 'Nie udalo sie pobrac szczeg'
      }
      console.error('Error loading request:', err)
    } finally {
      isLoading.value = false
    }
  }

  const handleApprove = async (comment: string): Promise<void> => {
    if (!currentStep.value) return

    try {
      await approveRequestStep(requestId, currentStep.value.id, {
        comment: comment || undefined
      })
      await loadRequest()
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'Nie udalo sie zatwierdzic wniosku'
      console.error('Error approving request:', err)
    }
  }

  const handleReject = async (reason: string): Promise<void> => {
    if (!currentStep.value) return

    if (!reason.trim()) {
      toast.warning('Komentarz jest wymagany przy odrzuceniu wniosku')
      return
    }

    try {
      await rejectRequestStep(requestId, currentStep.value.id, {
        reason
      })
      await loadRequest()
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'Nie udalo sie odrzucic wniosku'
      console.error('Error rejecting request:', err)
    }
  }

  const handleAddComment = async (data: { comment: string; attachments: File[] }): Promise<void> => {
    try {
      await addComment(requestId, data.comment, data.attachments)
      toast.success('Komentarz zostal dodany')
      await loadRequest()
    } catch (err: unknown) {
      console.error('Error adding comment:', err)
      toast.error('Nie udalo sie dodac komentarza')
    }
  }

  return {
    request,
    template,
    isLoading,
    error,
    isRequestSubmitter,
    isCurrentApprover,
    currentStep,
    canAddComment,
    requiresQuiz,
    quizPassed,
    quizScore,
    hasQuizQuestions,
    loadRequest,
    handleApprove,
    handleReject,
    handleAddComment
  }
}
