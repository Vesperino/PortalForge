<template>
  <div class="relative">
    <!-- Bell Icon Button -->
    <button
      @click="toggleDropdown"
      class="relative p-2 text-gray-600 hover:text-gray-900 dark:text-gray-300 dark:hover:text-white transition-colors"
      :class="{ 'text-blue-600 dark:text-blue-400': isOpen }"
    >
      <Icon name="heroicons:bell" class="w-6 h-6" />
      
      <!-- Unread Badge -->
      <span
        v-if="unreadCount > 0"
        class="absolute top-0 right-0 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white transform translate-x-1/2 -translate-y-1/2 bg-red-600 rounded-full"
      >
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
    </button>

    <!-- Dropdown -->
    <Transition
      enter-active-class="transition ease-out duration-200"
      enter-from-class="opacity-0 scale-95"
      enter-to-class="opacity-100 scale-100"
      leave-active-class="transition ease-in duration-150"
      leave-from-class="opacity-100 scale-100"
      leave-to-class="opacity-0 scale-95"
    >
      <div
        v-if="isOpen"
        v-click-outside="closeDropdown"
        class="absolute right-0 mt-2 w-96 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 z-50"
      >
        <!-- Header -->
        <div class="flex items-center justify-between px-4 py-3 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
            Powiadomienia
          </h3>
          <button
            v-if="unreadCount > 0"
            @click="handleMarkAllAsRead"
            class="text-sm text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
          >
            Oznacz wszystkie jako przeczytane
          </button>
        </div>

        <!-- Notifications List -->
        <div class="max-h-96 overflow-y-auto">
          <div v-if="loading" class="p-8 text-center">
            <Icon name="svg-spinners:ring-resize" class="w-8 h-8 mx-auto text-blue-600" />
            <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">Ładowanie...</p>
          </div>

          <div v-else-if="notifications.length === 0" class="p-8 text-center">
            <Icon name="heroicons:bell-slash" class="w-12 h-12 mx-auto text-gray-400" />
            <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
              Brak powiadomień
            </p>
          </div>

          <div v-else>
            <div
              v-for="notification in notifications"
              :key="notification.id"
              @click="handleNotificationClick(notification)"
              class="px-4 py-3 border-b border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer transition-colors"
              :class="{ 'bg-blue-50 dark:bg-blue-900/20': !notification.isRead }"
            >
              <div class="flex items-start gap-3">
                <!-- Icon based on type -->
                <div class="flex-shrink-0 mt-1">
                  <Icon
                    :name="getNotificationIcon(notification.type)"
                    class="w-5 h-5"
                    :class="getNotificationIconColor(notification.type)"
                  />
                </div>

                <!-- Content -->
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-gray-900 dark:text-white">
                    {{ notification.title }}
                  </p>
                  <p class="text-sm text-gray-600 dark:text-gray-300 mt-1">
                    {{ notification.message }}
                  </p>
                  <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                    {{ formatDate(notification.createdAt) }}
                  </p>
                </div>

                <!-- Unread indicator -->
                <div v-if="!notification.isRead" class="flex-shrink-0">
                  <div class="w-2 h-2 bg-blue-600 rounded-full"></div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-4 py-3 border-t border-gray-200 dark:border-gray-700">
          <NuxtLink
            to="/dashboard/notifications"
            class="block text-center text-sm text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
            @click="closeDropdown"
          >
            Zobacz wszystkie powiadomienia
          </NuxtLink>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { storeToRefs } from 'pinia'
import type { Notification, NotificationType } from '~/types/requests'

const notificationsStore = useNotificationsStore()
const { notifications, unreadCount, loading } = storeToRefs(notificationsStore)

const isOpen = ref(false)
let pollingInterval: NodeJS.Timeout | null = null

const toggleDropdown = async () => {
  isOpen.value = !isOpen.value
  if (isOpen.value) {
    await notificationsStore.fetchNotifications(false)
  }
}

const closeDropdown = () => {
  isOpen.value = false
}

const handleNotificationClick = async (notification: Notification) => {
  if (!notification.isRead) {
    await notificationsStore.markAsRead(notification.id)
  }
  
  if (notification.actionUrl) {
    await navigateTo(notification.actionUrl)
    closeDropdown()
  }
}

const handleMarkAllAsRead = async () => {
  await notificationsStore.markAllAsRead()
}

const getNotificationIcon = (type: NotificationType) => {
  const icons: Record<NotificationType, string> = {
    RequestPendingApproval: 'heroicons:clock',
    RequestApproved: 'heroicons:check-circle',
    RequestRejected: 'heroicons:x-circle',
    RequestCompleted: 'heroicons:check-badge',
    RequestCommented: 'heroicons:chat-bubble-left-right',
    System: 'heroicons:cog-6-tooth',
    Announcement: 'heroicons:megaphone'
  }
  return icons[type] || 'heroicons:bell'
}

const getNotificationIconColor = (type: NotificationType) => {
  const colors: Record<NotificationType, string> = {
    RequestPendingApproval: 'text-yellow-600',
    RequestApproved: 'text-green-600',
    RequestRejected: 'text-red-600',
    RequestCompleted: 'text-blue-600',
    RequestCommented: 'text-purple-600',
    System: 'text-gray-600',
    Announcement: 'text-indigo-600'
  }
  return colors[type] || 'text-gray-600'
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 1) return 'Teraz'
  if (diffMins < 60) return `${diffMins} min temu`
  if (diffHours < 24) return `${diffHours} godz. temu`
  if (diffDays < 7) return `${diffDays} dni temu`
  
  return date.toLocaleDateString('pl-PL', {
    day: 'numeric',
    month: 'short',
    year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
  })
}

// Start polling on mount
onMounted(() => {
  notificationsStore.fetchUnreadCount()
  pollingInterval = notificationsStore.startPolling(30000) // Poll every 30 seconds
})

// Stop polling on unmount
onUnmounted(() => {
  if (pollingInterval) {
    clearInterval(pollingInterval)
  }
})

// Click outside directive
interface HTMLElementWithClickOutside extends HTMLElement {
  clickOutsideEvent?: (event: Event) => void
}

const vClickOutside = {
  mounted(el: HTMLElementWithClickOutside, binding: any) {
    el.clickOutsideEvent = (event: Event) => {
      if (!(el === event.target || el.contains(event.target as Node))) {
        binding.value()
      }
    }
    document.addEventListener('click', el.clickOutsideEvent)
  },
  unmounted(el: HTMLElementWithClickOutside) {
    if (el.clickOutsideEvent) {
      document.removeEventListener('click', el.clickOutsideEvent)
    }
  }
}
</script>

