/**
 * Icon mapping for request templates
 * Maps internal icon names to Iconify icon names
 */

export const iconMapping: Record<string, string> = {
  // Urlopy i czas wolny
  'beach-umbrella': 'fluent-emoji-flat:beach-with-umbrella',
  'plane': 'fluent-emoji-flat:airplane',
  'palm-tree': 'fluent-emoji-flat:palm-tree',
  'tent': 'fluent-emoji-flat:camping',
  'bed': 'fluent-emoji-flat:bed',
  'calendar': 'fluent-emoji-flat:calendar',
  'sunglasses': 'fluent-emoji-flat:sunglasses',
  'mountain': 'fluent-emoji-flat:mountain',
  'home-office': 'fluent-emoji-flat:house',
  'clock': 'fluent-emoji-flat:clock',

  // Sprzęt IT i technologia
  'laptop': 'fluent-emoji-flat:laptop',
  'computer': 'fluent-emoji-flat:desktop-computer',
  'phone': 'fluent-emoji-flat:mobile-phone',
  'printer': 'fluent-emoji-flat:printer',
  'keyboard': 'fluent-emoji-flat:keyboard',
  'monitor': 'fluent-emoji-flat:desktop-computer',
  'toolbox': 'fluent-emoji-flat:toolbox',
  'battery': 'fluent-emoji-flat:battery',

  // HR i Kadry
  'briefcase': 'fluent-emoji-flat:briefcase',
  'id-card': 'fluent-emoji-flat:identification-card',
  'family': 'fluent-emoji-flat:family',
  'handshake': 'fluent-emoji-flat:handshake',
  'clipboard-hr': 'fluent-emoji-flat:clipboard',

  // Finanse i budżet
  'money-bag': 'fluent-emoji-flat:money-bag',
  'credit-card': 'fluent-emoji-flat:credit-card',
  'dollar': 'fluent-emoji-flat:dollar-banknote',
  'chart': 'fluent-emoji-flat:chart-increasing',
  'receipt': 'fluent-emoji-flat:receipt',

  // Biuro i infrastruktura
  'office': 'fluent-emoji-flat:office-building',
  'desk': 'fluent-emoji-flat:desk',
  'key': 'fluent-emoji-flat:key',
  'parking': 'fluent-emoji-flat:automobile',
  'light-bulb': 'fluent-emoji-flat:light-bulb',

  // Zdrowie i bezpieczeństwo
  'medical': 'fluent-emoji-flat:hospital',
  'first-aid': 'fluent-emoji-flat:adhesive-bandage',
  'sick': 'fluent-emoji-flat:face-with-thermometer',
  'shield': 'fluent-emoji-flat:shield',
  'warning': 'fluent-emoji-flat:warning',

  // Szkolenia i rozwój
  'graduation': 'fluent-emoji-flat:graduation-cap',
  'books': 'fluent-emoji-flat:books',
  'trophy': 'fluent-emoji-flat:trophy',
  'teacher': 'fluent-emoji-flat:teacher',
  'rocket': 'fluent-emoji-flat:rocket',

  // Dokumenty
  'document': 'fluent-emoji-flat:page-facing-up',
  'folder': 'fluent-emoji-flat:file-folder',
  'memo': 'fluent-emoji-flat:memo',
  'signature': 'fluent-emoji-flat:pen',

  // Ogólne
  'checkmark': 'fluent-emoji-flat:check-mark-button',
  'bell': 'fluent-emoji-flat:bell',
  'megaphone': 'fluent-emoji-flat:megaphone',

  // Backward compatibility - old icon names
  'FileText': 'fluent-emoji-flat:page-facing-up',
  'Laptop': 'fluent-emoji-flat:laptop',
  'Users': 'heroicons:user-group',
  'DollarSign': 'fluent-emoji-flat:dollar-banknote',
  'Briefcase': 'fluent-emoji-flat:briefcase'
}

/**
 * Get Iconify icon name from internal icon name
 */
export function useIconMapping() {
  const getIconifyName = (iconName: string): string => {
    return iconMapping[iconName] || 'heroicons:question-mark-circle'
  }

  return {
    iconMapping,
    getIconifyName
  }
}
