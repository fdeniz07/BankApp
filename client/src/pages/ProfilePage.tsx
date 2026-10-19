import { useEffect, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'react-toastify'
import {
  getCorporateCustomer,
  getIndividualCustomer,
  updateCorporateCustomer,
  updateIndividualCustomer,
} from '../api/customersApi'
import {
  getMyProfile,
  resolveProfileImageUrl,
  updateMyStaffProfile,
  uploadProfileAvatar,
} from '../api/profileApi'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { AvatarCropModal } from '../components/AvatarCropModal'
import type {
  CorporateCustomerDetail,
  Gender,
  IndividualCustomerDetail,
  MyProfile,
  PersonTitle,
  StaffPosition,
} from '../types/api'
import { mapApiError } from '../utils/mapApiError'

const GENDERS: Gender[] = ['Female', 'Male', 'PreferNotToSay']
const TITLES: PersonTitle[] = ['Dr', 'Prof']
const POSITIONS: StaffPosition[] = [
  'CreditAnalyst',
  'RelationshipManager',
  'BranchOfficer',
  'RiskOfficer',
  'OperationsSpecialist',
  'BranchManager',
]

export function ProfilePage() {
  const { t } = useTranslation()
  const { user, isStaff, updateProfileLocal } = useAuth()
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [uploading, setUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)
  const [individual, setIndividual] = useState<IndividualCustomerDetail | null>(null)
  const [corporate, setCorporate] = useState<CorporateCustomerDetail | null>(null)
  const [staffProfile, setStaffProfile] = useState<MyProfile | null>(null)
  const [avatarUrl, setAvatarUrl] = useState<string | null>(null)
  const [newPassword, setNewPassword] = useState('')
  const [cropFile, setCropFile] = useState<File | null>(null)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)

    const load = async () => {
      try {
        const mine = await getMyProfile()
        if (cancelled) return
        const url = resolveProfileImageUrl(mine.profileImageUrl)
        setAvatarUrl(url)
        if (url) updateProfileLocal({ profileImageUrl: url })

        if (isStaff || mine.isStaff) {
          setStaffProfile(mine)
          setIndividual(null)
          setCorporate(null)
          return
        }

        if (!user?.customerId) {
          setStaffProfile(mine)
          return
        }

        if (user.customerCategory === 'Corporate') {
          const data = await getCorporateCustomer(user.customerId)
          if (!cancelled) {
            setCorporate(data)
            setIndividual(null)
          }
          return
        }

        try {
          const data = await getIndividualCustomer(user.customerId)
          if (!cancelled) {
            setIndividual(data)
            setCorporate(null)
            if (!user.customerCategory) {
              updateProfileLocal({ customerCategory: 'Individual' })
            }
          }
        } catch {
          const data = await getCorporateCustomer(user.customerId)
          if (!cancelled) {
            setCorporate(data)
            setIndividual(null)
            updateProfileLocal({ customerCategory: 'Corporate' })
          }
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof ApiError ? mapApiError(err, t) : t('common.error'))
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [user?.customerId, user?.customerCategory, isStaff, t, updateProfileLocal])

  async function uploadCropped(file: File) {
    setCropFile(null)
    setUploading(true)
    setError(null)
    try {
      const res = await uploadProfileAvatar(file)
      const url = resolveProfileImageUrl(res.profileImageUrl, { cacheBust: true })
      setAvatarUrl(url)
      updateProfileLocal({ profileImageUrl: url })
      if (staffProfile) {
        setStaffProfile({ ...staffProfile, profileImageUrl: res.profileImageUrl })
      }
      toast.success(t('profile.avatarSuccess'))
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setUploading(false)
    }
  }

  async function onSubmitStaff(e: FormEvent) {
    e.preventDefault()
    if (!staffProfile) return
    setSaving(true)
    setError(null)
    setMessage(null)
    try {
      const updated = await updateMyStaffProfile({
        email: staffProfile.email.trim(),
        newPassword: newPassword || null,
        isActive: staffProfile.isActive,
        isManager: staffProfile.roles.includes('BankManager'),
        firstName: staffProfile.firstName || null,
        lastName: staffProfile.lastName || null,
        title: staffProfile.title || null,
        gender: staffProfile.gender || null,
        dateOfBirth: staffProfile.dateOfBirth || null,
        position: staffProfile.position || null,
      })
      setStaffProfile({
        ...staffProfile,
        email: updated.email,
        firstName: updated.firstName,
        lastName: updated.lastName,
        title: updated.title,
        gender: updated.gender,
        dateOfBirth: updated.dateOfBirth,
        position: updated.position,
        profileImageUrl: updated.profileImageUrl,
      })
      const displayName = `${updated.firstName ?? ''} ${updated.lastName ?? ''}`.trim()
      updateProfileLocal({ displayName: displayName || updated.email })
      setNewPassword('')
      setMessage(t('profile.saveSuccess'))
      toast.success(t('profile.saveSuccess'))
    } catch (err) {
      setError(mapApiError(err, t))
      toast.error(mapApiError(err, t))
    } finally {
      setSaving(false)
    }
  }

  async function onSubmitCustomer(e: FormEvent) {
    e.preventDefault()
    if (!user?.customerId) return
    setSaving(true)
    setError(null)
    setMessage(null)
    try {
      if (individual) {
        const updated = await updateIndividualCustomer(individual)
        setIndividual(updated)
        updateProfileLocal({
          displayName: `${updated.firstName} ${updated.lastName}`.trim(),
          customerCategory: 'Individual',
        })
      } else if (corporate) {
        const updated = await updateCorporateCustomer(corporate)
        setCorporate(updated)
        updateProfileLocal({
          displayName: updated.companyName,
          customerCategory: 'Corporate',
        })
      }
      setMessage(t('profile.saveSuccess'))
      toast.success(t('profile.saveSuccess'))
    } catch (err) {
      setError(mapApiError(err, t))
      toast.error(mapApiError(err, t))
    } finally {
      setSaving(false)
    }
  }

  function renderAvatar() {
    return (
      <div className="avatar-block">
        <div className="avatar-preview" aria-hidden={!avatarUrl}>
          {avatarUrl ? (
            <img src={avatarUrl} alt="" />
          ) : (
            <span className="avatar-fallback">
              {(user?.displayName || user?.email || '?').slice(0, 1).toUpperCase()}
            </span>
          )}
        </div>
        <label className="btn ghost avatar-upload-btn">
          {uploading ? t('common.loading') : t('profile.changeAvatar')}
          <input
            type="file"
            accept="image/jpeg,image/png,image/webp"
            hidden
            disabled={uploading}
            onChange={(e) => {
              const file = e.target.files?.[0] ?? null
              e.target.value = ''
              if (file) setCropFile(file)
            }}
          />
        </label>
        <p className="muted avatar-hint">{t('profile.avatarHint')}</p>
      </div>
    )
  }

  if (loading) {
    return (
      <section>
        <h1>{t('profile.title')}</h1>
        <p className="muted">{t('common.loading')}</p>
      </section>
    )
  }

  return (
    <section>
      {cropFile && (
        <AvatarCropModal
          file={cropFile}
          onCancel={() => setCropFile(null)}
          onConfirm={(file) => void uploadCropped(file)}
        />
      )}

      <h1>{t('profile.title')}</h1>
      {error && <p className="error-text">{error}</p>}
      {message && <p className="success-text">{message}</p>}
      {renderAvatar()}

      {(isStaff || staffProfile?.isStaff) && staffProfile && (
        <form className="form create-panel" onSubmit={onSubmitStaff}>
          <label>
            {t('profile.academicTitle')}
            <select
              value={staffProfile.title || ''}
              onChange={(e) =>
                setStaffProfile({
                  ...staffProfile,
                  title: (e.target.value || null) as PersonTitle | null,
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
              value={staffProfile.gender || ''}
              onChange={(e) =>
                setStaffProfile({
                  ...staffProfile,
                  gender: (e.target.value || null) as Gender | null,
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
              value={staffProfile.firstName || ''}
              onChange={(e) => setStaffProfile({ ...staffProfile, firstName: e.target.value })}
            />
          </label>
          <label>
            {t('profile.lastName')}
            <input
              value={staffProfile.lastName || ''}
              onChange={(e) => setStaffProfile({ ...staffProfile, lastName: e.target.value })}
            />
          </label>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={staffProfile.email}
              onChange={(e) => setStaffProfile({ ...staffProfile, email: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.dateOfBirth')}
            <input
              type="date"
              value={staffProfile.dateOfBirth || ''}
              onChange={(e) =>
                setStaffProfile({ ...staffProfile, dateOfBirth: e.target.value || null })
              }
            />
          </label>
          <label>
            {t('profile.position')}
            <select
              value={staffProfile.position || ''}
              onChange={(e) =>
                setStaffProfile({
                  ...staffProfile,
                  position: (e.target.value || null) as StaffPosition | null,
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
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              minLength={6}
              autoComplete="new-password"
              placeholder={t('profile.newPasswordHint')}
            />
          </label>
          <div className="info-grid">
            <div>
              <dt>{t('profile.role')}</dt>
              <dd>
                {staffProfile.roles.map((r) => t(`roles.${r}`, { defaultValue: r })).join(', ')}
              </dd>
            </div>
          </div>
          <button type="submit" className="btn" disabled={saving}>
            {saving ? t('common.loading') : t('common.save')}
          </button>
        </form>
      )}

      {!isStaff && !staffProfile?.isStaff && user?.customerCategory && (
        <p className="muted">
          {t('profile.category')}:{' '}
          {t(`category.${user.customerCategory}`, { defaultValue: user.customerCategory })}
        </p>
      )}

      {!isStaff && individual && (
        <form className="form create-panel" onSubmit={onSubmitCustomer}>
          <label>
            {t('customers.customerNumber')}
            <input value={individual.customerNumber} readOnly disabled />
          </label>
          <label>
            {t('customers.nationalId')}
            <input value={individual.nationalId} readOnly disabled />
          </label>
          <label>
            {t('profile.firstName')}
            <input
              value={individual.firstName}
              onChange={(e) => setIndividual({ ...individual, firstName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.lastName')}
            <input
              value={individual.lastName}
              onChange={(e) => setIndividual({ ...individual, lastName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={individual.email}
              onChange={(e) => setIndividual({ ...individual, email: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.phone')}
            <input
              value={individual.phoneNumber}
              onChange={(e) => setIndividual({ ...individual, phoneNumber: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.address')}
            <input
              value={individual.address}
              onChange={(e) => setIndividual({ ...individual, address: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.dateOfBirth')}
            <input
              type="date"
              value={individual.dateOfBirth?.slice(0, 10) || ''}
              onChange={(e) =>
                setIndividual({ ...individual, dateOfBirth: e.target.value || null })
              }
            />
          </label>
          <label>
            {t('customers.gender')}
            <select
              value={individual.gender || ''}
              onChange={(e) =>
                setIndividual({
                  ...individual,
                  gender: e.target.value || null,
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
            {t('customers.motherName')}
            <input
              value={individual.motherName || ''}
              onChange={(e) =>
                setIndividual({ ...individual, motherName: e.target.value || null })
              }
            />
          </label>
          <label>
            {t('customers.fatherName')}
            <input
              value={individual.fatherName || ''}
              onChange={(e) =>
                setIndividual({ ...individual, fatherName: e.target.value || null })
              }
            />
          </label>
          <button type="submit" className="btn" disabled={saving}>
            {saving ? t('common.loading') : t('common.save')}
          </button>
        </form>
      )}

      {!isStaff && corporate && (
        <form className="form create-panel" onSubmit={onSubmitCustomer}>
          <label>
            {t('customers.customerNumber')}
            <input value={corporate.customerNumber} readOnly disabled />
          </label>
          <label>
            {t('profile.companyName')}
            <input
              value={corporate.companyName}
              onChange={(e) => setCorporate({ ...corporate, companyName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.authorizedPerson')}
            <input
              value={corporate.authorizedPersonName}
              onChange={(e) =>
                setCorporate({ ...corporate, authorizedPersonName: e.target.value })
              }
              required
            />
          </label>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={corporate.email}
              onChange={(e) => setCorporate({ ...corporate, email: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.phone')}
            <input
              value={corporate.phoneNumber}
              onChange={(e) => setCorporate({ ...corporate, phoneNumber: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.address')}
            <input
              value={corporate.address}
              onChange={(e) => setCorporate({ ...corporate, address: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.taxNumber')}
            <input
              value={corporate.taxNumber}
              onChange={(e) => setCorporate({ ...corporate, taxNumber: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.taxOffice')}
            <input
              value={corporate.taxOffice}
              onChange={(e) => setCorporate({ ...corporate, taxOffice: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.companyRegistrationNumber')}
            <input
              value={corporate.companyRegistrationNumber}
              onChange={(e) =>
                setCorporate({ ...corporate, companyRegistrationNumber: e.target.value })
              }
              required
            />
          </label>
          <button type="submit" className="btn" disabled={saving}>
            {saving ? t('common.loading') : t('common.save')}
          </button>
        </form>
      )}
    </section>
  )
}
