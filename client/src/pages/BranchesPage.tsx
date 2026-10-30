import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { createBranch, getBranches, updateBranch } from '../api/branchesApi'
import { useAuth } from '../auth/AuthContext'
import type { BranchListItem } from '../types/api'
import { mapApiError } from '../utils/mapApiError'

export function BranchesPage() {
  const { t } = useTranslation()
  const { isSuperAdmin } = useAuth()
  const [items, setItems] = useState<BranchListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [form, setForm] = useState({ code: '', name: '', city: '', isActive: true })
  const [editing, setEditing] = useState<BranchListItem | null>(null)

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

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSuccess(null)
    try {
      await createBranch({
        code: form.code.trim(),
        name: form.name.trim(),
        city: form.city.trim() || null,
        isActive: form.isActive,
      })
      setForm({ code: '', name: '', city: '', isActive: true })
      setSuccess(t('branches.createSuccess'))
      await load()
    } catch (err) {
      setError(mapApiError(err, t))
    }
  }

  async function onSaveEdit(e: FormEvent) {
    e.preventDefault()
    if (!editing) return
    setError(null)
    setSuccess(null)
    try {
      await updateBranch({
        id: editing.id,
        code: editing.code.trim(),
        name: editing.name.trim(),
        city: editing.city?.trim() || null,
        isActive: editing.isActive,
      })
      setEditing(null)
      setSuccess(t('branches.updateSuccess'))
      await load()
    } catch (err) {
      setError(mapApiError(err, t))
    }
  }

  return (
    <section className="panel">
      <h1>{t('branches.title')}</h1>
      <p className="muted">{t('branches.hint')}</p>
      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}

      <form className="form inline-form" onSubmit={onCreate}>
        <h2>{t('branches.newBranch')}</h2>
        <label>
          {t('branches.code')}
          <input
            value={form.code}
            onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))}
            required
            maxLength={20}
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
        <label>
          {t('branches.city')}
          <input
            value={form.city}
            onChange={(e) => setForm((f) => ({ ...f, city: e.target.value }))}
            maxLength={100}
          />
        </label>
        <label className="checkbox-row">
          <input
            type="checkbox"
            checked={form.isActive}
            onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
          />
          <span>{t('branches.active')}</span>
        </label>
        <button type="submit" className="btn primary">
          {t('branches.create')}
        </button>
      </form>

      {editing && (
        <form className="form inline-form" onSubmit={onSaveEdit}>
          <h2>{t('branches.editBranch')}</h2>
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
          <label>
            {t('branches.city')}
            <input
              value={editing.city ?? ''}
              onChange={(e) => setEditing({ ...editing, city: e.target.value })}
            />
          </label>
          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={editing.isActive}
              onChange={(e) => setEditing({ ...editing, isActive: e.target.checked })}
            />
            <span>{t('branches.active')}</span>
          </label>
          <div className="row-actions">
            <button type="submit" className="btn primary">
              {t('common.save')}
            </button>
            <button type="button" className="btn ghost" onClick={() => setEditing(null)}>
              {t('common.cancel')}
            </button>
          </div>
        </form>
      )}

      {loading ? (
        <p>{t('common.loading')}</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>{t('branches.code')}</th>
              <th>{t('branches.name')}</th>
              <th>{t('branches.city')}</th>
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
                <td>{item.isActive ? t('branches.active') : t('branches.inactive')}</td>
                <td>
                  <button type="button" className="btn ghost" onClick={() => setEditing(item)}>
                    {t('common.edit')}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
