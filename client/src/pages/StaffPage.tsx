import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'react-toastify'
import { createStaffUser, getStaffUsers, updateStaffUser } from '../api/staffApi'
import { resolveProfileImageUrl } from '../api/profileApi'
import { useAuth } from '../auth/AuthContext'
import type {
  Gender,
  GetListResponse,
  PersonTitle,
  StaffPosition,
  StaffUserListItem,
} from '../types/api'
import { mapApiError } from '../utils/mapApiError'

type ActiveFilter = 'all' | 'active' | 'inactive'

const TITLES: PersonTitle[] = ['Dr', 'Prof']
const GENDERS: Gender[] = ['Female', 'Male', 'PreferNotToSay']
const POSITIONS: StaffPosition[] = [
  'CreditAnalyst',
  'RelationshipManager',
  'BranchOfficer',
  'RiskOfficer',
  'OperationsSpecialist',
  'BranchManager',
]

const emptyCreate = {
  email: '',
  password: '',
  isManager: false,
  firstName: '',
  lastName: '',
  title: '' as '' | PersonTitle,
  gender: '' as '' | Gender,
  dateOfBirth: '',
  position: '' as '' | StaffPosition,
}

type EditForm = {
  id: string
  email: string
  newPassword: string
  isActive: boolean
  isManager: boolean
  firstName: string
  lastName: string
  title: '' | PersonTitle
  gender: '' | Gender
  dateOfBirth: string
  position: '' | StaffPosition
}

function toEditForm(item: StaffUserListItem): EditForm {
  return {
    id: item.id,
    email: item.email,
    newPassword: '',
    isActive: item.isActive,
    isManager: item.isManager,
    firstName: item.firstName || '',
    lastName: item.lastName || '',
    title: item.title || '',
    gender: item.gender || '',
    dateOfBirth: item.dateOfBirth || '',
    position: item.position || '',
  }
}

export function StaffPage() {
  const { t, i18n } = useTranslation()
  const { isManager, user } = useAuth()
  const [pageIndex, setPageIndex] = useState(0)
  const [activeFilter, setActiveFilter] = useState<ActiveFilter>('all')
  const [data, setData] = useState<GetListResponse<StaffUserListItem> | null>(null)
  const [loading, setLoading] = useState(true)
  const [showCreate, setShowCreate] = useState(false)
  const [creating, setCreating] = useState(false)
  const [updatingId, setUpdatingId] = useState<string | null>(null)
  const [form, setForm] = useState(emptyCreate)
  const [editing, setEditing] = useState<EditForm | null>(null)
  const [savingEdit, setSavingEdit] = useState(false)

  const loadList = useCallback(() => {
    if (!isManager) return Promise.resolve()
    setLoading(true)
    const isActive =
      activeFilter === 'active' ? true : activeFilter === 'inactive' ? false : null
    return getStaffUsers(pageIndex, 10, isActive)
      .then((res) => setData(res))
      .catch((err: unknown) => {
        toast.error(mapApiError(err, t))
      })
      .finally(() => setLoading(false))
  }, [isManager, pageIndex, activeFilter, t])

  useEffect(() => {
    void loadList()
  }, [loadList])

  if (!isManager) {
    return (
      <section>
        <h1>{t('staff.title')}</h1>
        <p className="muted">{t('staff.managerOnly')}</p>
      </section>
    )
  }

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setCreating(true)
    try {
      await createStaffUser({
        email: form.email.trim(),
        password: form.password,
        isManager: form.isManager,
        firstName: form.firstName.trim() || null,
        lastName: form.lastName.trim() || null,
        title: form.title || null,
        gender: form.gender || null,
        dateOfBirth: form.dateOfBirth || null,
        position: form.position || null,
      })
      toast.success(t('staff.createSuccess'))
      setForm(emptyCreate)
      setShowCreate(false)
      setPageIndex(0)
      await loadList()
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setCreating(false)
    }
  }

  async function onSaveEdit(e: FormEvent) {
    e.preventDefault()
    if (!editing) return
    setSavingEdit(true)
    try {
      await updateStaffUser({
        id: editing.id,
        email: editing.email.trim(),
        newPassword: editing.newPassword || null,
        isActive: editing.isActive,
        isManager: editing.isManager,
        firstName: editing.firstName.trim() || null,
        lastName: editing.lastName.trim() || null,
        title: editing.title || null,
        gender: editing.gender || null,
        dateOfBirth: editing.dateOfBirth || null,
        position: editing.position || null,
      })
      toast.success(t('staff.updateSuccess'))
      setEditing(null)
      await loadList()
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setSavingEdit(false)
    }
  }

  async function onToggleActive(item: StaffUserListItem) {
    if (item.id === user?.userId) {
      toast.error(t('errors.staffCannotModifySelf'))
      return
    }
    const nextActive = !item.isActive
    if (!window.confirm(nextActive ? t('staff.confirmActivate') : t('staff.confirmDeactivate'))) {
      return
    }
    setUpdatingId(item.id)
    try {
      await updateStaffUser({
        id: item.id,
        email: item.email,
        isActive: nextActive,
        isManager: item.isManager,
        firstName: item.firstName,
        lastName: item.lastName,
        title: item.title,
        gender: item.gender,
        dateOfBirth: item.dateOfBirth,
        position: item.position,
      })
      toast.success(t('staff.updateSuccess'))
      await loadList()
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setUpdatingId(null)
    }
  }

  async function onToggleManager(item: StaffUserListItem) {
    if (item.id === user?.userId) {
      toast.error(t('errors.staffCannotModifySelf'))
      return
    }
    const nextManager = !item.isManager
    if (
      !window.confirm(nextManager ? t('staff.confirmPromote') : t('staff.confirmDemote'))
    ) {
      return
    }
    setUpdatingId(item.id)
    try {
      await updateStaffUser({
        id: item.id,
        email: item.email,
        isActive: item.isActive,
        isManager: nextManager,
        firstName: item.firstName,
        lastName: item.lastName,
        title: item.title,
        gender: item.gender,
        dateOfBirth: item.dateOfBirth,
        position: item.position,
      })
      toast.success(t('staff.updateSuccess'))
      await loadList()
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setUpdatingId(null)
    }
  }

  function displayName(item: StaffUserListItem) {
    const name = `${item.firstName || ''} ${item.lastName || ''}`.trim()
    return name || '—'
  }

  return (
    <section>
      <div className="page-header-row">
        <h1>{t('staff.title')}</h1>
        <button type="button" className="btn primary" onClick={() => setShowCreate((v) => !v)}>
          {showCreate ? t('common.cancel') : t('staff.newStaff')}
        </button>
      </div>

      <p className="muted">{t('staff.hint')}</p>

      {showCreate && (
        <form className="panel form" onSubmit={onCreate}>
          <h2>{t('staff.newStaff')}</h2>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={form.email}
              onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
              required
              autoComplete="off"
            />
          </label>
          <label>
            {t('login.password')}
            <input
              type="password"
              value={form.password}
              onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))}
              required
              minLength={6}
              autoComplete="new-password"
            />
          </label>
          <label>
            {t('profile.academicTitle')}
            <select
              value={form.title}
              onChange={(e) =>
                setForm((f) => ({ ...f, title: (e.target.value || '') as '' | PersonTitle }))
              }
            >
              <option value="">{t('profile.titleUnset')}</option>
              {TITLES.map((title) => (
                <option key={title} value={title}>
                  {t(`personTitle.${title}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('customers.gender')}
            <select
              value={form.gender}
              onChange={(e) =>
                setForm((f) => ({ ...f, gender: (e.target.value || '') as '' | Gender }))
              }
            >
              <option value="">{t('customers.genderUnset')}</option>
              {GENDERS.map((g) => (
                <option key={g} value={g}>
                  {t(`gender.${g}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('profile.firstName')}
            <input
              value={form.firstName}
              onChange={(e) => setForm((f) => ({ ...f, firstName: e.target.value }))}
            />
          </label>
          <label>
            {t('profile.lastName')}
            <input
              value={form.lastName}
              onChange={(e) => setForm((f) => ({ ...f, lastName: e.target.value }))}
            />
          </label>
          <label>
            {t('profile.dateOfBirth')}
            <input
              type="date"
              value={form.dateOfBirth}
              onChange={(e) => setForm((f) => ({ ...f, dateOfBirth: e.target.value }))}
            />
          </label>
          <label>
            {t('profile.position')}
            <select
              value={form.position}
              onChange={(e) =>
                setForm((f) => ({
                  ...f,
                  position: (e.target.value || '') as '' | StaffPosition,
                }))
              }
            >
              <option value="">{t('profile.positionUnset')}</option>
              {POSITIONS.map((p) => (
                <option key={p} value={p}>
                  {t(`staffPosition.${p}`)}
                </option>
              ))}
            </select>
          </label>
          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={form.isManager}
              onChange={(e) => setForm((f) => ({ ...f, isManager: e.target.checked }))}
            />
            {t('staff.isManager')}
          </label>
          <button type="submit" className="btn success" disabled={creating}>
            {creating ? t('common.loading') : t('staff.create')}
          </button>
        </form>
      )}

      {editing && (
        <form className="panel form" onSubmit={onSaveEdit}>
          <h2>{t('staff.editStaff')}</h2>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={editing.email}
              onChange={(e) => setEditing({ ...editing, email: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.academicTitle')}
            <select
              value={editing.title}
              onChange={(e) =>
                setEditing({
                  ...editing,
                  title: (e.target.value || '') as '' | PersonTitle,
                })
              }
            >
              <option value="">{t('profile.titleUnset')}</option>
              {TITLES.map((title) => (
                <option key={title} value={title}>
                  {t(`personTitle.${title}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('customers.gender')}
            <select
              value={editing.gender}
              onChange={(e) =>
                setEditing({
                  ...editing,
                  gender: (e.target.value || '') as '' | Gender,
                })
              }
            >
              <option value="">{t('customers.genderUnset')}</option>
              {GENDERS.map((g) => (
                <option key={g} value={g}>
                  {t(`gender.${g}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('profile.firstName')}
            <input
              value={editing.firstName}
              onChange={(e) => setEditing({ ...editing, firstName: e.target.value })}
            />
          </label>
          <label>
            {t('profile.lastName')}
            <input
              value={editing.lastName}
              onChange={(e) => setEditing({ ...editing, lastName: e.target.value })}
            />
          </label>
          <label>
            {t('profile.dateOfBirth')}
            <input
              type="date"
              value={editing.dateOfBirth}
              onChange={(e) => setEditing({ ...editing, dateOfBirth: e.target.value })}
            />
          </label>
          <label>
            {t('profile.position')}
            <select
              value={editing.position}
              onChange={(e) =>
                setEditing({
                  ...editing,
                  position: (e.target.value || '') as '' | StaffPosition,
                })
              }
            >
              <option value="">{t('profile.positionUnset')}</option>
              {POSITIONS.map((p) => (
                <option key={p} value={p}>
                  {t(`staffPosition.${p}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('profile.newPassword')}
            <input
              type="password"
              value={editing.newPassword}
              onChange={(e) => setEditing({ ...editing, newPassword: e.target.value })}
              minLength={6}
              autoComplete="new-password"
              placeholder={t('profile.newPasswordHint')}
            />
          </label>
          {editing.id !== user?.userId && (
            <>
              <label className="checkbox-row">
                <input
                  type="checkbox"
                  checked={editing.isActive}
                  onChange={(e) => setEditing({ ...editing, isActive: e.target.checked })}
                />
                {t('staff.active')}
              </label>
              <label className="checkbox-row">
                <input
                  type="checkbox"
                  checked={editing.isManager}
                  onChange={(e) => setEditing({ ...editing, isManager: e.target.checked })}
                />
                {t('staff.isManager')}
              </label>
            </>
          )}
          <div className="actions-cell">
            <button type="submit" className="btn success" disabled={savingEdit}>
              {savingEdit ? t('common.loading') : t('common.save')}
            </button>
            <button
              type="button"
              className="btn ghost"
              onClick={() => setEditing(null)}
              disabled={savingEdit}
            >
              {t('common.cancel')}
            </button>
          </div>
        </form>
      )}

      <div className="chip-row" role="tablist" aria-label={t('staff.filter')}>
        {(['all', 'active', 'inactive'] as const).map((f) => (
          <button
            key={f}
            type="button"
            role="tab"
            className={`chip-btn${activeFilter === f ? ' active' : ''}`}
            onClick={() => {
              setActiveFilter(f)
              setPageIndex(0)
            }}
          >
            {t(`staff.filter_${f}`)}
          </button>
        ))}
      </div>

      {loading && <p className="muted">{t('common.loading')}</p>}

      {!loading && data && data.items.length === 0 && (
        <p className="muted">{t('common.empty')}</p>
      )}

      {!loading && data && data.items.length > 0 && (
        <>
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('profile.avatar')}</th>
                  <th>{t('login.email')}</th>
                  <th>{t('profile.firstName')}</th>
                  <th>{t('profile.position')}</th>
                  <th>{t('staff.roles')}</th>
                  <th>{t('staff.status')}</th>
                  <th>{t('staff.created')}</th>
                  <th>{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((item) => {
                  const isSelf = item.id === user?.userId
                  const busy = updatingId === item.id
                  const img = resolveProfileImageUrl(item.profileImageUrl)
                  return (
                    <tr key={item.id}>
                      <td>
                        <div className="avatar-preview avatar-sm">
                          {img ? (
                            <img src={img} alt="" />
                          ) : (
                            <span className="avatar-fallback">
                              {(item.firstName || item.email).slice(0, 1).toUpperCase()}
                            </span>
                          )}
                        </div>
                      </td>
                      <td>
                        {item.email}
                        {isSelf && <span className="muted"> ({t('staff.you')})</span>}
                      </td>
                      <td>{displayName(item)}</td>
                      <td>
                        {item.position
                          ? t(`staffPosition.${item.position}`, { defaultValue: item.position })
                          : '—'}
                      </td>
                      <td>
                        {item.roles
                          .map((r) => t(`roles.${r}`, { defaultValue: r }))
                          .join(', ')}
                      </td>
                      <td>
                        <span
                          className={`badge ${item.isActive ? 'status-approved' : 'status-rejected'}`}
                        >
                          {item.isActive ? t('staff.active') : t('staff.inactive')}
                        </span>
                      </td>
                      <td>
                        {new Date(item.createdDate).toLocaleString(i18n.language, {
                          day: '2-digit',
                          month: '2-digit',
                          year: 'numeric',
                        })}
                      </td>
                      <td className="actions-cell">
                        <button
                          type="button"
                          className="btn ghost"
                          disabled={busy}
                          onClick={() => setEditing(toEditForm(item))}
                        >
                          {t('common.edit')}
                        </button>
                        <button
                          type="button"
                          className={item.isActive ? 'btn reject' : 'btn success'}
                          disabled={busy || isSelf}
                          onClick={() => onToggleActive(item)}
                        >
                          {busy
                            ? t('common.loading')
                            : item.isActive
                              ? t('staff.deactivate')
                              : t('staff.activate')}
                        </button>
                        <button
                          type="button"
                          className="btn ghost"
                          disabled={busy || isSelf}
                          onClick={() => onToggleManager(item)}
                        >
                          {item.isManager ? t('staff.removeManager') : t('staff.makeManager')}
                        </button>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>

          <div className="pager">
            <button
              type="button"
              className="btn ghost"
              disabled={!data.hasPrevious}
              onClick={() => setPageIndex((p) => Math.max(0, p - 1))}
            >
              {t('common.prev')}
            </button>
            <span>
              {t('common.page')} {data.index + 1} {t('common.of')} {Math.max(data.pages, 1)}
            </span>
            <button
              type="button"
              className="btn ghost"
              disabled={!data.hasNext}
              onClick={() => setPageIndex((p) => p + 1)}
            >
              {t('common.next')}
            </button>
          </div>
        </>
      )}
    </section>
  )
}
