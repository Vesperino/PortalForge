<script setup lang="ts">
interface Props {
  hashtags: string[]
  clickable?: boolean
  size?: 'sm' | 'md' | 'lg'
}

interface Emits {
  (e: 'tagClick', tag: string): void
}

const props = withDefaults(defineProps<Props>(), {
  clickable: true,
  size: 'md'
})

const emit = defineEmits<Emits>()

const router = useRouter()

function handleTagClick(tag: string) {
  if (!props.clickable) return
  
  emit('tagClick', tag)
  
  // Navigate to news list with hashtag filter
  router.push(`/dashboard/news?hashtag=${encodeURIComponent(tag.replace('#', ''))}`)
}

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'px-2 py-0.5 text-xs'
    case 'lg':
      return 'px-4 py-2 text-base'
    default:
      return 'px-3 py-1 text-sm'
  }
})
</script>

<template>
  <div v-if="hashtags && hashtags.length > 0" class="flex flex-wrap gap-2">
    <component
      :is="clickable ? 'button' : 'span'"
      v-for="tag in hashtags"
      :key="tag"
      type="button"
      :class="[
        sizeClasses,
        'inline-flex items-center gap-1 rounded-full font-medium transition-all',
        'bg-blue-100 dark:bg-blue-900/50 text-blue-800 dark:text-blue-200',
        clickable ? 'hover:bg-blue-200 dark:hover:bg-blue-800 hover:scale-105 cursor-pointer' : 'cursor-default'
      ]"
      @click="handleTagClick(tag)"
    >
      <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 20l4-16m2 16l4-16M6 9h14M4 15h14" />
      </svg>
      {{ tag.startsWith('#') ? tag.substring(1) : tag }}
    </component>
  </div>
</template>




