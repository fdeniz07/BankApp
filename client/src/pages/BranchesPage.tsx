import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { createBranch, getBranches, updateBranch } from '../api/branchesApi'
import { useAuth } from '../auth/AuthContext'
import type { BranchListItem } from '../types/api'
import { currencySymbol, formatMoney } from '../utils/formatMoney'
import { mapApiError } from '../utils/mapApiError'

const CURRENCIES = ['TRY', 'EUR', 'USD'] as const

type BranchFormState = {
  code: string
  name: string
  city: string
  isActive: boolean
  maxApprovalAmount: string
  maxApprovalCurrencyCode: (typeof CURRENCIES)[number]
}

const emptyForm = (): BranchFormState => ({
  code: '',
  name: '',
  city: '',
  isActive: true,
  maxApprovalAmount: '',
  maxApprovalCurrencyCode: 'TRY',
})

function parseOptionalAmount(raw: string): number | null {
  const trimmed = raw.trim()
  if (!trimmed) return null
  const n = Number(trimmed.replace(',', '.'))
  return Number.isFinite(n) && n > 0 ? n : null
}

function currencyFromItem(item: BranchListItem): (typeof CURRENCIES)[number] {
  const cc = (item.maxApprovalCurrencyCode || 'TRY').toUpperCase()
  return (CURRENCIES as readonly string[]).includes(cc)
    ? (cc as (typeof CURRENCIES)[number])
    : 'TRY'
}

export function BranchesPage() {
  const { t, i18n } = useTranslation()
  const { isSuperAdmin } = useAuth()
  const [items, setItems] = useState<BranchListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const [creating, setCreating] = useState(false)
  const [savingEdit, setSavingEdit] = useState(false)
  const [form, setForm] = useState<BranchFormState>(emptyForm)
  const [editing, setEditing] = useState<BranchListItem | null>(null)
  const [editMaxApproval, setEditMaxApproval] = useState('')
  const [editCurrency, setEditCurrency] = useState<(typeof CURRENCIES)[number]>('TRY')

  const load = useCallback(() => {
    if (!isSuperAdmin) return Promise.resolve()
    setLoading(true)
    return getBranches(null)
      .then(setItems)
      .catch((err) => setError(mapApiError(err, t)))
      .finally(() => setLoading(false))
  }, [isSuperAdmin, t])

  useEffect(() => {
    void load()
  }, [load])

  if (!isSuperAdmin) {
    return (
      <section className="panel">
        <h1>{t('branches.title')}</h1>
        <p className="muted">{t('branches.superAdminOnly')}</p>
      </section>
    )
  }

  function openCreate() {
    setEditing(null)
    setForm(emptyForm())
    setError(null)
    setShowCreate(true)
  }

  function closeCreate() {
    if (creating) return
    setShowCreate(false)
    setForm(emptyForm())
  }

  function openEdit(item: BranchListItem) {
    setShowCreate(false)
    setError(null)
    setEditing({ ...item })
    setEditMaxApproval(
      item.maxApprovalAmount != null && item.maxApprovalAmount > 0
        ? String(item.maxApprovalAmount)
        : '',
    )
    setEditCurrency(currencyFromItem(item))
  }

  function closeEdit() {
    if (savingEdit) return
    setEditing(null)
    setEditMaxApproval('')
    setEditCurrency('TRY')
  }

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSuccess(null)
    setCreating(true)
    try {
      const amount = parseOptionalAmount(form.maxApprovalAmount)
      await createBranch({
        code: form.code.trim(),
        name: form.name.trim(),
        city: form.city.trim() || null,
        isActive: form.isActive,
        maxApprovalAmount: amount,
        maxApprovalCurrencyCode: amount != null ? form.maxApprovalCurrencyCode : null,
      })
      setForm(emptyForm())
      setShowCreate(false)
      setSuccess(t('branches.createSuccess'))
      await load()
    } catch (err) {
      setError(mapApiError(err, t))
    } finally {
      setCreating(false)
    }
  }

  async function onSaveEdit(e: FormEvent) {
    e.preventDefault()
    if (!editing) return
    setError(null)
    setSuccess(null)
    setSavingEdit(true)
    try {
      const amount = parseOptionalAmount(editMaxApproval)
      await updateBranch({
        id: editing.id,
        code: editing.code.trim(),
        name: editing.name.trim(),
        city: editing.city?.trim() || null,
        isActive: editing.isActive,
        maxApprovalAmount: amount,
        maxApprovalCurrencyCode: amount != null ? editCurrency : null,
      })
      setEditing(null)
      setEditMaxApproval('')
      setEditCurrency('TRY')
      setSuccess(t('branches.updateSuccess'))
      await load()
    } catch (err) {
      setError(mapApiError(err, t))
    } finally {
      setSavingEdit(false)
    }
  }

  function formatCap(value?: number | null, currency?: string | null) {
    if (value == null || value <= 0) return t('branches.noCap')
    return formatMoney(value, currency || 'TRY', i18n.language, 2)
  }

  return (
    <section className="panel">
      <div className="page-header-row">
        <h1>{t('branches.title')}</h1>
        <button type="button" className="btn primary" onClick={openCreate}>
          {t('branches.newBranch')}
        </button>
      </div>
      <p className="muted">{t('branches.hint')}</p>
      {error && !showCreate && !editing && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}

      {loading ? (
        <p>{t('common.loading')}</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>{t('branches.code')}</th>
              <th>{t('branches.name')}</th>
              <th>{t('branches.city')}</th>
              <th>{t('branches.maxApprovalAmount')}</th>
              <th>{t('branches.status')}</th>
              <th>{t('common.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.id}>
                <td>{item.code}</td>
                <td>{item.name}</td>
                <td>{item.city || '—'}</td>
                <td>{formatCap(item.maxApprovalAmount, item.maxApprovalCurrencyCode)}</td>
                <td>{item.isActive ? t('branches.active') : t('branches.inactive')}</td>
                <td>
                  <button type="button" className="btn ghost" onClick={() => openEdit(item)}>
                    {t('common.edit')}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {showCreate && (
        <div
          className="staff-modal-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="branch-create-title"
          onClick={(e) => {
            if (e.target === e.currentTarget) closeCreate()
          }}
        >
          <form className="staff-modal-panel wide form" onSubmit={onCreate}>
            <h2 id="branch-create-title">{t('branches.newBranch')}</h2>
            {error && <p className="error-text">{error}</p>}
            <div className="form-row form-row-2">
              <label>
                {t('branches.code')}
                <input
                  value={form.code}
                  onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))}
                  required
                  maxLength={20}
                  autoComplete="off"
                />
              </label>
              <label>
                {t('branches.name')}
                <input
                  value={form.name}
                  onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                  required
                  maxLength={150}
                />
              </label>
            </div>
            <div className="form-row form-row-2">
              <label>
                {t('branches.city')}
                <input
                  value={form.city}
                  onChange={(e) => setForm((f) => ({ ...f, city: e.target.value }))}
                  maxLength={100}
                />
              </label>
              <label>
                {t('branches.maxApprovalAmount')}
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  value={form.maxApprovalAmount}
                  onChange={(e) => setForm((f) => ({ ...f, maxApprovalAmount: e.target.value }))}
                  placeholder={t('branches.maxApprovalPlaceholder')}
                />
              </label>
            </div>
            <div className="form-row form-row-2">
              <label>
                {t('branches.currency')}
                <select
                  value={form.maxApprovalCurrencyCode}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      maxApprovalCurrencyCode: e.target.value as (typeof CURRENCIES)[number],
                    }))
                  }
                  disabled={!form.maxApprovalAmount.trim()}
                >
                  {CURRENCIES.map((c) => (
                    <option key={c} value={c}>
                      {currencySymbol(c)} ({c})
                    </option>
                  ))}
                </select>
              </label>
              <label className="checkbox-row">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
                />
                <span>{t('branches.active')}</span>
              </label>
            </div>
            <div className="actions-cell">
              <button type="submit" className="btn success" disabled={creating}>
                {creating ? t('common.loading') : t('branches.create')}
              </button>
              <button type="button" className="btn ghost" onClick={closeCreate} disabled={creating}>
                {t('common.cancel')}
              </button>
            </div>
          </form>
        </div>
      )}

      {editing && (
        <div
          className="staff-modal-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="branch-edit-title"
          onClick={(e) => {
            if (e.target === e.currentTarget) closeEdit()
          }}
        >
          <form className="staff-modal-panel wide form" onSubmit={onSaveEdit}>
            <h2 id="branch-edit-title">{t('branches.editBranch')}</h2>
            {error && <p className="error-text">{error}</p>}
            <div className="form-row form-row-2">
              <label>
                {t('branches.code')}
                <input
                  value={editing.code}
                  onChange={(e) => setEditing({ ...editing, code: e.target.value })}
                  required
                />
              </label>
              <label>
                {t('branches.name')}
                <input
                  value={editing.name}
                  onChange={(e) => setEditing({ ...editing, name: e.target.value })}
                  required
                />
              </label>
            </div>
            <div className="form-row form-row-2">
              <label>
                {t('branches.city')}
                <input
                  value={editing.city ?? ''}
                  onChange={(e) => setEditing({ ...editing, city: e.target.value })}
                />
              </label>
              <label>
                {t('branches.maxApprovalAmount')}
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  value={editMaxApproval}
                  onChange={(e) => setEditMaxApproval(e.target.value)}
                  placeholder={t('branches.maxApprovalPlaceholder')}
                />
              </label>
            </div>
            <div className="form-row form-row-2">
              <label>
                {t('branches.currency')}
                <select
                  value={editCurrency}
                  onChange={(e) => setEditCurrency(e.target.value as (typeof CURRENCIES)[number])}
                  disabled={!editMaxApproval.trim()}
                >
                  {CURRENCIES.map((c) => (
                    <option key={c} value={c}>
                      {currencySymbol(c)} ({c})
                    </option>
                  ))}
                </select>
              </label>
              <label className="checkbox-row">
                <input
                  type="checkbox"
                  checked={editing.isActive}
                  onChange={(e) => setEditing({ ...editing, isActive: e.target.checked })}
                />
                <span>{t('branches.active')}</span>
              </label>
            </div>
            <div className="actions-cell">
              <button type="submit" className="btn primary" disabled={savingEdit}>
                {savingEdit ? t('common.loading') : t('common.save')}
              </button>
              <button type="button" className="btn ghost" onClick={closeEdit} disabled={savingEdit}>
                {t('common.cancel')}
              </button>
            </div>
          </form>
        </div>
      )}
    </section>
  )
}
