type SuggestInputProps = {
  id: string
  value: string
  options: string[]
  onChange: (value: string) => void
  required?: boolean
  disabled?: boolean
  type?: 'text' | 'email'
  autoComplete?: string
}

/** Text input with datalist: pick an existing suggestion or type a new value. */
export function SuggestInput({
  id,
  value,
  options,
  onChange,
  required,
  disabled,
  type = 'text',
  autoComplete = 'off',
}: SuggestInputProps) {
  const listId = `${id}-suggestions`

  return (
    <>
      <input
        id={id}
        type={type}
        list={listId}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        required={required}
        disabled={disabled}
        autoComplete={autoComplete}
      />
      <datalist id={listId}>
        {options.map((opt) => (
          <option key={opt} value={opt} />
        ))}
      </datalist>
    </>
  )
}
