import { ref } from 'vue'

export type ToastType = 'success' | 'error' | 'warning' | 'info'

export interface Toast {
  id: string
  type: ToastType
  title: string
  message?: string
  duration?: number
}

const toasts = ref<Toast[]>([])

export function useNotificationToast() {
  const show = (toast: Omit<Toast, 'id'>) => {
    const id = `toast-${Date.now()}-${Math.random()}`
    const duration = toast.duration || 5000
    
    const newToast: Toast = {
      ...toast,
      id
    }
    
    toasts.value.push(newToast)
    
    // Auto remove after duration
    if (duration > 0) {
      setTimeout(() => {
        remove(id)
      }, duration)
    }
    
    return id
  }
  
  const remove = (id: string) => {
    const index = toasts.value.findIndex(t => t.id === id)
    if (index > -1) {
      toasts.value.splice(index, 1)
    }
  }
  
  const success = (title: string, message?: string, duration?: number) => {
    return show({ type: 'success', title, message, duration })
  }
  
  const error = (title: string, message?: string, duration?: number) => {
    return show({ type: 'error', title, message, duration })
  }
  
  const warning = (title: string, message?: string, duration?: number) => {
    return show({ type: 'warning', title, message, duration })
  }
  
  const info = (title: string, message?: string, duration?: number) => {
    return show({ type: 'info', title, message, duration })
  }
  
  return {
    toasts: readonly(toasts),
    show,
    remove,
    success,
    error,
    warning,
    info
  }
}

