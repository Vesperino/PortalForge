<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

interface InternalService {
  id: string
  name: string
  description: string
  url: string
  icon: string
  category: 'auth' | 'ticketing' | 'monitoring' | 'documentation' | 'development' | 'communication' | 'other'
  isExternal?: boolean
}

const servicesSearch = ref('')

// Internal services data
const internalServices: InternalService[] = [
  {
    id: 'keycloak',
    name: 'Keycloak',
    description: 'System autoryzacji i zarządzania tożsamością (SSO)',
    url: 'https://auth.company.local',
    icon: '🔐',
    category: 'auth'
  },
  {
    id: 'mantis',
    name: 'Mantis Bug Tracker',
    description: 'System ticketowy do zgłaszania błędów i zadań',
    url: 'https://mantis.company.local',
    icon: '🐛',
    category: 'ticketing'
  },
  {
    id: 'jira',
    name: 'Jira',
    description: 'Zarządzanie projektami i zadaniami zespołowymi',
    url: 'https://jira.company.local',
    icon: '📋',
    category: 'ticketing'
  },
  {
    id: 'gitlab',
    name: 'GitLab',
    description: 'Repozytorium kodu źródłowego i CI/CD',
    url: 'https://gitlab.company.local',
    icon: '🦊',
    category: 'development'
  },
  {
    id: 'jenkins',
    name: 'Jenkins',
    description: 'Automatyzacja buildów i deploymentów',
    url: 'https://jenkins.company.local',
    icon: '⚙️',
    category: 'development'
  },
  {
    id: 'grafana',
    name: 'Grafana',
    description: 'Monitoring i wizualizacja metryk systemowych',
    url: 'https://grafana.company.local',
    icon: '📊',
    category: 'monitoring'
  },
  {
    id: 'kibana',
    name: 'Kibana',
    description: 'Analiza logów i wyszukiwanie w Elasticsearch',
    url: 'https://kibana.company.local',
    icon: '🔍',
    category: 'monitoring'
  },
  {
    id: 'confluence',
    name: 'Confluence',
    description: 'Wiki firmowe i dokumentacja projektów',
    url: 'https://confluence.company.local',
    icon: '📚',
    category: 'documentation'
  },
  {
    id: 'nexus',
    name: 'Nexus Repository',
    description: 'Repozytorium artefaktów i paczek',
    url: 'https://nexus.company.local',
    icon: '📦',
    category: 'development'
  },
  {
    id: 'sonarqube',
    name: 'SonarQube',
    description: 'Analiza jakości kodu i bezpieczeństwa',
    url: 'https://sonar.company.local',
    icon: '🔬',
    category: 'development'
  },
  {
    id: 'mattermost',
    name: 'Mattermost',
    description: 'Komunikator zespołowy',
    url: 'https://chat.company.local',
    icon: '💬',
    category: 'communication'
  },
  {
    id: 'rocketchat',
    name: 'Rocket.Chat',
    description: 'Alternatywny komunikator dla projektów',
    url: 'https://rocket.company.local',
    icon: '🚀',
    category: 'communication'
  },
  {
    id: 'nextcloud',
    name: 'Nextcloud',
    description: 'Chmura firmowa - pliki i współpraca',
    url: 'https://cloud.company.local',
    icon: '☁️',
    category: 'other'
  },
  {
    id: 'portainer',
    name: 'Portainer',
    description: 'Zarządzanie kontenerami Docker',
    url: 'https://portainer.company.local',
    icon: '🐳',
    category: 'development'
  },
  {
    id: 'vault',
    name: 'HashiCorp Vault',
    description: 'Zarządzanie sekretami i certyfikatami',
    url: 'https://vault.company.local',
    icon: '🔒',
    category: 'auth'
  }
]

const filteredServices = computed(() => {
  if (!servicesSearch.value.trim()) return internalServices
  const query = servicesSearch.value.toLowerCase()
  return internalServices.filter(service =>
    service.name.toLowerCase().includes(query) ||
    service.description.toLowerCase().includes(query) ||
    service.category.toLowerCase().includes(query)
  )
})

const getServiceCategoryLabel = (category: InternalService['category']) => {
  switch (category) {
    case 'auth': return 'Autoryzacja'
    case 'ticketing': return 'Ticketing'
    case 'monitoring': return 'Monitoring'
    case 'documentation': return 'Dokumentacja'
    case 'development': return 'Development'
    case 'communication': return 'Komunikacja'
    case 'other': return 'Inne'
  }
}

const getServiceCategoryColor = (category: InternalService['category']) => {
  switch (category) {
    case 'auth': return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
    case 'ticketing': return 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300'
    case 'monitoring': return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
    case 'documentation': return 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-300'
    case 'development': return 'bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-300'
    case 'communication': return 'bg-teal-100 text-teal-800 dark:bg-teal-900/30 dark:text-teal-300'
    case 'other': return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Serwisy wewnętrzne</h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Szybki dostęp do wszystkich narzędzi i systemów firmowych
        </p>
      </div>
      <div class="text-sm text-gray-500 dark:text-gray-400">
        Dostępnych serwisów: <span class="font-bold text-gray-900 dark:text-white">{{ internalServices.length }}</span>
      </div>
    </div>

    <!-- Search -->
    <div class="rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
      <div class="relative">
        <svg class="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
        <input
          v-model="servicesSearch"
          type="text"
          placeholder="Szukaj serwisu po nazwie, opisie lub kategorii..."
          class="w-full pl-12 pr-4 py-3 rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-base text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
        >
      </div>
    </div>

    <!-- Services Grid -->
    <div v-if="filteredServices.length === 0" class="rounded-xl border-2 border-dashed border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50 p-12 text-center">
      <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
      </svg>
      <h3 class="mt-4 text-lg font-semibold text-gray-900 dark:text-white">Brak serwisów</h3>
      <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
        Nie znaleziono serwisów spełniających kryteria wyszukiwania.
      </p>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <a
        v-for="service in filteredServices"
        :key="service.id"
        :href="service.url"
        target="_blank"
        rel="noopener noreferrer"
        class="group rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6 transition-all hover:border-blue-500 hover:shadow-xl hover:-translate-y-1"
      >
        <div class="flex items-start gap-4">
          <!-- Icon -->
          <div class="flex-shrink-0 w-14 h-14 rounded-xl bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-900/30 dark:to-blue-800/30 flex items-center justify-center text-3xl group-hover:scale-110 transition-transform">
            {{ service.icon }}
          </div>

          <!-- Content -->
          <div class="flex-1 min-w-0">
            <div class="flex items-start justify-between gap-2 mb-2">
              <h3 class="text-lg font-bold text-gray-900 dark:text-white group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
                {{ service.name }}
              </h3>
              <svg class="flex-shrink-0 h-5 w-5 text-gray-400 group-hover:text-blue-500 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
            </div>

            <p class="text-sm text-gray-600 dark:text-gray-400 mb-3 line-clamp-2">
              {{ service.description }}
            </p>

            <div class="flex items-center gap-2">
              <span :class="getServiceCategoryColor(service.category)" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold">
                {{ getServiceCategoryLabel(service.category) }}
              </span>
              <span v-if="service.isExternal" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300">
                🌐 Zewnętrzny
              </span>
            </div>

            <div class="mt-3 text-xs text-gray-500 dark:text-gray-400 truncate">
              {{ service.url }}
            </div>
          </div>
        </div>
      </a>
    </div>
  </div>
</template>

