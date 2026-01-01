import DOMPurify from 'dompurify'

export function useSanitize() {
  const sanitizeHtml = (html: string): string => {
    return DOMPurify.sanitize(html, {
      ALLOWED_TAGS: [
        'p', 'br', 'b', 'i', 'u', 'strong', 'em', 'a', 'ul', 'ol', 'li',
        'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'code',
        'img', 'table', 'thead', 'tbody', 'tr', 'th', 'td', 'span', 'div'
      ],
      ALLOWED_ATTR: ['href', 'src', 'alt', 'class', 'style', 'target', 'rel'],
      ALLOW_DATA_ATTR: false
    })
  }

  return { sanitizeHtml }
}
