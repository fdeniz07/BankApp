import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import {
  approveApplication,
  downloadApplicationDocument,
  exportPaymentPlan,
  getApplicationById,
  getApplicationDocuments,
  getPaymentPlan,
  rejectApplication,
  uploadApplicationDocument,
} from '../api/applicationsApi'
import { getCreditSubTypes } from '../api/creditSubTypesApi'
import {
  createDecisionLetterTemplate,
  deleteDecisionLetterTemplate,
  getDecisionLetterTemplates,
  updateDecisionLetterTemplate,
  type DecisionLetterTemplateItem,
  type DecisionTemplateKind,
} from '../api/decisionTemplatesApi'
import { getCreditDocumentsConfig } from '../api/creditDocumentsConfigApi'
import { useAuth } from '../auth/AuthContext'
import { canModifyBranchResource } from '../auth/token'
import type {
  CreditApplicationDetail,
  CreditApplicationDocumentItem,
  CreditDocumentType,
  CreditSubTypeListItem,
  PaymentPlanResponse,
} from '../types/api'
import { ApiError } from '../api/client'
import {
  CREDIT_APPLICATION_CHANGED_EVENT,
  type CreditApplicationChangedPayload,
} from '../realtime/creditApplicationEvents'
import { fillDecisionTemplate } from '../utils/decisionMessage'
import { printDownloadedFile, saveDownloadedFile } from '../utils/downloadFile'
import { currencySymbol, formatMoney } from '../utils/formatMoney'
import { mapApiError } from '../utils/mapApiError'
import { hasMissingRequiredDocuments } from '../utils/requiredDocuments'

export function ApplicationDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { t, i18n } = useTranslation()
  const { isStaff, user } = useAuth()
  const [item, setItem] = useState<CreditApplicationDetail | null>(null)
  const [subType, setSubType] = useState<CreditSubTypeListItem | null>(null)
  const [paymentPlan, setPaymentPlan] = useState<PaymentPlanResponse | null>(null)
  const [planTableOpen, setPlanTableOpen] = useState(false)
  const [planExportBusy, setPlanExportBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)

  const [approvedAmount, setApprovedAmount] = useState('')
  const [approvedTerm, setApprovedTerm] = useState('')
  const [interestRate, setInterestRate] = useState('')
  const [approveMessage, setApproveMessage] = useState('')
  const [rejectMessage, setRejectMessage] = useState('')
  const [approveDirty, setApproveDirty] = useState(false)
  const [rejectDirty, setRejectDirty] = useState(false)
  const [approveTemplateKey, setApproveTemplateKey] = useState('system')
  const [rejectTemplateKey, setRejectTemplateKey] = useState('system')
  const [userTemplates, setUserTemplates] = useState<DecisionLetterTemplateItem[]>([])
  const [templateEditor, setTemplateEditor] = useState<{
    kind: DecisionTemplateKind
    id?: string
    name: string
    locale: string
    body: string
  } | null>(null)
  const [rejectionReason, setRejectionReason] = useState('')
  const [documents, setDocuments] = useState<CreditApplicationDocumentItem[]>([])
  const [documentType, setDocumentType] = useState<CreditDocumentType>('IdentityDocument')
  const [documentFile, setDocumentFile] = useState<File | null>(null)
  const [documentBusy, setDocumentBusy] = useState(false)
  const [softApproveGateEnabled, setSoftApproveGateEnabled] = useState(false)

  const uiLocale = (i18n.language || 'tr').slice(0, 2)

  const loadUserTemplates = useCallback(async () => {
    if (!isStaff) return
    try {
      const list = await getDecisionLetterTemplates()
      setUserTemplates(list)
    } catch {
      setUserTemplates([])
    }
  }, [isStaff])

  async function loadDocuments(applicationId: string) {
    const list = await getApplicationDocuments(applicationId)
    setDocuments(list)
  }

  async function load() {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      const data = await getApplicationById(id)
      setItem(data)
      await loadDocuments(data.id)
      setApprovedAmount(String(data.requestedAmount))
      setApprovedTerm(String(data.requestedTermMonths))
      setInterestRate(String(data.approvedInterestRate ?? 1.75))
      setRejectionReason('')
      setApproveMessage('')
      setRejectMessage('')
      setApproveDirty(false)
      setRejectDirty(false)
      setApproveTemplateKey('system')
      setRejectTemplateKey('system')

      const subTypes = await getCreditSubTypes(0, 200)
      const match = subTypes.items.find((s) => s.id === data.creditSubTypeId) ?? null
      setSubType(match)
      if (match) {
        setInterestRate(String(data.approvedInterestRate ?? match.interestRate))
        if (!match.amountOptions.includes(data.requestedAmount) && match.amountOptions.length) {
          setApprovedAmount(String(match.amountOptions[0]))
        }
        if (!match.termOptions.includes(data.requestedTermMonths) && match.termOptions.length) {
          setApprovedTerm(String(match.termOptions[0]))
        }
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  useEffect(() => {
    if (!isStaff) {
      setSoftApproveGateEnabled(false)
      return
    }
    let cancelled = false
    getCreditDocumentsConfig()
      .then((cfg) => {
        if (!cancelled) setSoftApproveGateEnabled(!!cfg.softApproveGateEnabled)
      })
      .catch(() => {
        if (!cancelled) setSoftApproveGateEnabled(false)
      })
    return () => {
      cancelled = true
    }
  }, [isStaff])

  useEffect(() => {
    const onRealtime = (event: Event) => {
      const detail = (event as CustomEvent<CreditApplicationChangedPayload>).detail
      if (!id || !detail || detail.applicationId !== id) return
      void load()
    }
    window.addEventListener(CREDIT_APPLICATION_CHANGED_EVENT, onRealtime)
    return () => window.removeEventListener(CREDIT_APPLICATION_CHANGED_EVENT, onRealtime)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  const planAmount = useMemo(() => {
    if (!item) return 0
    if (item.status === 'Pending' && isStaff) return Number(approvedAmount) || item.requestedAmount
    if (item.status === 'Approved') return item.approvedAmount ?? item.requestedAmount
    return item.requestedAmount
  }, [item, isStaff, approvedAmount])

  const planTerm = useMemo(() => {
    if (!item) return 0
    if (item.status === 'Pending' && isStaff) return Number(approvedTerm) || item.requestedTermMonths
    if (item.status === 'Approved') return item.approvedTermMonths ?? item.requestedTermMonths
    return item.requestedTermMonths
  }, [item, isStaff, approvedTerm])

  const planRate = useMemo(() => {
    if (!item) return 0
    if (item.status === 'Pending' && isStaff) return Number(interestRate) || subType?.interestRate || 0
    if (item.status === 'Approved') return item.approvedInterestRate ?? subType?.interestRate ?? 0
    return subType?.interestRate ?? item.approvedInterestRate ?? 0
  }, [item, isStaff, interestRate, subType])

  const planSourceKey = useMemo(() => {
    if (!item) return 'sourceRequested'
    if (item.status === 'Pending' && isStaff) return 'sourcePending'
    if (item.status === 'Approved') return 'sourceApproved'
    return 'sourceRequested'
  }, [item, isStaff])

  function systemTemplateBody(kind: DecisionTemplateKind, locale: string) {
    const key =
      kind === 'Approve' ? 'applications.templates.approve' : 'applications.templates.reject'
    return String(i18n.getResource(locale, 'common', key) ?? t(key, { lng: locale }))
  }

  function fillBody(kind: DecisionTemplateKind, raw: string) {
    if (!item) return ''
    if (kind === 'Approve') {
      return fillDecisionTemplate(raw, {
        customerName: item.customerName || item.customerId,
        applicationNumber: item.applicationNumber,
        amount: formatMoney(
          Number(approvedAmount) || item.requestedAmount,
          item.currencyCode ?? 'USD',
          i18n.language,
        ),
        term: String(Number(approvedTerm) || item.requestedTermMonths),
        rate: String(Number(interestRate) || subType?.interestRate || 0),
      })
    }
    return fillDecisionTemplate(raw, {
      customerName: item.customerName || item.customerId,
      applicationNumber: item.applicationNumber,
      reason: rejectionReason.trim() || '…',
    })
  }

  function resolveTemplateRaw(kind: DecisionTemplateKind, key: string) {
    if (key === 'system') return systemTemplateBody(kind, uiLocale)
    if (key.startsWith('system:')) {
      const locale = key.slice('system:'.length)
      return systemTemplateBody(kind, locale)
    }
    const found = userTemplates.find((x) => x.id === key)
    return found?.body ?? systemTemplateBody(kind, uiLocale)
  }

  function applyTemplate(kind: DecisionTemplateKind, key: string) {
    const filled = fillBody(kind, resolveTemplateRaw(kind, key))
    if (kind === 'Approve') {
      setApproveTemplateKey(key)
      setApproveMessage(filled)
      setApproveDirty(false)
    } else {
      setRejectTemplateKey(key)
      setRejectMessage(filled)
      setRejectDirty(false)
    }
  }

  useEffect(() => {
    void loadUserTemplates()
  }, [loadUserTemplates])

  useEffect(() => {
    if (!item || !isStaff || item.status !== 'Pending') return
    if (!approveDirty) applyTemplate('Approve', approveTemplateKey === 'system' ? 'system' : approveTemplateKey)
    if (!rejectDirty) applyTemplate('Reject', rejectTemplateKey === 'system' ? 'system' : rejectTemplateKey)
    // Language / amount / reason changes refresh undirtied letters.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    item?.id,
    isStaff,
    item?.status,
    i18n.language,
    approvedAmount,
    approvedTerm,
    interestRate,
    rejectionReason,
    userTemplates,
  ])

  async function saveTemplateEditor() {
    if (!templateEditor) return
    const payload = {
      kind: templateEditor.kind,
      locale: templateEditor.locale,
      name: templateEditor.name.trim(),
      body: templateEditor.body.trim(),
    }
    if (!payload.name || !payload.body) return
    if (templateEditor.id) {
      await updateDecisionLetterTemplate({ ...payload, id: templateEditor.id })
    } else {
      await createDecisionLetterTemplate(payload)
    }
    setTemplateEditor(null)
    await loadUserTemplates()
  }

  async function removeUserTemplate(id: string) {
    if (!window.confirm(t('applications.templatesUi.confirmDelete'))) return
    await deleteDecisionLetterTemplate(id)
    if (approveTemplateKey === id) setApproveTemplateKey('system')
    if (rejectTemplateKey === id) setRejectTemplateKey('system')
    await loadUserTemplates()
  }

  function templateOptions(kind: DecisionTemplateKind) {
    const systemLocales = ['tr', 'en', 'de'] as const
    const system = [
      {
        key: 'system',
        label: t('applications.templatesUi.systemCurrent', { locale: uiLocale.toUpperCase() }),
      },
      ...systemLocales.map((locale) => ({
        key: `system:${locale}`,
        label: t('applications.templatesUi.systemLocale', {
          locale: t(`locale.${locale}`, { defaultValue: locale }),
        }),
      })),
    ]
    const mine = userTemplates
      .filter((x) => x.kind === kind)
      .map((x) => ({
        key: x.id,
        label: `${x.name} (${x.locale.toUpperCase()})`,
      }))
    return [...system, ...mine]
  }

  async function fetchPlanExport() {
    if (!item || !planAmount || !planTerm) throw new Error('missing-plan')
    return exportPaymentPlan({
      format: 'pdf',
      creditSubTypeId: item.creditSubTypeId,
      amount: planAmount,
      termMonths: planTerm,
      interestRate: planRate,
      locale: i18n.language,
      applicationNumber: item.applicationNumber,
      customerName: item.customerName,
    })
  }

  async function onDownloadPlan() {
    setPlanExportBusy(true)
    setError(null)
    try {
      saveDownloadedFile(await fetchPlanExport())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('paymentPlan.exportFailed'))
    } finally {
      setPlanExportBusy(false)
    }
  }

  async function onPrintPlan() {
    setPlanExportBusy(true)
    setError(null)
    try {
      printDownloadedFile(await fetchPlanExport())
    } catch (err) {
      if (err instanceof Error && err.message === 'popup-blocked') {
        setError(t('paymentPlan.printBlocked'))
      } else {
        setError(err instanceof ApiError ? err.message : t('paymentPlan.exportFailed'))
      }
    } finally {
      setPlanExportBusy(false)
    }
  }

  useEffect(() => {
    setPlanTableOpen(false)
    if (!item || !planAmount || !planTerm) {
      setPaymentPlan(null)
      return
    }
    let cancelled = false
    getPaymentPlan({
      creditSubTypeId: item.creditSubTypeId,
      amount: planAmount,
      termMonths: planTerm,
      interestRate: planRate,
    })
      .then((plan) => {
        if (!cancelled) setPaymentPlan(plan)
      })
      .catch(() => {
        if (!cancelled) setPaymentPlan(null)
      })
    return () => {
      cancelled = true
    }
  }, [item, planAmount, planTerm, planRate])

  async function onUploadDocument() {
    if (!id || !documentFile) return
    setDocumentBusy(true)
    setMessage(null)
    setError(null)
    try {
      await uploadApplicationDocument(id, documentType, documentFile)
      setDocumentFile(null)
      setMessage(t('applications.documentUploadSuccess'))
      await loadDocuments(id)
    } catch (err) {
      setError(mapApiError(err, t))
    } finally {
      setDocumentBusy(false)
    }
  }

  async function onDownloadDocument(documentId: string) {
    if (!id) return
    setDocumentBusy(true)
    setError(null)
    try {
      saveDownloadedFile(await downloadApplicationDocument(id, documentId))
    } catch (err) {
      setError(mapApiError(err, t))
    } finally {
      setDocumentBusy(false)
    }
  }

  function selectedTemplateLabel(kind: DecisionTemplateKind) {
    const key = kind === 'Approve' ? approveTemplateKey : rejectTemplateKey
    const opt = templateOptions(kind).find((o) => o.key === key)
    return opt?.label ?? key
  }

  async function onApprove(e: FormEvent) {
    e.preventDefault()
    if (!id || !item) return
    const missingDocs =
      softApproveGateEnabled && hasMissingRequiredDocuments(documents)
    if (missingDocs) {
      if (!window.confirm(t('applications.confirmApproveMissingDocs'))) return
    } else if (!window.confirm(t('applications.confirmApprove'))) {
      return
    }
    setBusy(true)
    setMessage(null)
    setError(null)
    const idempotencyKey = crypto.randomUUID()
    try {
      await approveApplication(
        id,
        {
          approvedAmount: Number(approvedAmount),
          approvedTermMonths: Number(approvedTerm),
          approvedInterestRate: Number(interestRate),
          notes: approveMessage.trim() || undefined,
          decisionTemplateName: selectedTemplateLabel('Approve'),
        },
        idempotencyKey,
      )
      setMessage(t('applications.approveSuccess'))
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'))
    } finally {
      setBusy(false)
    }
  }

  async function onReject(e: FormEvent) {
    e.preventDefault()
    if (!id || !item) return
    if (!window.confirm(t('applications.confirmReject'))) return
    setBusy(true)
    setMessage(null)
    setError(null)
    const idempotencyKey = crypto.randomUUID()
    try {
      await rejectApplication(
        id,
        {
          rejectionReason: rejectionReason.trim(),
          notes: rejectMessage.trim() || undefined,
          decisionTemplateName: selectedTemplateLabel('Reject'),
        },
        idempotencyKey,
      )
      setMessage(t('applications.rejectSuccess'))
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'))
    } finally {
      setBusy(false)
    }
  }

  if (loading) return <p>{t('common.loading')}</p>
  if (error && !item) return <p className="error-text">{error}</p>
  if (!item) return <p className="muted">{t('common.empty')}</p>

  const canDecide =
    isStaff && item.status === 'Pending' && canModifyBranchResource(user, item.branchId)
  const docsMissing =
    item.status === 'Pending' &&
    (item.missingRequiredDocuments || hasMissingRequiredDocuments(documents))
  const money = (value: number | null | undefined) =>
    formatMoney(value, item.currencyCode ?? 'USD', i18n.language)
  const amountOptions = subType?.amountOptions?.length
    ? subType.amountOptions
    : [item.requestedAmount]
  const termOptions = subType?.termOptions?.length
    ? subType.termOptions
    : [item.requestedTermMonths]

  return (
    <section className="detail">
      <Link to="/applications" className="back-link">
        ← {t('common.back')}
      </Link>
      <h1>
        {item.applicationNumber}{' '}
        <span className={`badge status-${item.status.toLowerCase()}`}>
          {t(`status.${item.status}`, { defaultValue: item.status })}
        </span>
      </h1>

      {message && <p className="success-text">{message}</p>}
      {error && <p className="error-text">{error}</p>}

      <dl className="info-grid">
        <div>
          <dt>{t('applications.customer')}</dt>
          <dd>
            {item.customerName || item.customerId}
            {item.customerCategory
              ? ` (${t(`category.${item.customerCategory}`, { defaultValue: item.customerCategory })})`
              : ''}
          </dd>
        </div>
        <div>
          <dt>{t('customers.customerNumber')}</dt>
          <dd>{item.customerNumber || '—'}</dd>
        </div>
        <div>
          <dt>
            {item.customerCategory === 'Corporate'
              ? t('customers.taxNumber')
              : t('customers.nationalId')}
          </dt>
          <dd>{item.customerIdentityNumber || '—'}</dd>
        </div>
        <div>
          <dt>{t('applications.creditType')}</dt>
          <dd>{item.creditSubTypeName || '—'}</dd>
        </div>
        <div>
          <dt>{t('applications.amount')}</dt>
          <dd>{money(item.requestedAmount)}</dd>
        </div>
        <div>
          <dt>{t('applications.term')}</dt>
          <dd>
            {item.requestedTermMonths} {t('applications.termUnit')}
          </dd>
        </div>
        <div>
          <dt>{t('applications.interestRate')}</dt>
          <dd>
            %
            {item.approvedInterestRate != null
              ? item.approvedInterestRate
              : (subType?.interestRate ?? planRate)}
          </dd>
        </div>
        <div>
          <dt>{t('applications.date')}</dt>
          <dd>{new Date(item.applicationDate).toLocaleString()}</dd>
        </div>
        <div>
          <dt>{t('applications.purpose')}</dt>
          <dd>{item.purpose ?? '—'}</dd>
        </div>
        <div>
          <dt>{t('applications.monthlyIncome')}</dt>
          <dd>{money(item.monthlyIncome)}</dd>
        </div>
        <div>
          <dt>{t('applications.existingDebt')}</dt>
          <dd>{money(item.existingDebtAmount)}</dd>
        </div>
        <div>
          <dt>{t('applications.decisionDate')}</dt>
          <dd>{item.decisionDate ? new Date(item.decisionDate).toLocaleString() : '—'}</dd>
        </div>
        <div className={item.status === 'Approved' ? 'info-highlight' : undefined}>
          <dt>{t('applications.approvedAmount')}</dt>
          <dd>{money(item.approvedAmount)}</dd>
        </div>
        <div className={item.status === 'Approved' ? 'info-highlight' : undefined}>
          <dt>{t('applications.approvedTerm')}</dt>
          <dd>
            {item.approvedTermMonths != null
              ? `${item.approvedTermMonths} ${t('applications.termUnit')}`
              : '—'}
          </dd>
        </div>
        <div>
          <dt>{t('applications.rejectionReason')}</dt>
          <dd>{item.rejectionReason ?? '—'}</dd>
        </div>
        <div>
          <dt>{t('applications.decisionBy')}</dt>
          <dd>
            {item.decisionByName || item.decisionByEmail || item.decisionBy ? (
              <>
                <div>
                  {item.decisionByName || item.decisionByEmail || item.decisionBy}
                  {item.decisionByPosition
                    ? ` · ${t(`staffPosition.${item.decisionByPosition}`, {
                        defaultValue: item.decisionByPosition,
                      })}`
                    : ''}
                </div>
                {item.decisionByName && (item.decisionByEmail || item.decisionBy) ? (
                  <div className="muted">{item.decisionByEmail || item.decisionBy}</div>
                ) : null}
              </>
            ) : (
              '—'
            )}
          </dd>
        </div>
        <div>
          <dt>{t('applications.decisionBranch')}</dt>
          <dd>
            {item.decisionBranchCode || item.decisionBranchName
              ? `${item.decisionBranchCode ?? ''}${
                  item.decisionBranchCode && item.decisionBranchName ? ' · ' : ''
                }${item.decisionBranchName ?? ''}`
              : '—'}
          </dd>
          <dt>{t('applications.decisionTemplate')}</dt>
          <dd>{item.decisionTemplateName || '—'}</dd>
          {(item.decisionBy || item.decisionBranchCode || item.decisionTemplateName) && (
            <>
              <dt>{t('applications.decisionAudit')}</dt>
              <dd className="decision-audit-line">
                {[
                  item.decisionByName || item.decisionByEmail || item.decisionBy,
                  item.decisionBranchCode,
                  item.decisionTemplateName,
                ]
                  .filter(Boolean)
                  .join(' · ')}
              </dd>
            </>
          )}
        </div>
        <div>
          <dt>{t('applications.customerMessage')}</dt>
          <dd className="customer-message-preview">{item.notes ?? '—'}</dd>
        </div>
      </dl>

      <section className="panel documents-panel">
        <h2 className="documents-title-row">
          {t('applications.documentsTitle')}
          {docsMissing && (
            <span className="docs-missing-dot" title={t('applications.missingDocsBadge')}>
              {t('applications.missingDocsBadge')}
            </span>
          )}
        </h2>
        <p className="muted">{t('applications.documentsHint')}</p>
        {softApproveGateEnabled && docsMissing && (
          <p className="warn-text">{t('applications.missingDocsWarn')}</p>
        )}

        {item.status === 'Pending' && (
          <form
            className="documents-upload"
            onSubmit={(e) => {
              e.preventDefault()
              void onUploadDocument()
            }}
          >
            <label>
              {t('applications.documentType')}
              <select
                value={documentType}
                onChange={(e) => setDocumentType(e.target.value as CreditDocumentType)}
                disabled={documentBusy}
              >
                <option value="IdentityDocument">
                  {t('applications.documentTypes.IdentityDocument')}
                </option>
                <option value="IncomeProof">{t('applications.documentTypes.IncomeProof')}</option>
                <option value="Other">{t('applications.documentTypes.Other')}</option>
              </select>
            </label>
            <label>
              {t('applications.documentFile')}
              <input
                type="file"
                accept=".pdf,image/jpeg,image/png,application/pdf"
                disabled={documentBusy}
                onChange={(e) => setDocumentFile(e.target.files?.[0] ?? null)}
              />
            </label>
            <button type="submit" className="btn" disabled={documentBusy || !documentFile}>
              {documentBusy ? t('common.loading') : t('applications.documentUpload')}
            </button>
          </form>
        )}

        {documents.length === 0 ? (
          <p className="muted">{t('applications.documentsEmpty')}</p>
        ) : (
          <ul className="documents-list">
            {documents.map((doc) => (
              <li key={doc.id}>
                <span>
                  {t(`applications.documentTypes.${doc.documentType}`, {
                    defaultValue: doc.documentType,
                  })}
                  {' · '}
                  {doc.originalFileName}
                  {' · '}
                  {(doc.sizeBytes / 1024).toFixed(0)} KB
                </span>
                <button
                  type="button"
                  className="btn ghost"
                  disabled={documentBusy}
                  onClick={() => void onDownloadDocument(doc.id)}
                >
                  {t('applications.documentDownload')}
                </button>
              </li>
            ))}
          </ul>
        )}
      </section>

      {paymentPlan && paymentPlan.installments.length > 0 && (
        <div className="payment-plan panel">
          <h2>{t('paymentPlan.title')}</h2>
          <p className="plan-basis">
            {t('paymentPlan.basis', {
              amount: formatMoney(planAmount, paymentPlan.currencyCode, i18n.language),
              term: planTerm,
              source: t(`paymentPlan.${planSourceKey}`),
            })}
          </p>
          <p className="muted">
            {t('paymentPlan.rateLine', { rate: paymentPlan.interestRate })}
          </p>
          <p className="muted">
            {t('paymentPlan.summary', {
              monthly: formatMoney(paymentPlan.monthlyPayment, paymentPlan.currencyCode, i18n.language),
              total: formatMoney(paymentPlan.totalPayment, paymentPlan.currencyCode, i18n.language),
              interest: formatMoney(paymentPlan.totalInterest, paymentPlan.currencyCode, i18n.language),
            })}
          </p>
          <div className="payment-plan-actions">
            <button
              type="button"
              className="btn ghost"
              disabled={planExportBusy}
              onClick={() => void onDownloadPlan()}
            >
              {planExportBusy ? t('common.loading') : t('paymentPlan.downloadPdf')}
            </button>
            <button
              type="button"
              className="btn ghost"
              disabled={planExportBusy}
              onClick={() => void onPrintPlan()}
            >
              {t('paymentPlan.print')}
            </button>
            <button
              type="button"
              className="btn ghost payment-plan-toggle"
              aria-expanded={planTableOpen}
              onClick={() => setPlanTableOpen((open) => !open)}
            >
              {planTableOpen
                ? t('paymentPlan.hideInstallments')
                : t('paymentPlan.showInstallments', {
                    count: paymentPlan.installments.length,
                  })}
            </button>
          </div>
          {planTableOpen && (
            <div className="table-wrap payment-plan-table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>{t('paymentPlan.month')}</th>
                    <th>{t('paymentPlan.payment')}</th>
                    <th>{t('paymentPlan.principal')}</th>
                    <th>{t('paymentPlan.interest')}</th>
                    <th>{t('paymentPlan.balance')}</th>
                  </tr>
                </thead>
                <tbody>
                  {paymentPlan.installments.map((row) => (
                    <tr key={row.month}>
                      <td>{row.month}</td>
                      <td>{formatMoney(row.payment, paymentPlan.currencyCode, i18n.language)}</td>
                      <td>{formatMoney(row.principal, paymentPlan.currencyCode, i18n.language)}</td>
                      <td>{formatMoney(row.interest, paymentPlan.currencyCode, i18n.language)}</td>
                      <td>
                        {formatMoney(row.remainingBalance, paymentPlan.currencyCode, i18n.language)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {isStaff && item.status !== 'Pending' && (
        <p className="muted">{t('applications.onlyPending')}</p>
      )}

      {canDecide && (
        <div className="decision-panels">
          <form className="panel form" onSubmit={onApprove}>
            <h2>{t('applications.approve')}</h2>
            <label>
              {t('applications.approvedAmount')} ({currencySymbol(item.currencyCode)})
              <select
                value={approvedAmount}
                onChange={(e) => setApprovedAmount(e.target.value)}
                required
              >
                {amountOptions.map((amount) => (
                  <option key={amount} value={amount}>
                    {formatMoney(amount, item.currencyCode, i18n.language)}
                  </option>
                ))}
              </select>
            </label>
            <label>
              {t('applications.approvedTerm')}
              <select
                value={approvedTerm}
                onChange={(e) => setApprovedTerm(e.target.value)}
                required
              >
                {termOptions.map((term) => (
                  <option key={term} value={term}>
                    {term} {t('applications.termUnit')}
                  </option>
                ))}
              </select>
            </label>
            <label>
              {t('applications.interestRate')}
              <input
                type="number"
                step="0.01"
                value={interestRate}
                onChange={(e) => setInterestRate(e.target.value)}
                required
              />
            </label>
            <label>
              {t('applications.templatesUi.pick')}
              <select
                value={approveTemplateKey}
                onChange={(e) => applyTemplate('Approve', e.target.value)}
              >
                {templateOptions('Approve').map((opt) => (
                  <option key={opt.key} value={opt.key}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </label>
            <label>
              {t('applications.customerMessage')}
              <textarea
                value={approveMessage}
                onChange={(e) => {
                  setApproveDirty(true)
                  setApproveMessage(e.target.value)
                }}
                rows={8}
                required
              />
            </label>
            <p className="field-hint">{t('applications.customerMessageHint')}</p>
            <div className="row-actions">
              <button
                type="button"
                className="btn ghost"
                onClick={() => applyTemplate('Approve', approveTemplateKey)}
              >
                {t('applications.reloadTemplate')}
              </button>
              <button
                type="button"
                className="btn ghost"
                onClick={() =>
                  setTemplateEditor({
                    kind: 'Approve',
                    name: '',
                    locale: uiLocale,
                    body: resolveTemplateRaw('Approve', approveTemplateKey),
                  })
                }
              >
                {t('applications.templatesUi.saveAs')}
              </button>
              {approveTemplateKey &&
                !approveTemplateKey.startsWith('system') &&
                approveTemplateKey !== 'system' && (
                  <button
                    type="button"
                    className="btn danger"
                    onClick={() => void removeUserTemplate(approveTemplateKey)}
                  >
                    {t('applications.templatesUi.delete')}
                  </button>
                )}
            </div>
            <button type="submit" className="btn success decision-btn" disabled={busy}>
              {t('applications.approve')}
            </button>
          </form>

          <form className="panel form" onSubmit={onReject}>
            <h2>{t('applications.reject')}</h2>
            <label>
              {t('applications.rejectionReason')}
              <textarea
                value={rejectionReason}
                onChange={(e) => setRejectionReason(e.target.value)}
                rows={2}
                required
              />
            </label>
            <p className="field-hint">{t('applications.rejectionReasonHint')}</p>
            <label>
              {t('applications.templatesUi.pick')}
              <select
                value={rejectTemplateKey}
                onChange={(e) => applyTemplate('Reject', e.target.value)}
              >
                {templateOptions('Reject').map((opt) => (
                  <option key={opt.key} value={opt.key}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </label>
            <label>
              {t('applications.customerMessage')}
              <textarea
                value={rejectMessage}
                onChange={(e) => {
                  setRejectDirty(true)
                  setRejectMessage(e.target.value)
                }}
                rows={8}
                required
              />
            </label>
            <p className="field-hint">{t('applications.customerMessageHint')}</p>
            <div className="row-actions">
              <button
                type="button"
                className="btn ghost"
                onClick={() => applyTemplate('Reject', rejectTemplateKey)}
              >
                {t('applications.reloadTemplate')}
              </button>
              <button
                type="button"
                className="btn ghost"
                onClick={() =>
                  setTemplateEditor({
                    kind: 'Reject',
                    name: '',
                    locale: uiLocale,
                    body: resolveTemplateRaw('Reject', rejectTemplateKey),
                  })
                }
              >
                {t('applications.templatesUi.saveAs')}
              </button>
              {rejectTemplateKey &&
                !rejectTemplateKey.startsWith('system') &&
                rejectTemplateKey !== 'system' && (
                  <button
                    type="button"
                    className="btn danger"
                    onClick={() => void removeUserTemplate(rejectTemplateKey)}
                  >
                    {t('applications.templatesUi.delete')}
                  </button>
                )}
            </div>
            <button type="submit" className="btn reject decision-btn" disabled={busy}>
              {t('applications.reject')}
            </button>
          </form>
        </div>
      )}

      {templateEditor && (
        <form
          className="panel form"
          onSubmit={(e) => {
            e.preventDefault()
            void saveTemplateEditor().catch((err) => setError(mapApiError(err, t)))
          }}
        >
          <h2>
            {templateEditor.id
              ? t('applications.templatesUi.edit')
              : t('applications.templatesUi.create')}
          </h2>
          <label>
            {t('applications.templatesUi.name')}
            <input
              value={templateEditor.name}
              onChange={(e) => setTemplateEditor({ ...templateEditor, name: e.target.value })}
              required
            />
          </label>
          <label>
            {t('creditTypes.locale')}
            <select
              value={templateEditor.locale}
              onChange={(e) => setTemplateEditor({ ...templateEditor, locale: e.target.value })}
            >
              {(['tr', 'en', 'de'] as const).map((locale) => (
                <option key={locale} value={locale}>
                  {t(`locale.${locale}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('applications.templatesUi.body')}
            <textarea
              value={templateEditor.body}
              onChange={(e) => setTemplateEditor({ ...templateEditor, body: e.target.value })}
              rows={8}
              required
            />
          </label>
          <p className="field-hint">{t('applications.templatesUi.placeholders')}</p>
          <div className="row-actions">
            <button type="submit" className="btn primary">
              {t('common.save')}
            </button>
            <button type="button" className="btn ghost" onClick={() => setTemplateEditor(null)}>
              {t('common.cancel')}
            </button>
          </div>
        </form>
      )}
    </section>
  )
}
