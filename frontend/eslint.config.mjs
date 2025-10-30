// @ts-check
import { createConfigForNuxt } from '@nuxt/eslint-config/flat'

export default createConfigForNuxt({
  features: {
    tooling: true,
    stylistic: false,
  },
  dirs: {
    src: ['.'],
  },
})
  .append(
    // Your custom configs here
    {
      files: ['**/*.ts', '**/*.tsx', '**/*.vue', '**/*.js', '**/*.mjs'],
      rules: {
        // Vue/Nuxt specific rules
        'vue/multi-word-component-names': 'off',
        'vue/no-v-html': 'warn',

        // TypeScript rules
        '@typescript-eslint/no-explicit-any': 'warn',
        '@typescript-eslint/no-unused-vars': ['error', {
          argsIgnorePattern: '^_',
          varsIgnorePattern: '^_',
        }],

        // General rules
        'no-console': process.env.NODE_ENV === 'production' ? 'warn' : 'off',
        'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
      },
    },
    // Disable specific rules for specific files
    {
      files: ['components/LocationPickerOSM.vue'],
      rules: {
        '@typescript-eslint/unified-signatures': 'off',
        'prefer-const': 'off',
        'unicorn/prefer-number-properties': 'off',
      },
    },
    {
      files: ['components/RichTextEditor.vue'],
      rules: {
        'regexp/prefer-w': 'off',
        'regexp/no-useless-character-class': 'off',
      },
    },
    {
      files: ['pages/admin/settings/system.vue'],
      rules: {
        '@typescript-eslint/no-unused-vars': 'off',
      },
    },
  )
