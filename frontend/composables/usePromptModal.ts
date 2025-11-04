import { ref } from 'vue'

export interface PromptModalOptions {
  title: string
  message?: string
  placeholder?: string
  defaultValue?: string
}

interface PromptModalState extends PromptModalOptions {
  isOpen: boolean
  resolve?: (value: string | null) => void
}

const modalState = ref<PromptModalState>({
  isOpen: false,
  title: '',
  message: '',
  placeholder: '',
  defaultValue: ''
})

export function usePromptModal() {
  const show = (options: PromptModalOptions): Promise<string | null> => {
    return new Promise((resolve) => {
      modalState.value = {
        ...options,
        isOpen: true,
        resolve
      }
    })
  }

  const confirm = (value: string) => {
    if (modalState.value.resolve) {
      modalState.value.resolve(value || null)
    }
    modalState.value.isOpen = false
  }

  const cancel = () => {
    if (modalState.value.resolve) {
      modalState.value.resolve(null)
    }
    modalState.value.isOpen = false
  }

  return {
    modalState: readonly(modalState),
    show,
    confirm,
    cancel
  }
}
