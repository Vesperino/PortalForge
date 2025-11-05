import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import DynamicFormRenderer from '~/components/requests/DynamicFormRenderer.vue'
import type { RequestTemplate, RequestTemplateField } from '~/types/requests'

describe('DynamicFormRenderer', () => {
  const mockTemplate: RequestTemplate = {
    id: 'template1',
    name: 'Test Template',
    description: 'Test Description',
    category: 'Test',
    icon: '📝',
    requiresApproval: true,
    estimatedProcessingDays: 3,
    isActive: true,
    fields: [
      {
        id: 'field1',
        label: 'Text Field',
        fieldType: 'Text',
        placeholder: 'Enter text',
        isRequired: true,
        order: 0,
        helpText: 'This is help text',
        validationRules: '{"MinLength": {"value": "3", "message": "Minimum 3 characters"}}',
        conditionalLogic: '{}',
        isConditional: false,
        defaultValue: '',
        autoCompleteSource: '/api/autocomplete'
      },
      {
        id: 'field2',
        label: 'Conditional Field',
        fieldType: 'Textarea',
        placeholder: 'Enter details',
        isRequired: false,
        order: 1,
        helpText: '',
        validationRules: '{}',
        conditionalLogic: '{"dependsOn": "field1", "condition": "not_empty", "value": ""}',
        isConditional: true,
        defaultValue: '',
        autoCompleteSource: ''
      },
      {
        id: 'field3',
        label: 'Select Field',
        fieldType: 'Select',
        placeholder: 'Choose option',
        isRequired: true,
        order: 2,
        helpText: '',
        validationRules: '{}',
        conditionalLogic: '{}',
        isConditional: false,
        defaultValue: '',
        autoCompleteSource: '',
        options: '[{"value": "option1", "label": "Option 1"}, {"value": "option2", "label": "Option 2"}]'
      },
      {
        id: 'field4',
        label: 'File Upload',
        fieldType: 'FileUpload',
        placeholder: '',
        isRequired: false,
        order: 3,
        helpText: '',
        validationRules: '{}',
        conditionalLogic: '{}',
        isConditional: false,
        defaultValue: '',
        autoCompleteSource: '',
        fileMaxSize: 5,
        allowedFileTypes: '["pdf", "jpg", "png"]'
      }
    ]
  }

  beforeEach(() => {
    vi.clearAllMocks()
    // Mock URL.createObjectURL
    global.URL.createObjectURL = vi.fn(() => 'mock-url')
  })

  describe('Conditional Field Rendering', () => {
    it('should initially hide conditional fields when dependency is empty', () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      // Text field should be visible
      expect(wrapper.find('input[placeholder="Enter text"]').exists()).toBe(true)
      
      // Conditional field should be hidden initially
      expect(wrapper.find('textarea[placeholder="Enter details"]').exists()).toBe(false)
    })

    it('should show conditional field when dependency condition is met', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      // Fill the dependency field
      const textInput = wrapper.find('input[placeholder="Enter text"]')
      await textInput.setValue('test value')

      // Conditional field should now be visible
      expect(wrapper.find('textarea[placeholder="Enter details"]').exists()).toBe(true)
    })

    it('should hide conditional field when dependency condition is not met', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      
      // Fill and then clear the dependency field
      await textInput.setValue('test value')
      await textInput.setValue('')

      // Conditional field should be hidden again
      expect(wrapper.find('textarea[placeholder="Enter details"]').exists()).toBe(false)
    })

    it('should handle equals condition correctly', async () => {
      const conditionalTemplate = {
        ...mockTemplate,
        fields: [
          mockTemplate.fields[0],
          {
            ...mockTemplate.fields[1],
            conditionalLogic: '{"dependsOn": "field1", "condition": "equals", "value": "show"}'
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: conditionalTemplate }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      
      // Field should be hidden when value doesn't match
      await textInput.setValue('hide')
      expect(wrapper.find('textarea[placeholder="Enter details"]').exists()).toBe(false)

      // Field should be visible when value matches
      await textInput.setValue('show')
      expect(wrapper.find('textarea[placeholder="Enter details"]').exists()).toBe(true)
    })

    it('should handle not_equals condition correctly', async () => {
      const conditionalTemplate = {
        ...mockTemplate,
        fields: [
          mockTemplate.fields[0],
          {
            ...mockTemplate.fields[1],
            conditionalLogic: '{"dependsOn": "field1", "condition": "not_equals", "value": "hide"}'
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: conditionalTemplate }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      
      // Field should be hidden when value matches
      await textInput.setValue('hide')
      expect(wrapper.find('textarea[placeholder="Enter details"]').exists()).toBe(false)

      // Field should be visible when value doesn't match
      await textInput.setValue('show')
      expect(wrapper.find('textarea[placeholder="Enter details"]').exists()).toBe(true)
    })

    it('should handle contains condition for multi-select fields', async () => {
      const multiSelectTemplate = {
        ...mockTemplate,
        fields: [
          {
            ...mockTemplate.fields[0],
            fieldType: 'MultiSelect' as const,
            options: '[{"value": "option1", "label": "Option 1"}, {"value": "option2", "label": "Option 2"}]'
          },
          {
            ...mockTemplate.fields[1],
            conditionalLogic: '{"dependsOn": "field1", "condition": "contains", "value": "option1"}'
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: multiSelectTemplate }
      })

      // Select option1
      const checkbox = wrapper.find('input[value="option1"]')
      await checkbox.setChecked(true)

      // Conditional field should be visible
      expect(wrapper.find('textarea[placeholder="Enter details"]').exists()).toBe(true)
    })

    it('should skip validation for hidden conditional fields', async () => {
      const requiredConditionalTemplate = {
        ...mockTemplate,
        fields: [
          mockTemplate.fields[0],
          {
            ...mockTemplate.fields[1],
            isRequired: true,
            conditionalLogic: '{"dependsOn": "field1", "condition": "equals", "value": "show"}'
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: requiredConditionalTemplate }
      })

      // Conditional field is hidden and required, but validation should pass
      expect(wrapper.emitted('validation-change')).toBeTruthy()
      
      // Fill the visible required field
      const textInput = wrapper.find('input[placeholder="Enter text"]')
      await textInput.setValue('test')

      // Should emit validation change
      const validationEvents = wrapper.emitted('validation-change') as boolean[][]
      expect(validationEvents[validationEvents.length - 1][0]).toBe(true)
    })
  })

  describe('Form Validation', () => {
    it('should validate required fields', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      // Initially should be invalid due to required fields
      expect(wrapper.emitted('validation-change')).toBeTruthy()
      const initialValidation = wrapper.emitted('validation-change')?.[0]?.[0]
      expect(initialValidation).toBe(false)

      // Fill required fields
      const textInput = wrapper.find('input[placeholder="Enter text"]')
      await textInput.setValue('test')

      const selectInput = wrapper.find('select')
      await selectInput.setValue('option1')

      // Should now be valid
      const validationEvents = wrapper.emitted('validation-change') as boolean[][]
      expect(validationEvents[validationEvents.length - 1][0]).toBe(true)
    })

    it('should validate minimum length rule', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      
      // Enter text shorter than minimum
      await textInput.setValue('ab')

      // Should show validation error
      expect(wrapper.text()).toContain('Minimum 3 characters')
      
      // Enter valid text
      await textInput.setValue('abc')

      // Error should be gone
      expect(wrapper.text()).not.toContain('Minimum 3 characters')
    })

    it('should validate regex patterns', async () => {
      const regexTemplate = {
        ...mockTemplate,
        fields: [
          {
            ...mockTemplate.fields[0],
            validationRules: '{"Regex": {"value": "^[A-Z]+$", "message": "Only uppercase letters allowed"}}'
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: regexTemplate }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      
      // Enter invalid text
      await textInput.setValue('abc')
      expect(wrapper.text()).toContain('Only uppercase letters allowed')
      
      // Enter valid text
      await textInput.setValue('ABC')
      expect(wrapper.text()).not.toContain('Only uppercase letters allowed')
    })

    it('should validate number ranges', async () => {
      const numberTemplate = {
        ...mockTemplate,
        fields: [
          {
            ...mockTemplate.fields[0],
            fieldType: 'Number' as const,
            validationRules: '{"Range": {"value": "1-10", "message": "Value must be between 1 and 10"}}'
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: numberTemplate }
      })

      const numberInput = wrapper.find('input[type="number"]')
      
      // Enter invalid number
      await numberInput.setValue('15')
      expect(wrapper.text()).toContain('Value must be between 1 and 10')
      
      // Enter valid number
      await numberInput.setValue('5')
      expect(wrapper.text()).not.toContain('Value must be between 1 and 10')
    })
  })

  describe('File Upload Functionality', () => {
    it('should validate file size', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      const fileInput = wrapper.find('input[type="file"]')
      
      // Create a mock file that exceeds size limit (5MB limit, create 6MB file)
      const largeFile = new File(['x'.repeat(6 * 1024 * 1024)], 'large.pdf', { type: 'application/pdf' })
      
      Object.defineProperty(fileInput.element, 'files', {
        value: [largeFile],
        writable: false
      })

      await fileInput.trigger('change')

      expect(wrapper.text()).toContain('Plik jest za duży. Maksymalny rozmiar: 5MB')
    })

    it('should validate file types', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      const fileInput = wrapper.find('input[type="file"]')
      
      // Create a file with disallowed extension
      const invalidFile = new File(['content'], 'test.txt', { type: 'text/plain' })
      
      Object.defineProperty(fileInput.element, 'files', {
        value: [invalidFile],
        writable: false
      })

      await fileInput.trigger('change')

      expect(wrapper.text()).toContain('Niedozwolony typ pliku. Dozwolone: pdf, jpg, png')
    })

    it('should show upload progress', async () => {
      vi.useFakeTimers()
      
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      const fileInput = wrapper.find('input[type="file"]')
      
      // Create a valid file
      const validFile = new File(['content'], 'test.pdf', { type: 'application/pdf' })
      
      Object.defineProperty(fileInput.element, 'files', {
        value: [validFile],
        writable: false
      })

      await fileInput.trigger('change')

      // Should show progress
      expect(wrapper.text()).toContain('Przesyłanie...')
      
      // Fast forward time to complete upload
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Should show uploaded file
      expect(wrapper.text()).toContain('test.pdf')
      
      vi.useRealTimers()
    })

    it('should allow file removal', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      // Simulate uploaded file
      await wrapper.setData({
        formData: {
          field4: {
            name: 'test.pdf',
            size: 1024,
            type: 'application/pdf',
            url: 'mock-url'
          }
        }
      })

      const removeButton = wrapper.find('button[title="Remove file"]')
      await removeButton.trigger('click')

      expect(wrapper.vm.formData.field4).toBeNull()
    })
  })

  describe('Auto-complete Functionality', () => {
    it('should trigger autocomplete search on input', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      await textInput.setValue('te')

      // Should show autocomplete options
      expect(wrapper.text()).toContain('te - Opcja 1')
      expect(wrapper.text()).toContain('te - Opcja 2')
    })

    it('should select autocomplete option', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      await textInput.setValue('te')

      const option = wrapper.find('button:has-text("te - Opcja 1")')
      await option.trigger('click')

      expect(textInput.element.value).toBe('option1')
    })

    it('should not show autocomplete for short queries', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      await textInput.setValue('t')

      // Should not show autocomplete options for single character
      expect(wrapper.text()).not.toContain('t - Opcja 1')
    })
  })

  describe('Field Type Rendering', () => {
    it('should render rating field correctly', () => {
      const ratingTemplate = {
        ...mockTemplate,
        fields: [
          {
            ...mockTemplate.fields[0],
            fieldType: 'Rating' as const
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: ratingTemplate }
      })

      // Should show 5 stars
      const stars = wrapper.findAll('button svg')
      expect(stars.length).toBe(5)
    })

    it('should handle rating selection', async () => {
      const ratingTemplate = {
        ...mockTemplate,
        fields: [
          {
            ...mockTemplate.fields[0],
            fieldType: 'Rating' as const
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: ratingTemplate }
      })

      const thirdStar = wrapper.findAll('button')[2]
      await thirdStar.trigger('click')

      expect(wrapper.text()).toContain('3/5')
    })

    it('should render date range field correctly', () => {
      const dateRangeTemplate = {
        ...mockTemplate,
        fields: [
          {
            ...mockTemplate.fields[0],
            fieldType: 'DateRange' as const
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: dateRangeTemplate }
      })

      const dateInputs = wrapper.findAll('input[type="date"]')
      expect(dateInputs.length).toBe(2)
      expect(wrapper.text()).toContain('Data od')
      expect(wrapper.text()).toContain('Data do')
    })

    it('should render signature field correctly', () => {
      const signatureTemplate = {
        ...mockTemplate,
        fields: [
          {
            ...mockTemplate.fields[0],
            fieldType: 'Signature' as const
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: signatureTemplate }
      })

      expect(wrapper.text()).toContain('Kliknij, aby dodać podpis')
    })

    it('should handle signature addition', async () => {
      const signatureTemplate = {
        ...mockTemplate,
        fields: [
          {
            ...mockTemplate.fields[0],
            fieldType: 'Signature' as const
          }
        ]
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { template: signatureTemplate }
      })

      const signatureArea = wrapper.find('.cursor-pointer')
      await signatureArea.trigger('click')

      expect(wrapper.text()).toContain('Podpis dodany')
    })
  })

  describe('Form Data Management', () => {
    it('should initialize form data with default values', () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      expect(wrapper.emitted('update:formData')).toBeTruthy()
      const formData = wrapper.emitted('update:formData')?.[0]?.[0] as Record<string, any>
      expect(formData.field1).toBe('')
      expect(formData.field2).toBe('')
      expect(formData.field3).toBe('')
    })

    it('should use initial data when provided', () => {
      const initialData = {
        field1: 'Initial value',
        field3: 'option2'
      }

      const wrapper = mount(DynamicFormRenderer, {
        props: { 
          template: mockTemplate,
          initialData
        }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      expect(textInput.element.value).toBe('Initial value')

      const selectInput = wrapper.find('select')
      expect(selectInput.element.value).toBe('option2')
    })

    it('should emit form data changes', async () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { template: mockTemplate }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      await textInput.setValue('new value')

      expect(wrapper.emitted('update:formData')).toBeTruthy()
      const formDataEvents = wrapper.emitted('update:formData') as Record<string, any>[][]
      const lastFormData = formDataEvents[formDataEvents.length - 1][0]
      expect(lastFormData.field1).toBe('new value')
    })
  })

  describe('Readonly Mode', () => {
    it('should disable all inputs in readonly mode', () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { 
          template: mockTemplate,
          readonly: true
        }
      })

      const textInput = wrapper.find('input[placeholder="Enter text"]')
      expect(textInput.attributes('readonly')).toBeDefined()

      const selectInput = wrapper.find('select')
      expect(selectInput.attributes('disabled')).toBeDefined()
    })

    it('should not show file upload controls in readonly mode', () => {
      const wrapper = mount(DynamicFormRenderer, {
        props: { 
          template: mockTemplate,
          readonly: true
        }
      })

      expect(wrapper.find('input[type="file"]').exists()).toBe(false)
      expect(wrapper.text()).not.toContain('Wybierz plik')
    })
  })
})