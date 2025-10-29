# Rekomendowane biblioteki dla rozbudowy systemu newsów

## 📝 Rich Text Editor - Rozbudowa edytora

### 🏆 **TinyMCE** (Rekomendacja #1)
- **Strona**: https://www.tiny.cloud/
- **Integracja**: `@tinymce/tinymce-vue`
- **Licencja**: Open Source (MIT) + Commercial
- **Zalety**:
  - Najpopularniejszy WYSIWYG editor (350M+ downloads/rok)
  - Oficjalne wsparcie dla Vue 3
  - 400+ API, pełna customizacja
  - PowerPaste - kopiowanie z Word/Excel/Google Docs
  - Cloud CDN lub self-hosted
  - Doskonała dokumentacja
- **Instalacja**:
```bash
npm install @tinymce/tinymce-vue
```

### 🥈 **Tiptap** (Rekomendacja #2)
- **Strona**: https://tiptap.dev/
- **Integracja**: `@tiptap/vue-3`
- **Licencja**: MIT
- **Zalety**:
  - Headless, renderless - pełna kontrola nad UI
  - Extensible - łatwe dodawanie własnych rozszerzeń
  - TypeScript support
  - Współpraca real-time (Collaboration)
  - Lekki i szybki
- **Uwaga**: Już używamy Tiptap! Możemy go rozbudować o dodatkowe rozszerzenia
- **Rozszerzenia do dodania**:
  - `@tiptap/extension-image` - upload obrazków
  - `@tiptap/extension-table` - tabele
  - `@tiptap/extension-youtube` - embed YouTube
  - `@tiptap/extension-color` - kolory tekstu

### 🥉 **Quill** (Alternatywa)
- **Strona**: https://quilljs.com/
- **Integracja**: `vue-quilly` (Vue 3)
- **Licencja**: BSD-3-Clause
- **Zalety**:
  - Prosty w użyciu
  - Modułowa architektura
  - Dobra dokumentacja

---

## 📅 Date Picker - Wybór daty wydarzenia

### 🏆 **VCalendar** (Rekomendacja #1)
- **Strona**: https://vcalendar.io/
- **Integracja**: `v-calendar`
- **Licencja**: MIT
- **Zalety**:
  - Najlepszy date picker dla Vue 3
  - Date range picker
  - Highlights i popovers
  - Pełna customizacja
  - Dark mode support
  - 4500+ GitHub stars
- **Instalacja**:
```bash
npm install v-calendar@next
```
- **Użycie**:
```vue
<DatePicker v-model="eventDateTime" mode="dateTime" />
```

### 🥈 **Vue Datepicker** (Alternatywa)
- **Strona**: https://vue3datepicker.com/
- **Integracja**: `@vuepic/vue-datepicker`
- **Licencja**: MIT
- **Zalety**:
  - Lekki i szybki
  - Time picker included
  - Range selection
  - TypeScript support

---

## 🗺️ Google Maps - Wybór lokalizacji

### 🏆 **vue3-google-map** (Rekomendacja #1)
- **Strona**: https://vue-map.netlify.app/
- **Integracja**: `vue3-google-map`
- **Licencja**: MIT
- **Zalety**:
  - Oficjalne komponenty dla Vue 3
  - GoogleMap, Marker, Autocomplete
  - TypeScript support
  - Reactive
- **Instalacja**:
```bash
npm install vue3-google-map
```
- **Użycie**:
```vue
<GoogleMap
  api-key="YOUR_GOOGLE_MAPS_API_KEY"
  :center="center"
  :zoom="15"
>
  <Marker :options="{ position: center }" />
</GoogleMap>
```

### 🥈 **v-use-places-autocomplete** (Composable)
- **Integracja**: `v-use-places-autocomplete`
- **Licencja**: MIT
- **Zalety**:
  - Vue 3 Composable
  - Google Places Autocomplete
  - Headless - pełna kontrola nad UI
- **Użycie**:
```vue
<script setup>
import { usePlacesAutocomplete } from 'v-use-places-autocomplete'

const { suggestions, value, setValue } = usePlacesAutocomplete({
  apiKey: 'YOUR_API_KEY'
})
</script>
```

---

## 📸 Image Upload - Upload lokalnych zdjęć

### 🏆 **Nuxt UI FileUpload** (Rekomendacja #1)
- **Strona**: https://ui.nuxt.com/docs/components/file-upload
- **Integracja**: Wbudowane w Nuxt UI
- **Licencja**: MIT
- **Zalety**:
  - Oficjalny komponent Nuxt
  - Drag & drop
  - Multiple files
  - Preview
  - Validation
- **Użycie**:
```vue
<UFileUpload v-model="files" multiple accept="image/*" />
```

### 🥈 **vue-upload-component** (Alternatywa)
- **Integracja**: `vue-upload-component`
- **Licencja**: Apache-2.0
- **Zalety**:
  - Multi-file upload
  - Drag & drop
  - Upload directory
  - Chunk upload (duże pliki)
  - Image preview
- **Instalacja**:
```bash
npm install vue-upload-component
```

### 🥉 **PrimeVue FileUpload** (Alternatywa)
- **Strona**: https://primevue.org/fileupload/
- **Integracja**: `primevue`
- **Licencja**: MIT
- **Zalety**:
  - Drag & drop
  - Auto upload
  - Progress tracking
  - Validation
  - Template customization

---

## 🎨 Dodatkowe biblioteki

### **Cropper - Przycinanie obrazków**
- **vue-advanced-cropper** - Zaawansowany cropper z pełną kontrolą
- **vue-picture-cropper** - Prosty cropper dla Vue 3

### **Color Picker - Wybór kolorów**
- **@vueform/toggle** - Toggle/switch component
- **radial-color-picker** - Minimalistyczny color picker

---

## 📦 Plan implementacji

### Faza 1: Rich Text Editor (Tiptap Extensions)
1. Dodać `@tiptap/extension-image` dla upload obrazków
2. Dodać `@tiptap/extension-table` dla tabel
3. Dodać `@tiptap/extension-color` dla kolorów tekstu
4. Dodać `@tiptap/extension-youtube` dla embed YouTube

### Faza 2: Date Picker
1. Zainstalować `v-calendar`
2. Zastąpić `<input type="datetime-local">` komponentem `<DatePicker>`
3. Dodać dark mode support
4. Dodać walidację dat

### Faza 3: Google Maps Location Picker
1. Zainstalować `vue3-google-map`
2. Uzyskać Google Maps API key
3. Utworzyć komponent `LocationPicker.vue`
4. Zintegrować z formularzem tworzenia wydarzenia
5. Zapisywać współrzędne (lat, lng) + adres

### Faza 4: Image Upload
1. Zainstalować `vue-upload-component` lub użyć Nuxt UI FileUpload
2. Skonfigurować backend endpoint dla upload obrazków
3. Zintegrować z Supabase Storage lub lokalnym storage
4. Dodać preview i cropping
5. Generować thumbnails

---

## 🔑 Wymagane klucze API

### Google Maps API
1. Przejdź do https://console.cloud.google.com/
2. Utwórz nowy projekt lub wybierz istniejący
3. Włącz następujące API:
   - Maps JavaScript API
   - Places API
   - Geocoding API
4. Utwórz klucz API
5. Dodaj do `.env`:
```env
NUXT_PUBLIC_GOOGLE_MAPS_API_KEY=your_api_key_here
```

### Supabase Storage (dla upload obrazków)
- Już skonfigurowane w projekcie
- Bucket: `news-images`
- RLS policies: Tylko zalogowani użytkownicy mogą uploadować

---

## 💰 Koszty

| Biblioteka | Licencja | Koszt |
|-----------|----------|-------|
| TinyMCE | MIT / Commercial | Free (open source) lub $49/mies (premium) |
| Tiptap | MIT | Free (open source) lub $299/mies (pro) |
| VCalendar | MIT | Free |
| vue3-google-map | MIT | Free |
| Google Maps API | Pay-as-you-go | $200 free credit/mies, potem $7/1000 requests |
| vue-upload-component | Apache-2.0 | Free |

**Rekomendacja**: Użyj darmowych wersji open source. Dla większości projektów wystarczą.

---

## 🚀 Następne kroki

1. **Zdecyduj**, które biblioteki chcesz zainstalować
2. **Uzyskaj** Google Maps API key
3. **Zainstaluj** wybrane biblioteki
4. **Zaimplementuj** komponenty
5. **Przetestuj** funkcjonalność
6. **Zaktualizuj** dokumentację

Czy chcesz, abym rozpoczął implementację którejś z tych bibliotek?

