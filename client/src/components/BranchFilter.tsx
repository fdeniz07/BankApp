import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { getBranches } from '../api/branchesApi'
import { useAuth } from '../auth/AuthContext'
import type { BranchListItem } from '../types/api'

type Props = {
  value: string
  onChange: (branchId: string) => void
  /** SuperAdmin: empty = all. Manager: empty = own (server default). */
  allowAll?: boolean
}

export function BranchFilter({ value, onChange, allowAll = true }: Props) {
  const { t } = useTranslation()
  const { isStaff, isManager, isSuperAdmin } = useAuth()
  const [branches, setBranches] = useState<BranchListItem[]>([])

  useEffect(() => {
    if (!isStaff) return
    void getBranches(true)
      .then(setBranches)
      .catch(() => setBranches([]))
  }, [isStaff])

  if (!isStaff || (!isManager && !isSuperAdmin)) return null

  return (
    <label className="branch-filter">
      {t('branches.filter')}
      <select value={value} onChange={(e) => onChange(e.target.value)}>
        {allowAll && (
          <option value="">
            {isSuperAdmin ? t('branches.allBranches') : t('branches.ownBranchDefault')}
          </option>
        )}
        {branches.map((b) => (
          <option key={b.id} value={b.id}>
            {b.code} — {b.name}
          </option>
        ))}
      </select>
    </label>
  )
}
