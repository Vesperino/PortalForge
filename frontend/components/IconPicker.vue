<template>
  <div class="icon-picker">
    <div class="mb-3">
      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Wybierz ikone
      </label>
      <input
        v-model="searchQuery"
        type="text"
        placeholder="Szukaj ikony... (np. urlop, sprzet, dokument)"
        class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
      >
    </div>

    <div class="mb-3 flex gap-2 overflow-x-auto pb-2">
      <button
        v-for="category in categories"
        :key="category.id"
        type="button"
        @click="selectedCategory = category.id"
        :class="[
          'px-3 py-1.5 rounded-lg text-sm font-medium whitespace-nowrap transition-colors',
          selectedCategory === category.id
            ? 'bg-blue-600 text-white'
            : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-300 dark:hover:bg-gray-600'
        ]"
      >
        {{ category.label }}
      </button>
    </div>

    <div class="icon-grid grid grid-cols-6 sm:grid-cols-8 md:grid-cols-10 gap-2 max-h-96 overflow-y-auto p-2 border border-gray-200 dark:border-gray-700 rounded-lg bg-gray-50 dark:bg-gray-800">
      <button
        v-for="icon in filteredIcons"
        :key="icon.name"
        type="button"
        @click="selectIcon(icon.name)"
        :class="[
          'flex flex-col items-center justify-center p-3 rounded-lg transition-all hover:bg-blue-50 dark:hover:bg-blue-900',
          selectedIcon === icon.name ? 'bg-blue-100 dark:bg-blue-800 ring-2 ring-blue-500' : 'bg-white dark:bg-gray-700'
        ]"
        :title="icon.label"
      >
        <Icon :name="icon.iconifyName" class="w-8 h-8" />
        <span class="text-xs mt-1 text-gray-600 dark:text-gray-400 truncate w-full text-center">
          {{ icon.label }}
        </span>
      </button>
    </div>

    <div v-if="selectedIcon" class="mt-3 p-3 bg-blue-50 dark:bg-blue-900/30 rounded-lg flex items-center justify-between">
      <div class="flex items-center gap-2">
        <Icon :name="getIconifyName(selectedIcon)" class="w-6 h-6" />
        <span class="text-sm font-medium text-blue-900 dark:text-blue-300">
          Wybrano: {{ getIconLabel(selectedIcon) }}
        </span>
      </div>
      <button
        type="button"
        @click="clearSelection"
        class="text-sm text-red-600 hover:text-red-700 dark:text-red-400"
      >
        Wyczysc
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

interface IconDefinition {
  name: string
  label: string
  iconifyName: string
  category: string
  keywords: string[]
}

const props = defineProps<{
  modelValue?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const searchQuery = ref('')
const selectedIcon = ref(props.modelValue || '')
const selectedCategory = ref('all')

const categories = [
  { id: 'all', label: 'Wszystkie' },
  { id: 'vacation', label: 'Urlopy' },
  { id: 'equipment', label: 'Sprzęt IT' },
  { id: 'hr', label: 'HR/Kadry' },
  { id: 'finance', label: 'Finanse' },
  { id: 'office', label: 'Biuro' },
  { id: 'health', label: 'Zdrowie' },
  { id: 'training', label: 'Szkolenia' },
  { id: 'documents', label: 'Dokumenty' },
  { id: 'general', label: 'Pozostałe' }
]

const icons: IconDefinition[] = [
  // Urlopy i czas wolny (10)
  { name: 'beach-umbrella', label: 'Urlop wypoczynkowy', iconifyName: 'fluent-emoji-flat:beach-with-umbrella', category: 'vacation', keywords: ['urlop', 'wakacje', 'plaża', 'odpoczynek'] },
  { name: 'plane', label: 'Delegacja', iconifyName: 'fluent-emoji-flat:airplane', category: 'vacation', keywords: ['delegacja', 'podróż', 'samolot', 'wyjazd'] },
  { name: 'palm-tree', label: 'Urlop zagraniczny', iconifyName: 'fluent-emoji-flat:palm-tree', category: 'vacation', keywords: ['urlop', 'tropiki', 'wyjazd'] },
  { name: 'tent', label: 'Urlop okolicznościowy', iconifyName: 'fluent-emoji-flat:camping', category: 'vacation', keywords: ['okolicznościowy', 'urlop', 'camping'] },
  { name: 'bed', label: 'Urlop na żądanie', iconifyName: 'fluent-emoji-flat:bed', category: 'vacation', keywords: ['na żądanie', 'ondemand', 'odpoczynek'] },
  { name: 'calendar', label: 'Plan urlopów', iconifyName: 'fluent-emoji-flat:calendar', category: 'vacation', keywords: ['kalendarz', 'termin', 'plan', 'harmonogram'] },
  { name: 'sunglasses', label: 'Wolne', iconifyName: 'fluent-emoji-flat:sunglasses', category: 'vacation', keywords: ['wolne', 'odpoczynek', 'relaks'] },
  { name: 'mountain', label: 'Wyjazd górski', iconifyName: 'fluent-emoji-flat:mountain', category: 'vacation', keywords: ['góry', 'urlop', 'wyjazd'] },
  { name: 'home-office', label: 'Praca zdalna', iconifyName: 'fluent-emoji-flat:house', category: 'vacation', keywords: ['home office', 'praca zdalna', 'dom'] },
  { name: 'clock', label: 'Urlop bezpłatny', iconifyName: 'fluent-emoji-flat:clock', category: 'vacation', keywords: ['bezpłatny', 'czas', 'urlop'] },

  // Sprzęt IT i technologia (8)
  { name: 'laptop', label: 'Laptop', iconifyName: 'fluent-emoji-flat:laptop', category: 'equipment', keywords: ['laptop', 'komputer', 'sprzęt IT'] },
  { name: 'computer', label: 'Komputer stacjonarny', iconifyName: 'fluent-emoji-flat:desktop-computer', category: 'equipment', keywords: ['komputer', 'desktop', 'PC', 'stacjonarny'] },
  { name: 'phone', label: 'Telefon służbowy', iconifyName: 'fluent-emoji-flat:mobile-phone', category: 'equipment', keywords: ['telefon', 'smartfon', 'komórka'] },
  { name: 'printer', label: 'Drukarka', iconifyName: 'fluent-emoji-flat:printer', category: 'equipment', keywords: ['drukarka', 'drukowanie', 'wydruk'] },
  { name: 'keyboard', label: 'Klawiatura', iconifyName: 'fluent-emoji-flat:keyboard', category: 'equipment', keywords: ['klawiatura', 'akcesoria', 'mysz'] },
  { name: 'monitor', label: 'Monitor', iconifyName: 'fluent-emoji-flat:desktop-computer', category: 'equipment', keywords: ['monitor', 'ekran', 'wyświetlacz'] },
  { name: 'toolbox', label: 'Narzędzia IT', iconifyName: 'fluent-emoji-flat:toolbox', category: 'equipment', keywords: ['narzędzia', 'serwis', 'naprawa', 'IT'] },
  { name: 'battery', label: 'Akcesoria', iconifyName: 'fluent-emoji-flat:battery', category: 'equipment', keywords: ['akcesoria', 'zasilacz', 'bateria'] },

  // HR i Kadry (5)
  { name: 'briefcase', label: 'Zatrudnienie', iconifyName: 'fluent-emoji-flat:briefcase', category: 'hr', keywords: ['zatrudnienie', 'praca', 'umowa'] },
  { name: 'id-card', label: 'Dane osobowe', iconifyName: 'fluent-emoji-flat:identification-card', category: 'hr', keywords: ['dane', 'karta', 'identyfikacja', 'dokumenty'] },
  { name: 'family', label: 'Sprawy rodzinne', iconifyName: 'fluent-emoji-flat:family', category: 'hr', keywords: ['rodzina', 'opieka', 'dziecko'] },
  { name: 'handshake', label: 'Onboarding', iconifyName: 'fluent-emoji-flat:handshake', category: 'hr', keywords: ['wdrożenie', 'onboarding', 'powitanie'] },
  { name: 'clipboard-hr', label: 'Ankiety HR', iconifyName: 'fluent-emoji-flat:clipboard', category: 'hr', keywords: ['ankieta', 'formularz', 'badanie'] },

  // Finanse i budżet (5)
  { name: 'money-bag', label: 'Budżet', iconifyName: 'fluent-emoji-flat:money-bag', category: 'finance', keywords: ['budżet', 'pieniądze', 'finanse'] },
  { name: 'credit-card', label: 'Karta płatnicza', iconifyName: 'fluent-emoji-flat:credit-card', category: 'finance', keywords: ['karta', 'płatność', 'wydatek'] },
  { name: 'dollar', label: 'Zwrot kosztów', iconifyName: 'fluent-emoji-flat:dollar-banknote', category: 'finance', keywords: ['zwrot', 'koszty', 'refundacja', 'pieniądze'] },
  { name: 'chart', label: 'Raport finansowy', iconifyName: 'fluent-emoji-flat:chart-increasing', category: 'finance', keywords: ['raport', 'wykres', 'analiza'] },
  { name: 'receipt', label: 'Faktura', iconifyName: 'fluent-emoji-flat:receipt', category: 'finance', keywords: ['faktura', 'paragon', 'dokument'] },

  // Biuro i infrastruktura (5)
  { name: 'office', label: 'Biuro', iconifyName: 'fluent-emoji-flat:office-building', category: 'office', keywords: ['biuro', 'budynek', 'siedziba'] },
  { name: 'desk', label: 'Miejsce pracy', iconifyName: 'fluent-emoji-flat:desk', category: 'office', keywords: ['biurko', 'miejsce', 'stanowisko'] },
  { name: 'key', label: 'Dostęp', iconifyName: 'fluent-emoji-flat:key', category: 'office', keywords: ['klucz', 'dostęp', 'uprawnienia'] },
  { name: 'parking', label: 'Parking', iconifyName: 'fluent-emoji-flat:automobile', category: 'office', keywords: ['parking', 'samochód', 'miejsce parkingowe'] },
  { name: 'light-bulb', label: 'Pomysł/Zgłoszenie', iconifyName: 'fluent-emoji-flat:light-bulb', category: 'office', keywords: ['pomysł', 'innowacja', 'sugestia'] },

  // Zdrowie i bezpieczeństwo (5)
  { name: 'medical', label: 'Opieka medyczna', iconifyName: 'fluent-emoji-flat:hospital', category: 'health', keywords: ['zdrowie', 'medycyna', 'lekarz', 'szpital'] },
  { name: 'first-aid', label: 'Pierwsza pomoc', iconifyName: 'fluent-emoji-flat:adhesive-bandage', category: 'health', keywords: ['BHP', 'bezpieczeństwo', 'wypadek'] },
  { name: 'sick', label: 'Zwolnienie lekarskie', iconifyName: 'fluent-emoji-flat:face-with-thermometer', category: 'health', keywords: ['choroba', 'L4', 'zwolnienie'] },
  { name: 'shield', label: 'Bezpieczeństwo', iconifyName: 'fluent-emoji-flat:shield', category: 'health', keywords: ['bezpieczeństwo', 'ochrona', 'BHP'] },
  { name: 'warning', label: 'Zgłoszenie incydentu', iconifyName: 'fluent-emoji-flat:warning', category: 'health', keywords: ['incydent', 'zagrożenie', 'alarm'] },

  // Szkolenia i rozwój (5)
  { name: 'graduation', label: 'Szkolenie', iconifyName: 'fluent-emoji-flat:graduation-cap', category: 'training', keywords: ['szkolenie', 'kurs', 'nauka', 'certyfikat'] },
  { name: 'books', label: 'Materiały', iconifyName: 'fluent-emoji-flat:books', category: 'training', keywords: ['materiały', 'wiedza', 'dokumentacja'] },
  { name: 'trophy', label: 'Certyfikacja', iconifyName: 'fluent-emoji-flat:trophy', category: 'training', keywords: ['certyfikat', 'osiągnięcie', 'sukces'] },
  { name: 'teacher', label: 'Mentor', iconifyName: 'fluent-emoji-flat:teacher', category: 'training', keywords: ['mentor', 'nauczyciel', 'szkoleniowiec'] },
  { name: 'rocket', label: 'Rozwój', iconifyName: 'fluent-emoji-flat:rocket', category: 'training', keywords: ['rozwój', 'kariera', 'awans'] },

  // Dokumenty (4)
  { name: 'document', label: 'Dokument', iconifyName: 'fluent-emoji-flat:page-facing-up', category: 'documents', keywords: ['dokument', 'plik', 'formularz'] },
  { name: 'folder', label: 'Folder', iconifyName: 'fluent-emoji-flat:file-folder', category: 'documents', keywords: ['folder', 'teczka', 'archiwum'] },
  { name: 'memo', label: 'Notatka', iconifyName: 'fluent-emoji-flat:memo', category: 'documents', keywords: ['notatka', 'memo', 'informacja'] },
  { name: 'signature', label: 'Podpis', iconifyName: 'fluent-emoji-flat:pen', category: 'documents', keywords: ['podpis', 'zatwierdzenie', 'podpisanie'] },

  // Ogólne (3)
  { name: 'checkmark', label: 'Zatwierdzenie', iconifyName: 'fluent-emoji-flat:check-mark-button', category: 'general', keywords: ['zatwierdzenie', 'akceptacja', 'OK'] },
  { name: 'bell', label: 'Powiadomienie', iconifyName: 'fluent-emoji-flat:bell', category: 'general', keywords: ['powiadomienie', 'alert', 'komunikat'] },
  { name: 'megaphone', label: 'Ogłoszenie', iconifyName: 'fluent-emoji-flat:megaphone', category: 'general', keywords: ['ogłoszenie', 'komunikat', 'informacja'] }
]

const filteredIcons = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  return icons.filter(icon => {
    const matchesCategory = selectedCategory.value === 'all' || icon.category === selectedCategory.value
    if (!matchesCategory) {
      return false
    }

    if (!query) {
      return true
    }

    const haystack = [
      icon.name,
      icon.label,
      icon.iconifyName,
      ...icon.keywords
    ]
      .join(' ')
      .toLowerCase()

    return haystack.includes(query)
  })
})

const getIconifyName = (iconName: string) => {
  return icons.find(icon => icon.name === iconName)?.iconifyName || icons[0]?.iconifyName || ''
}

const getIconLabel = (iconName: string) => {
  return icons.find(icon => icon.name === iconName)?.label || iconName
}

const selectIcon = (iconName: string) => {
  selectedIcon.value = iconName
  emit('update:modelValue', iconName)
}

const clearSelection = () => {
  selectedIcon.value = ''
  emit('update:modelValue', '')
}

watch(
  () => props.modelValue,
  newValue => {
    selectedIcon.value = newValue || ''
  }
)
</script>

<style scoped>
.icon-picker {
  @apply w-full;
}

.icon-grid::-webkit-scrollbar {
  width: 8px;
}

.icon-grid::-webkit-scrollbar-track {
  @apply bg-gray-100 dark:bg-gray-700 rounded;
}

.icon-grid::-webkit-scrollbar-thumb {
  @apply bg-gray-300 dark:bg-gray-600 rounded;
}

.icon-grid::-webkit-scrollbar-thumb:hover {
  @apply bg-gray-400 dark:bg-gray-500;
}
</style>
