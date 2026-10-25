export const CREDIT_APPLICATION_CHANGED_EVENT = 'bankapp:credit-application-changed'

export type CreditApplicationChangedPayload = {
  applicationId: string
  applicationNumber: string
  customerId: string
  eventType: string
  occurredOnUtc: string
  amount: number | null
  currencyCode: string | null
}

export function dispatchCreditApplicationChanged(payload: CreditApplicationChangedPayload) {
  window.dispatchEvent(
    new CustomEvent<CreditApplicationChangedPayload>(CREDIT_APPLICATION_CHANGED_EVENT, {
      detail: payload,
    }),
  )
}
