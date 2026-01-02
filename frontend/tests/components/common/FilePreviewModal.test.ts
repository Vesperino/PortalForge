import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'
import FilePreviewModal from '~/components/common/FilePreviewModal.vue'

describe('FilePreviewModal', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    global.fetch = vi.fn()

    Object.defineProperty(window, 'addEventListener', {
      value: vi.fn(),
      writable: true
    })

    Object.defineProperty(window, 'removeEventListener', {
      value: vi.fn(),
      writable: true
    })
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  const defaultProps = {
    visible: true,
    fileUrl: 'https://example.com/file.pdf',
    fileName: 'document.pdf'
  }

  describe('Modal Visibility', () => {
    it('should not render when visible is false', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          visible: false
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.find('.fixed.inset-0').exists()).toBe(false)
    })

    it('should render when visible is true', () => {
      const wrapper = mount(FilePreviewModal, {
        props: defaultProps,
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.find('.fixed.inset-0').exists()).toBe(true)
    })

    it('should emit update:visible when close button is clicked', async () => {
      const wrapper = mount(FilePreviewModal, {
        props: defaultProps,
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const closeButton = wrapper.find('button[title="Zamknij (ESC)"]')
      await closeButton.trigger('click')

      expect(wrapper.emitted('update:visible')).toBeTruthy()
      expect(wrapper.emitted('update:visible')![0]).toEqual([false])
    })

    it('should emit update:visible when clicking backdrop', async () => {
      const wrapper = mount(FilePreviewModal, {
        props: defaultProps,
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const backdrop = wrapper.find('.fixed.inset-0')
      await backdrop.trigger('click')

      expect(wrapper.emitted('update:visible')).toBeTruthy()
      expect(wrapper.emitted('update:visible')![0]).toEqual([false])
    })
  })

  describe('File Type Detection', () => {
    it('should detect PDF file type', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'document.pdf'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.text()).toContain('pdf')
    })

    it('should detect image file types', () => {
      const imageExtensions = ['png', 'jpg', 'jpeg', 'gif', 'webp', 'bmp', 'svg']

      imageExtensions.forEach(ext => {
        const wrapper = mount(FilePreviewModal, {
          props: {
            ...defaultProps,
            fileName: `image.${ext}`,
            fileUrl: `https://example.com/image.${ext}`
          },
          global: {
            stubs: {
              Transition: false,
              ClientOnly: true
            }
          }
        })

        expect(wrapper.text()).toContain(ext)
      })
    })

    it('should detect DOCX file type', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'document.docx'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.text()).toContain('docx')
    })

    it('should detect text file types', () => {
      const textExtensions = ['txt', 'log', 'md']

      textExtensions.forEach(ext => {
        const wrapper = mount(FilePreviewModal, {
          props: {
            ...defaultProps,
            fileName: `file.${ext}`
          },
          global: {
            stubs: {
              Transition: false,
              ClientOnly: true
            }
          }
        })

        expect(wrapper.text()).toContain(ext)
      })
    })

    it('should show unsupported file type message for unknown extensions', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'archive.zip'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.text()).toContain('Podgląd niedostępny')
      expect(wrapper.text()).toContain('ZIP')
    })
  })

  describe('Header Information', () => {
    it('should display file name in header', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'test-document.pdf'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.find('h3').text()).toBe('test-document.pdf')
    })

    it('should display file extension in header', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'document.pdf'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const headerInfo = wrapper.find('.text-sm.text-gray-500')
      expect(headerInfo.text()).toContain('pdf')
    })
  })

  describe('Image Preview Controls', () => {
    it('should show image controls for image files', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg',
          fileUrl: 'https://example.com/photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.find('button[title="Pomniejsz"]').exists()).toBe(true)
      expect(wrapper.find('button[title="Powiększ"]').exists()).toBe(true)
      expect(wrapper.find('button[title="Obróć"]').exists()).toBe(true)
    })

    it('should not show image controls for non-image files', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'document.pdf'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.find('button[title="Pomniejsz"]').exists()).toBe(false)
      expect(wrapper.find('button[title="Powiększ"]').exists()).toBe(false)
      expect(wrapper.find('button[title="Obróć"]').exists()).toBe(false)
    })

    it('should display current zoom level', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg',
          fileUrl: 'https://example.com/photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.text()).toContain('100%')
    })

    it('should increase zoom when zoom in button is clicked', async () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg',
          fileUrl: 'https://example.com/photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const zoomInButton = wrapper.find('button[title="Powiększ"]')
      await zoomInButton.trigger('click')

      expect(wrapper.text()).toContain('110%')
    })

    it('should decrease zoom when zoom out button is clicked', async () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg',
          fileUrl: 'https://example.com/photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const zoomOutButton = wrapper.find('button[title="Pomniejsz"]')
      await zoomOutButton.trigger('click')

      expect(wrapper.text()).toContain('90%')
    })

    it('should not zoom below 50%', async () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg',
          fileUrl: 'https://example.com/photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const zoomOutButton = wrapper.find('button[title="Pomniejsz"]')

      for (let i = 0; i < 10; i++) {
        await zoomOutButton.trigger('click')
      }

      expect(wrapper.text()).toContain('50%')
    })

    it('should not zoom above 200%', async () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg',
          fileUrl: 'https://example.com/photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const zoomInButton = wrapper.find('button[title="Powiększ"]')

      for (let i = 0; i < 15; i++) {
        await zoomInButton.trigger('click')
      }

      expect(wrapper.text()).toContain('200%')
    })
  })

  describe('Fullscreen Toggle', () => {
    it('should toggle fullscreen mode', async () => {
      const wrapper = mount(FilePreviewModal, {
        props: defaultProps,
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const fullscreenButton = wrapper.find('button[title="Pełny ekran"]')
      expect(fullscreenButton.exists()).toBe(true)

      await fullscreenButton.trigger('click')

      const normalViewButton = wrapper.find('button[title="Normalny widok"]')
      expect(normalViewButton.exists()).toBe(true)
    })
  })

  describe('Download Button', () => {
    it('should have download button', () => {
      const wrapper = mount(FilePreviewModal, {
        props: defaultProps,
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const downloadButton = wrapper.find('button[title="Pobierz plik"]')
      expect(downloadButton.exists()).toBe(true)
    })
  })

  describe('Loading State', () => {
    it('should show loading spinner for previewable files', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.find('.animate-spin').exists()).toBe(true)
      expect(wrapper.text()).toContain('Ładowanie pliku...')
    })
  })

  describe('Image Preview', () => {
    it('should render image element for image files', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg',
          fileUrl: 'https://example.com/photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const img = wrapper.find('img')
      expect(img.exists()).toBe(true)
      expect(img.attributes('src')).toBe('https://example.com/photo.jpg')
      expect(img.attributes('alt')).toBe('photo.jpg')
    })

    it('should apply zoom and rotation styles to image', async () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg',
          fileUrl: 'https://example.com/photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const img = wrapper.find('img')
      const initialStyle = img.attributes('style')
      expect(initialStyle).toContain('scale(1)')
      expect(initialStyle).toContain('rotate(0deg)')

      const rotateButton = wrapper.find('button[title="Obróć"]')
      await rotateButton.trigger('click')

      await nextTick()

      const rotatedStyle = wrapper.find('img').attributes('style')
      expect(rotatedStyle).toContain('rotate(90deg)')
    })
  })

  describe('Unsupported File Types', () => {
    it('should show download prompt for unsupported files', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'archive.rar'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.text()).toContain('Podgląd niedostępny')
      expect(wrapper.text()).toContain('RAR')
      expect(wrapper.text()).toContain('Pobierz plik')
    })

    it('should show file extension in unsupported message', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'spreadsheet.xlsx'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.text()).toContain('XLSX')
    })
  })

  describe('Modal Container', () => {
    it('should have proper modal container classes', () => {
      const wrapper = mount(FilePreviewModal, {
        props: defaultProps,
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const modal = wrapper.find('.bg-white.dark\\:bg-gray-800.rounded-2xl')
      expect(modal.exists()).toBe(true)
    })

    it('should prevent event propagation when clicking modal content', async () => {
      const wrapper = mount(FilePreviewModal, {
        props: defaultProps,
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const modalContent = wrapper.find('.bg-white.dark\\:bg-gray-800')
      await modalContent.trigger('click')

      expect(wrapper.emitted('update:visible')).toBeFalsy()
    })
  })

  describe('Keyboard Navigation', () => {
    it('should have ESC key close button hint', () => {
      const wrapper = mount(FilePreviewModal, {
        props: defaultProps,
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      // The close button should have ESC key hint in title
      const closeButton = wrapper.find('button[title="Zamknij (ESC)"]')
      expect(closeButton.exists()).toBe(true)
    })
  })

  describe('Dark Mode Support', () => {
    it('should have dark mode classes', () => {
      const wrapper = mount(FilePreviewModal, {
        props: defaultProps,
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      const html = wrapper.html()
      expect(html).toContain('dark:bg-gray-800')
      expect(html).toContain('dark:text-white')
      expect(html).toContain('dark:border-gray-700')
    })
  })

  describe('Accessibility', () => {
    it('should have descriptive button titles', () => {
      const wrapper = mount(FilePreviewModal, {
        props: {
          ...defaultProps,
          fileName: 'photo.jpg'
        },
        global: {
          stubs: {
            Transition: false,
            ClientOnly: true
          }
        }
      })

      expect(wrapper.find('button[title="Pomniejsz"]').exists()).toBe(true)
      expect(wrapper.find('button[title="Powiększ"]').exists()).toBe(true)
      expect(wrapper.find('button[title="Obróć"]').exists()).toBe(true)
      expect(wrapper.find('button[title="Pobierz plik"]').exists()).toBe(true)
      expect(wrapper.find('button[title="Zamknij (ESC)"]').exists()).toBe(true)
    })
  })
})
