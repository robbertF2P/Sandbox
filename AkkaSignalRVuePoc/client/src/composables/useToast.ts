import { ref } from 'vue'

export type ToastVariant = 'info' | 'success'

export interface Toast {
  id: number
  message: string
  variant: ToastVariant
}

const toasts = ref<Toast[]>([])
let nextId = 0

export function useToast() {
  function showToast(message: string, variant: ToastVariant = 'info') {
    const id = ++nextId
    toasts.value = [...toasts.value, { id, message, variant }]
    window.setTimeout(() => {
      toasts.value = toasts.value.filter((toast) => toast.id !== id)
    }, 4500)
  }

  function dismissToast(id: number) {
    toasts.value = toasts.value.filter((toast) => toast.id !== id)
  }

  return { toasts, showToast, dismissToast }
}
