# FRD — Credit decision: document gate + approval authority

| Field | Value |
|-------|--------|
| Parent BRD | [brd.md](brd.md) — BR-CR-07, BR-CR-08 |
| Modules | Credit (Application / WebApi / client) |
| Last updated | 2026-11-02 |

---

## 1. Purpose

For staff credit approval:

1. Mandatory documents (identity + income) applied as **Soft** or **Hard** per environment  
2. **Position-based** approval amount ceiling (DoA) enforced  

Both behaviours are turned on/off / selected via **appsettings**; code paths always exist.

---

## 2. Configuration

### 2.1 Document gate

```json
"CreditDocuments": {
  "ApproveGateMode": "Hard"
}
```

| Value | UI | API Approve |
|--------|-----|-------------|
| `None` | No Soft confirm; no yellow band | No block |
| `Soft` | Confirm on missing docs + yellow band | No block |
| `Hard` | Approve button disabled + red message | `MissingRequiredDocuments` |

Environment recommendation:

- `appsettings.Development.json` → `Soft`
- `appsettings.json` (base / prod-like) → `Hard`

List badge, “Missing required documents” filter, and “Pending + missing documents” preset remain **in all modes**.

Required types: `IdentityDocument`, `IncomeProof` (`RequiredCreditDocuments`).

### 2.2 Approval authority (DoA)

```json
"ApprovalAuthority": {
  "Enabled": true,
  "LimitsByPosition": {
    "CreditAnalyst": 50000,
    "RelationshipManager": 100000,
    "BranchOfficer": 75000,
    "RiskOfficer": 250000,
    "OperationsSpecialist": 25000,
    "BranchManager": 500000
  },
  "UnlimitedRoles": [ "BankManager", "SuperAdmin" ]
}
```

| Condition | Behaviour |
|-----------|-------------|
| `Enabled: false` | No limit (Development default) |
| Role in `UnlimitedRoles` | No limit (`maxApprovalAmount: null`); branch ceiling bypassed too |
| In position limit table | PositionMax = value |
| `Branch.MaxApprovalAmount` set | BranchMax = value; AppliedMax = `min(PositionMax, BranchMax)` |
| Branch ceiling null / ≤0 | Position ceiling only |
| No / undefined position | PositionMax = 0 → approval blocked in practice |

Development: `Enabled: false`. Branch ceiling managed by SuperAdmin via `Branches` API/UI.

---

## 3. API

### 3.1 `GET /api/CreditDocuments/config`

Response (camelCase):

| Field | Type | Meaning |
|-------|------|---------|
| `approveGateMode` | string | `None` \| `Soft` \| `Hard` |
| `softApproveGateEnabled` | bool | `mode == Soft` (backward compatible) |
| `approvalAuthorityEnabled` | bool | Options.Enabled |
| `maxApprovalAmount` | number \| null | AppliedMax; null = unlimited / off |
| `positionMaxApprovalAmount` | number \| null | Position ceiling (informational) |
| `branchMaxApprovalAmount` | number \| null | Branch ceiling (informational) |

### 3.2 `POST .../Approve` (existing)

Additional checks (in order, after existing rules):

1. Hard + missing required documents → `BusinessException` / `MissingRequiredDocuments`
2. DoA on + amount > ceiling → `AmountExceedsApprovalAuthority`

---

## 4. UI (ApplicationDetailPage)

| Condition | Behaviour |
|-----------|-------------|
| Soft + missing documents | Yellow warning; extra confirm on approve |
| Hard + missing documents | Red message; submit disabled |
| DoA on | Limit text + position/branch → applied breakdown; red + submit disabled on exceed |
| API error message | Existing `mapApiError` / toast |

---

## 5. Acceptance criteria

- [ ] Development: Soft confirm works; Hard block not applied; high amount approvable when DoA off  
- [ ] Base config (or test override): Approve without docs under Hard returns 400/business rule  
- [ ] Approve rejected when CreditAnalyst limit exceeded; BankManager may exceed  
- [ ] Approve rejected per branch ceiling when lower than position  
- [ ] Unit: Hard missing docs + authority exceeded + branch min  
- [ ] `GET .../config` returns correct mode/limit/breakdown for staff session  

---

## 6. Where in the repo?

- Options: `CreditDocumentsOptions`, `ApprovalAuthorityOptions`
- Evaluate: `ApprovalAuthorityEvaluator`
- Handler: `ApproveCreditApplicationRequestHandler`
- Entity: `Branch.MaxApprovalAmount`
- Controller: `CreditDocumentsController.GetConfig`
- Client: `creditDocumentsConfigApi.ts`, `ApplicationDetailPage.tsx`, `BranchesPage.tsx`
- Learning: [30-approve-gate-authority.md](../../learning/tr/30-approve-gate-authority.md)
