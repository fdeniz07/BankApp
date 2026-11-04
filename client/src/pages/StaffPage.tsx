import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'react-toastify'
import { getBranches } from '../api/branchesApi'
import { createStaffUser, getStaffUsers, updateStaffUser } from '../api/staffApi'
import { resolveProfileImageUrl } from '../api/profileApi'
import { useAuth } from '../auth/AuthContext'
import { BranchFilter } from '../components/BranchFilter'
import type {
  BranchListItem,
  Gender,
  GetListResponse,
  PersonTitle,
  StaffPosition,
  StaffUserListItem,
} from '../types/api'
import { mapApiError } from '../utils/mapApiError'

type ActiveFilter = 'all' | 'active' | 'inactive'
type ManagerFilter = 'all' | 'managers' | 'staff'

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
  branchId: '',
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
  branchId: string
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
    branchId: item.branchId || '',
  }
}

export function StaffPage() {
  const { t, i18n } = useTranslation()
  const { isManager, isSuperAdmin, user } = useAuth()
  const [pageIndex, setPageIndex] = useState(0)
  const [activeFilter, setActiveFilter] = useState<ActiveFilter>('all')
  const [managerFilter, setManagerFilter] = useState<ManagerFilter>('all')
  const [branchFilter, setBranchFilter] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [data, setData] = useState<GetListResponse<StaffUserListItem> | null>(null)
  const [loading, setLoading] = useState(true)
  const [showCreate, setShowCreate] = useState(false)
  const [creating, setCreating] = useState(false)
  const [updatingId, setUpdatingId] = useState<string | null>(null)
  const [form, setForm] = useState(emptyCreate)
  const [editing, setEditing] = useState<EditForm | null>(null)
  const [savingEdit, setSavingEdit] = useState(false)
  const [branches, setBranches] = useState<BranchListItem[]>([])

  const loadList = useCallback(() => {
    if (!isManager) return Promise.resolve()
    setLoading(true)
    const isActive =
      activeFilter === 'active' ? true : activeFilter === 'inactive' ? false : null
    const isManagerFlag =
      managerFilter === 'managers' ? true : managerFilter === 'staff' ? false : null
    return getStaffUsers({
      pageIndex,
      pageSize: 10,
      isActive,
      isManager: isManagerFlag,
      branchId: branchFilter || null,
      search: search || null,
    })
      .then((res) => setData(res))
      .catch((err: unknown) => {
        toast.error(mapApiError(err, t))
      })
      .finally(() => setLoading(false))
  }, [isManager, pageIndex, activeFilter, managerFilter, branchFilter, search, t])

  useEffect(() => {
    void loadList()
  }, [loadList])

  useEffect(() => {
    if (!isManager) return
    void getBranches(true)
      .then(setBranches)
      .catch(() => setBranches([]))
  }, [isManager])

  useEffect(() => {
    if (!showCreate && !editing) return
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') {
        setShowCreate(false)
        setEditing(null)
      }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [showCreate, editing])

  useEffect(() => {
    const handle = window.setTimeout(() => setSearch(searchInput.trim()), 350)
    return () => window.clearTimeout(handle)
  }, [searchInput])

  useEffect(() => {
    setPageIndex(0)
  }, [search, activeFilter, managerFilter, branchFilter])

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
        isManager: isSuperAdmin ? form.isManager : false,
        firstName: form.firstName.trim() || null,
        lastName: form.lastName.trim() || null,
        title: form.title || null,
        gender: form.gender || null,
        dateOfBirth: form.dateOfBirth || null,
        position: form.position || null,
        branchId: form.branchId || user?.branchId || null,
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
        branchId: editing.branchId || null,
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
        branchId: item.branchId,
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
        branchId: item.branchId,
      })
      toast.success(t('staff.updateSuccess'))
      await loadList()
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setUpdatingId(null)
    }
  }

  function canEditTarget(item: StaffUserListItem) {
    if (isSuperAdmin) return true
    if (item.id === user?.userId) return true
    return !item.isManager
  }

  function canToggleActive(item: StaffUserListItem) {
    if (item.id === user?.userId) return false
    if (isSuperAdmin) return true
    return !item.isManager
  }

  function canToggleManager(item: StaffUserListItem) {
    if (!isSuperAdmin) return false
    return item.id !== user?.userId
  }

  function displayName(item: StaffUserListItem) {
    const name = `${item.firstName || ''} ${item.lastName || ''}`.trim()
    return name || '—'
  }

  function openCreate() {
    setEditing(null)
    setShowCreate(true)
  }

  function openEdit(item: StaffUserListItem) {
    setShowCreate(false)
    setEditing(toEditForm(item))
  }

  return (
    <section>
      <div className="page-header-row">
        <h1>{t('staff.title')}</h1>
        <button type="button" className="btn primary" onClick={openCreate}>
          {t('staff.newStaff')}
        </button>
      </div>

      <p className="muted">{t('staff.hint')}</p>

      <div className="staff-filters">
        <label className="staff-search">
          {t('staff.search')}
          <input
            type="search"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder={t('staff.searchPlaceholder')}
            autoComplete="off"
          />
        </label>
        <BranchFilter
          value={branchFilter}
          onChange={(id) => setBranchFilter(id)}
          allowAll
        />
        <div className="chip-row" role="tablist" aria-label={t('staff.managerFilter')}>
          {([
            ['all', 'filter_manager_all'],
            ['managers', 'filter_managers'],
            ['staff', 'filter_non_managers'],
          ] as const).map(([key, labelKey]) => (
            <button
              key={key}
              type="button"
              role="tab"
              className={`chip-btn${managerFilter === key ? ' active' : ''}`}
              onClick={() => setManagerFilter(key)}
            >
              {t(`staff.${labelKey}`)}
            </button>
          ))}
        </div>
      </div>

      {showCreate && (
        <div
          className="staff-modal-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="staff-create-title"
          onClick={(e) => {
            if (e.target === e.currentTarget && !creating) setShowCreate(false)
          }}
        >
          <form className="staff-modal-panel form" onSubmit={onCreate}>
          <h2 id="staff-create-title">{t('staff.newStaff')}</h2>
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
          <label>
            {t('staff.branch')}
            <select
              value={form.branchId || user?.branchId || ''}
              onChange={(e) => setForm((f) => ({ ...f, branchId: e.target.value }))}
            >
              <option value="">{t('staff.branchUnset')}</option>
              {branches.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.code} — {b.name}
                </option>
              ))}
            </select>
          </label>
          {isSuperAdmin && (
            <label className="checkbox-row">
              <input
                type="checkbox"
                checked={form.isManager}
                onChange={(e) => setForm((f) => ({ ...f, isManager: e.target.checked }))}
              />
              {t('staff.isManager')}
            </label>
          )}
          <div className="actions-cell">
            <button type="submit" className="btn success" disabled={creating}>
              {creating ? t('common.loading') : t('staff.create')}
            </button>
            <button
              type="button"
              className="btn ghost"
              onClick={() => setShowCreate(false)}
              disabled={creating}
            >
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
          aria-labelledby="staff-edit-title"
          onClick={(e) => {
            if (e.target === e.currentTarget && !savingEdit) setEditing(null)
          }}
        >
          <form className="staff-modal-panel form" onSubmit={onSaveEdit}>
          <h2 id="staff-edit-title">{t('staff.editStaff')}</h2>
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
            {t('profile.branch')}
            <select
              value={editing.branchId}
              onChange={(e) => setEditing({ ...editing, branchId: e.target.value })}
            >
              <option value="">{t('profile.branchUnset')}</option>
              {branches.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.code} · {b.name}
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
          {editing.id !== user?.userId &&
            (isSuperAdmin || !editing.isManager) && (
              <label className="checkbox-row">
                <input
                  type="checkbox"
                  checked={editing.isActive}
                  onChange={(e) => setEditing({ ...editing, isActive: e.target.checked })}
                />
                {t('staff.active')}
              </label>
            )}
          {isSuperAdmin && editing.id !== user?.userId && (
            <label className="checkbox-row">
              <input
                type="checkbox"
                checked={editing.isManager}
                onChange={(e) => setEditing({ ...editing, isManager: e.target.checked })}
              />
              {t('staff.isManager')}
            </label>
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
        </div>
      )}

      <div className="chip-row" role="tablist" aria-label={t('staff.filter')}>
        {(['all', 'active', 'inactive'] as const).map((f) => (
          <button
            key={f}
            type="button"
            role="tab"
            className={`chip-btn${activeFilter === f ? ' active' : ''}`}
            onClick={() => setActiveFilter(f)}
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
                  <th>{t('profile.branch')}</th>
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
                        {item.branchCode || item.branchName
                          ? `${item.branchCode ?? ''}${
                              item.branchCode && item.branchName ? ' · ' : ''
                            }${item.branchName ?? ''}`
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
                        {canEditTarget(item) && (
                          <button
                            type="button"
                            className="btn ghost"
                            disabled={busy}
                            onClick={() => openEdit(item)}
                          >
                            {t('common.edit')}
                          </button>
                        )}
                        {canToggleActive(item) && (
                          <button
                            type="button"
                            className={item.isActive ? 'btn reject' : 'btn success'}
                            disabled={busy}
                            onClick={() => onToggleActive(item)}
                          >
                            {busy
                              ? t('common.loading')
                              : item.isActive
                                ? t('staff.deactivate')
                                : t('staff.activate')}
                          </button>
                        )}
                        {canToggleManager(item) && (
                          <button
                            type="button"
                            className="btn ghost"
                            disabled={busy}
                            onClick={() => onToggleManager(item)}
                          >
                            {item.isManager ? t('staff.removeManager') : t('staff.makeManager')}
                          </button>
                        )}
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
