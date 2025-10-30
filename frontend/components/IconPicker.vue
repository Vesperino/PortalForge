<template>
  <div class="icon-picker">
    <div class="mb-3">
      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Wybierz ikonę
      </label>
      <input
        v-model="searchQuery"
        type="text"
        placeholder="Szukaj ikony..."
        class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
      >
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
        <component :is="icon.component" class="w-6 h-6 text-gray-700 dark:text-gray-300" />
        <span class="text-xs mt-1 text-gray-600 dark:text-gray-400 truncate w-full text-center">
          {{ icon.label }}
        </span>
      </button>
    </div>

    <div v-if="selectedIcon" class="mt-3 p-3 bg-blue-50 dark:bg-blue-900/30 rounded-lg flex items-center justify-between">
      <div class="flex items-center gap-2">
        <component :is="getIconComponent(selectedIcon)" class="w-5 h-5 text-blue-600 dark:text-blue-400" />
        <span class="text-sm font-medium text-blue-900 dark:text-blue-300">
          Wybrano: {{ selectedIcon }}
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
import { ref, computed } from 'vue'
import {
  Laptop,
  Monitor,
  Smartphone,
  Tablet,
  Keyboard,
  Mouse,
  Headphones,
  Printer,
  Server,
  HardDrive,
  FileText,
  File,
  Folder,
  FolderOpen,
  Upload,
  Download,
  ClipboardList,
  ClipboardCheck,
  Users,
  User,
  UserPlus,
  UserCheck,
  Calendar,
  Clock,
  Bell,
  Mail,
  MessageSquare,
  Phone,
  Video,
  Camera,
  Image,
  Settings,
  Wrench,
  Package,
  ShoppingCart,
  CreditCard,
  DollarSign,
  TrendingUp,
  BarChart,
  PieChart,
  Activity,
  Shield,
  Lock,
  Unlock,
  Key,
  AlertCircle,
  CheckCircle,
  XCircle,
  Info,
  HelpCircle,
  Star,
  Heart,
  Bookmark,
  Flag,
  Tag,
  MapPin,
  Navigation,
  Compass,
  Globe,
  Map,
  Home,
  Building,
  Briefcase,
  Book,
  BookOpen,
  GraduationCap,
  Award,
  Target,
  Zap,
  Cpu,
  Database,
  Code,
  Terminal,
  GitBranch
} from 'lucide-vue-next'

const props = defineProps<{
  modelValue?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const searchQuery = ref('')
const selectedIcon = ref(props.modelValue || '')

const icons = [
  // Hardware
  { name: 'Laptop', label: 'Laptop', component: Laptop },
  { name: 'Monitor', label: 'Monitor', component: Monitor },
  { name: 'Smartphone', label: 'Telefon', component: Smartphone },
  { name: 'Tablet', label: 'Tablet', component: Tablet },
  { name: 'Keyboard', label: 'Klawiatura', component: Keyboard },
  { name: 'Mouse', label: 'Mysz', component: Mouse },
  { name: 'Headphones', label: 'Słuchawki', component: Headphones },
  { name: 'Printer', label: 'Drukarka', component: Printer },
  { name: 'Server', label: 'Serwer', component: Server },
  { name: 'HardDrive', label: 'Dysk', component: HardDrive },
  
  // Documents
  { name: 'FileText', label: 'Dokument', component: FileText },
  { name: 'File', label: 'Plik', component: File },
  { name: 'Folder', label: 'Folder', component: Folder },
  { name: 'FolderOpen', label: 'Folder otw.', component: FolderOpen },
  { name: 'Upload', label: 'Upload', component: Upload },
  { name: 'Download', label: 'Download', component: Download },
  { name: 'ClipboardList', label: 'Lista', component: ClipboardList },
  { name: 'ClipboardCheck', label: 'Zaznacz', component: ClipboardCheck },
  
  // People
  { name: 'Users', label: 'Użytkownicy', component: Users },
  { name: 'User', label: 'Użytkownik', component: User },
  { name: 'UserPlus', label: 'Dodaj użyt.', component: UserPlus },
  { name: 'UserCheck', label: 'Zatwierdź', component: UserCheck },
  
  // Time & Communication
  { name: 'Calendar', label: 'Kalendarz', component: Calendar },
  { name: 'Clock', label: 'Zegar', component: Clock },
  { name: 'Bell', label: 'Powiadomienie', component: Bell },
  { name: 'Mail', label: 'Email', component: Mail },
  { name: 'MessageSquare', label: 'Wiadomość', component: MessageSquare },
  { name: 'Phone', label: 'Telefon', component: Phone },
  { name: 'Video', label: 'Video', component: Video },
  { name: 'Camera', label: 'Kamera', component: Camera },
  { name: 'Image', label: 'Obraz', component: Image },
  
  // Tools & Settings
  { name: 'Settings', label: 'Ustawienia', component: Settings },
  { name: 'Wrench', label: 'Narzędzie', component: Wrench },
  { name: 'Package', label: 'Paczka', component: Package },
  
  // Business
  { name: 'ShoppingCart', label: 'Koszyk', component: ShoppingCart },
  { name: 'CreditCard', label: 'Karta', component: CreditCard },
  { name: 'DollarSign', label: 'Pieniądze', component: DollarSign },
  { name: 'TrendingUp', label: 'Wzrost', component: TrendingUp },
  { name: 'BarChart', label: 'Wykres', component: BarChart },
  { name: 'PieChart', label: 'Wykres kołowy', component: PieChart },
  { name: 'Activity', label: 'Aktywność', component: Activity },
  
  // Security
  { name: 'Shield', label: 'Tarcza', component: Shield },
  { name: 'Lock', label: 'Zamek', component: Lock },
  { name: 'Unlock', label: 'Otwórz', component: Unlock },
  { name: 'Key', label: 'Klucz', component: Key },
  
  // Status
  { name: 'AlertCircle', label: 'Alert', component: AlertCircle },
  { name: 'CheckCircle', label: 'OK', component: CheckCircle },
  { name: 'XCircle', label: 'Błąd', component: XCircle },
  { name: 'Info', label: 'Info', component: Info },
  { name: 'HelpCircle', label: 'Pomoc', component: HelpCircle },
  
  // Favorites & Tags
  { name: 'Star', label: 'Gwiazda', component: Star },
  { name: 'Heart', label: 'Serce', component: Heart },
  { name: 'Bookmark', label: 'Zakładka', component: Bookmark },
  { name: 'Flag', label: 'Flaga', component: Flag },
  { name: 'Tag', label: 'Tag', component: Tag },
  
  // Location
  { name: 'MapPin', label: 'Pin', component: MapPin },
  { name: 'Navigation', label: 'Nawigacja', component: Navigation },
  { name: 'Compass', label: 'Kompas', component: Compass },
  { name: 'Globe', label: 'Glob', component: Globe },
  { name: 'Map', label: 'Mapa', component: Map },
  
  // Buildings & Education
  { name: 'Home', label: 'Dom', component: Home },
  { name: 'Building', label: 'Budynek', component: Building },
  { name: 'Briefcase', label: 'Teczka', component: Briefcase },
  { name: 'Book', label: 'Książka', component: Book },
  { name: 'BookOpen', label: 'Książka otw.', component: BookOpen },
  { name: 'GraduationCap', label: 'Edukacja', component: GraduationCap },
  { name: 'Award', label: 'Nagroda', component: Award },
  { name: 'Target', label: 'Cel', component: Target },
  
  // Tech
  { name: 'Zap', label: 'Błyskawica', component: Zap },
  { name: 'Cpu', label: 'CPU', component: Cpu },
  { name: 'Database', label: 'Baza danych', component: Database },
  { name: 'Code', label: 'Kod', component: Code },
  { name: 'Terminal', label: 'Terminal', component: Terminal },
  { name: 'GitBranch', label: 'Git', component: GitBranch }
]

const filteredIcons = computed(() => {
  if (!searchQuery.value) return icons
  
  const query = searchQuery.value.toLowerCase()
  return icons.filter(icon => 
    icon.name.toLowerCase().includes(query) || 
    icon.label.toLowerCase().includes(query)
  )
})

const getIconComponent = (iconName: string) => {
  return icons.find(i => i.name === iconName)?.component
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

