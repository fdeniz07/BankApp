import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'react-toastify'
import { useAuth } from '../auth/AuthContext'
import { getToken } from '../auth/token'
import { formatMoney } from '../utils/formatMoney'
import {
  dispatchCreditApplicationChanged,
  type CreditApplicationChangedPayload,
} from './creditApplicationEvents'

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'
const HUB_PATH = '/hubs/credit-applications'

type ServerMessage = {
  applicationId: string
  applicationNumber: string
  customerId: string
  eventType: string
  occurredOnUtc: string
  amount?: number | null
}

function normalizeMessage(raw: ServerMessage): CreditApplicationChangedPayload {
  const amount =
    typeof raw.amount === 'number' && !Number.isNaN(raw.amount) ? raw.amount : null
  return {
    applicationId: raw.applicationId,
    applicationNumber: raw.applicationNumber,
    customerId: raw.customerId,
    eventType: raw.eventType,
    occurredOnUtc: raw.occurredOnUtc,
    amount,
  }
}

export function useCreditApplicationRealtime() {
  const { isAuthenticated } = useAuth()
  const { t, i18n } = useTranslation()
  const connectionRef = useRef<HubConnection | null>(null)

  useEffect(() => {
    if (!isAuthenticated) {
      void connectionRef.current?.stop()
      connectionRef.current = null
      return
    }

    const token = getToken()
    if (!token) return

    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE}${HUB_PATH}`, {
        accessTokenFactory: () => getToken() ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build()

    connection.on('creditApplicationChanged', (raw: ServerMessage) => {
      const payload = normalizeMessage(raw)
      dispatchCreditApplicationChanged(payload)

      const event = t(`realtime.events.${payload.eventType}`, {
        defaultValue: payload.eventType,
      })

      const message =
        payload.amount != null
          ? t('realtime.creditChangedWithAmount', {
              event,
              amount: formatMoney(payload.amount, 'TRY', i18n.language, 0),
            })
          : t('realtime.creditChanged', { event })

      switch (payload.eventType) {
        case 'CreditApplicationApproved':
          toast.success(message)
          break
        case 'CreditApplicationRejected':
          toast.error(message)
          break
        case 'CreditApplicationCancelled':
          toast.warning(message)
          break
        default:
          toast.info(message)
          break
      }
    })

    connectionRef.current = connection
    void connection.start().catch((err: unknown) => {
      console.warn('SignalR connect failed', err)
    })

    return () => {
      void connection.stop()
      if (connectionRef.current === connection) connectionRef.current = null
    }
  }, [isAuthenticated, t, i18n.language])
}
