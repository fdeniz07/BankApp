import type { DownloadedFile } from '../api/client'

export function saveDownloadedFile(file: DownloadedFile) {
  const url = URL.createObjectURL(file.blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = file.fileName
  anchor.rel = 'noopener'
  document.body.appendChild(anchor)
  anchor.click()
  anchor.remove()
  URL.revokeObjectURL(url)
}

/** Open the file in a new tab and trigger the browser print dialog (PDF-friendly). */
export function printDownloadedFile(file: DownloadedFile) {
  const url = URL.createObjectURL(file.blob)
  const popup = window.open(url, '_blank', 'noopener,noreferrer')
  if (!popup) {
    URL.revokeObjectURL(url)
    throw new Error('popup-blocked')
  }

  const revoke = () => URL.revokeObjectURL(url)
  popup.addEventListener('load', () => {
    try {
      popup.focus()
      popup.print()
    } finally {
      // Keep blob alive briefly so the print preview can load.
      window.setTimeout(revoke, 60_000)
    }
  })
  // Fallback if load never fires (some browsers with PDF plugins).
  window.setTimeout(() => {
    try {
      popup.print()
    } catch {
      // ignore
    }
  }, 750)
}
