import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import FormBuilder from '~/components/admin/FormBuilder.vue'
import type { RequestTemplateField } from '~/types/requests'

// Mock vuedraggable
vi.mock('vuedraggable', () => ({
  default: {
    name: 'draggable',
    props: ['modelValue', 'itemKey'],
    emits: ['update:modelValue', 'end'],
    template: `
      <div class="draggable-container">
        <div 
          v-for="(item, index) in modelValue" 
          :key="item[itemKey]"
          class="draggable-item"
          :data-testid="'field-' + item.id"
        >
          <slot name="item" :element="item" :index="index" />
        </div>
      </div>
    `
  }
}))

describe('FormBuilder', () => {
  const mockFields: RequestTemplateField[] = [
    {
      id: 'field1',
      label: 'Test Field 1',
      fieldType: 'Text',
      placeholder: 'Enter text',
      isRequired: true,
      order: 0,
      helpText: 'Help text',
      validationRules: '{}',
      conditionalLogic: '{}',
      isConditional: false,
      defaultValue: '',
      autoCompleteSource: ''
    },
    {
      id: 'field2',
      label: 'Test Field 2',
      fieldType: 'Select',
      placeholder: 'Select option',
      isRequired: false,
      order: 1,
      helpText: '',
      validationRules: '{}',
      conditionalLogic: '{}',
      isConditional: false,
      defaultValue: '',
      autoCompleteSource: ''
    }
  ]

  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('Drag and Drop Functionality', () => {
    it('should render draggable field palette', () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: [] }
      })

      const fieldTypes = [
        'Tekst', 'Obszar tekstu', 'Liczba', 'Lista wyboru', 
        'Wielokrotny wybór', 'Data', 'Zakres dat', 'Pole wyboru',
        'Plik', 'Ocena', 'Podpis', 'Wybór użytkownika', 'Wybór działu'
      ]

      fieldTypes.forEach(fieldType => {
        expect(wrapper.text()).toContain(fieldType)
      })
    })

    it('should add new field when palette item is clicked', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: [] }
      })

      const buttons = wrapper.findAll('button')
      const textFieldButton = buttons.find(btn => btn.text().includes('Tekst'))
      await textFieldButton?.trigger('click')

      expect(wrapper.emitted('update:fields')).toBeTruthy()
      const emittedFields = wrapper.emitted('update:fields')?.[0]?.[0] as RequestTemplateField[]
      expect(emittedFields).toHaveLength(1)
      expect(emittedFields[0].fieldType).toBe('Text')
      expect(emittedFields[0].label).toBe('Nowe pole Text')
    })

    it('should render existing fields in draggable container', () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      expect(wrapper.find('[data-testid="field-field1"]').exists()).toBe(true)
      expect(wrapper.find('[data-testid="field-field2"]').exists()).toBe(true)
      expect(wrapper.text()).toContain('Test Field 1')
      expect(wrapper.text()).toContain('Test Field 2')
    })

    it('should show drag handle on field hover', () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      const fieldElement = wrapper.find('[data-testid="field-field1"]')
      expect(fieldElement.find('.cursor-grab').exists()).toBe(true)
    })

    it('should emit field selection when field is clicked', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      const fieldElement = wrapper.find('[data-testid="field-field1"]')
      await fieldElement.trigger('click')

      expect(wrapper.emitted('field-selected')).toBeTruthy()
      expect(wrapper.emitted('field-selected')?.[0]?.[0]).toEqual(mockFields[0])
    })

    it('should update field orders after drag end', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      // Simulate drag end event
      const draggableContainer = wrapper.find('.draggable-container')
      await draggableContainer.vm.$emit('end')

      expect(wrapper.emitted('update:fields')).toBeTruthy()
      const emittedFields = wrapper.emitted('update:fields')?.[0]?.[0] as RequestTemplateField[]
      emittedFields.forEach((field, index) => {
        expect(field.order).toBe(index)
      })
    })

    it('should remove field when delete button is clicked', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      const deleteButton = wrapper.find('[title="Usuń pole"]')
      await deleteButton.trigger('click')

      expect(wrapper.emitted('update:fields')).toBeTruthy()
      const emittedFields = wrapper.emitted('update:fields')?.[0]?.[0] as RequestTemplateField[]
      expect(emittedFields).toHaveLength(1)
    })

    it('should duplicate field when duplicate button is clicked', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: [mockFields[0]] }
      })

      const duplicateButton = wrapper.find('[title="Duplikuj pole"]')
      await duplicateButton.trigger('click')

      expect(wrapper.emitted('update:fields')).toBeTruthy()
      const emittedFields = wrapper.emitted('update:fields')?.[0]?.[0] as RequestTemplateField[]
      expect(emittedFields).toHaveLength(2)
      expect(emittedFields[1].label).toBe('Test Field 1 (kopia)')
    })

    it('should highlight selected field', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      const fieldElement = wrapper.find('[data-testid="field-field1"]')
      await fieldElement.trigger('click')

      expect(fieldElement.classes()).toContain('border-blue-500')
    })

    it('should show empty state when no fields exist', () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: [] }
      })

      expect(wrapper.text()).toContain('Rozpocznij tworzenie formularza')
      expect(wrapper.text()).toContain('Przeciągnij komponenty z palety')
    })
  })

  describe('Field Type Support', () => {
    it('should support all field types in palette', () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: [] }
      })

      const expectedFieldLabels = [
        'Tekst', 'Obszar tekstu', 'Liczba', 'Lista wyboru', 'Wielokrotny wybór',
        'Data', 'Zakres dat', 'Pole wyboru', 'Plik', 'Ocena',
        'Podpis', 'Wybór użytkownika', 'Wybór działu'
      ]

      expectedFieldLabels.forEach(fieldLabel => {
        expect(wrapper.text()).toContain(fieldLabel)
      })
    })

    it('should create field with correct default properties for FileUpload', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: [] }
      })

      const fileUploadButton = wrapper.find('button:has-text("Plik")')
      await fileUploadButton.trigger('click')

      const emittedFields = wrapper.emitted('update:fields')?.[0]?.[0] as RequestTemplateField[]
      const fileField = emittedFields[0]
      
      expect(fileField.fieldType).toBe('FileUpload')
      expect(fileField.fileMaxSize).toBe(10)
      expect(fileField.allowedFileTypes).toBe('["pdf","doc","docx","jpg","png"]')
    })

    it('should create field with correct default properties for other types', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: [] }
      })

      const textButton = wrapper.find('button:has-text("Tekst")')
      await textButton.trigger('click')

      const emittedFields = wrapper.emitted('update:fields')?.[0]?.[0] as RequestTemplateField[]
      const textField = emittedFields[0]
      
      expect(textField.fieldType).toBe('Text')
      expect(textField.fileMaxSize).toBeUndefined()
      expect(textField.allowedFileTypes).toBeUndefined()
    })
  })

  describe('Preview Mode', () => {
    it('should toggle preview mode when preview button is clicked', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      const previewButton = wrapper.find('button:has-text("Podgląd")')
      await previewButton.trigger('click')

      expect(previewButton.text()).toContain('Edycja')
    })

    it('should show FormPreview component in preview mode', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      const previewButton = wrapper.find('button:has-text("Podgląd")')
      await previewButton.trigger('click')

      // In preview mode, the draggable fields should be hidden
      expect(wrapper.find('.draggable-container').exists()).toBe(false)
    })
  })

  describe('Field Management', () => {
    it('should clear field selection when field is removed', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      // Select a field first
      const fieldElement = wrapper.find('[data-testid="field-field1"]')
      await fieldElement.trigger('click')

      // Remove the selected field
      const deleteButton = wrapper.find('[title="Usuń pole"]')
      await deleteButton.trigger('click')

      expect(wrapper.emitted('field-selected')).toHaveLength(2)
      expect(wrapper.emitted('field-selected')?.[1]?.[0]).toBeNull()
    })

    it('should maintain field order consistency', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      // Add a new field
      const textButton = wrapper.find('button:has-text("Tekst")')
      await textButton.trigger('click')

      const emittedFields = wrapper.emitted('update:fields')?.[0]?.[0] as RequestTemplateField[]
      expect(emittedFields[2].order).toBe(2)
    })

    it('should show field icons and descriptions correctly', () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: [] }
      })

      // Check that field types have proper icons and descriptions
      expect(wrapper.text()).toContain('📝')
      expect(wrapper.text()).toContain('Pojedyncza linia tekstu')
      expect(wrapper.text()).toContain('📄')
      expect(wrapper.text()).toContain('Wieloliniowy tekst')
    })
  })

  describe('Accessibility', () => {
    it('should have proper ARIA labels and roles', () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      const buttons = wrapper.findAll('button')
      buttons.forEach(button => {
        expect(button.attributes('title') || button.text()).toBeTruthy()
      })
    })

    it('should support keyboard navigation', async () => {
      const wrapper = mount(FormBuilder, {
        props: { fields: mockFields }
      })

      const fieldElement = wrapper.find('[data-testid="field-field1"]')
      await fieldElement.trigger('keydown.enter')

      expect(wrapper.emitted('field-selected')).toBeTruthy()
    })
  })
})