<script setup lang="ts">
import { useEditor, EditorContent } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Link from '@tiptap/extension-link'
import Image from '@tiptap/extension-image'
import { Table } from '@tiptap/extension-table'
import TableRow from '@tiptap/extension-table-row'
import TableCell from '@tiptap/extension-table-cell'
import TableHeader from '@tiptap/extension-table-header'
import TextAlign from '@tiptap/extension-text-align'
import Underline from '@tiptap/extension-underline'
import { TextStyle } from '@tiptap/extension-text-style'
import Color from '@tiptap/extension-color'
import Youtube from '@tiptap/extension-youtube'
import CodeBlock from '@tiptap/extension-code-block'

interface Props {
  modelValue: string
}

interface Emits {
  (e: 'update:modelValue', value: string): void
  (e: 'hashtagsDetected', hashtags: string[]): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()
const promptModal = usePromptModal()

const editor = useEditor({
  content: props.modelValue,
  extensions: [
    StarterKit.configure({
      codeBlock: false, // Disable default codeBlock to use custom CodeBlock extension
    }),
    Link.configure({
      openOnClick: false,
    }),
    Image.configure({
      inline: true,
      allowBase64: true,
    }),
    Table.configure({
      resizable: true,
    }),
    TableRow,
    TableCell,
    TableHeader,
    TextAlign.configure({
      types: ['heading', 'paragraph'],
    }),
    Underline,
    TextStyle,
    Color,
    Youtube.configure({
      width: 640,
      height: 480,
    }),
    CodeBlock.configure({
      languageClassPrefix: 'language-',
      HTMLAttributes: {
        class: 'code-block',
      },
    }),
  ],
  onUpdate: ({ editor }) => {
    const html = editor.getHTML()
    emit('update:modelValue', html)
    
    // Detect hashtags
    const hashtagPattern = /#[\w]+/g
    const matches = html.match(hashtagPattern)
    if (matches && matches.length > 0) {
      const uniqueHashtags = [...new Set(matches)]
      emit('hashtagsDetected', uniqueHashtags)
    }
  },
})

watch(() => props.modelValue, (value) => {
  if (editor.value && value !== editor.value.getHTML()) {
    editor.value.commands.setContent(value, { emitUpdate: false })
  }
})

onBeforeUnmount(() => {
  editor.value?.destroy()
})

function toggleBold() {
  editor.value?.chain().focus().toggleBold().run()
}

function toggleItalic() {
  editor.value?.chain().focus().toggleItalic().run()
}

function toggleUnderline() {
  editor.value?.chain().focus().toggleUnderline().run()
}

function toggleHeading(level: 1 | 2 | 3) {
  editor.value?.chain().focus().toggleHeading({ level }).run()
}

function toggleBulletList() {
  editor.value?.chain().focus().toggleBulletList().run()
}

function toggleOrderedList() {
  editor.value?.chain().focus().toggleOrderedList().run()
}

function setTextAlign(align: 'left' | 'center' | 'right' | 'justify') {
  editor.value?.chain().focus().setTextAlign(align).run()
}

async function setTextColor() {
  const color = await promptModal.show({
    title: 'Kolor tekstu',
    message: 'Wprowadź kolor tekstu',
    placeholder: 'np. #ff0000 lub red'
  })
  if (color) {
    editor.value?.chain().focus().setColor(color).run()
  }
}

async function addLink() {
  const url = await promptModal.show({
    title: 'Dodaj link',
    message: 'Wprowadź adres URL',
    placeholder: 'https://example.com'
  })
  if (url) {
    editor.value?.chain().focus().setLink({ href: url }).run()
  }
}

function removeLink() {
  editor.value?.chain().focus().unsetLink().run()
}

async function addImage() {
  const url = await promptModal.show({
    title: 'Dodaj obrazek',
    message: 'Wprowadź adres URL obrazka',
    placeholder: 'https://example.com/image.jpg'
  })
  if (url) {
    editor.value?.chain().focus().setImage({ src: url }).run()
  }
}

async function addYoutube() {
  const url = await promptModal.show({
    title: 'Dodaj wideo YouTube',
    message: 'Wprowadź adres URL wideo z YouTube',
    placeholder: 'https://www.youtube.com/watch?v=...'
  })
  if (url) {
    editor.value?.chain().focus().setYoutubeVideo({ src: url }).run()
  }
}

function insertTable() {
  editor.value?.chain().focus().insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run()
}

function deleteTable() {
  editor.value?.chain().focus().deleteTable().run()
}

function addColumnBefore() {
  editor.value?.chain().focus().addColumnBefore().run()
}

function addColumnAfter() {
  editor.value?.chain().focus().addColumnAfter().run()
}

function deleteColumn() {
  editor.value?.chain().focus().deleteColumn().run()
}

function addRowBefore() {
  editor.value?.chain().focus().addRowBefore().run()
}

function addRowAfter() {
  editor.value?.chain().focus().addRowAfter().run()
}

function deleteRow() {
  editor.value?.chain().focus().deleteRow().run()
}

function toggleCodeBlock() {
  editor.value?.chain().focus().toggleCodeBlock().run()
}
</script>

<template>
  <div class="border border-gray-300 dark:border-gray-600 rounded-lg overflow-hidden bg-white dark:bg-gray-800">
    <div v-if="editor" class="bg-gray-100 dark:bg-gray-700 border-b border-gray-300 dark:border-gray-600 p-2 flex flex-wrap gap-1" data-testid="rich-editor-toolbar">
      <!-- Formatowanie tekstu -->
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('bold') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 font-bold text-gray-700 dark:text-gray-200 transition-colors"
        title="Pogrubienie"
        data-testid="rich-editor-bold-btn"
        @click="toggleBold"
      >
        B
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('italic') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 italic text-gray-700 dark:text-gray-200 transition-colors"
        title="Kursywa"
        data-testid="rich-editor-italic-btn"
        @click="toggleItalic"
      >
        I
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('underline') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 underline text-gray-700 dark:text-gray-200 transition-colors"
        title="Podkreślenie"
        data-testid="rich-editor-underline-btn"
        @click="toggleUnderline"
      >
        U
      </button>

      <div class="w-px bg-gray-300 dark:bg-gray-600 mx-1"/>

      <!-- Nagłówki -->
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('heading', { level: 1 }) }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Nagłówek 1"
        data-testid="rich-editor-h1-btn"
        @click="toggleHeading(1)"
      >
        H1
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('heading', { level: 2 }) }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Nagłówek 2"
        data-testid="rich-editor-h2-btn"
        @click="toggleHeading(2)"
      >
        H2
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('heading', { level: 3 }) }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Nagłówek 3"
        data-testid="rich-editor-h3-btn"
        @click="toggleHeading(3)"
      >
        H3
      </button>

      <div class="w-px bg-gray-300 dark:bg-gray-600 mx-1"/>

      <!-- Wyrównanie -->
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive({ textAlign: 'left' }) }"
        class="px-2 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Wyrównaj do lewej"
        data-testid="rich-editor-align-left-btn"
        @click="setTextAlign('left')"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h10M4 18h16" />
        </svg>
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive({ textAlign: 'center' }) }"
        class="px-2 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Wyśrodkuj"
        data-testid="rich-editor-align-center-btn"
        @click="setTextAlign('center')"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M8 12h8M4 18h16" />
        </svg>
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive({ textAlign: 'right' }) }"
        class="px-2 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Wyrównaj do prawej"
        data-testid="rich-editor-align-right-btn"
        @click="setTextAlign('right')"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M10 12h10M4 18h16" />
        </svg>
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive({ textAlign: 'justify' }) }"
        class="px-2 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Wyjustuj"
        data-testid="rich-editor-align-justify-btn"
        @click="setTextAlign('justify')"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
        </svg>
      </button>

      <div class="w-px bg-gray-300 dark:bg-gray-600 mx-1"/>

      <!-- Listy -->
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('bulletList') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Lista punktowana"
        data-testid="rich-editor-bullet-list-btn"
        @click="toggleBulletList"
      >
        • Lista
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('orderedList') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Lista numerowana"
        data-testid="rich-editor-ordered-list-btn"
        @click="toggleOrderedList"
      >
        1. Lista
      </button>

      <div class="w-px bg-gray-300 dark:bg-gray-600 mx-1"/>

      <!-- Code Block -->
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('codeBlock') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors font-mono"
        title="Blok kodu"
        data-testid="rich-editor-code-btn"
        @click="toggleCodeBlock"
      >
        &lt;/&gt; Kod
      </button>

      <div class="w-px bg-gray-300 dark:bg-gray-600 mx-1"/>

      <!-- Kolor tekstu -->
      <button
        type="button"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Kolor tekstu"
        data-testid="rich-editor-color-btn"
        @click="setTextColor"
      >
        🎨 Kolor
      </button>

      <div class="w-px bg-gray-300 dark:bg-gray-600 mx-1"/>

      <!-- Link -->
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('link') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Dodaj link"
        data-testid="rich-editor-link-btn"
        @click="addLink"
      >
        🔗 Link
      </button>
      <button
        v-if="editor.isActive('link')"
        type="button"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Usuń link"
        data-testid="rich-editor-unlink-btn"
        @click="removeLink"
      >
        ❌ Unlink
      </button>

      <div class="w-px bg-gray-300 dark:bg-gray-600 mx-1"/>

      <!-- Media -->
      <button
        type="button"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Dodaj obrazek"
        data-testid="rich-editor-image-btn"
        @click="addImage"
      >
        🖼️ Obrazek
      </button>
      <button
        type="button"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Dodaj YouTube"
        data-testid="rich-editor-youtube-btn"
        @click="addYoutube"
      >
        ▶️ YouTube
      </button>

      <div class="w-px bg-gray-300 dark:bg-gray-600 mx-1"/>

      <!-- Tabela -->
      <button
        type="button"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        title="Wstaw tabelę"
        data-testid="rich-editor-table-btn"
        @click="insertTable"
      >
        📊 Tabela
      </button>
      <template v-if="editor.isActive('table')">
        <button
          type="button"
          class="px-2 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors text-xs"
          title="Dodaj kolumnę przed"
          @click="addColumnBefore"
        >
          ⬅️ Kol
        </button>
        <button
          type="button"
          class="px-2 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors text-xs"
          title="Dodaj kolumnę po"
          @click="addColumnAfter"
        >
          Kol ➡️
        </button>
        <button
          type="button"
          class="px-2 py-1 rounded hover:bg-red-100 dark:hover:bg-red-900 text-gray-700 dark:text-gray-200 transition-colors text-xs"
          title="Usuń kolumnę"
          @click="deleteColumn"
        >
          ❌ Kol
        </button>
        <button
          type="button"
          class="px-2 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors text-xs"
          title="Dodaj wiersz przed"
          @click="addRowBefore"
        >
          ⬆️ Wiersz
        </button>
        <button
          type="button"
          class="px-2 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors text-xs"
          title="Dodaj wiersz po"
          @click="addRowAfter"
        >
          Wiersz ⬇️
        </button>
        <button
          type="button"
          class="px-2 py-1 rounded hover:bg-red-100 dark:hover:bg-red-900 text-gray-700 dark:text-gray-200 transition-colors text-xs"
          title="Usuń wiersz"
          @click="deleteRow"
        >
          ❌ Wiersz
        </button>
        <button
          type="button"
          class="px-2 py-1 rounded hover:bg-red-100 dark:hover:bg-red-900 text-gray-700 dark:text-gray-200 transition-colors text-xs"
          title="Usuń tabelę"
          @click="deleteTable"
        >
          ❌ Tabela
        </button>
      </template>
    </div>
    <EditorContent :editor="editor" class="prose dark:prose-invert max-w-none p-4 min-h-[300px] focus:outline-none text-gray-900 dark:text-gray-100" data-testid="rich-editor-content" />
  </div>
</template>

<style>
.ProseMirror {
  min-height: 300px;
  outline: none;
}

.ProseMirror p {
  margin: 0.5rem 0;
}

.ProseMirror h1,
.ProseMirror h2,
.ProseMirror h3 {
  margin: 1rem 0 0.5rem;
  font-weight: bold;
}

.ProseMirror h1 {
  font-size: 2rem;
}

.ProseMirror h2 {
  font-size: 1.5rem;
}

.ProseMirror h3 {
  font-size: 1.25rem;
}

.ProseMirror ul,
.ProseMirror ol {
  padding-left: 1.5rem;
  margin: 0.5rem 0;
}

.ProseMirror a {
  color: #3b82f6;
  text-decoration: underline;
}

.dark .ProseMirror a {
  color: #60a5fa;
}

/* Tabele */
.ProseMirror table {
  border-collapse: collapse;
  table-layout: fixed;
  width: 100%;
  margin: 1rem 0;
  overflow: hidden;
}

.ProseMirror td,
.ProseMirror th {
  min-width: 1em;
  border: 2px solid #cbd5e1;
  padding: 0.5rem;
  vertical-align: top;
  box-sizing: border-box;
  position: relative;
}

.dark .ProseMirror td,
.dark .ProseMirror th {
  border-color: #475569;
}

.ProseMirror th {
  font-weight: bold;
  text-align: left;
  background-color: #f1f5f9;
}

.dark .ProseMirror th {
  background-color: #334155;
}

.ProseMirror .selectedCell:after {
  z-index: 2;
  position: absolute;
  content: "";
  left: 0;
  right: 0;
  top: 0;
  bottom: 0;
  background: rgba(59, 130, 246, 0.1);
  pointer-events: none;
}

.ProseMirror .column-resize-handle {
  position: absolute;
  right: -2px;
  top: 0;
  bottom: -2px;
  width: 4px;
  background-color: #3b82f6;
  pointer-events: none;
}

.ProseMirror.resize-cursor {
  cursor: ew-resize;
  cursor: col-resize;
}

/* Obrazki */
.ProseMirror img {
  max-width: 100%;
  height: auto;
  margin: 0.5rem 0;
  border-radius: 0.25rem;
}

/* YouTube */
.ProseMirror iframe {
  max-width: 100%;
  margin: 1rem 0;
  border-radius: 0.5rem;
}

/* Wyrównanie tekstu */
.ProseMirror [style*="text-align: left"] {
  text-align: left;
}

.ProseMirror [style*="text-align: center"] {
  text-align: center;
}

.ProseMirror [style*="text-align: right"] {
  text-align: right;
}

.ProseMirror [style*="text-align: justify"] {
  text-align: justify;
}

/* Code blocks */
.ProseMirror pre {
  background-color: #f1f5f9;
  border: 1px solid #cbd5e1;
  border-radius: 0.5rem;
  padding: 1rem;
  margin: 1rem 0;
  overflow-x: auto;
  font-family: 'Courier New', Courier, monospace;
  font-size: 0.875rem;
  line-height: 1.5;
}

.dark .ProseMirror pre {
  background-color: #1e293b;
  border-color: #475569;
  color: #e2e8f0;
}

.ProseMirror code {
  background-color: #f1f5f9;
  padding: 0.125rem 0.25rem;
  border-radius: 0.25rem;
  font-family: 'Courier New', Courier, monospace;
  font-size: 0.875em;
}

.dark .ProseMirror code {
  background-color: #1e293b;
  color: #e2e8f0;
}

.ProseMirror pre code {
  background-color: transparent;
  padding: 0;
  border-radius: 0;
  color: inherit;
}
</style>
