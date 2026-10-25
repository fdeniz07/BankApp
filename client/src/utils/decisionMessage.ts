export type DecisionTemplateVars = {
  customerName: string
  applicationNumber: string
  reason?: string
  amount?: string
  term?: string
  rate?: string
}

/** Replace `{{key}}` placeholders in a decision letter template. */
export function fillDecisionTemplate(
  template: string,
  vars: DecisionTemplateVars,
): string {
  const map: Record<string, string> = {
    customerName: vars.customerName,
    applicationNumber: vars.applicationNumber,
    reason: vars.reason ?? '',
    amount: vars.amount ?? '',
    term: vars.term ?? '',
    rate: vars.rate ?? '',
  }

  return template.replace(/\{\{(\w+)\}\}/g, (_match, key: string) => map[key] ?? '')
}
