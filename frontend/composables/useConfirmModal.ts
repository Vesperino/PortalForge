import { ref } from 'vue'

export type ConfirmModalVariant = 'danger' | 'warning' | 'info' | 'success'

export interface ConfirmModalOptions {
  title: string
  message?: string
  confirmText?: string
  cancelText?: string
  variant?: ConfirmModalVariant
  hideCancel?: boolean
}

interface ConfirmModalState extends ConfirmModalOptions {
  isOpen: boolean
  resolve?: (value: boolean) => void
}

const modalState = ref<ConfirmModalState>({
  isOpen: false,
  title: '',
  message: '',
  confirmText: 'Potwierdź',
  cancelText: 'Anuluj',
  variant: 'info',
  hideCancel: false
})

export function useConfirmModal() {
  const show = (options: ConfirmModalOptions): Promise<boolean> => {
    return new Promise((resolve) => {
      modalState.value = {
        ...options,
        isOpen: true,
        confirmText: options.confirmText || 'Potwierdź',
        cancelText: options.cancelText || 'Anuluj',
        variant: options.variant || 'info',
        hideCancel: options.hideCancel || false,
        resolve
      }
    })
  }

  const confirm = () => {
    if (modalState.value.resolve) {
      modalState.value.resolve(true)
    }
    modalState.value.isOpen = false
  }

  const cancel = () => {
    if (modalState.value.resolve) {
      modalState.value.resolve(false)
    }
    modalState.value.isOpen = false
  }

  const confirmDelete = (itemName: string, customMessage?: string): Promise<boolean> => {
    return show({
      title: 'Potwierdzenie usunięcia',
      message: customMessage || `Czy na pewno chcesz usunąć ${itemName}? Ta operacja jest nieodwracalna.`,
      confirmText: 'Usuń',
      cancelText: 'Anuluj',
      variant: 'danger'
    })
  }

  const confirmAction = (actionName: string, customMessage?: string): Promise<boolean> => {
    return show({
      title: `Potwierdzenie ${actionName}`,
      message: customMessage || `Czy na pewno chcesz wykonać tę operację?`,
      confirmText: 'Tak',
      cancelText: 'Nie',
      variant: 'warning'
    })
  }

  return {
    modalState: readonly(modalState),
    show,
    confirm,
    cancel,
    confirmDelete,
    confirmAction
  }
}
