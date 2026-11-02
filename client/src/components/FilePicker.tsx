import { useId, useRef, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'

type FilePickerProps = {
  accept?: string
  multiple?: boolean
  disabled?: boolean
  files?: File[]
  /** Single-file mode display when `multiple` is false. */
  file?: File | null
  onFilesChange?: (files: File[]) => void
  onFileChange?: (file: File | null) => void
  className?: string
}

/** Custom file control so button/status text follows app i18n, not OS/browser locale. */
export function FilePicker({
  accept,
  multiple = false,
  disabled = false,
  files = [],
  file = null,
  onFilesChange,
  onFileChange,
  className,
}: FilePickerProps) {
  const { t } = useTranslation()
  const inputRef = useRef<HTMLInputElement>(null)
  const inputId = useId()

  const selected = multiple ? files : file ? [file] : []
  const statusText =
    selected.length === 0
      ? t('filePicker.noneSelected')
      : selected.map((f) => f.name).join(', ')

  function onChange(e: ChangeEvent<HTMLInputElement>) {
    const list = Array.from(e.target.files ?? [])
    if (multiple) {
      onFilesChange?.(list)
    } else {
      onFileChange?.(list[0] ?? null)
    }
  }

  return (
    <div className={className ? `file-picker ${className}` : 'file-picker'}>
      <input
        ref={inputRef}
        id={inputId}
        type="file"
        accept={accept}
        multiple={multiple}
        disabled={disabled}
        className="file-picker-input"
        onChange={onChange}
      />
      <button
        type="button"
        className="btn ghost file-picker-btn"
        disabled={disabled}
        onClick={() => inputRef.current?.click()}
      >
        {multiple ? t('filePicker.chooseFiles') : t('filePicker.chooseFile')}
      </button>
      <span className="file-picker-status muted" title={statusText}>
        {statusText}
      </span>
    </div>
  )
}
