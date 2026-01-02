import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import RequestDetailsModal from '~/components/requests/RequestDetailsModal.vue'
import type { RequestWithDetails, RequestTemplate, RequestApprovalStep } from '~/types/requests'

const mockGetIconifyName = vi.fn((name: string) => `icon-${name}`)

describe('RequestDetailsModal', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Override the global useIconMapping mock for this test file
    globalThis.useIconMapping = vi.fn(() => ({
      iconMapping: {},
      getIconifyName: mockGetIconifyName
    }))
  })

  const mockApprovalSteps: RequestApprovalStep[] = [
    {
      id: 'step-1',
      stepOrder: 1,
      approverId: 'approver-1',
      approverName: 'John Manager',
      status: 'Approved',
      startedAt: '2024-01-15T10:00:00Z',
      finishedAt: '2024-01-16T09:00:00Z',
      comment: 'Approved as requested',
      requiresQuiz: false
    },
    {
      id: 'step-2',
      stepOrder: 2,
      approverId: 'approver-2',
      approverName: 'Jane Director',
      status: 'Pending',
      requiresQuiz: false
    }
  ]

  const mockRequest: RequestWithDetails = {
    id: 'req-123',
    requestNumber: 'REQ-2024-001',
    requestTemplateId: 'template-1',
    requestTemplateName: 'Wniosek urlopowy',
    requestTemplateIcon: 'beach-umbrella',
    submittedById: 'user-1',
    submittedByName: 'Jan Kowalski',
    submittedAt: '2024-01-15T09:30:00Z',
    priority: 'Standard',
    formData: JSON.stringify({
      startDate: '2024-02-01',
      endDate: '2024-02-07',
      reason: 'Rodzinne wakacje'
    }),
    status: 'InReview',
    approvalSteps: mockApprovalSteps,
    comments: [
      {
        id: 'comment-1',
        userId: 'user-1',
        userName: 'Jan Kowalski',
        comment: 'Prosze o szybkie rozpatrzenie',
        attachments: [],
        createdAt: '2024-01-15T10:00:00Z'
      }
    ],
    attachments: ['https://example.com/doc1.pdf'],
    editHistory: []
  }

  const mockTemplate: RequestTemplate = {
    id: 'template-1',
    name: 'Wniosek urlopowy',
    description: 'Wniosek o urlop wypoczynkowy',
    icon: 'beach-umbrella',
    category: 'HR',
    isActive: true,
    requiresApproval: true,
    createdById: 'admin-1',
    createdByName: 'Admin',
    createdAt: '2024-01-01T00:00:00Z',
    fields: [
      {
        id: 'field-1',
        label: 'Data rozpoczecia',
        fieldType: 'Date',
        isRequired: true,
        order: 1
      },
      {
        id: 'field-2',
        label: 'Data zakonczenia',
        fieldType: 'Date',
        isRequired: true,
        order: 2
      },
      {
        id: 'field-3',
        label: 'Powod',
        fieldType: 'Textarea',
        isRequired: false,
        order: 3
      }
    ],
    approvalStepTemplates: [],
    quizQuestions: []
  }

  const globalStubs = {
    Icon: {
      template: '<span data-testid="icon">{{ name }}</span>',
      props: ['name']
    },
    RequestStatusBadge: {
      template: '<span data-testid="status-badge">{{ status }}</span>',
      props: ['status']
    },
    RequestTimeline: {
      template: '<div data-testid="timeline">Timeline</div>',
      props: ['steps']
    },
    RequestFormDataDisplay: {
      template: '<div data-testid="form-data-display">Form Data</div>',
      props: ['formData', 'fields']
    },
    RequestAttachments: {
      template: '<div data-testid="attachments">Attachments</div>',
      props: ['attachments']
    },
    RequestComments: {
      template: '<div data-testid="comments">Comments</div>',
      props: ['comments', 'canAddComment']
    },
    RequestEditHistory: {
      template: '<div data-testid="edit-history">Edit History</div>',
      props: ['editHistory']
    },
    Teleport: {
      template: '<div><slot /></div>'
    }
  }

  describe('Modal Visibility', () => {
    it('should not render when request is null', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: null,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('[data-testid="request-details-modal"]').exists()).toBe(false)
    })

    it('should render when request is provided', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('[data-testid="request-details-modal"]').exists()).toBe(true)
    })

    it('should emit close event when close button is clicked', async () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const closeButton = wrapper.find('button')
      await closeButton.trigger('click')

      expect(wrapper.emitted('close')).toBeTruthy()
    })

    it('should emit close event when clicking backdrop', async () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const backdrop = wrapper.find('[data-testid="request-details-modal"]')
      await backdrop.trigger('click')

      expect(wrapper.emitted('close')).toBeTruthy()
    })
  })

  describe('Request Information Display', () => {
    it('should display request template name', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Wniosek urlopowy')
    })

    it('should display request number', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('REQ-2024-001')
    })

    it('should display submitter name', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Jan Kowalski')
    })

    it('should display formatted submission date', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('15 stycznia 2024')
    })
  })

  describe('Request Status', () => {
    it('should render RequestStatusBadge component', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const statusBadge = wrapper.find('[data-testid="status-badge"]')
      expect(statusBadge.exists()).toBe(true)
      expect(statusBadge.text()).toBe('InReview')
    })
  })

  describe('Priority Display', () => {
    it('should display Standard priority', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Standard')
    })

    it('should display Urgent priority with warning style', () => {
      const urgentRequest = { ...mockRequest, priority: 'Urgent' as const }

      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: urgentRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Pilne')
      expect(wrapper.html()).toContain('text-red-600')
    })
  })

  describe('Icon Mapping', () => {
    it('should call getIconifyName with request template icon', () => {
      mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(mockGetIconifyName).toHaveBeenCalledWith('beach-umbrella')
    })
  })

  describe('Approval Timeline', () => {
    it('should render RequestTimeline component with steps', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const timeline = wrapper.find('[data-testid="timeline"]')
      expect(timeline.exists()).toBe(true)
    })
  })

  describe('Form Data Display', () => {
    it('should render RequestFormDataDisplay component', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const formDataDisplay = wrapper.find('[data-testid="form-data-display"]')
      expect(formDataDisplay.exists()).toBe(true)
    })
  })

  describe('Attachments', () => {
    it('should render RequestAttachments component when attachments exist', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const attachments = wrapper.find('[data-testid="attachments"]')
      expect(attachments.exists()).toBe(true)
    })

    it('should not render RequestAttachments when no attachments', () => {
      const requestWithoutAttachments = { ...mockRequest, attachments: [] }

      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: requestWithoutAttachments,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const attachments = wrapper.find('[data-testid="attachments"]')
      expect(attachments.exists()).toBe(false)
    })

    it('should not render RequestAttachments when attachments is undefined', () => {
      const requestWithUndefinedAttachments = { ...mockRequest, attachments: undefined }

      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: requestWithUndefinedAttachments,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const attachments = wrapper.find('[data-testid="attachments"]')
      expect(attachments.exists()).toBe(false)
    })
  })

  describe('Comments', () => {
    it('should render RequestComments component', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const comments = wrapper.find('[data-testid="comments"]')
      expect(comments.exists()).toBe(true)
    })
  })

  describe('Edit History', () => {
    it('should render RequestEditHistory when edit history exists', () => {
      const requestWithEditHistory: RequestWithDetails = {
        ...mockRequest,
        editHistory: [
          {
            id: 'edit-1',
            editedById: 'user-1',
            editedByName: 'Jan Kowalski',
            editedAt: '2024-01-15T11:00:00Z',
            fieldName: 'reason',
            oldValue: 'Old reason',
            newValue: 'New reason'
          }
        ]
      }

      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: requestWithEditHistory,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const editHistory = wrapper.find('[data-testid="edit-history"]')
      expect(editHistory.exists()).toBe(true)
    })

    it('should not render RequestEditHistory when empty', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const editHistory = wrapper.find('[data-testid="edit-history"]')
      expect(editHistory.exists()).toBe(false)
    })

    it('should not render RequestEditHistory when undefined', () => {
      const requestWithoutHistory = { ...mockRequest, editHistory: undefined }

      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: requestWithoutHistory,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const editHistory = wrapper.find('[data-testid="edit-history"]')
      expect(editHistory.exists()).toBe(false)
    })
  })

  describe('Styling', () => {
    it('should have backdrop overlay', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const overlay = wrapper.find('.bg-black.bg-opacity-50')
      expect(overlay.exists()).toBe(true)
    })

    it('should have dark mode classes', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const html = wrapper.html()
      expect(html).toContain('dark:bg-gray-800')
      expect(html).toContain('dark:text-white')
      expect(html).toContain('dark:border-gray-700')
    })

    it('should have proper modal container styling', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const modalContainer = wrapper.find('.rounded-lg.shadow-xl')
      expect(modalContainer.exists()).toBe(true)
    })

    it('should have max width and height constraints', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const modal = wrapper.find('.max-w-6xl.max-h-\\[90vh\\]')
      expect(modal.exists()).toBe(true)
    })
  })

  describe('Header Styling', () => {
    it('should have sticky header', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      const header = wrapper.find('.sticky.top-0')
      expect(header.exists()).toBe(true)
    })
  })

  describe('Section Labels', () => {
    it('should display date label', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Data złożenia')
    })

    it('should display submitter label', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Wnioskodawca')
    })

    it('should display status label', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Status')
    })

    it('should display priority label', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Priorytet')
    })

    it('should display approval history section title', () => {
      const wrapper = mount(RequestDetailsModal, {
        props: {
          request: mockRequest,
          template: mockTemplate
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Historia zatwierdzeń')
    })
  })
})
