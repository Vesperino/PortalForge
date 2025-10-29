<script setup lang="ts">
import { useEditor, EditorContent } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Link from '@tiptap/extension-link'
import Image from '@tiptap/extension-image'

interface Props {
  modelValue: string
}

interface Emits {
  (e: 'update:modelValue', value: string): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const editor = useEditor({
  content: props.modelValue,
  extensions: [
    StarterKit,
    Link.configure({
      openOnClick: false,
    }),
    Image,
  ],
  onUpdate: ({ editor }) => {
    emit('update:modelValue', editor.getHTML())
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

function toggleHeading(level: 1 | 2 | 3) {
  editor.value?.chain().focus().toggleHeading({ level }).run()
}

function toggleBulletList() {
  editor.value?.chain().focus().toggleBulletList().run()
}

function toggleOrderedList() {
  editor.value?.chain().focus().toggleOrderedList().run()
}

function addLink() {
  const url = window.prompt('Enter URL:')
  if (url) {
    editor.value?.chain().focus().setLink({ href: url }).run()
  }
}

function removeLink() {
  editor.value?.chain().focus().unsetLink().run()
}
</script>

<template>
  <div class="border border-gray-300 dark:border-gray-600 rounded-lg overflow-hidden bg-white dark:bg-gray-800">
    <div v-if="editor" class="bg-gray-100 dark:bg-gray-700 border-b border-gray-300 dark:border-gray-600 p-2 flex flex-wrap gap-1">
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('bold') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 font-bold text-gray-700 dark:text-gray-200 transition-colors"
        @click="toggleBold"
      >
        B
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('italic') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 italic text-gray-700 dark:text-gray-200 transition-colors"
        @click="toggleItalic"
      >
        I
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('heading', { level: 1 }) }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        @click="toggleHeading(1)"
      >
        H1
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('heading', { level: 2 }) }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        @click="toggleHeading(2)"
      >
        H2
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('heading', { level: 3 }) }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        @click="toggleHeading(3)"
      >
        H3
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('bulletList') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        @click="toggleBulletList"
      >
        • List
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('orderedList') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        @click="toggleOrderedList"
      >
        1. List
      </button>
      <button
        type="button"
        :class="{ 'bg-blue-500 text-white dark:bg-blue-600': editor.isActive('link') }"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        @click="addLink"
      >
        Link
      </button>
      <button
        v-if="editor.isActive('link')"
        type="button"
        class="px-3 py-1 rounded hover:bg-blue-100 dark:hover:bg-blue-900 text-gray-700 dark:text-gray-200 transition-colors"
        @click="removeLink"
      >
        Unlink
      </button>
    </div>
    <EditorContent :editor="editor" class="prose dark:prose-invert max-w-none p-4 min-h-[300px] focus:outline-none text-gray-900 dark:text-gray-100" />
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
</style>
