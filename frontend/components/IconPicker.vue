<template>
  <div class="icon-picker">
    <div class="mb-3">
      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Wybierz ikonę
      </label>
      <input
        v-model="searchQuery"
        type="text"
        placeholder="Szukaj ikony... (np. urlop, plaża, komputer, dokumenty)"
        class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
      >
    </div>

    <!-- Category Tabs -->
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
        Wyczyść
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'

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
  { id: 'vacation', label: '🏖️ Urlopy' },
  { id: 'office', label: '💼 Biuro' },
  { id: 'tech', label: '💻 Technologia' },
  { id: 'documents', label: '📄 Dokumenty' },
  { id: 'people', label: '👥 Ludzie' },
  { id: 'time', label: '⏰ Czas' },
  { id: 'communication', label: '💬 Komunikacja' },
  { id: 'finance', label: '💰 Finanse' },
  { id: 'health', label: '🏥 Zdrowie' },
  { id: 'transport', label: '🚗 Transport' },
  { id: 'food', label: '🍔 Jedzenie' },
  { id: 'celebration', label: '🎉 Święta' },
  { id: 'tools', label: '🔧 Narzędzia' },
  { id: 'nature', label: '🌳 Natura' },
  { id: 'sports', label: '⚽ Sport' }
]

// Kolorowe emoji ikony z Fluent Emoji Flat
const icons: IconDefinition[] = [
  // Urlopy i wakacje
  { name: 'beach-umbrella', label: 'Parasol plażowy', iconifyName: 'fluent-emoji-flat:beach-with-umbrella', category: 'vacation', keywords: ['plaża', 'urlop', 'wakacje', 'morze'] },
  { name: 'palm-tree', label: 'Palma', iconifyName: 'fluent-emoji-flat:palm-tree', category: 'vacation', keywords: ['palma', 'urlop', 'wakacje', 'tropiki'] },
  { name: 'sun', label: 'Słońce', iconifyName: 'fluent-emoji-flat:sun', category: 'vacation', keywords: ['słońce', 'urlop', 'wakacje', 'lato'] },
  { name: 'airplane', label: 'Samolot', iconifyName: 'fluent-emoji-flat:airplane', category: 'vacation', keywords: ['samolot', 'podróż', 'urlop', 'lot'] },
  { name: 'luggage', label: 'Walizka', iconifyName: 'fluent-emoji-flat:luggage', category: 'vacation', keywords: ['walizka', 'podróż', 'urlop', 'bagaż'] },
  { name: 'island', label: 'Wyspa', iconifyName: 'fluent-emoji-flat:desert-island', category: 'vacation', keywords: ['wyspa', 'urlop', 'wakacje', 'plaża'] },
  { name: 'camping', label: 'Namiot', iconifyName: 'fluent-emoji-flat:camping', category: 'vacation', keywords: ['namiot', 'camping', 'urlop', 'biwak'] },
  { name: 'mountain', label: 'Góry', iconifyName: 'fluent-emoji-flat:mountain', category: 'vacation', keywords: ['góry', 'urlop', 'wycieczka'] },

  // Biuro i praca
  { name: 'briefcase', label: 'Teczka', iconifyName: 'fluent-emoji-flat:briefcase', category: 'office', keywords: ['teczka', 'praca', 'biuro', 'biznes'] },
  { name: 'office-building', label: 'Biurowiec', iconifyName: 'fluent-emoji-flat:office-building', category: 'office', keywords: ['biuro', 'budynek', 'firma', 'praca'] },
  { name: 'chart-increasing', label: 'Wykres rosnący', iconifyName: 'fluent-emoji-flat:chart-increasing', category: 'office', keywords: ['wykres', 'statystyki', 'wzrost', 'analiza'] },
  { name: 'clipboard', label: 'Schowek', iconifyName: 'fluent-emoji-flat:clipboard', category: 'office', keywords: ['schowek', 'notatki', 'lista', 'zadania'] },
  { name: 'calendar', label: 'Kalendarz', iconifyName: 'fluent-emoji-flat:calendar', category: 'office', keywords: ['kalendarz', 'data', 'termin', 'spotkanie'] },
  { name: 'pushpin', label: 'Pinezka', iconifyName: 'fluent-emoji-flat:pushpin', category: 'office', keywords: ['pinezka', 'przypięcie', 'ważne'] },
  { name: 'memo', label: 'Notatka', iconifyName: 'fluent-emoji-flat:memo', category: 'office', keywords: ['notatka', 'memo', 'dokument', 'pisanie'] },

  // Technologia
  { name: 'laptop', label: 'Laptop', iconifyName: 'fluent-emoji-flat:laptop', category: 'tech', keywords: ['laptop', 'komputer', 'praca', 'IT'] },
  { name: 'desktop-computer', label: 'Komputer', iconifyName: 'fluent-emoji-flat:desktop-computer', category: 'tech', keywords: ['komputer', 'PC', 'praca', 'IT'] },
  { name: 'keyboard', label: 'Klawiatura', iconifyName: 'fluent-emoji-flat:keyboard', category: 'tech', keywords: ['klawiatura', 'komputer', 'pisanie'] },
  { name: 'computer-mouse', label: 'Mysz', iconifyName: 'fluent-emoji-flat:computer-mouse', category: 'tech', keywords: ['mysz', 'komputer', 'klikanie'] },
  { name: 'printer', label: 'Drukarka', iconifyName: 'fluent-emoji-flat:printer', category: 'tech', keywords: ['drukarka', 'wydruk', 'biuro'] },
  { name: 'mobile-phone', label: 'Telefon', iconifyName: 'fluent-emoji-flat:mobile-phone', category: 'tech', keywords: ['telefon', 'komórka', 'smartfon'] },
  { name: 'battery', label: 'Bateria', iconifyName: 'fluent-emoji-flat:battery', category: 'tech', keywords: ['bateria', 'energia', 'ładowanie'] },
  { name: 'electric-plug', label: 'Wtyczka', iconifyName: 'fluent-emoji-flat:electric-plug', category: 'tech', keywords: ['wtyczka', 'prąd', 'energia'] },

  // Dokumenty
  { name: 'page-facing-up', label: 'Dokument', iconifyName: 'fluent-emoji-flat:page-facing-up', category: 'documents', keywords: ['dokument', 'plik', 'strona', 'papier'] },
  { name: 'page-with-curl', label: 'Dokument z zagięciem', iconifyName: 'fluent-emoji-flat:page-with-curl', category: 'documents', keywords: ['dokument', 'plik', 'strona'] },
  { name: 'bookmark-tabs', label: 'Zakładki', iconifyName: 'fluent-emoji-flat:bookmark-tabs', category: 'documents', keywords: ['zakładki', 'dokumenty', 'organizacja'] },
  { name: 'file-folder', label: 'Folder', iconifyName: 'fluent-emoji-flat:file-folder', category: 'documents', keywords: ['folder', 'katalog', 'pliki'] },
  { name: 'open-file-folder', label: 'Otwarty folder', iconifyName: 'fluent-emoji-flat:open-file-folder', category: 'documents', keywords: ['folder', 'otwarty', 'pliki'] },
  { name: 'card-index-dividers', label: 'Segregator', iconifyName: 'fluent-emoji-flat:card-index-dividers', category: 'documents', keywords: ['segregator', 'dokumenty', 'archiwum'] },
  { name: 'spiral-notepad', label: 'Notatnik', iconifyName: 'fluent-emoji-flat:spiral-notepad', category: 'documents', keywords: ['notatnik', 'notes', 'pisanie'] },

  // Ludzie
  { name: 'bust-in-silhouette', label: 'Osoba', iconifyName: 'fluent-emoji-flat:bust-in-silhouette', category: 'people', keywords: ['osoba', 'użytkownik', 'profil'] },
  { name: 'busts-in-silhouette', label: 'Ludzie', iconifyName: 'fluent-emoji-flat:busts-in-silhouette', category: 'people', keywords: ['ludzie', 'użytkownicy', 'zespół', 'grupa'] },
  { name: 'man-office-worker', label: 'Pracownik', iconifyName: 'fluent-emoji-flat:man-office-worker', category: 'people', keywords: ['pracownik', 'biuro', 'praca'] },
  { name: 'woman-office-worker', label: 'Pracownica', iconifyName: 'fluent-emoji-flat:woman-office-worker', category: 'people', keywords: ['pracownica', 'biuro', 'praca'] },
  { name: 'technologist', label: 'Programista', iconifyName: 'fluent-emoji-flat:technologist', category: 'people', keywords: ['programista', 'IT', 'developer', 'komputer'] },
  { name: 'man-teacher', label: 'Nauczyciel', iconifyName: 'fluent-emoji-flat:man-teacher', category: 'people', keywords: ['nauczyciel', 'szkolenie', 'edukacja'] },

  // Czas i komunikacja
  { name: 'alarm-clock', label: 'Budzik', iconifyName: 'fluent-emoji-flat:alarm-clock', category: 'time', keywords: ['budzik', 'czas', 'godzina', 'przypomnienie'] },
  { name: 'hourglass', label: 'Klepsydra', iconifyName: 'fluent-emoji-flat:hourglass-done', category: 'time', keywords: ['klepsydra', 'czas', 'oczekiwanie'] },
  { name: 'stopwatch', label: 'Stoper', iconifyName: 'fluent-emoji-flat:stopwatch', category: 'time', keywords: ['stoper', 'czas', 'pomiar'] },
  { name: 'envelope', label: 'Koperta', iconifyName: 'fluent-emoji-flat:envelope', category: 'communication', keywords: ['koperta', 'email', 'wiadomość', 'poczta'] },
  { name: 'incoming-envelope', label: 'Przychodząca poczta', iconifyName: 'fluent-emoji-flat:incoming-envelope', category: 'communication', keywords: ['poczta', 'email', 'wiadomość', 'odbiór'] },
  { name: 'outbox-tray', label: 'Wysłane', iconifyName: 'fluent-emoji-flat:outbox-tray', category: 'communication', keywords: ['wysłane', 'poczta', 'wiadomość'] },
  { name: 'inbox-tray', label: 'Odebrane', iconifyName: 'fluent-emoji-flat:inbox-tray', category: 'communication', keywords: ['odebrane', 'poczta', 'wiadomość'] },
  { name: 'telephone', label: 'Telefon', iconifyName: 'fluent-emoji-flat:telephone', category: 'communication', keywords: ['telefon', 'rozmowa', 'kontakt'] },
  { name: 'speech-balloon', label: 'Dymek', iconifyName: 'fluent-emoji-flat:speech-balloon', category: 'communication', keywords: ['dymek', 'rozmowa', 'czat', 'wiadomość'] },
  { name: 'megaphone', label: 'Megafon', iconifyName: 'fluent-emoji-flat:megaphone', category: 'communication', keywords: ['megafon', 'ogłoszenie', 'komunikat'] },

  // Finanse
  { name: 'money-bag', label: 'Worek pieniędzy', iconifyName: 'fluent-emoji-flat:money-bag', category: 'finance', keywords: ['pieniądze', 'finanse', 'budżet', 'kasa'] },
  { name: 'dollar-banknote', label: 'Banknot', iconifyName: 'fluent-emoji-flat:dollar-banknote', category: 'finance', keywords: ['banknot', 'pieniądze', 'finanse'] },
  { name: 'credit-card', label: 'Karta kredytowa', iconifyName: 'fluent-emoji-flat:credit-card', category: 'finance', keywords: ['karta', 'płatność', 'finanse'] },
  { name: 'receipt', label: 'Paragon', iconifyName: 'fluent-emoji-flat:receipt', category: 'finance', keywords: ['paragon', 'rachunek', 'faktura', 'wydatek'] },
  { name: 'chart-increasing-with-yen', label: 'Wykres wzrostu', iconifyName: 'fluent-emoji-flat:chart-increasing-with-yen', category: 'finance', keywords: ['wykres', 'wzrost', 'finanse', 'zysk'] },

  // Zdrowie
  { name: 'hospital', label: 'Szpital', iconifyName: 'fluent-emoji-flat:hospital', category: 'health', keywords: ['szpital', 'zdrowie', 'lekarz', 'medycyna'] },
  { name: 'pill', label: 'Tabletka', iconifyName: 'fluent-emoji-flat:pill', category: 'health', keywords: ['tabletka', 'lek', 'zdrowie', 'medycyna'] },
  { name: 'syringe', label: 'Strzykawka', iconifyName: 'fluent-emoji-flat:syringe', category: 'health', keywords: ['strzykawka', 'szczepienie', 'zdrowie'] },
  { name: 'stethoscope', label: 'Stetoskop', iconifyName: 'fluent-emoji-flat:stethoscope', category: 'health', keywords: ['stetoskop', 'lekarz', 'zdrowie', 'badanie'] },
  { name: 'thermometer', label: 'Termometr', iconifyName: 'fluent-emoji-flat:thermometer', category: 'health', keywords: ['termometr', 'temperatura', 'zdrowie', 'gorączka'] },
  { name: 'adhesive-bandage', label: 'Plaster', iconifyName: 'fluent-emoji-flat:adhesive-bandage', category: 'health', keywords: ['plaster', 'rana', 'zdrowie'] },

  // Transport
  { name: 'automobile', label: 'Samochód', iconifyName: 'fluent-emoji-flat:automobile', category: 'transport', keywords: ['samochód', 'auto', 'pojazd', 'transport'] },
  { name: 'bus', label: 'Autobus', iconifyName: 'fluent-emoji-flat:bus', category: 'transport', keywords: ['autobus', 'transport', 'komunikacja'] },
  { name: 'train', label: 'Pociąg', iconifyName: 'fluent-emoji-flat:train', category: 'transport', keywords: ['pociąg', 'transport', 'kolej'] },
  { name: 'bicycle', label: 'Rower', iconifyName: 'fluent-emoji-flat:bicycle', category: 'transport', keywords: ['rower', 'transport', 'sport'] },
  { name: 'fuel-pump', label: 'Stacja paliw', iconifyName: 'fluent-emoji-flat:fuel-pump', category: 'transport', keywords: ['paliwo', 'benzyna', 'stacja', 'tankowanie'] },
  { name: 'parking', label: 'Parking', iconifyName: 'fluent-emoji-flat:p-button', category: 'transport', keywords: ['parking', 'parkowanie', 'samochód'] },

  // Jedzenie
  { name: 'hamburger', label: 'Hamburger', iconifyName: 'fluent-emoji-flat:hamburger', category: 'food', keywords: ['hamburger', 'jedzenie', 'posiłek'] },
  { name: 'pizza', label: 'Pizza', iconifyName: 'fluent-emoji-flat:pizza', category: 'food', keywords: ['pizza', 'jedzenie', 'posiłek'] },
  { name: 'coffee', label: 'Kawa', iconifyName: 'fluent-emoji-flat:hot-beverage', category: 'food', keywords: ['kawa', 'napój', 'przerwa'] },
  { name: 'birthday-cake', label: 'Tort', iconifyName: 'fluent-emoji-flat:birthday-cake', category: 'food', keywords: ['tort', 'urodziny', 'ciasto', 'święto'] },
  { name: 'fork-and-knife', label: 'Sztućce', iconifyName: 'fluent-emoji-flat:fork-and-knife', category: 'food', keywords: ['sztućce', 'jedzenie', 'posiłek', 'obiad'] },
  { name: 'clinking-beer-mugs', label: 'Piwo', iconifyName: 'fluent-emoji-flat:clinking-beer-mugs', category: 'food', keywords: ['piwo', 'napój', 'toast', 'świętowanie'] },

  // Święta i celebracje
  { name: 'party-popper', label: 'Konfetti', iconifyName: 'fluent-emoji-flat:party-popper', category: 'celebration', keywords: ['konfetti', 'święto', 'celebracja', 'impreza'] },
  { name: 'wrapped-gift', label: 'Prezent', iconifyName: 'fluent-emoji-flat:wrapped-gift', category: 'celebration', keywords: ['prezent', 'gift', 'święto', 'urodziny'] },
  { name: 'balloon', label: 'Balon', iconifyName: 'fluent-emoji-flat:balloon', category: 'celebration', keywords: ['balon', 'święto', 'urodziny', 'impreza'] },
  { name: 'christmas-tree', label: 'Choinka', iconifyName: 'fluent-emoji-flat:christmas-tree', category: 'celebration', keywords: ['choinka', 'święta', 'boże narodzenie'] },
  { name: 'fireworks', label: 'Fajerwerki', iconifyName: 'fluent-emoji-flat:fireworks', category: 'celebration', keywords: ['fajerwerki', 'święto', 'celebracja', 'nowy rok'] },
  { name: 'trophy', label: 'Trofeum', iconifyName: 'fluent-emoji-flat:trophy', category: 'celebration', keywords: ['trofeum', 'nagroda', 'zwycięstwo', 'sukces'] },
  { name: 'medal', label: 'Medal', iconifyName: 'fluent-emoji-flat:1st-place-medal', category: 'celebration', keywords: ['medal', 'nagroda', 'zwycięstwo', 'pierwsze miejsce'] },

  // Narzędzia
  { name: 'hammer', label: 'Młotek', iconifyName: 'fluent-emoji-flat:hammer', category: 'tools', keywords: ['młotek', 'narzędzie', 'naprawa', 'budowa'] },
  { name: 'wrench', label: 'Klucz', iconifyName: 'fluent-emoji-flat:wrench', category: 'tools', keywords: ['klucz', 'narzędzie', 'naprawa', 'serwis'] },
  { name: 'hammer-and-wrench', label: 'Narzędzia', iconifyName: 'fluent-emoji-flat:hammer-and-wrench', category: 'tools', keywords: ['narzędzia', 'naprawa', 'serwis', 'konserwacja'] },
  { name: 'gear', label: 'Koło zębate', iconifyName: 'fluent-emoji-flat:gear', category: 'tools', keywords: ['koło zębate', 'ustawienia', 'konfiguracja', 'mechanizm'] },
  { name: 'toolbox', label: 'Skrzynka narzędziowa', iconifyName: 'fluent-emoji-flat:toolbox', category: 'tools', keywords: ['skrzynka', 'narzędzia', 'naprawa'] },
  { name: 'magnet', label: 'Magnes', iconifyName: 'fluent-emoji-flat:magnet', category: 'tools', keywords: ['magnes', 'przyciąganie', 'narzędzie'] },
  { name: 'key', label: 'Klucz', iconifyName: 'fluent-emoji-flat:key', category: 'tools', keywords: ['klucz', 'dostęp', 'bezpieczeństwo', 'zamek'] },
  { name: 'locked', label: 'Zamknięta kłódka', iconifyName: 'fluent-emoji-flat:locked', category: 'tools', keywords: ['kłódka', 'zamknięte', 'bezpieczeństwo', 'ochrona'] },
  { name: 'unlocked', label: 'Otwarta kłódka', iconifyName: 'fluent-emoji-flat:unlocked', category: 'tools', keywords: ['kłódka', 'otwarte', 'dostęp'] },

  // Natura
  { name: 'deciduous-tree', label: 'Drzewo', iconifyName: 'fluent-emoji-flat:deciduous-tree', category: 'nature', keywords: ['drzewo', 'natura', 'las', 'środowisko'] },
  { name: 'evergreen-tree', label: 'Choinka (drzewo)', iconifyName: 'fluent-emoji-flat:evergreen-tree', category: 'nature', keywords: ['choinka', 'drzewo', 'natura', 'las'] },
  { name: 'four-leaf-clover', label: 'Czterolistna koniczyna', iconifyName: 'fluent-emoji-flat:four-leaf-clover', category: 'nature', keywords: ['koniczyna', 'szczęście', 'natura'] },
  { name: 'seedling', label: 'Sadzonka', iconifyName: 'fluent-emoji-flat:seedling', category: 'nature', keywords: ['sadzonka', 'roślina', 'wzrost', 'natura'] },
  { name: 'herb', label: 'Zioło', iconifyName: 'fluent-emoji-flat:herb', category: 'nature', keywords: ['zioło', 'roślina', 'natura'] },
  { name: 'globe-showing-europe-africa', label: 'Kula ziemska', iconifyName: 'fluent-emoji-flat:globe-showing-europe-africa', category: 'nature', keywords: ['ziemia', 'planeta', 'świat', 'ekologia'] },
  { name: 'recycling-symbol', label: 'Recykling', iconifyName: 'fluent-emoji-flat:recycling-symbol', category: 'nature', keywords: ['recykling', 'ekologia', 'środowisko', 'odzysk'] },

  // Sport
  { name: 'soccer-ball', label: 'Piłka nożna', iconifyName: 'fluent-emoji-flat:soccer-ball', category: 'sports', keywords: ['piłka', 'sport', 'football', 'mecz'] },
  { name: 'basketball', label: 'Koszykówka', iconifyName: 'fluent-emoji-flat:basketball', category: 'sports', keywords: ['koszykówka', 'sport', 'piłka'] },
  { name: 'tennis', label: 'Tenis', iconifyName: 'fluent-emoji-flat:tennis', category: 'sports', keywords: ['tenis', 'sport', 'rakieta'] },
  { name: 'running-shoe', label: 'But sportowy', iconifyName: 'fluent-emoji-flat:running-shoe', category: 'sports', keywords: ['but', 'bieganie', 'sport', 'trening'] },
  { name: 'trophy-sports', label: 'Puchar', iconifyName: 'fluent-emoji-flat:trophy', category: 'sports', keywords: ['puchar', 'trofeum', 'sport', 'zwycięstwo'] },
  { name: 'medal-sports', label: 'Medal sportowy', iconifyName: 'fluent-emoji-flat:sports-medal', category: 'sports', keywords: ['medal', 'sport', 'nagroda'] },
  { name: 'person-biking', label: 'Rowerzysta', iconifyName: 'fluent-emoji-flat:person-biking', category: 'sports', keywords: ['rower', 'sport', 'jazda', 'aktywność'] }
]

const filteredIcons = computed(() => {
  let result = icons

  // Filter by category
  if (selectedCategory.value !== 'all') {
    result = result.filter(icon => icon.category === selectedCategory.value)
  }

  // Filter by search query
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(icon =>
      icon.name.toLowerCase().includes(query) ||
      icon.label.toLowerCase().includes(query) ||
      icon.keywords.some(keyword => keyword.toLowerCase().includes(query))
    )
  }

  return result
})

const getIconifyName = (iconName: string) => {
  return icons.find(i => i.name === iconName)?.iconifyName || 'fluent-emoji-flat:question-mark'
}

const getIconLabel = (iconName: string) => {
  return icons.find(i => i.name === iconName)?.label || iconName
}

const selectIcon = (iconName: string) => {
  selectedIcon.value = iconName
  emit('update:modelValue', iconName)
}

const clearSelection = () => {
  selectedIcon.value = ''
  emit('update:modelValue', '')
}

watch(() => props.modelValue, (newVal) => {
  selectedIcon.value = newVal || ''
})
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

